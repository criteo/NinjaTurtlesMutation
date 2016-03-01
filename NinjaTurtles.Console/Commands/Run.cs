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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Mono.Cecil;

using NinjaTurtles.Console.Options;

namespace NinjaTurtles.Console.Commands
{
    internal class Run : Command
    {
        private string _testAssemblyLocation;
        private string _message;
        private Type _runnerType;

        protected override string HelpText
        {
            get { return @"usage: NinjaTurtles.Console run [<options>] TEST_ASSEMBLY

Runs all mutation tests found in the specified test assembly.

Options:
   --class [-c]       : Specifies the full type name of the class for which
                        mutation testing should be applied. If no accompanying
                        method name is specified, all methods in the class are
                        identified, and mutation testing applied for each of
                        them.
   --format [f]       : Specifies the format for the output file specified by
                        the --output option. By default, this will be XML, but
                        HTML can be specified here to transform the results
                        into displayable format.
   --method [-m]      : Specifies the name of the method for which mutation
                        testing should be applied. Will be ignored if not used
                        in conjunction with the --class option.
   --output [-o]      : Specifies the name of a file to receive the mutation
                        testing output. This file will be deleted if it already
                        exists.
   --runner [-r]      : Specifies the type name of an implementation of
                        ITestRunner that is used to run the unit tests for each
                        code mutant.
   --type [-t]        : Specifies the type name of a parameter to the method,
                        used to resolve between overloads of the same method
                        name. Can be specified multiple times, and must be in
                        the same order as the method's parameters. Full type
                        names enclosed in double quotes are accepted.

Arguments:
   TEST_ASSEMBLY      : The file name of the test assembly to inspect, which
                        should be in the current working directory.

Example:
   NinjaTurtles.Console run -c NinjaTurtles.MethodDefinitionResolver
       -m ResolveMethod
       -tt ""Mono.Cecil.TypeDefinition, Mono.Cecil"" System.String
       -o ResolveMethod.html -f HTML
       NinjaTurtles.Tests.dll

   This command will identify all unit tests in NinjaTurtles.Tests.dll that
   exercise the two-parameter override of the ResolveMethod method, and use
   them to perform mutation testing of that method. The resulting output will
   be transformed to HTML and saved to the file ResolveMethod.html.";
            }
        }

        public override bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }
            if (Options.Arguments.Count == 0)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The 'run' command must take the path to a test assembly as an argument.");
                }
                return false;
            }
            _testAssemblyLocation = Path.Combine(Environment.CurrentDirectory, Options.Arguments[0]);
            if (!File.Exists(_testAssemblyLocation))
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"Test assembly '{0}' does not exist in the current working directory.",
                        Options.Arguments[0]);
                    return false;
                }
            }
            return true;
        }

        public override bool Execute()
        {
            System.Console.WriteLine("IN: options.Command.Execute()"); /////////////
            bool result;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                System.Console.WriteLine("IN: options.Command.Execute() >> double using"); /////////////
                string outputPath = "";
                var outputOption = Options.Options.OfType<Output>().FirstOrDefault();
                if (outputOption != null)
                {
                    outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.FileName);
                    if (File.Exists(outputPath)) File.Delete(outputPath);
                }
                System.Console.WriteLine("IN: options.Command.Execute() >> double using >> output path set"); /////////////
                var originalOut = System.Console.Out;
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(writer);
                }
                System.Console.WriteLine("IN: options.Command.Execute() >> double using >> output path set >> verbosity set"); /////////////
                var runnerOption = (Runner)Options.Options.FirstOrDefault(o => o is Runner);
                if (runnerOption != null)
                {
                    var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
                    _runnerType = TypeResolver.ResolveTypeFromReferences(testAssembly, runnerOption.RunnerType);
                    if (_runnerType == null || _runnerType.GetInterface("ITestRunner") == null)
                    {
                        _message = string.Format(
                            "Invalid runner type '{0}' specified.",
                            runnerOption.RunnerType);
                        ReportResult(false);
                        return false;
                    }
                }
                System.Console.WriteLine("IN: options.Command.Execute() >> double using >> output path set >> verbosity set >> runner opt set"); /////////////
