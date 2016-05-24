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
using NinjaTurtlesMutation;
using NinjaTurtlesMutation.Turtles;
using NUnit.Framework;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class BitwiseOperatorTurtleTests
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

        private static AssemblyDefinition CreateTestAssembly(OpCode arithmeticOperator)
        {
            var name = new AssemblyNameDefinition("TestBitwiseOperatorTurtleOr", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestBitwiseOperatorTurtleOr", "TestClass",
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
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, leftParam));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, rightParam));
            method.Body.Instructions.Add(processor.Create(arithmeticOperator));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldloc, resultVariable));
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
        public void DoMutate_Returns_Correct_Replacements_For_Or()
        {
            var assembly = CreateTestAssembly(OpCodes.Or);

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new BitwiseOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray());

            int and = 0;
            int xor = 0;
            int total = 0;
            foreach (var metaData in mutations)
            {
                total++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.And)) and++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Xor)) xor++;
            }

            Assert.AreEqual(2, total);
            Assert.AreEqual(1, and);
            Assert.AreEqual(1, xor);
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_And()
        {
            var assembly = CreateTestAssembly(OpCodes.And);

            var subtractMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new BitwiseOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(subtractMethod, module, subtractMethod.Body.Instructions.Select(i => i.Offset).ToArray());

            int or = 0;
            int xor = 0;
            int total = 0;
            foreach (var metaData in mutations)
            {
                total++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Or)) or++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Xor)) xor++;
            }

            Assert.AreEqual(2, total);
            Assert.AreEqual(1, or);
            Assert.AreEqual(1, xor);
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Xor_And_Describes_Appropriately()
        {
            var assembly = CreateTestAssembly(OpCodes.Xor);

            var divideMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new BitwiseOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(divideMethod, module, divideMethod.Body.Instructions.Select(i => i.Offset).ToArray());

            int or = 0;
            int and = 0;
            int total = 0;
            foreach (var metaData in mutations)
            {
                total++;
                or += MatchReplacement(metaData, OpCodes.Xor, OpCodes.Or);
                and += MatchReplacement(metaData, OpCodes.Xor, OpCodes.And);
            }

            Assert.AreEqual(2, total);
            Assert.AreEqual(1, or);
            Assert.AreEqual(1, and);
        }

        private int MatchReplacement(MutantMetaData metaData, OpCode from, OpCode to)
        {
            int result = 0;
            if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == to))
            {
                result = 1;
                StringAssert.Contains(string.Format("{0} => {1}", from.Code, to.Code),
                                      metaData.Description);
            }
            return result;
        }
    }
}

