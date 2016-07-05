#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Win32;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using NinjaTurtlesMutation.Reporting;
using NinjaTurtlesMutation.ServiceTestRunnerLib;
using NinjaTurtlesMutation.Turtles;

namespace NinjaTurtlesMutation
{
    internal class MutationTest : IMutationTest
	{
	    private const string ERROR_REPORTING_KEY = @"SOFTWARE\Microsoft\Windows\Windows Error Reporting";
	    private const string ERROR_REPORTING_VALUE = "DontShowUI";

        private MethodDefinition _method;
        private int[] _originalOffsets;
	    private readonly IList<Type> _mutationsToApply = new List<Type>();
        private readonly string _returnType;
        private readonly GenericParameter[] _genericParameters;
        private readonly Type[] _parameterTypes;
        private readonly TypeReference[] _parameterTypeReferences;
        private AssemblyDefinition _testAssembly;
	    private Module _module;
	    private MutationTestingReport _report;
        private ReportingStrategy _reportingStrategy = new NullReportingStrategy();
	    private string _reportFileName;
	    private readonly MethodReferenceComparer _comparer = new MethodReferenceComparer();
	    private static readonly Regex _automaticallyGeneratedNestedClassMatcher = new Regex("^\\<([A-Za-z0-9@_]+)\\>");

        private readonly TestsDispatcher _dispatcher;

        private Dictionary<string, MutantMetaData> _pendingTest;

		private string _testAssemblyLocation;
        private IDictionary<string, string> _testMethods;
        private IEnumerable<string> _testsToRun;
        private TestsBenchmark _benchmark;

        private MutationTest(string testAssemblyLocation, Type targetType, string targetMethod, TestsDispatcher dispatcher, IDictionary<string, string> testMethods)
        {
            TestAssemblyLocation = testAssemblyLocation;
            TargetType = targetType;
            TargetMethod = targetMethod;
            _dispatcher = dispatcher;
            _testMethods = testMethods;
        }

        internal MutationTest(string testAssemblyLocation, Type targetType, string targetMethod, Type[] parameterTypes, TestsDispatcher dispatcher, IDictionary<string, string> testMethods) : this(testAssemblyLocation, targetType, targetMethod, dispatcher, testMethods)
        {
            _parameterTypes = parameterTypes;
        }

        public MutationTest(string testAssemblyLocation, Type targetType, string returnType, string targetMethod, GenericParameter[] genericsParameters, Type[] parameterTypes, TestsDispatcher dispatcher, IDictionary<string, string> testMethods)
            : this(testAssemblyLocation, targetType, targetMethod, dispatcher, testMethods)
        {
            _returnType = returnType;
            _genericParameters = genericsParameters;
            _parameterTypes = parameterTypes;
        }

        public MutationTest(string testAssemblyLocation, Type targetType, string returnType, string targetMethod, GenericParameter[] genericsParameters, TypeReference[] parameterTypes, TestsDispatcher dispatcher, IDictionary<string, string> testMethods)
            : this(testAssemblyLocation, targetType, targetMethod, dispatcher, testMethods)
        {
            _returnType = returnType;
            _genericParameters = genericsParameters;
            _parameterTypeReferences = parameterTypes;
        }

        public Type TargetType { get; private set; }

		public string TargetMethod { get; private set; }

	    public string TestAssemblyLocation
	    {
	        get { return _testAssemblyLocation; }
	        set
	        {
	            _testAssemblyLocation = value;
	            _testAssembly = AssemblyDefinition.ReadAssembly(_testAssemblyLocation);
	        }
	    }

        public MutationTestingReport Report { get { return _report; } }

	    public float Run(bool detachBench)
		{
            int count;
            int failures;

            var errorReportingValue = TurnOffErrorReporting();
	        var matchingMethods = MethodDiscovery();
	        if (DiscoverTestsToRun(matchingMethods) == 0)
	            return 0;
            Console.WriteLine(
                "Suite of {0} tests identified for {1}.{2}",
                _testsToRun.Count(),
                TargetType.FullName,
                TargetMethod);
            _benchmark = new TestsBenchmark(_testAssemblyLocation, _testsToRun.ToArray());
	        _benchmark.LaunchBenchmark(detachBench);
            MutateAndTest(out count, out failures);
            RestoreErrorReporting(errorReportingValue);
            if (count == 0)
				Console.WriteLine("No valid mutations found (this is fine).");
	        if (failures == 0)
	            return 1;
	        return failures / (float) count;
		}

