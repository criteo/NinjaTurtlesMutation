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

namespace NinjaTurtles.Tests.Turtles.VariableWriteTurtleTestSuite
{
    public class VariableWriteClassUnderTest
    {
        private int _pointlessA;
        private int _pointlessB;

        public int AddWithPointlessNonsense(int a, int b)
        {
            int pointlessA = a;
            int pointlessB = b;
            return a + b;
        }

        public int AddWithPointlessNonsenseViaFields(int a, int b)
        {
            _pointlessA = a;
            _pointlessB = b;
            return a + b;
        }

        public int AddWithPointlessNonsenseViaMixture(int a, int b)
        {
            _pointlessA = a;
            int pointlessB = b;
            return _pointlessA + pointlessB;
        }

        public int AddWithoutPointlessNonsense(int a, int b)
        {
            return a + b;
        }
    }
}
