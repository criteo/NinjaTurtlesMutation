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
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Mono.Cecil;

namespace NinjaTurtlesMutation.Reporting
{
    /// <summary>
    /// Represents the top level of a mutation testing report for a project.
    /// </summary>
    [Serializable]
    public class MutationTestingReport
    {
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        private int _mutantsCount;
        private int _mutantsKilledCount;

        private readonly string _methodFullname;

        /// <summary>
        /// Initializes a new instance of <see cref="MutationTestingReport" />.
        /// </summary>
        private MutationTestingReport()
        {
            SourceFiles = new List<SourceFile>();
            _readerWriterLock = new ReaderWriterLockSlim();
            _mutantsCount = 0;
            _mutantsKilledCount = 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MutationTestingReport" />.
        /// </summary>
        /// <param name="testedMethod"></param>
        public MutationTestingReport(MethodDefinition testedMethod) : this()
        {
            _methodFullname = testedMethod.FullName;
        }

        /// <summary>
        /// True if the targeted method has matching tests
        /// </summary>
        [XmlIgnore]
        public bool TestsFound = false;

        /// <summary>
        /// Gets or sets a list of the <see cref="SourceFile" />s covered by
        /// this report.
        /// </summary>
        public List<SourceFile> SourceFiles { get; set; }

        /// <summary>
        /// Total number of mutants reported
        /// </summary>
        public int MutantsCount { get { return _mutantsCount; } }

        /// <summary>
        /// Total number of dead mutants reported
        /// </summary>
        public int MutantsKilledCount { get { return _mutantsKilledCount; } }

        /// <summary>
        /// The name of the tested method
        /// </summary>
        public string MethodFullname { get { return _methodFullname; } }

        internal void MergeFromFile(string fileName)
        {
            if (!File.Exists(fileName)) return;

            MutationTestingReport otherReport;
            using (var streamReader = File.OpenText(fileName))
            {
                otherReport = (MutationTestingReport)new XmlSerializer(typeof(MutationTestingReport)).Deserialize(streamReader);
            }

            foreach (var sourceFile in otherReport.SourceFiles)
            {
                if (SourceFiles.All(s => s.Url != sourceFile.Url))
                {
                    SourceFiles.Add(sourceFile);
                }
                else
                {
                    SourceFiles.First(s => s.Url == sourceFile.Url).MergeFrom(sourceFile);
                }
            }
        }

        internal void RegisterMethod(MethodDefinition method)
        {
            if (method.Body.Instructions.All(i => i.SequencePoint == null)) return;
            string sourceFileUrl =
                method.Body.Instructions.First(i => i.SequencePoint != null).SequencePoint.Document.Url;
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (SourceFiles.All(s => s.Url != sourceFileUrl))
                {
                    _readerWriterLock.EnterWriteLock();
                    var newSourceFile = new SourceFile();
                    newSourceFile.SetUrl(sourceFileUrl);
                    SourceFiles.Add(newSourceFile);
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
            var sourceFile = SourceFiles.First(s => s.Url == sourceFileUrl);
            var sequencePoints = method.Body.Instructions
                .Where(i => i.SequencePoint != null && i.ShouldReportSequencePoint())
                .Select(i => i.SequencePoint).Distinct();
            foreach (var point in sequencePoints)
            {
                sourceFile.AddSequencePoint(point);
            }
        }

        internal void AddResult(Mono.Cecil.Cil.SequencePoint sequencePoint, MutantMetaData mutantMetaData, bool mutantKilled)
        {
            if (sequencePoint == null || sequencePoint.Document == null) return;
            _mutantsCount++;
            _mutantsKilledCount += (mutantKilled ? 1 : 0);
            string sourceFileUrl = sequencePoint.Document.Url;
            _readerWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (SourceFiles.All(s => s.Url != sourceFileUrl))
                {
                    _readerWriterLock.EnterWriteLock();
                    var newSourceFile = new SourceFile();
                    newSourceFile.SetUrl(sourceFileUrl);
                    SourceFiles.Add(newSourceFile);
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }
            var sourceFile = SourceFiles.First(s => s.Url == sourceFileUrl);
            sourceFile.AddResult(sequencePoint, mutantMetaData, mutantKilled);
        }
    }
}
