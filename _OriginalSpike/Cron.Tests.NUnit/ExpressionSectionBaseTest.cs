using System;
using Cron;
using NUnit.Framework;

using NinjaTurtles;
using NinjaTurtles.Attributes;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class ExpressionSectionBaseTest
    {
        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void ValidateValue_DayOfMonth()
        {
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfMonth, -1));
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfMonth, 0));
            for (int i = 1; i <= 31; ++i)
            {
                Assert.True(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfMonth, i));
            }
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfMonth, 32));
        }

        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void ValidateValue_DayOfWeek()
        {
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfWeek, -1));
            for (int i = 0; i <= 7; ++i)
            {
                Assert.True(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfWeek, i));
            }
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.DayOfWeek, 8));
        }

        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void ValidateValue_Hour()
        {
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Hour, -1));
            for (int i = 0; i <= 23; ++i)
            {
                Assert.True(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Hour, i));
            }
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Hour, 24));
        }

        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void ValidateValue_Minute()
        {
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Minute, -1));
            for (int i = 0; i <= 59; ++i)
            {
                Assert.True(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Minute, i));
            }
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Minute, 60));
        }

        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void ValidateValue_Month()
        {
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Month, -1));
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Month, 0));
            for (int i = 1; i <= 12; ++i)
            {
                Assert.True(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Month, i));
            }
            Assert.False(ExpressionSectionBase.ValidateValue(ExpressionSectionType.Month, 13));
        }

        [Test]
        [MethodTested(typeof(ExpressionSectionBase), "ValidateValue")]
        public void TryParse_NoMatch_Empty()
        {
            ExpressionSectionBase value;
            bool result = ExpressionSectionBase.TryParse(null, ExpressionSectionType.Hour, out value);
            Assert.False(result);
        }

        [Test, Category("Mutation")]
        public void ValidateValue_MutationTests()
        {
            MutationTestBuilder<ExpressionSectionBase>.For("ValidateValue")
                .Run();
        }
    }
}