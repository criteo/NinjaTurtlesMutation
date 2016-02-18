using System;
using NUnit.Framework;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.Turtles.Method;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class DayOfWeekExpressionSectionTest
    {
        [Test]
        public void Value_Works()
        {
            var value = new DayOfWeekExpressionSection();
            value.Value = 1;
            Assert.AreEqual(DayOfWeek.Monday, value.DayOfWeek);
            value.Value = 5;
            Assert.AreEqual(DayOfWeek.Friday, value.DayOfWeek);
            value.DayOfWeek = DayOfWeek.Tuesday;
            Assert.AreEqual(2, value.Value);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_1()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("1", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Monday, value.DayOfWeek);
            Assert.AreEqual(ExpressionSectionType.DayOfWeek, value.Type);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_7()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("7", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Sunday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_0()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("0", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Sunday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Friday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("FRI", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Friday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Saturday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("SAT", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Saturday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Saturday_IsLastDayOfWeek()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("L", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Saturday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Monday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("MON", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Monday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Monday_CaseInsensitive()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("mOn", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Monday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Sunday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("SUN", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Sunday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Thursday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("THU", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Thursday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Tuesday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("TUE", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Tuesday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_Match_Wednesday()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("WED", out value);
            Assert.True(result);
            Assert.AreEqual(DayOfWeek.Wednesday, value.DayOfWeek);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_NoMatch1()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("abc", out value);
            Assert.False(result);
            Assert.IsNull(value);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_NoMatch2()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse("8", out value);
            Assert.False(result);
        }

        [Test]
        [MethodTested(typeof(DayOfWeekExpressionSection), "TryParse")]
        public void TryParse_NoMatch_Empty()
        {
            DayOfWeekExpressionSection value;
            bool result = DayOfWeekExpressionSection.TryParse(null, out value);
            Assert.False(result);
            Assert.IsNull(value);
        }

        [Test, Category("Mutation")]
        public void TryParse_MutationTests()
        {
            MutationTestBuilder<DayOfWeekExpressionSection>.For("TryParse")
                .Run();
        }
    }
}