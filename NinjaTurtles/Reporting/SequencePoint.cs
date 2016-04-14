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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

using Cil = Mono.Cecil.Cil;

namespace NinjaTurtles.Reporting
{
    /// <summary>
    /// Represents an IL sequence point.
    /// </summary>
    [Serializable]
    public class SequencePoint
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of <see cref="SequencePoint" />.
        /// </summary>
        public SequencePoint()
        {
            AppliedMutants = new List<AppliedMutant>();
            _readerWriterLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SequencePoint" />, copying
        /// data from the provided <see cref="Cil.SequencePoint" />.
        /// </summary>
        /// <param name="sequencePoint">
        /// An instance of <see cref="Cil.SequencePoint" /> from which to copy
        /// property values.
        /// </param>
        public SequencePoint(Cil.SequencePoint sequencePoint)
            : this()
        {
            StartLine = sequencePoint.StartLine;
            StartColumn = sequencePoint.StartColumn;
            EndLine = sequencePoint.EndLine;
            EndColumn = sequencePoint.EndColumn;
        }

        /// <summary>
        /// Gets or sets the number of the first line of code covered by this
        /// sequence point.
        /// </summary>
        [XmlAttribute]
        public int StartLine { get; set; }

        /// <summary>
        /// Gets or sets the start column within the first line.
        /// </summary>
        [XmlAttribute]
        public int StartColumn { get; set; }

        /// <summary>
        /// Gets or sets the number of the last line of code covered by this
        /// sequence point.
        /// </summary>
        [XmlAttribute]
        public int EndLine { get; set; }

        /// <summary>
        /// Gets or sets the end column within the final line.
        /// </summary>
        [XmlAttribute]
        public int EndColumn { get; set; }

        internal string GetIdentifier()
        {
            return GetIdentifier(StartLine, StartColumn, EndLine, EndColumn);
        }

        static internal string GetIdentifier(Cil.SequencePoint sequencePoint)
        {
            return GetIdentifier(sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine,
                                 sequencePoint.EndColumn);
        }

        static internal string GetIdentifier(int startLine, int startColumn, int endLine, int endColumn)
        {
            return string.Format("{0}_{1}_{2}_{3}", startLine, startColumn, endLine, endColumn);
        }

        /// <summary>
        /// Gets or sets a list of <see cref="AppliedMutant" />s, representing
        /// the mutants applied.
        /// </summary>
        public List<AppliedMutant> AppliedMutants { get; set; }

        internal void AddResult(MutantMetaData mutantMetaData, bool mutantKilled)
        {
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (AppliedMutants.All(s => s.Description != mutantMetaData.Description))
                {
                    _readerWriterLock.EnterWriteLock();
                    AppliedMutants.Add(new AppliedMutant
                                             {
                                                 GenericDescription = mutantMetaData.GenericDescription,
                                                 Description = mutantMetaData.Description,
                                                 Killed = mutantKilled
                                             });
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
        }

        internal void MergeFrom(SequencePoint sequencePoint)
        {
            foreach (var appliedMutant in sequencePoint.AppliedMutants)
            {
                if (AppliedMutants.All(a => a.Description != appliedMutant.Description))
                {
                    AppliedMutants.Add(appliedMutant);
                }
            }
        }
    }
}
