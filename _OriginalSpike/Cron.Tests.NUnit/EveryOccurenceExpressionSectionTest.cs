using System;
using Cron;
using NinjaTurtles;
using NinjaTurtles.Attributes;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class EveryOccurenceExpressionSectionTest
    {
        [Test]
		[MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_Match1()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("*", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Hour, value.Type);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_Match2()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("*", ExpressionSectionType.DayOfMonth, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.DayOfMonth, value.Type);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_Match3()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("*", ExpressionSectionType.DayOfWeek, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.DayOfWeek, value.Type);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_Match4()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("*", ExpressionSectionType.Minute, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Minute, value.Type);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_Match5()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("*", ExpressionSectionType.Month, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Month, value.Type);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_NoMatch()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        [MethodTested(typeof(EveryOccurenceExpressionSection), "TryParse")]
        public void TryParse_NoMatch_Empty()
        {
            EveryOccurenceExpressionSection value;
            bool result = EveryOccurenceExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

		[Test, Category("Mutation")]
		public void TryParse_MutationTests()
		{
			MutationTestBuilder<EveryOccurenceExpressionSection>.For("TryParse")
				.Run ();
		}
    }
}