        private List<MethodReference> MethodDiscovery()
        {
            _method = ValidateMethod();
            _module.LoadDebugInformation();
            var matchingMethods = new List<MethodReference>();
            AddMethod(_method, matchingMethods);
            _originalOffsets = _method.Body.Instructions.Select(i => i.Offset).ToArray();
            _report = new MutationTestingReport(_method);
            return matchingMethods;
        }

        private int DiscoverTestsToRun(IList<MethodReference> matchingMethods)
        {
            try
            {
                _testsToRun = GetMatchingTestsFromTree(_method, matchingMethods, _testMethods);
            }
            catch (MutationTestFailureException)
            {
                _report.RegisterMethod(_method);
                _reportingStrategy.WriteReport(_report, _reportFileName);
                return 0;
            }
            _report.TestsFounded = true;
            return _testsToRun.Count();
        }

        private void MutateAndTest(out int mutationCount, out int mutationFailures)
        {
            mutationCount = 0;
            mutationFailures = 0;
            if (_mutationsToApply.Count == 0) PopulateDefaultTurtles();
            foreach (var turtleType in _mutationsToApply)
            {
                var turtle = (MethodTurtleBase)Activator.CreateInstance(turtleType);
                Console.WriteLine(turtle.Description);
                _pendingTest = new Dictionary<string, MutantMetaData>();
                foreach (var mutation in turtle.Mutate(_method, _module, _originalOffsets))
                    SendMutationTestToDispatcher(mutation);
                ProceedTestResult(turtle, ref mutationFailures, ref mutationCount);
            }

            _report.RegisterMethod(_method);
            _reportingStrategy.WriteReport(_report, _reportFileName);
        }

        private void SendMutationTestToDispatcher(MutantMetaData mutation)
        {
            TestDescription testDescription = new TestDescription(Path.Combine(mutation.TestDirectory.FullName, Path.GetFileName(TestAssemblyLocation)), _testsToRun, _benchmark.TotalMs);
            _pendingTest.Add(testDescription.Uid, mutation);
            _dispatcher.SendTest(testDescription);
        }

        private void ProceedTestResult(MethodTurtleBase turtle, ref int failures, ref int count)
        {
            while (_pendingTest.Count != 0)
            {
                var testResult = _dispatcher.ReadATest();
                var mutation = _pendingTest[testResult.Uid];
                if (!CheckTestResult(turtle, mutation, testResult))
                    Interlocked.Increment(ref failures);
                Interlocked.Increment(ref count);
                _pendingTest.Remove(testResult.Uid);
            }
        }

        private bool CheckTestResult(MethodTurtleBase turtle, MutantMetaData mutation, TestDescription testDescription)
        {
            var exitedInTime = testDescription.ExitedInTime;
            var exitCode = (testDescription.TestsPass ? 0 : 1);
            bool testSuitePassed = exitCode == 0 && exitedInTime;
            string result = string.Format(" Mutant: {0}. {1}",
                              mutation.Description,
                              testSuitePassed
                                ? "Survived."
                                : "Killed.");
            _report.AddResult(mutation.MethodDefinition.GetCurrentSequencePoint(mutation.ILIndex), mutation, !testSuitePassed);

            if (testSuitePassed)
            {
                mutation.TestDirectory.DoNotDelete = true;
                var sourceFilename = mutation.MethodDefinition.GetOriginalSourceFileName(mutation.ILIndex);
                var testDirectoryPath = mutation.TestDirectoryName;
                if (sourceFilename != null)
                {
                    var sourceCode = mutation.GetOriginalSourceCode(mutation.ILIndex);
                    result =
                        string.Format(
                            "{0}\nOriginal source code around surviving mutant (in {1}):\n{2}\nFiles left for inspection in: {3}",
                            result,
                            sourceFilename,
                            sourceCode,
                            testDirectoryPath);
                }
                else
                    result = string.Format(
                            "{0}\nOriginal source code couldn't be retrieve\nFiles left for inspection in: {1}",
                            result,
                            testDirectoryPath);

            }

            Console.WriteLine(result);

            turtle.MutantComplete(mutation);
            return !testSuitePassed;
        }

