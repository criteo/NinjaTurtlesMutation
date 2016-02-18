using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class RepeatingExpressionSectionTest
    {
        [Test]
        public void TryParse_Match1()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/2", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            var start = value.StartValue as SimpleExpressionSection;
            Assert.NotNull(start);
            Assert.AreEqual(1, start.Value);
            var frequency = value.Frequency as SimpleExpressionSection;
            Assert.NotNull(frequency);
            Assert.AreEqual(2, frequency.Value);
        }

        [Test]
        public void TryParse_Match2()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/8", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            var start = value.StartValue as SimpleExpressionSection;
            Assert.NotNull(start);
            Assert.AreEqual(1, start.Value);
            var frequency = value.Frequency as SimpleExpressionSection;
            Assert.NotNull(frequency);
            Assert.AreEqual(8, frequency.Value);
        }

        [Test]
        public void TryParse_NoMatch1()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch2()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch3()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("/1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch4()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/123", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch5()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("123/1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch6()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("?/1", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch7()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/9", ExpressionSectionType.DayOfWeek, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_Match3()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("*/9", ExpressionSectionType.DayOfMonth, out value);
            Assert.True(result);
            var start = value.StartValue;
            Assert.NotNull(start);
            Assert.AreEqual(1, start.Value);
            var frequency = value.Frequency;
            Assert.NotNull(frequency);
            Assert.AreEqual(9, frequency.Value);
        }

        [Test]
        public void TryParse_Match4()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("*/9", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            var start = value.StartValue;
            Assert.NotNull(start);
            Assert.AreEqual(0, start.Value);
            var frequency = value.Frequency;
            Assert.NotNull(frequency);
            Assert.AreEqual(9, frequency.Value);
        }

        [Test]
        public void TryParse_NoMatch8()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse("1/?", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            RepeatingExpressionSection value;
            bool result = RepeatingExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }
    }
}