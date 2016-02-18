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

using System;
using System.Xml.Serialization;

namespace NinjaTurtles.Reporting
{
    /// <summary>
    /// Represents a line of source code.
    /// </summary>
    [Serializable]
    public class Line
    {
        /// <summary>
        /// Gets or sets the text of the line of code.
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the index within its source code file of the line
        /// of code.
        /// </summary>
        [XmlAttribute]
        public int Number { get; set; }
    }
}
