﻿#region Copyright & licence

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

using NUnit.Framework;

namespace NinjaTurtlesMutation.Tests.Turtles.SequencePointDeletionTurtleTestSuite.Tests
{
    [TestFixture]
    public class SequencePointDeletionClassUnderTestTests
    {
        [Test]
        public void StupidParse_Works()
        {
            Assert.AreEqual(7, new SequencePointDeletionClassUnderTest().SimpleMethod(1, 0, 3, 2));
        }

        [Test]
        public void WorkingStupidParse_Works()
        {
            Assert.AreEqual(7, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(1, 0, 3, 2));
            Assert.AreEqual(11, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(1, 2, 3, 2));
            Assert.AreEqual(24, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(2, -1, 3, -3));
        }
    }
}
