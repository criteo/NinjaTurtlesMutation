using System;
using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class IgnoreExpressionSectionTest
    {
        [Test]
        public void TryParse_Match1()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("?", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Hour, value.Type);
        }

        [Test]
        public void TryParse_Match2()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("?", ExpressionSectionType.DayOfMonth, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.DayOfMonth, value.Type);
        }

        [Test]
        public void TryParse_Match3()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("?", ExpressionSectionType.DayOfWeek, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.DayOfWeek, value.Type);
        }

        [Test]
        public void TryParse_Match4()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("?", ExpressionSectionType.Minute, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Minute, value.Type);
        }

        [Test]
        public void TryParse_Match5()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("?", ExpressionSectionType.Month, out value);
            Assert.True(result);
            Assert.AreEqual(ExpressionSectionType.Month, value.Type);
        }

        [Test]
        public void TryParse_NoMatch()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            IgnoreExpressionSection value;
            bool result = IgnoreExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_Fails()
        {
            IgnoreExpressionSection value;
            Assert.Throws<InvalidOperationException>(() => IgnoreExpressionSection.TryParse("?", (ExpressionSectionType)99, out value));
        }
    }
}