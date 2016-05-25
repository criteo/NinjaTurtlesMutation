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
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Xml;
using System.Xml.Xsl;
using Mono.Cecil;
using NinjaTurtlesMutation.Console.Options;
using NinjaTurtlesMutation.Console.Reporting;
using NinjaTurtlesMutation.ServiceTestRunnerLib.Utilities;

namespace NinjaTurtlesMutation.Console.Commands
{
    internal class Run : Command
    {
        private string _testAssemblyLocation;
        private string _message;
        private readonly MutationTestingReportSummary _report = new MutationTestingReportSummary();

        private Output _outputOption;
        private readonly TextWriter _originalOut = System.Console.Out;
        private string _outputPath;

        private Process _testDispatcher;
        private AnonymousPipeServerStream _testDispatcherPipeIn;
        private AnonymousPipeServerStream _testDispatcherPipeOut;
        private AnonymousPipeServerStream _testDispatcherPipeCmd;
        private StreamReader _testDispatcherStreamIn;
        private StreamWriter _testDispatcherStreamOut;
        private StreamWriter _testDispatcherStreamCmd;

        protected override string HelpText
        {
            get { return @"usage: NinjaTurtles.Console run [<options>] TEST_ASSEMBLY

Runs all mutation tests found in the specified test assembly.

Options:
   --class [-c]                 : Specifies the full type name of the class for which
                                  mutation testing should be applied. If no accompanying
                                  method name is specified, all methods in the class are
                                  identified, and mutation testing applied for each of
                                  them.
   --format [-f]                 : Specifies the format for the output file specified by
                                  the --output option. By default, this will be XML, but
                                  HTML can be specified here to transform the results
                                  into displayable format.
   --namespace [-N]             : Specifies the namespace class for which mutation testing
                                  should be applied. All classes and method under that
                                  namespace are identified, and mutation testing is applied
                                  for each of them.
   --output [-o]                : Specifies the name of a file to receive the mutation
                                  testing output. This file will be deleted if it already
                                  exists.
   --parallelization [-p]       : Set the number of test runner to use. Default value is 8

Arguments:
   TEST_ASSEMBLY      : The file name of the test assembly to inspect, which
                        should be in the current working directory.

Example:
   NinjaTurtlesMutation.Console run -N NinjaTurtlesMutation
       -o NinjaTurtlesMutation.html -f HTML
       NinjaTurtlesMutation.Tests.dll

   This command will identify all classes and methods under NinjaTurtlesMutation
   namespace, and use unit tests from NinjaTurtlesMutation.Tests.dll to perform
   mutation testing of these methods. The resulting output will be transformed
   to HTML and saved to the file NinjaTurtlesMutation.html.";
            }
        }

        private void InitTestDispatcher()
        {
            var parallelLevel = Options.Options.OfType<ParallelLevel>().SingleOrDefault();
            var parallelValue = (parallelLevel == null ? 8 : parallelLevel.ParallelValue);
            _testDispatcherPipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _testDispatcherPipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _testDispatcherPipeCmd = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _testDispatcherStreamIn = new StreamReader(_testDispatcherPipeIn);
            _testDispatcherStreamOut = new StreamWriter(_testDispatcherPipeOut);
            _testDispatcherStreamCmd = new StreamWriter(_testDispatcherPipeCmd);
            _testDispatcher = new Process
            {
                StartInfo =
                {
                    FileName = "NTMDispatcher.exe",
                    UseShellExecute = false,
                    Arguments = _testDispatcherPipeOut.GetClientHandleAsString() + " " +
                                _testDispatcherPipeIn.GetClientHandleAsString() + " " +
                                _testDispatcherPipeCmd.GetClientHandleAsString() + " " + parallelValue
                }
            };
            _testDispatcher.Start();
            _testDispatcherPipeCmd.DisposeLocalCopyOfClientHandle();
            _testDispatcherPipeOut.DisposeLocalCopyOfClientHandle();
            _testDispatcherPipeIn.DisposeLocalCopyOfClientHandle();
        }

