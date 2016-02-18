using System;
using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class RangeExpressionSectionTest
    {
        [Test]
        public void TryParse_Match1()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-2", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            var start = value.StartValue as SimpleExpressionSection;
            Assert.NotNull(start);
            Assert.AreEqual(1, start.Value);
            var end = value.EndValue as SimpleExpressionSection;
            Assert.NotNull(end);
            Assert.AreEqual(2, end.Value);
        }

        [Test]
        public void TryParse_Match2()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-8", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            var start = value.StartValue as SimpleExpressionSection;
            Assert.NotNull(start);
            Assert.AreEqual(1, start.Value);
            var end = value.EndValue as SimpleExpressionSection;
            Assert.NotNull(end);
            Assert.AreEqual(8, end.Value);
        }

        [Test]
        public void TryParse_Match3()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("MON-FRI", ExpressionSectionType.DayOfWeek, out value);
            Assert.True(result);
            var start = value.StartValue as DayOfWeekExpressionSection;
            Assert.NotNull(start);
            Assert.AreEqual(DayOfWeek.Monday, start.DayOfWeek);
            var end = value.EndValue as DayOfWeekExpressionSection;
            Assert.NotNull(end);
            Assert.AreEqual(DayOfWeek.Friday, end.DayOfWeek);
        }

        [Test]
        public void TryParse_NoMatch1()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch2()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch3()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("-1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch4()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-123", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch5()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("123-1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch6()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-9", ExpressionSectionType.DayOfWeek, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch7()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("?-1", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch8()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-?", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch9()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("?-?", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch10()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-/", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch11()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse("1-2,3", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            RangeExpressionSection value;
            bool result = RangeExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }
    }
}