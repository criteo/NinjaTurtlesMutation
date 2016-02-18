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
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;
using NinjaTurtles.Tests.Turtles.ArithmeticOperatorTurtleTestSuite;
using NinjaTurtles.Tests.Turtles.BitwiseOperatorTurtleTestSuite;
using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class MutationTestTests
	{
		[Test]
        [ExpectedException(typeof(MutationTestFailureException), ExpectedMessage = "No matching tests were found to run.")]
        public void UncoveredAdd_Mutation_Tests_Fail()
		{
			MutationTestBuilder<AdditionClassUnderTest>
				.For("UncoveredAdd")
				.With<ArithmeticOperatorTurtle>()
				.Run();
		}
		
		[Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"Method ""UnknownMethod"" is unrecognised.
Parameter name: methodName")]
        public void Unknown_Method_Mutation_Tests_Fail()
		{
			MutationTestBuilder<AdditionClassUnderTest>
				.For("UnknownMethod")
				.With<ArithmeticOperatorTurtle>()
				.Run();
        }
		
		[Test]
        public void Mutation_Tests_Produce_Correct_Output()
		{
			using (var capturer = new ConsoleCapturer())
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("WorkingAdd")
					.With<ArithmeticOperatorTurtle>()
					.Run();
				string output = capturer.Output;
				StringAssert.Contains("Mutant: ", output);
				StringAssert.Contains("Killed.", output);
				StringAssert.Contains("Add => Sub", output);
				StringAssert.Contains("Add => Mul", output);
				StringAssert.Contains("Add => Div", output);
				StringAssert.Contains("Add => Rem", output);
			}
		}

        [Test]
        public void Mutate_Failed_Tests_Report_Source_Code()
        {
            using (var capturer = new ConsoleCapturer())
            {
                try
                {
                    MutationTestBuilder<AdditionClassUnderTest>
                        .For("Add")
                        .With<ArithmeticOperatorTurtle>()
                        .Run();
                }
                catch (MutationTestFailureException)
                {
                    StringAssert.Contains("return left + right;", capturer.Output);
                    return;
                }
            }
            Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [Test]
        public void Mutate_Merges_Results_Into_Single_File()
        {
            string file = Path.GetTempFileName();
            if (File.Exists(file)) File.Delete(file);
            try
            {
                MutationTestBuilder<AdditionClassUnderTest>
                    .For("Add")
                    .With<ArithmeticOperatorTurtle>()
                    .WriteReportTo(file)
                    .Run();
            }
            catch (MutationTestFailureException)
            {
            }
            MutationTestBuilder<AdditionClassUnderTest>
                .For("WorkingAdd")
                .With<ArithmeticOperatorTurtle>()
                .MergeReportTo(file)
                .Run();
            Assert.IsTrue(File.Exists(file));
            var xDocument = XDocument.Load(file);
            Assert.AreEqual(1, xDocument.Root.Descendants().Where(e => e.Name == "SourceFile").Count());
            Assert.GreaterOrEqual(xDocument.Root.Descendants().Where(e => e.Name == "SequencePoint").Count(), 2);
            Assert.AreEqual(8, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant").Count());
            Assert.AreEqual(1, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant" && e.Attributes().Any(a => a.Name == "Killed" && a.Value == "false")).Count());
            Assert.AreEqual(7, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant" && e.Attributes().Any(a => a.Name == "Killed" && a.Value == "true")).Count());
            File.Delete(file);
        }

		[Test, Category("Mutation"), MutationTest]
		public void Run_Mutation_Tests()
		{
			MutationTestBuilder.For("NinjaTurtles.MutationTest", "Run")
                .MergeReportTo("SampleReport.xml")
                .Run();
		}

        [Test, Category("Mutation"), MutationTest]
        public void RunMutation_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "RunMutation")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("SlowMutation"), MutationTest]
        public void AddMethod_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "AddMethod")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("SlowMutation"), MutationTest]
        public void AddMethodsForInterfaces_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "AddMethodsForInterfaces")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void MethodsMatch_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "MethodsMatch")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void GetMatchingTestsFromTree_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "GetMatchingTestsFromTree")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void DoesMethodReferenceType_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "DoesMethodReferenceType")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void AddCallingMethods_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "AddCallingMethods")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("SlowMutation"), MutationTest]
        public void AddCallingMethodsForType_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "AddCallingMethodsForType")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void CheckTestProcessFails_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "CheckTestProcessFails")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void LongRunning_Test_Method()
        {
            new LongRunningWhenMutated().LongRunning();
        }

        [Test, Category("Mutation"), MutationTest]
        public void DoSomethingBanale_Mutation_Tests_Work()
        {
            MutationTestBuilder<OnlyTestedByNestedTestClass>.For("DoSomethingBanale")
                .Run();
        }

        [TestFixture]
        public class NestedUnitTests
	    {
	        [Test]
            public void DoSomethingBanale_Works()
	        {
	            var nested = new OnlyTestedByNestedTestClass();
                Assert.AreEqual(0, nested.DoSomethingBanale(0));
                Assert.AreEqual(-2, nested.DoSomethingBanale(-1));
                Assert.AreEqual(2, nested.DoSomethingBanale(1));
                Assert.AreEqual(14, nested.DoSomethingBanale(7));
            }
	    }

        public class OnlyTestedByNestedTestClass
	    {
            public int DoSomethingBanale(int a)
            {
                return 2 * a;
            }
	    
	    }

	    public class LongRunningWhenMutated
	    {
            public void LongRunning()
            {
                Thread.Sleep(31000);
            }
	    }
	}
}

