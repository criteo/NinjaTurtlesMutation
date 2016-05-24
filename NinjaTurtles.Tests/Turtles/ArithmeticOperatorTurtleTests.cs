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
using NinjaTurtlesMutation.Turtles;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.Turtles
{
	[TestFixture]
	public class ArithmeticOperatorTurtleTests
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
        public void DoMutate_Returns_Correct_Replacements_For_Addition()
		{
			var assembly = CreateTestAssembly(OpCodes.Add);
			
			var addMethod = assembly.MainModule
				.Types.Single(t => t.Name == "TestClass")
				.Methods.Single(t => t.Name == "TestMethod");

		    string tempAssemblyFileName = GetTempAssemblyFileName();
		    assembly.Write(tempAssemblyFileName);
		    var module = new Module(tempAssemblyFileName);
			
			var mutator = new ArithmeticOperatorTurtle();
			IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray());
			
			int sub = 0;
			int mul = 0;
			int div = 0;
			int rem = 0;
			int total = 0;
			foreach (var metaData in mutations)
			{
				total++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Sub)) sub++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Mul)) mul++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Div)) div++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Rem)) rem++;
			}
			
			Assert.AreEqual(4, total);
			Assert.AreEqual(1, sub);
			Assert.AreEqual(1, mul);
			Assert.AreEqual(1, div);
			Assert.AreEqual(1, rem);
		}

		[Test]
        public void DoMutate_Returns_Correct_Replacements_For_Subtraction()
		{
			var assembly = CreateTestAssembly(OpCodes.Sub);
			
			var subtractMethod = assembly.MainModule
				.Types.Single(t => t.Name == "TestClass")
				.Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ArithmeticOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(subtractMethod, module, subtractMethod.Body.Instructions.Select(i => i.Offset).ToArray());
			
			int add = 0;
			int mul = 0;
			int div = 0;
			int rem = 0;
			int total = 0;
			foreach (var metaData in mutations)
			{
				total++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Add)) add++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Mul)) mul++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Div)) div++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Rem)) rem++;
			}
			
			Assert.AreEqual(4, total);
			Assert.AreEqual(1, add);
			Assert.AreEqual(1, mul);
			Assert.AreEqual(1, div);
			Assert.AreEqual(1, rem);
		}

		[Test]
        public void DoMutate_Returns_Correct_Replacements_For_Division_And_Describes_Appropriately()
		{
			var assembly = CreateTestAssembly(OpCodes.Div);
			
			var divideMethod = assembly.MainModule
				.Types.Single(t => t.Name == "TestClass")
				.Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ArithmeticOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(divideMethod, module, divideMethod.Body.Instructions.Select(i => i.Offset).ToArray());
			
			int add = 0;
			int sub = 0;
			int mul = 0;
			int rem = 0;
			int total = 0;
			foreach (var metaData in mutations)
			{
				total++;
				add += MatchReplacement(metaData, OpCodes.Div, OpCodes.Add);
				sub += MatchReplacement(metaData, OpCodes.Div, OpCodes.Sub);
				mul += MatchReplacement(metaData, OpCodes.Div, OpCodes.Mul);
				rem += MatchReplacement(metaData, OpCodes.Div, OpCodes.Rem);
			}
			
			Assert.AreEqual(4, total);
			Assert.AreEqual(1, add);
			Assert.AreEqual(1, sub);
			Assert.AreEqual(1, mul);
			Assert.AreEqual(1, rem);
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

		[Test]
        public void DoMutate_Returns_Correct_Replacements_For_Multiplication()
		{
			var assembly = CreateTestAssembly(OpCodes.Mul);
			
			var multiplicationMethod = assembly.MainModule
				.Types.Single(t => t.Name == "TestClass")
				.Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ArithmeticOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(multiplicationMethod, module, multiplicationMethod.Body.Instructions.Select(i => i.Offset).ToArray());
			
			int add = 0;
			int sub = 0;
			int div = 0;
			int rem = 0;
			int total = 0;
			foreach (var metaData in mutations)
			{
				total++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Add)) add++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Sub)) sub++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Div)) div++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Rem)) rem++;
			}
			
			Assert.AreEqual(4, total);
			Assert.AreEqual(1, add);
			Assert.AreEqual(1, sub);
			Assert.AreEqual(1, div);
			Assert.AreEqual(1, rem);
		}

		[Test]
        public void DoMutate_Returns_Correct_Replacements_For_Remainder()
		{
			var assembly = CreateTestAssembly(OpCodes.Rem);
			
			var remainderMethod = assembly.MainModule
				.Types.Single(t => t.Name == "TestClass")
				.Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ArithmeticOperatorTurtle();
            IEnumerable<MutantMetaData> mutations = mutator
                .Mutate(remainderMethod, module, remainderMethod.Body.Instructions.Select(i => i.Offset).ToArray());
			
			int add = 0;
			int sub = 0;
			int mul = 0;
			int div = 0;
			int total = 0;
			foreach (var metaData in mutations)
			{
				total++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Add)) add++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Sub)) sub++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Mul)) mul++;
				if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Div)) div++;
			}
			
			Assert.AreEqual(4, total);
			Assert.AreEqual(1, add);
			Assert.AreEqual(1, sub);
			Assert.AreEqual(1, mul);
			Assert.AreEqual(1, div);
		}
	}
}

