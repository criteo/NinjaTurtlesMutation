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

using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class InstructionExtensionsTests
    {
        [Test]
        public void IsMeaninglessUnconditionalBranch_Works()
        {
            var i1 = Instruction.Create(OpCodes.Nop);
            var i5 = Instruction.Create(OpCodes.Nop);
            var i3 = Instruction.Create(OpCodes.Br, i5);
            var i2 = Instruction.Create(OpCodes.Br, i3);
            var i4 = Instruction.Create(OpCodes.Nop);
            var i8 = Instruction.Create(OpCodes.Nop);
            var i6 = Instruction.Create(OpCodes.Br, i8);
            var i7 = Instruction.Create(OpCodes.Nop);
            i1.Offset = 1;
            i2.Offset = 2;
            i3.Offset = 3;
            i4.Offset = 4;
            i5.Offset = 5;
            i6.Offset = 6;
            i7.Offset = 7;
            i8.Offset = 8;
            i1.Next = i2;
            i2.Next = i3;
            i3.Next = i4;
            i4.Next = i5;
            i5.Next = i6;
            i6.Next = i7;
            i7.Next = i8;
            Assert.IsFalse(i1.IsMeaninglessUnconditionalBranch());
            Assert.IsTrue(i2.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i3.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i4.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i5.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i6.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i7.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i8.IsMeaninglessUnconditionalBranch());
        }

        [Test]
        public void ShouldReportSequencePoint_Kicks_Out_Nop_Only()
        {
            var module = new Module(typeof(ArithmeticOperatorTurtle).Assembly.Location);
            module.LoadDebugInformation();

            var turtle = module.Definition.Types
                .Single(t => t.Name == typeof(ArithmeticOperatorTurtle).Name);

            var methodDefinition = turtle.Methods.Single(m => m.Name == Methods.CONSTRUCTOR);

            var instruction = methodDefinition
                .Body.Instructions.Where(i => i.SequencePoint != null)
                .Skip(1).First();

            Assert.IsFalse(instruction.ShouldReportSequencePoint());
        }

        [Test]
        public void ShouldReportSequencePoint_Kicks_Out_Nop_And_Ret_Only()
        {
            var module = new Module(typeof(ArithmeticOperatorTurtle).Assembly.Location);
            module.LoadDebugInformation();

            var turtle = module.Definition.Types
                .Single(t => t.Name == typeof(ArithmeticOperatorTurtle).Name);

            var methodDefinition = turtle.Methods.Single(m => m.Name == Methods.CONSTRUCTOR);

            var instruction = methodDefinition
                .Body.Instructions.Where(i => i.SequencePoint != null)
                .Skip(3).First();

            Assert.IsFalse(instruction.ShouldReportSequencePoint());
        }

        [Test]
        public void ShouldReportSequencePoint_Kicks_Out_Base_Constructor_Call()
        {
            var module = new Module(typeof(ArithmeticOperatorTurtle).Assembly.Location);
            module.LoadDebugInformation();

            var turtle = module.Definition.Types
                .Single(t => t.Name == typeof(ArithmeticOperatorTurtle).Name);

            var methodDefinition = turtle.Methods.Single(m => m.Name == Methods.CONSTRUCTOR);

            var instruction = methodDefinition
                .Body.Instructions
                .First(i => i.SequencePoint != null);

            Assert.IsFalse(instruction.ShouldReportSequencePoint());
        }

        [Test]
        public void ShouldReportSequencePoint_Kicks_Out_Base_Constructor_Call_When_Simplified()
        {
            var module = new Module(typeof(ArithmeticOperatorTurtle).Assembly.Location);
            module.LoadDebugInformation();

            var turtle = module.Definition.Types
                .Single(t => t.Name == typeof(ArithmeticOperatorTurtle).Name);

            var methodDefinition = turtle.Methods.Single(m => m.Name == Methods.CONSTRUCTOR);
            methodDefinition.Body.SimplifyMacros();

            var instruction = methodDefinition
                .Body.Instructions
                .First(i => i.SequencePoint != null);

            Assert.IsFalse(instruction.ShouldReportSequencePoint());
        }

        [Test]
        public void ShouldReportSequencePoint_Lets_Through_Meaningful_Sequence_Point()
        {
            var module = new Module(typeof(ArithmeticOperatorTurtle).Assembly.Location);
            module.LoadDebugInformation();

            var turtle = module.Definition.Types
                .Single(t => t.Name == typeof(ArithmeticOperatorTurtle).Name);

            var methodDefinition = turtle.Methods.Single(m => m.Name == Methods.CONSTRUCTOR);

            var instruction = methodDefinition
                .Body.Instructions.Where(i => i.SequencePoint != null)
                .Skip(2).First();

            Assert.IsTrue(instruction.ShouldReportSequencePoint());
        }

        [Test, Category("Mutation"), MutationTest]
        public void IsMeaninglessUnconditionalBranch_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.InstructionExtensions", "IsMeaninglessUnconditionalBranch")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void ShouldReportSequencePoint_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.InstructionExtensions", "ShouldReportSequencePoint")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
