using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class MonthExpressionSectionTest
    {
        [Test]
        public void Value_Works()
        {
            MonthExpressionSection value = new MonthExpressionSection();
            value.Value = 1;
            Assert.AreEqual(Month.January, value.Month);
            value.Value = 8;
            Assert.AreEqual(Month.August, value.Month);
            value.Month = Month.June;
            Assert.AreEqual(6, value.Value);
        }

        [Test]
        public void TryParse_Match_Apr()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("APR", out value);
            Assert.True(result);
            Assert.AreEqual(Month.April, value.Month);
        }

        [Test]
        public void TryParse_Match_Aug()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("AUG", out value);
            Assert.True(result);
            Assert.AreEqual(Month.August, value.Month);
        }

        [Test]
        public void TryParse_Match_Dev()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("DEC", out value);
            Assert.True(result);
            Assert.AreEqual(Month.December, value.Month);
        }

        [Test]
        public void TryParse_Match_Feb()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("FEB", out value);
            Assert.True(result);
            Assert.AreEqual(Month.February, value.Month);
        }

        [Test]
        public void TryParse_Match_Jan()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("JAN", out value);
            Assert.True(result);
            Assert.AreEqual(Month.January, value.Month);
        }

        [Test]
        public void TryParse_Match_Jan_CaseInsensitive()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("jAn", out value);
            Assert.True(result);
            Assert.AreEqual(Month.January, value.Month);
        }

        [Test]
        public void TryParse_Match_Jul()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("JUL", out value);
            Assert.True(result);
            Assert.AreEqual(Month.July, value.Month);
        }

        [Test]
        public void TryParse_Match_Jun()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("JUN", out value);
            Assert.True(result);
            Assert.AreEqual(Month.June, value.Month);
        }

        [Test]
        public void TryParse_Match_Mar()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("MAR", out value);
            Assert.True(result);
            Assert.AreEqual(Month.March, value.Month);
        }

        [Test]
        public void TryParse_Match_May()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("MAY", out value);
            Assert.True(result);
            Assert.AreEqual(Month.May, value.Month);
        }

        [Test]
        public void TryParse_Match_Nov()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("NOV", out value);
            Assert.True(result);
            Assert.AreEqual(Month.November, value.Month);
        }

        [Test]
        public void TryParse_Match_Oct()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("OCT", out value);
            Assert.True(result);
            Assert.AreEqual(Month.October, value.Month);
        }

        [Test]
        public void TryParse_Match_Sep()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("SEP", out value);
            Assert.True(result);
            Assert.AreEqual(Month.September, value.Month);
        }

        [Test]
        public void TryParse_Match_1()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("1", out value);
            Assert.True(result);
            Assert.AreEqual(Month.January, value.Month);
        }

        [Test]
        public void TryParse_NoMatch1()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("abc", out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch2()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse("13", out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            MonthExpressionSection value;
            bool result = MonthExpressionSection.TryParse(null, out value);
            Assert.False(result);
        }
    }
}