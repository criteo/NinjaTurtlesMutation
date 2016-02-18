using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class SimpleExpressionSectionTest
    {
        [Test]
        public void TryParse_Match1()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("1", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(1, value.Value);
        }

        [Test]
        public void TryParse_Match2()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("13", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(13, value.Value);
        }

        [Test]
        public void TryParse_NoMatch1()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch2()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("25", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch3()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("13", ExpressionSectionType.Month, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch4()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("8", ExpressionSectionType.DayOfWeek, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch5()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse("60", ExpressionSectionType.Minute, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            SimpleExpressionSection value;
            bool result = SimpleExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }
    }
}