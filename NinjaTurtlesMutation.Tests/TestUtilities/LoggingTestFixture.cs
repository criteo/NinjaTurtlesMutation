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

using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.TestUtilities
{
    [TestFixture]
    public abstract class LoggingTestFixture
    {
        private MemoryTarget _logTarget;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var config = new LoggingConfiguration();

            _logTarget = new MemoryTarget();
            _logTarget.Layout = "${level:uppercase=true}|${message}|${exception:format=tostring}";
            config.AddTarget("memory", _logTarget);

            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = "${longdate}|${logger}|${level:uppercase=true}|${message}|${exception:format=tostring}";
            config.AddTarget("console", consoleTarget);

            var rule = new LoggingRule("*", LogLevel.Trace, _logTarget);
            rule.Targets.Add(consoleTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogManager.Configuration = null;
        }

        [SetUp]
        public void SetUp()
        {
            Logs.Clear();
            Assert.AreEqual(0, Logs.Count);
        }

        protected IList<string> Logs
        {
            get { return _logTarget.Logs; }
        }

        public void AssertLogContains(string message, bool startOfMessageOnly = false)
        {
            if (startOfMessageOnly)
            {
                Assert.IsTrue(Logs.Any(m => m.StartsWith(message)));
            }
            else
            {
                Assert.IsTrue(Logs.Any(m => m == message));
            }
        }

        public void AssertLogDoesNotContain(string message, bool startOfMessageOnly = false)
        {
            if (startOfMessageOnly)
            {
                Assert.IsFalse(Logs.Any(m => m.StartsWith(message)));
            }
            else
            {
                Assert.IsFalse(Logs.Any(m => m == message));
            }
        }
    }
}
