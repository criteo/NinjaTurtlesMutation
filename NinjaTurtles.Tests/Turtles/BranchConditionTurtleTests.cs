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

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class BranchConditionTurtleTests
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

        private static AssemblyDefinition CreateTestAssembly()
        {
            var name = new AssemblyNameDefinition("TestBranchConditionTurtle", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestBranchConditionTurtle", "TestClass",
                               TypeAttributes.Class | TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var boolType = assembly.MainModule.Import(typeof(bool));
            var method = new MethodDefinition("TestMethod", MethodAttributes.Public, intType);
            var leftParam = new ParameterDefinition("left", ParameterAttributes.In, intType);
            var rightParam = new ParameterDefinition("right", ParameterAttributes.In, intType);
            method.Parameters.Add(leftParam);
            method.Parameters.Add(rightParam);
            var resultVariable = new VariableDefinition(boolType);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            Instruction loadReturnValueInstruction = processor.Create(OpCodes.Ldloc, resultVariable);
            Instruction storeTrueInReturnValueInstruction = processor.Create(OpCodes.Ldc_I4, -1);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, leftParam));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, rightParam));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ceq));
            method.Body.Instructions.Add(processor.Create(OpCodes.Brtrue_S, storeTrueInReturnValueInstruction));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 0));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Br_S, loadReturnValueInstruction));
            method.Body.Instructions.Add(storeTrueInReturnValueInstruction);
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(loadReturnValueInstruction);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));

            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Addition()
        {
            var assembly = CreateTestAssembly();

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new BranchConditionTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray());

            int brTrue = 0;
            int brFalse = 0;
            int br = 0;
            int total = 0;
            foreach (var metaData in mutations)
            {
                total++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Brtrue)) brTrue++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Brfalse)) brFalse++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Br)) br++;
            }

            Assert.AreEqual(3, total);
            Assert.AreEqual(0, brTrue);
            Assert.AreEqual(1, brFalse);
            Assert.AreEqual(3, br);
        }
    }
}