        private static object TurnOffErrorReporting()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(ERROR_REPORTING_KEY,
                                                           RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key == null) return null;
                var errorReportingValue = key.GetValue(ERROR_REPORTING_VALUE, null);
                key.SetValue(ERROR_REPORTING_VALUE, 1, RegistryValueKind.DWord);
                key.Close();
                return errorReportingValue;
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        private static void RestoreErrorReporting(object errorReportingValue)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(ERROR_REPORTING_KEY,
                                                           RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key == null) return;
                if (errorReportingValue == null)
                {
                    key.DeleteValue(ERROR_REPORTING_VALUE);
                }
                else
                {
                    key.SetValue(ERROR_REPORTING_VALUE, errorReportingValue, RegistryValueKind.DWord);
                }
                key.Close();
            }
            catch (SecurityException) {}
        }

        private MethodDefinition ValidateMethod()
        {
            _module = new Module(TargetType.Assembly.Location);

            var type = ResolveFromTypeCollection(_module.Definition.Types);
            if (_parameterTypes != null)
            {
                return MethodDefinitionResolver.ResolveMethod(type, _returnType, TargetMethod, _genericParameters, _parameterTypes);
            }
            return MethodDefinitionResolver.ResolveMethod(type, _returnType, TargetMethod, _genericParameters, _parameterTypeReferences);
        }

        private void AddMethod(MethodDefinition targetMethod, List<MethodReference> matchingMethods)
        {
            if (matchingMethods.Contains(targetMethod, _comparer)) return;
            matchingMethods.Add(targetMethod);
            AddCallingMethods(targetMethod, matchingMethods);
            var declaringType = targetMethod.DeclaringType;
            TypeDefinition type = declaringType;
            while (type != null && type.BaseType != null)
            {
                var thisModule = type.BaseType.Module;
                if (type.BaseType.Scope.Name != type.BaseType.Module.Assembly.Name.Name
                    && type.BaseType.Scope is AssemblyNameReference)
                {
                    thisModule =
                        type.BaseType.Module.AssemblyResolver.Resolve((AssemblyNameReference)type.BaseType.Scope).
                            MainModule;
                }
                type = thisModule.Types.SingleOrDefault(t => t.FullName == type.BaseType.FullName);
                if (type != null)
                {
                    var baseMethod = type.Methods
                        .SingleOrDefault(m => MethodsMatch(m, targetMethod));
                    if (baseMethod != null)
                    {
                        AddMethod(baseMethod, matchingMethods);
                        break;
                    }
                    AddMethodsForInterfaces(targetMethod, matchingMethods, type);
                }
            }
            AddMethodsForInterfaces(targetMethod, matchingMethods, declaringType);
        }

	    private void AddMethodsForInterfaces(MethodDefinition targetMethod, List<MethodReference> matchingMethods, TypeDefinition type)
	    {
	        foreach (var interfaceReference in type.Interfaces)
	        {
	            var thisModule = type.Module;
	            if (interfaceReference.Scope.Name != type.Module.Assembly.Name.Name
	                && interfaceReference.Scope is AssemblyNameReference)
	            {
	                thisModule =
	                    interfaceReference.Module.AssemblyResolver.Resolve((AssemblyNameReference)interfaceReference.Scope).
	                        MainModule;
	            }
	            var interfaceDefinition =
	                thisModule.Types.SingleOrDefault(t => t.FullName == interfaceReference.FullName);
	            if (interfaceDefinition != null)
	            {
                    var interfaceMethods = interfaceDefinition.Methods.Where(m => MethodsMatch(m, targetMethod));
	                foreach (var interfaceMethod in interfaceMethods)
                        AddMethod(interfaceMethod, matchingMethods);
                }
	        }
	    }

        private ISet<string> GetMatchingTestsFromTree(MethodDefinition targetmethod, IList<MethodReference> matchingMethods, IDictionary<string, string> testMethods,  bool force = false)
        {
            ISet<string> result = new HashSet<string>();
            foreach (var type in _testAssembly.MainModule.Types)
                AddTestsForType(targetmethod, matchingMethods, force, type, testMethods, result);
            if (!force && result.Count == 0)
            {
                result = GetMatchingTestsFromTree(targetmethod, matchingMethods, testMethods, true);
            }
            if (result.Count == 0)
            {
                Console.WriteLine("No matching tests found so mutation testing cannot be applied.");
                throw new MutationTestFailureException("No matching tests were found to run.");
            }
            return result;
	    }

        private void AddTestsForType(MethodDefinition targetmethod, IList<MethodReference> matchingMethods, bool force, TypeDefinition type, IDictionary<string, string> testMethods, ISet<string> result)
	    {
	        String          targetType = targetmethod.DeclaringType.FullName;

            foreach (MethodDefinition method in type.Methods.Where(m => m.HasBody))
            {
                var methodName = method.Name;
                if (methodName.StartsWith("<"))
                {
                    var parts = methodName.Split('<', '>');
                    methodName = parts[1];
                }
                if (!testMethods.Keys.Contains(methodName))
                    continue;
                if (!force && !DoesMethodReferenceType(method, targetType))
                    continue;
                if (!MethodCallTargetDirectOrIndirect(method, matchingMethods))
                    continue;
                var methodFinalName = testMethods[methodName];
                result.Add(methodFinalName);
            }
            if (type.NestedTypes == null)
                return;
            foreach (TypeDefinition typeDefinition in type.NestedTypes)
                AddTestsForType(targetmethod, matchingMethods, force, typeDefinition, testMethods, result);
        }

        private bool MethodCallTargetDirectOrIndirect(MethodDefinition methodDefinition, IList<MethodReference> matchingMethods)
        {
            foreach (Instruction instruction in methodDefinition.Body.Instructions)
            {
                if (!(instruction.OpCode == OpCodes.Call // Call method
                      || instruction.OpCode == OpCodes.Callvirt // Call a method associated with an object
                      || instruction.OpCode == OpCodes.Newobj // Allocate an uninitialized object or value type and call ctor
                      || instruction.OpCode == OpCodes.Ldftn)) // Push a pointer to a method referenced by method, on the stack
                    continue;
                var operandAsMethodReference = instruction.Operand as MethodReference;
                if (operandAsMethodReference == null)
                    continue;
                MethodDefinition operandAsMethodDefinition = null;
                try
                {
                    operandAsMethodDefinition = operandAsMethodReference.Resolve();
                }
                catch {}
                var operandAsComparableMethod = operandAsMethodDefinition ?? operandAsMethodReference;
                if (!matchingMethods.Any(m => _comparer.Equals(m, operandAsComparableMethod)))  // At least one matching method correspond to the instruction's operand
                    continue;
                if (methodDefinition.CustomAttributes.Any(a => a.AttributeType.Name == "MutationTestAttribute"))
                    continue;
                return (true);
            }
            return (false);
        }

	    private static bool DoesMethodReferenceType(MethodDefinition method, string targetType)
	    {
	        bool typeUsed = false;
	        foreach (var instruction in method.Body.Instructions)
	        {
	            if (instruction.OpCode == OpCodes.Call
	                || instruction.OpCode == OpCodes.Callvirt
	                || instruction.OpCode == OpCodes.Newobj
	                || instruction.OpCode == OpCodes.Ldftn)
	            {
	                var reference = (MethodReference)instruction.Operand;
	                var declaringType = reference.DeclaringType;
	                if (declaringType.FullName == targetType)
	                {
	                    typeUsed = true;
	                    break;
	                }
	                var genericType = declaringType as GenericInstanceType;
	                if (genericType != null)
	                {
	                    if (genericType.GenericArguments.Any(a => a.FullName == targetType))
	                    {
	                        typeUsed = true;
	                        break;
	                    }
	                }
	                var genericMethod = reference as GenericInstanceMethod;
	                if (genericMethod != null)
	                {
	                    if (genericMethod.GenericArguments.Any(a => a.FullName == targetType))
	                    {
	                        typeUsed = true;
	                        break;
	                    }
	                }
	            }
	            if (instruction.OpCode == OpCodes.Newobj)
	            {
	                var constructor = (MemberReference)instruction.Operand;
	                var declaringType = constructor.DeclaringType;
	                if (declaringType.FullName == targetType)
	                {
	                    typeUsed = true;
	                    break;
	                }
	            }
	        }
	        return typeUsed;
	    }

        private void AddCallingMethods(MethodReference targetMethod, List<MethodReference> matchingMethods)
        {
            foreach (var type in _module.Definition.Types)
            AddCallingMethodsForType(targetMethod, matchingMethods, type);
        }

	    private void AddCallingMethodsForType(MethodReference targetMethod, List<MethodReference> matchingMethods, TypeDefinition type)
	    {
	        foreach (var method in type.Methods.Where(m => m.HasBody))
	        foreach (var instruction in method.Body.Instructions)
	        {
	            if (instruction.OpCode == OpCodes.Call
	                || instruction.OpCode == OpCodes.Callvirt
                    || instruction.OpCode == OpCodes.Newobj
                    || instruction.OpCode == OpCodes.Ldftn)
	            {
	                var reference = (MethodReference)instruction.Operand;
	                if (_comparer.Equals(reference, targetMethod)
	                    && !matchingMethods.Contains(method, _comparer))
	                {
                        AddMethod(method, matchingMethods);
                        var match = _automaticallyGeneratedNestedClassMatcher.Match(type.Name);
                        if (match.Success && type.DeclaringType != null)
                        {
                            if (type.DeclaringType.Methods.Any(m => m.Name == match.Groups[1].Value))
                            {
                                AddMethod(type.DeclaringType.Methods.First(m => m.Name == match.Groups[1].Value), matchingMethods);
                            }
                        }
	                }
	            }
	        }
            foreach (var nestedType in type.NestedTypes)
            {
                AddCallingMethodsForType(targetMethod, matchingMethods, nestedType);
            }
	    }

	    private class MethodReferenceComparer : IEqualityComparer<MethodReference>
        {
            public bool Equals(MethodReference x, MethodReference y)
            {
                if (x.Name != y.Name)
                    return false;
                return x.DeclaringType.FullName == y.DeclaringType.FullName
                       && x.Parameters.Select(p => p.ParameterType.Name)
                              .SequenceEqual(y.Parameters.Select(p => p.ParameterType.Name))
                       && x.GenericParameters.Count == y.GenericParameters.Count;
            }

            public int GetHashCode(MethodReference obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private bool MethodsMatch(MethodDefinition first, MethodDefinition second)
        {
            return first.Name == second.Name
                   && first.Parameters.Select(p => p.ParameterType.Name)
                          .SequenceEqual(second.Parameters.Select(p => p.ParameterType.Name))
                   && first.GenericParameters.Count == second.GenericParameters.Count;
        }
        
		private void PopulateDefaultTurtles()
		{
            foreach (var type in GetType().Assembly.GetTypes()
                .Where(t => t.GetInterface("IMethodTurtle") != null
                && !t.IsAbstract))
            {
                _mutationsToApply.Add(type);
            }
		}

	    private TypeDefinition ResolveFromTypeCollection(Collection<TypeDefinition> types)
	    {
// ReSharper disable PossibleNullReferenceException
	        var type = types.SingleOrDefault(t => t.FullName == TargetType.FullName.Replace("+", "/"));
// ReSharper restore PossibleNullReferenceException
            if (type == null)
            {
                foreach (var typeDefinition in types)
                {
                    if (typeDefinition.NestedTypes != null)
                    {
                        type = ResolveFromTypeCollection(typeDefinition.NestedTypes);
                        if (type != null) return type;
                    }
                }
            }
	        return type;
	    }

	    public IMutationTest With<T>() where T : IMethodTurtle
		{
			_mutationsToApply.Add(typeof(T));
			return this;
		}

        public void With(ISet<Type> turtleSet)
        {
            foreach (var type in turtleSet)
                _mutationsToApply.Add(type);
        }

        public IMutationTest WriteReportTo(string fileName)
	    {
	        _reportingStrategy = new OverwriteReportingStrategy();
	        _reportFileName = fileName;
	        return this;
	    }

	    public IMutationTest MergeReportTo(string fileName)
	    {
	        _reportingStrategy = new MergeReportingStrategy();
            _reportFileName = fileName;
            return this;
	    }

        private abstract class ReportingStrategy
        {
            public abstract void WriteReport(MutationTestingReport report, string fileName);
        }

        private class NullReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName) { }
        }

        private class OverwriteReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName)
            {
                using (var streamWriter = File.CreateText(fileName))
                {
                    new XmlSerializer(typeof(MutationTestingReport)).Serialize(streamWriter, report);
                }
            }
        }

        private class MergeReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName)
            {
                report.MergeFromFile(fileName);
                using (var streamWriter = File.CreateText(fileName))
                {
                    new XmlSerializer(typeof(MutationTestingReport)).Serialize(streamWriter, report);
                }
            }
        }
	}
}

