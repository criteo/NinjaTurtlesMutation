using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class ListExpressionSectionTest
    {
        [Test]
        public void TryParse_Match2()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("1,2", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(2, value.Sections.Count);
        }

        [Test]
        public void TryParse_Match3()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("1,2,3", ExpressionSectionType.Hour, out value);
            Assert.True(result);
            Assert.AreEqual(3, value.Sections.Count);
        }

        [Test]
        public void TryParse_NoMatch1()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("no", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch2()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch3()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("?,1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch4()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("1,", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch5()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse(",1", ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch6()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse("1,?", ExpressionSectionType.DayOfMonth, out value);
            Assert.False(result);
        }

        [Test]
        public void TryParse_NoMatch_Empty()
        {
            ListExpressionSection value;
            bool result = ListExpressionSection.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }
    }
}