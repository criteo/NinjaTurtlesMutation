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
using System.Threading;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NinjaTurtles.Tests.Turtles.Tests;
using NUnit.Framework;

using NinjaTurtles.Tests.Turtles.ArithmeticOperatorTurtleTestSuite;
using NinjaTurtlesMutation;
using NinjaTurtlesMutation.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class MethodTurtleBaseTests
    {
        private string _testFolder;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testFolder);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Directory.Delete(_testFolder, true);
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }

        private static AssemblyDefinition CreateTestAssembly()
        {
            var name = new AssemblyNameDefinition("TestArithmeticOperatorTurtleAdd", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestArithmeticOperatorTurtleAdd", "TestClass",
                               TypeAttributes.Class | TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var method = new MethodDefinition("TestMethod", MethodAttributes.Public, intType);
            var leftParam = new ParameterDefinition("left", ParameterAttributes.In, intType);
            var rightParam = new ParameterDefinition("right", ParameterAttributes.In, intType);
            method.Parameters.Add(leftParam);
            method.Parameters.Add(rightParam);
            var resultVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_1));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_2));
            method.Body.Instructions.Add(processor.Create(OpCodes.Add));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));

            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        [Test]
        public void Mutate_Simplifies_Macros_In_IL()
        {
            var assembly = CreateTestAssembly();
            var turtle = new DummyTurtle();
            var method = assembly.MainModule.Types
                .Single(t => t.Name == "TestClass")
                .Methods.Single(m => m.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutation = turtle.Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).First();
            Assert.AreEqual(OpCodes.Ldarg, mutation.MethodDefinition.Body.Instructions[0].OpCode);
        }

        [Test]
        public void DoYield_Saves_Assembly()
        {
            var assembly = CreateTestAssembly();
            var turtle = new DummyTurtle();
            var method = assembly.MainModule.Types
                .Single(t => t.Name == "TestClass")
                .Methods.Single(m => m.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var originalFile = File.ReadAllBytes(tempAssemblyFileName);
            Thread.Sleep(1);
            var module = new Module(tempAssemblyFileName);
            method = module.Definition
                .Types.First(t => t.Name == "TestClass")
                .Methods.First(m => m.Name == "TestMethod");

            var mutation = turtle.Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).First();
            string mutatedAssemblyFileName = Path.Combine(mutation.TestDirectory.FullName,
                                                          Path.GetFileName(tempAssemblyFileName));
            var newFile = File.ReadAllBytes(mutatedAssemblyFileName);
            Assert.IsFalse(Enumerable.SequenceEqual(originalFile, newFile));
        }

        [Test]
        public void Mutate_Creates_And_Destroys_Directories()
        {
            var assembly = CreateTestAssembly();

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new DummyTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray());

            var directories = new List<string>();

            foreach (var mutation in mutations)
            {
                string directoryName = mutation.TestDirectoryName;
                directories.Add(directoryName);
                Assert.IsTrue(Directory.Exists(directoryName));
                mutator.MutantComplete(mutation);
            }

            foreach (var directory in directories)
            {
                Assert.IsFalse(Directory.Exists(directory));
            }
        }

        private class DummyTurtle : MethodTurtleBase
        {
            public override string Description
            {
                get { return "Dummy turtle"; }
            }

            protected override IEnumerable<MutantMetaData> CreateMutant(MethodDefinition method, Module module, int[] originalOffsets)
            {
                var processor = method.Body.GetILProcessor();
                method.Body.Instructions.Add(processor.Create(OpCodes.Nop));
                yield return DoYield(method, module, "Dummy", 0);
            }
        }
    }
}