        private void DisposeTestDispatcher()
        {
            CommandExchanger.SendData(_testDispatcherStreamCmd, CommandExchanger.Commands.STOP);

            _testDispatcherStreamIn.Dispose();
            _testDispatcherStreamOut.Dispose();
            _testDispatcherStreamCmd.Dispose();
            _testDispatcherPipeIn.Dispose();
            _testDispatcherPipeOut.Dispose();
            _testDispatcherPipeCmd.Dispose();
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
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The 'run' command need admin privilege to create symlink.");
                }
                return false;
            }
            var benchmark = new TestsBenchmark(_testAssemblyLocation);
            benchmark.LaunchBenchmark();
            var originalSourcesPassTests = benchmark.TestsPass;
            if (!originalSourcesPassTests)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The original source code tests fail. All your tests must pass in order to start mutation testing.");
                }
                return false;
            }
            return true;
        }

        public override bool Execute()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                ConfigureOutput(writer);
                var runnerMethod = ConfigureRun();
                InitTestDispatcher();
                var result = runnerMethod();
                DisposeTestDispatcher();
                RestoreOutput();
                ReportResult(result, _report);
                return result;
            }
        }

        private bool RunMutationTestsForClass()
        {
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, targetClass);
            if (matchedType == null)
            {
                _message = string.Format(@"Unknown type '{0}'", targetClass);
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
                result &= runResultBuf;
            }
            if (string.IsNullOrEmpty(_message))
            {
                _message = string.Format(
                    @"Mutation testing {0}",
                    result ? "passed" : "failed");
            }
            return result;
        }

        private bool RunMutationTestsForClassAndMethod()
        {
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, targetClass);
            if (matchedType == null)
            {
                _message = string.Format(@"Unknown type '{0}'", targetClass);
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
            return result;
        }

        private bool RunMutationTestsForAllClassAndMethods()
        {
            bool result = true;

            var nspace = Options.Options.OfType<TargetNamespace>().Single().NamespaceName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedTypes = TypeResolver.ResolveNamespaceTypesFromReferences(testAssembly, nspace);
            System.Console.WriteLine("testassembly : [{0}], matched types : [[{1}]]", testAssembly, string.Join("], [", matchedTypes.Select(t => t.FullName))); ////////////////
            System.Console.WriteLine("{0} types matched under {1}", matchedTypes.Length, nspace); ////////////////////
            if (matchedTypes.Length == 0)
            {
                _message = string.Format(@"No types found under {0}", nspace);
                return false;
            }
            foreach (var type in matchedTypes)
            {
                var resultBuf = RunMutationTestsForType(type, type.FullName);
                result &= resultBuf;
            }
            if (!string.IsNullOrEmpty(_message))
                return result;
            _message = string.Format(@"Mutation testing {0}", result ? "passed" : "failed");
            return result;
        }

        private bool RunMutationTestsForType(Type type, string targetClass)
        {
            bool result = true;

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(type.Assembly.Location);
            var targetType = assemblyDefinition.MainModule.Types.FirstOrDefault(t => t.FullName == targetClass);
            foreach (var methodInfo in targetType.Methods.Where(m => m.HasBody && m.Name != Methods.STATIC_CONSTRUCTOR))
            {
                string targetMethod = methodInfo.Name;
                string methodReturnType = methodInfo.ReturnType.FullName;
                var methodsGenerics = methodInfo.GenericParameters.ToArray();
                var parameterTypes = methodInfo.Parameters.Select(p => p.ParameterType).ToArray();
                bool runResultBuf = RunTests(type.Assembly.Location, targetClass, methodReturnType, targetMethod, methodsGenerics, parameterTypes);
                result &= runResultBuf;
            }
            return result;
        }

        private bool RunTests(string targetAssemblyLocation, string targetClass, string returnType, string targetMethod, GenericParameter[] methodGenerics, TypeReference[] parameterTypes)
        {
            var parameterList = parameterTypes == null || parameterTypes.Length == 0
                                    ? null
                                    : string.Join(", ", parameterTypes.Select(t => t.Name).ToArray());
            OutputMethod(targetClass, targetMethod, parameterList);
            MutationTest mutationTest =
                parameterTypes == null
                    ? (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, returnType, targetMethod, methodGenerics, _testDispatcherStreamOut, _testDispatcherStreamIn)
                    : (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, returnType, targetMethod, methodGenerics, _testDispatcherStreamOut, _testDispatcherStreamIn, parameterTypes);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            var result = BuildAndRunMutationTest(mutationTest);
            return result;
        }

        private bool RunTests(string targetAssemblyLocation, string targetClass, string targetMethod, Type[] parameterTypes = null)
        {
            var parameterList = parameterTypes == null || parameterTypes.Length == 0
                                    ? null
                                    : string.Join(", ", parameterTypes.Select(t => t.Name).ToArray());
            OutputMethod(targetClass, targetMethod, parameterList);
            MutationTest mutationTest =
                parameterTypes == null
                    ? (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, targetMethod, _testDispatcherStreamOut, _testDispatcherStreamIn)
                    : (MutationTest)MutationTestBuilder.For(targetAssemblyLocation, targetClass, targetMethod, _testDispatcherStreamOut, _testDispatcherStreamIn, parameterTypes);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            var result = BuildAndRunMutationTest(mutationTest);
            return result;
        }

        private bool BuildAndRunMutationTest(MutationTest mutationTest)
        {
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
                _message =
                    @"
An exception was thrown setting up the mutation tests. The exception details
are below. If you are unable to resolve the problem from these details, please
post the details in our issue tracker at:

https://github.com/criteo/NinjaTurtlesMutation/issues

Exception details:

" + ex;
            }
            _report.MergeMutationTestReport(mutationTest.Report);
            return result;
        }

        private Func<Boolean> ConfigureRun()
        {
            var runnerMethod = Options.Options.Any(o => o is TargetNamespace) ? RunMutationTestsForAllClassAndMethods
                                : Options.Options.Any(o => o is TargetClass)
                                    ? (Options.Options.Any(o => o is TargetMethod)
                                        ? (Func<bool>)RunMutationTestsForClassAndMethod
                                            : RunMutationTestsForClass)
                                                : RunAllMutationTestsInAssembly;
            return runnerMethod;
        }

        private void ConfigureOutput(StreamWriter sinkStream)
        {
            _outputPath = "";
            _outputOption = Options.Options.OfType<Output>().FirstOrDefault();
            if (_outputOption != null)
            {
                _outputPath = Path.Combine(Environment.CurrentDirectory, _outputOption.FileName);
                if (File.Exists(_outputPath)) File.Delete(_outputPath);
            }
            if (!Options.Options.Any(o => o is Verbose))
            {
                System.Console.SetOut(sinkStream);
            }
        }

        private void RestoreOutput()
        {
            if (!Options.Options.Any(o => o is Verbose))
            {
                System.Console.SetOut(_originalOut);
            }
        }

        private void ReportResult(bool result, MutationTestingReportSummary reportSummary = null)
        {
            var formatOption = Options.Options.OfType<Format>().FirstOrDefault();
            if (_outputOption != null && formatOption != null && formatOption.OutputFormat == "HTML")
            {
                FormatOutput(_outputPath);
            }
            OutputWriter.WriteLine();
            var statusColor = result
                                  ? ConsoleColor.Green
                                  : ConsoleColor.Red;
            using (new OutputWriterHighlight(statusColor))
            {
                if (reportSummary != null)
                    OutputWriter.WriteLine(reportSummary.ToString());
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
    }
}
