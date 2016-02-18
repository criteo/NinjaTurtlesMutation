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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// An abstract implementation of <see cref="IMethodTurtle" /> which
    /// handles assembly rewriting for derived classes, requiring them to just
    /// implement their own <see mref="DoMutate" /> method which internally
    /// uses the <see mref="PrepareTests" /> method.
    /// </summary>
    public abstract class MethodTurtle : IMethodTurtle
    {
        private CSharpLanguage _cSharpDecompiler;
        private DecompilationOptions _decompilationOptions;
        private SideBySideDiffBuilder _differ;
        private string _oldText;

        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> of detailed descriptions
        /// of mutations, having first carried out the mutation in question and
        /// saved the modified assembly under test to disk.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutationTestMetaData" /> structures.
        /// </returns>
        public IEnumerable<MutationTestMetaData> Mutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            if (!method.HasBody) yield break;
            method.Body.SimplifyMacros();

			SetupDecompilerAndOriginalSource(method);

            foreach (var data in LockAndMutate(method, assembly, fileName))
            {
                yield return data;

            }
        }
		
		private void SetupDecompilerAndOriginalSource(MethodDefinition method)
		{
			_cSharpDecompiler = new CSharpLanguage();
            var decompilationOutput = new PlainTextOutput();
            _decompilationOptions = new DecompilationOptions();
            _cSharpDecompiler.DecompileMethod(method, decompilationOutput, _decompilationOptions);
            _oldText = decompilationOutput.ToString();
            _differ = new SideBySideDiffBuilder(new Differ());
		}

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        private IEnumerable<MutationTestMetaData> LockAndMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            IEnumerator<MutationTestMetaData> mutations = DoMutate(method, assembly, fileName).GetEnumerator();
            Monitor.Enter(method);
            while (mutations.MoveNext())
            {
                yield return mutations.Current;
                Monitor.Enter(method);
            }
            Monitor.Exit(method);
        }

        /// <summary>
        /// When implemented in a subclass, performs the actual mutations on
        /// the source assembly
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutationTestMetaData" /> structures.
        /// </returns>
        protected abstract IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName);

        /// <summary>
        /// Moves the original assembly aside, and writes the mutated copy in
        /// its place before returning the test meta data  to allow the test
        /// suite to be run.
        /// </summary>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="method"> </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <param name="output">
        /// The string describing the mutation, returned to calling code with
        /// <c>yield return</c>.
        /// </param>
        /// <returns>
        /// A <see cref="MutationTestMetaData" /> instance.
        /// </returns>
        protected MutationTestMetaData PrepareTests(AssemblyDefinition assembly, MethodDefinition method, string fileName, string output)
        {
            string sourceFolder = Path.GetDirectoryName(fileName);
            string targetFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            CopyDirectory(sourceFolder, targetFolder);
            string targetFileName = Path.Combine(targetFolder, Path.GetFileName(fileName));
            assembly.Write(targetFileName);
            var metaData = new MutationTestMetaData
                             {
                                 TestFolder = targetFolder,
                                 Description = output,
                                 DiffRepresentation = ConstructEstimateCodeDiff(method)
                             };
            Monitor.Exit(method);
            return metaData;
        }

        private string ConstructEstimateCodeDiff(MethodDefinition method)
        {
            var decompilationOutput = new PlainTextOutput();
            try
            {
                _cSharpDecompiler.DecompileMethod(method, decompilationOutput, _decompilationOptions);
            }
            catch
            {
                return "\t\tNo decompilation available for mutated version.\n";
            }
            string newText = decompilationOutput.ToString();
            var model = _differ.BuildDiffModel(_oldText, newText);
            string diffOutput = "\t\tApproximate source code difference from IL decompilation:\n";
            var lines = new SortedSet<int>();
            for (int i = 0; i < Math.Max(model.OldText.Lines.Count, model.NewText.Lines.Count); i++)
            {
                if ((i < model.OldText.Lines.Count && model.OldText.Lines[i].Type != ChangeType.Unchanged)
                    || (i < model.NewText.Lines.Count && model.NewText.Lines[i].Type != ChangeType.Unchanged))
                {
                    lines.Add(i - 2);
                    lines.Add(i - 1);
                    lines.Add(i);
                    lines.Add(i + 1);
                    lines.Add(i + 2);
                }
            }
            int lastLine = -1;
            string adds = "";
            string takes = "";
            foreach (var line in lines)
            {
                if (line < 0) continue;
                if (line > lastLine + 1)
                {
                    diffOutput += string.Format("{1}{2}\t\t@@ {0} @@\n", line,
                        takes, adds);
                    takes = "";
                    adds = "";
                }
                if (line < model.OldText.Lines.Count)
                {
                    takes += string.Format("\t\t- {0}\n",
                        (model.OldText.Lines[line].Text ?? "").Replace("\t", "  "));
                }
                if (line < model.NewText.Lines.Count)
                {
                    adds += string.Format("\t\t+ {0}\n",
                        (model.NewText.Lines[line].Text ?? "").Replace("\t", "  "));
                }
                lastLine = line;
            }
            if (!string.IsNullOrEmpty(adds) || !string.IsNullOrEmpty(takes))
            {
                diffOutput += takes + adds;
            }
            return diffOutput;
        }
    }
}
