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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mono.Cecil;

using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles;
using NinjaTurtles.Turtles.Method;

namespace NinjaTurtles
{
    internal class MutationTest : IMutationTest
    {
        private readonly AssemblyDefinition _assembly;
        private readonly ISet<Type> _methodTurtles = new HashSet<Type>();
        private readonly string _methodName;
        private readonly Type[] _parameterTypes;
        private readonly Type _targetClass;
        private readonly string _testAssemblyLocation;

        internal MutationTest(Type targetClass, string methodName, Type[] parameterTypes, string testAssemblyLocation)
        {
            _targetClass = targetClass;
            _methodName = methodName;
            _parameterTypes = parameterTypes;
            _testAssemblyLocation = testAssemblyLocation;
            _assembly = AssemblyDefinition.ReadAssembly(targetClass.Assembly.Location);
            TestRunner = typeof(NUnitTestRunner);
        }

        internal Type TestRunner { get; set; }

        #region IMutationTest Members

        public void Run(int? maxThreads = null)
        {
            using (var runner = (ITestRunner)Activator.CreateInstance(TestRunner))
            {
                string fileName = _targetClass.Assembly.Location;
                if (_methodTurtles.Count == 0)
                {
                    PopulateDefaultTurtles();
                }
                bool allFailed = true;
                foreach (TypeDefinition type in _assembly.MainModule.Types.Where(t => t.FullName == _targetClass.FullName))
                {
                    int passCount = 0;
                    var methodsWithMatchingName = type.Methods.Where(m => m.Name == _methodName);
                    if (!methodsWithMatchingName.Any())
                    {
                        throw new MutationTestFailureException(
                            string.Format("Unknown method '{0}'",
                                          _methodName));
                    }
                    if (_parameterTypes != null)
                    {
                        methodsWithMatchingName = methodsWithMatchingName
                            .Where(m => Enumerable.SequenceEqual(
                                m.Parameters.Select(p => p.ParameterType.FullName),
                                _parameterTypes.Select(t => _assembly.MainModule.Import(t).FullName)));
                    }
                    var methodDefinition = methodsWithMatchingName.SingleOrDefault();
                    if (methodDefinition == null)
                    {
                        throw new MutationTestFailureException(
                            string.Format("Unable to identify unique method named '{0}'",
                                          _methodName));
                    }

                    var assemblyContainingTests = AssemblyDefinition.ReadAssembly(_testAssemblyLocation);
                    var testsToRun = new List<string>();
                    foreach (var typeDefinition in assemblyContainingTests.MainModule.Types)
                    {
                        testsToRun
                            .AddRange(typeDefinition.Methods
                                          .Where(m => m.HasTestedAttributeMatching(methodDefinition))
                                          .Select(m => m.GetQualifiedName()));
                    }

                    if (!testsToRun.Any())
                    {
                        throw new MutationTestFailureException("No matching tests found.");
                    }

                    bool mutationsFound = false;
                    var parallelOptions = new ParallelOptions();
                    if (maxThreads.HasValue)
                    {
                        parallelOptions.MaxDegreeOfParallelism = maxThreads.Value;
                    }
                    foreach (Type methodTurtle in _methodTurtles)
                    {
                        var turtle = (IMethodTurtle)Activator.CreateInstance(methodTurtle);
                        Console.WriteLine(turtle.Description);
                        Parallel.ForEach(turtle.Mutate(methodDefinition, _assembly, fileName),
                                         parallelOptions,
                                         mutation =>
                                             {
                                                 mutationsFound = true;
                                                 string testAssembly = Path.Combine(mutation.TestFolder,
                                                                                    Path.GetFileName(
                                                                                        _testAssemblyLocation));
                                                 bool? result = runner.RunTestsWithMutations(methodDefinition,
                                                                                             testAssembly, testsToRun.ToArray());
                                                 OutputResultToConsole(mutation, result);
                                                 if (result ?? false) Interlocked.Increment(ref passCount);
                                                 mutation.Dispose();
                                             });
                        if (!mutationsFound)
                        {
                            Console.WriteLine("\tNo valid mutations found (this is fine)");
                        }

                        allFailed &= (passCount == 0);
                    }
                }

                if (!allFailed) throw new MutationTestFailureException();
            }
        }

        public IMutationTest With<T>() where T : ITurtle
        {
            _methodTurtles.Add(typeof(T));
            return this;
        }

        public IMutationTest UsingRunner<T>() where T : ITestRunner
        {
            TestRunner = typeof(T);
            return this;
        }

        #endregion

        private static void OutputResultToConsole(MutationTestMetaData metaData, bool? result)
        {
            string interpretation;
            if (!result.HasValue)
            {
                interpretation = "No valid tests found to run, or tests timed out";
            }
            else if (result.Value)
            {
                interpretation = "Passed (this is bad)";
            }
            else
            {
                interpretation = "Failed (this is good)";
            }
            string output = string.Format("\t{0}: {1}\n", metaData.Description, interpretation);
            if (!string.IsNullOrEmpty(metaData.DiffRepresentation) && (result ?? false))
            {
                output += metaData.DiffRepresentation;
            }
            Console.Write(output);
        }

        private void PopulateDefaultTurtles()
        {
            foreach (Type type in GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterface(typeof(IMethodTurtle).Name) != null))
            {
                _methodTurtles.Add(type);
            }
        }
    }
}