/*                var runnerMethod = Options.Options.Any(o => o is TargetClass)
                                       ? (Options.Options.Any(o => o is TargetMethod)
                                              ? (Func<bool>)RunMutationTestsForClassAndMethod
                                              : RunMutationTestsForClass)
                                       : RunAllMutationTestsInAssembly;*/
                var runnerMethod = Options.Options.Any(o => o is TargetClass)
                                                       ? (Options.Options.Any(o => o is TargetMethod)
                                                              ? (Func<bool>)RunMutationTestsForClassAndMethod
                                                              : RunMutationTestsForClass)
                                                       : RunMutationTestsForAllClassAndMethods;
                result = runnerMethod();
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(originalOut);
                }
                var formatOption = Options.Options.OfType<Format>().FirstOrDefault();
                if (outputOption != null && formatOption != null && formatOption.OutputFormat == "HTML")
                {
                    FormatOutput(outputPath);
                }
                System.Console.WriteLine("IN: options.Command.Execute() >> double using >> output path set >> verbosity set >> runner opt set >> format out set"); /////////////
                OutputWriter.WriteLine();
                ReportResult(result);
                System.Console.WriteLine("IN: options.Command.Execute() >> double using >> output path set >> verbosity set >> runner opt set >> format out set >> report done"); /////////////
                return result;
            }
        }

        private void ReportResult(bool result)
        {
            System.Console.WriteLine("IN: options.Command.ReportResult()"); /////////////
            var statusColor = result
                                  ? ConsoleColor.Green
                                  : ConsoleColor.Red;
            using (new OutputWriterHighlight(statusColor))
            {
                OutputWriter.WriteLine(_message);
            }
        }

        private static void FormatOutput(string outputPath)
        {
            if (!File.Exists(outputPath)) return;
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            File.Move(outputPath, tempPath);
            var xslt = new XslCompiledTransform();
            var resolver = new EmbeddedResourceResolver();
            xslt.Load("ReportXslt.xslt", XsltSettings.TrustedXslt, resolver);
            using (var reader = XmlReader.Create(tempPath))
            {                
                xslt.Transform(reader, XmlWriter.Create(outputPath));
            }
            File.Delete(tempPath);
        }

        private bool RunMutationTestsForClass()
        {
            System.Console.WriteLine("  IN: options.Command.RunMutationTestsForClass()"); /////////////
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, targetClass);
            if (matchedType == null)
            {
                _message = string.Format(@"Unknown type '{0}'", targetClass);
                System.Console.WriteLine("  OUT PREMATURE: options.Command.RunMutationTestsForClass()"); /////////////
                return false;
            }
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(matchedType.Assembly.Location);
            var targetType = assemblyDefinition.MainModule.Types.FirstOrDefault(t => t.FullName == targetClass);
            bool result = true;
            foreach (var methodInfo in targetType.Methods
                .Where(m => m.HasBody && m.Name != Methods.STATIC_CONSTRUCTOR))
            {
                string targetMethod = methodInfo.Name;
                string methodReturnType = methodInfo.ReturnType.FullName;
                var methodsGenerics = methodInfo.GenericParameters.ToArray();
                var parameterTypes = methodInfo.Parameters.Select(p => p.ParameterType).ToArray();
                bool runResultBuf = RunTests(matchedType.Assembly.Location, targetClass, methodReturnType, targetMethod, methodsGenerics, parameterTypes);
                System.Console.WriteLine("  RunTests(AssemblyLoc: {0}, TargetClass: {1}, TargetMethod: {2}, paramTypes len: {3}) = {4}",    matchedType.Assembly.Location,
                                                                                                                                            targetClass,
                                                                                                                                            targetMethod,
                                                                                                                                            parameterTypes.Length,
                                                                                                                                            runResultBuf); //////////
                result &= runResultBuf;
            }
            if (string.IsNullOrEmpty(_message))
            {
                _message = string.Format(
                    @"Mutation testing {0}",
                    result ? "passed" : "failed");
            }
            System.Console.WriteLine("  OUT: options.Command.RunMutationTestsForClass()"); /////////////
            return result;
        }

        private bool RunMutationTestsForClassAndMethod()
        {
            System.Console.WriteLine("  IN: options.Command.RunMutationTestsForClassAndMethod()"); /////////////
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, targetClass);
            if (matchedType == null)
            {
                _message = string.Format(@"Unknown type '{0}'", targetClass);
                System.Console.WriteLine("  OUT PREMATURE: options.Command.RunMutationTestsForClassAndMethod()"); /////////////
                return false;
            }
            string targetMethod = Options.Options.OfType<TargetMethod>().Single().MethodName;
            var typeOptions = Options.Options.OfType<ParameterType>().Select(p => p.ResolvedType).ToArray();
            var result = 
                typeOptions.Any()
                ? RunTests(matchedType.Assembly.Location, targetClass, targetMethod, typeOptions)
                : RunTests(matchedType.Assembly.Location, targetClass, targetMethod);
            if (string.IsNullOrEmpty(_message))
            {
                _message = string.Format(
                    @"Mutation testing {0}",
                    result ? "passed" : "failed");
            }
            System.Console.WriteLine("  OUT: options.Command.RunMutationTestsForClassAndMethod()"); /////////////
            return result;
        }

        private bool RunTests(string targetAssemblyLocation, string targetClass, string returnType, string targetMethod, GenericParameter[] methodGenerics, TypeReference[] parameterTypes)
        {
            System.Console.WriteLine("      IN: Command.RunTests(string, string, string, TypeReference[]) TAL:{0}, TC:{1}, TM:{2}, TR PT.len:{3}",    targetAssemblyLocation,
                                                                                                                                                targetClass,
                                                                                                                                                targetMethod,
                                                                                                                                                parameterTypes.Length); ////////////////
            var parameterList = parameterTypes == null || parameterTypes.Length == 0
                                    ? null
                                    : string.Join(", ", parameterTypes.Select(t => t.Name).ToArray());
            OutputMethod(targetClass, targetMethod, parameterList);
            MutationTest mutationTest =
                parameterTypes == null
                    ? (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, returnType, targetMethod, methodGenerics)
                    : (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, returnType, targetMethod, methodGenerics, parameterTypes);
            if (_runnerType != null)
                mutationTest.UsingRunner(_runnerType);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            var result = BuildAndRunMutationTest(mutationTest);
            System.Console.WriteLine("      OUT: Command.RunTests()"); ////////////////
            return result;
        }

        private bool BuildAndRunMutationTest(MutationTest mutationTest)
        {
            System.Console.WriteLine("          IN: Run.BuildAndRunMutationTest()"); ////////////
            var outputOption = Options.Options.OfType<Output>().FirstOrDefault();
            if (outputOption != null)
            {
                string outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.FileName);
                mutationTest.MergeReportTo(outputPath);
            }
            bool result = false;
            try
            {
                mutationTest.Run();
                result = true;
            }
            catch (MutationTestFailureException)
            {
            }
            catch (Exception ex)
            {
                //Debugger.Launch();
                _message =
                    @"
An exception was thrown setting up the mutation tests. The exception details
are below. If you are unable to resolve the problem from these details, please
post the details in our issue tracker at:

http://ninjaturtles.codeplex.com/workitem/list/basic

Exception details:

" + ex;
            }
            System.Console.WriteLine("          OUT: Run.BuildAndRunMutationTest()"); ////////////
            return result;
        }

        private static void OutputMethod(string targetClass, string targetMethod, string parameterList)
        {
            if (string.IsNullOrEmpty(parameterList))
            {
                OutputWriter.WriteLine(
                    @"Running mutation tests for {0}.{1}",
                    targetClass,
                    targetMethod);
            }
            else
            {
                OutputWriter.WriteLine(
                    @"Running mutation tests for {0}.{1}({2})",
                    targetClass,
                    targetMethod,
                    parameterList);
            }
        }
        
        private bool RunTests(string targetAssemblyLocation, string targetClass, string targetMethod, Type[] parameterTypes = null)
        {
            System.Console.WriteLine("IN: Command.RunTests() TAL:{0}, TC:{1}, TM:{2}, T PT.len:{3}", targetAssemblyLocation, targetClass, targetMethod, (parameterTypes != null ? parameterTypes.Length : 0)); ////////////////
            var parameterList = parameterTypes == null || parameterTypes.Length == 0
                                    ? null
                                    : string.Join(", ", parameterTypes.Select(t => t.Name).ToArray());
            OutputMethod(targetClass, targetMethod, parameterList);
            MutationTest mutationTest =
                parameterTypes == null
                    ? (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, targetMethod)
                    : (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, targetMethod, parameterTypes);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            var result = BuildAndRunMutationTest(mutationTest);
            System.Console.WriteLine("OUT: Command.RunTests()"); ////////////////
            return result;
        }
        
        private bool RunAllMutationTestsInAssembly()
        {
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            int tests = 0;
            int failures = 0;
            foreach (var type in testAssembly.GetTypes())
            {
                if (!type.IsPublic) continue;
                var mutationTests = type.GetMethods()
                    .Where(m => m.GetCustomAttributes(true).Any(a => a.GetType() == typeof(MutationTestAttribute)));
                if (mutationTests.Any())
                {
                    var testFixtureInstance = Activator.CreateInstance(type);
                    foreach (var mutationTest in mutationTests)
                    {
                        tests++;
                        try
                        {
                            mutationTest.Invoke(testFixtureInstance, null);
                        }
                        catch (MutationTestFailureException)
                        {
                            failures++;
                        }
                        OutputWriter.Write(".");
                    }
                }
            }
            OutputWriter.WriteLine();
            _message = string.Format(
                @"Mutation test summary: {0} passed, {1} failed",
                tests - failures,
                failures);
            return tests > 0 && failures == 0;
        }

        private bool RunMutationTestsForAllClassAndMethods()
        {
            System.Console.WriteLine("This is a start...."); ////////////
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, "PrimeFinderMutationPlayground.PrimeFinder");
            System.Console.WriteLine("testassembly : [{0}], matched type : [{1}]", testAssembly, matchedType);
            _message = @"The strict necessary";
            return true;
        }
    }
}
