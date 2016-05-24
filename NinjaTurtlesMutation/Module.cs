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
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace NinjaTurtlesMutation
{
    /// <summary>
    /// Class representing the main module of a .NET assembly.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Module" /> class.
        /// </summary>
        /// <param name="assemblyLocation">
        /// The location on disk of the assembly whose main module is to be
        /// loaded.
        /// </param>
        public Module(string assemblyLocation)
        {
            AssemblyLocation = assemblyLocation;
            AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation);
            Definition = AssemblyDefinition.MainModule;
            SourceFiles = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Gets the location on disk of the assembly.
        /// </summary>
        public string AssemblyLocation { get; private set; }

        /// <summary>
        /// Gets the <see cref="AssemblyDefinition" />.
        /// </summary>
        public AssemblyDefinition AssemblyDefinition { get; private set; }

        /// <summary>
        /// Gets the <see cref="ModuleDefinition" />.
        /// </summary>
        public ModuleDefinition Definition { get; private set; }

        /// <summary>
        /// Gets a dictionary of source code files with their contained lines
        /// of code.
        /// </summary>
        public IDictionary<string, string[]> SourceFiles { get; private set; } 

        internal void LoadDebugInformation()
        {
            var reader = ResolveSymbolReader();
            if (reader == null) return;

            Definition.ReadSymbols(reader);

            LoadSourceCodeForTypes(Definition.Types, reader);

            foreach (var method in Definition.Types
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody))
            {
                MethodDefinition capturedMethod = method;
                reader.Read(capturedMethod.Body,
                    o => capturedMethod.Body.Instructions.FirstOrDefault(i => i.Offset >= o));

                var sourceFiles = method.Body.Instructions.Where(i => i.SequencePoint != null)
                    .Select(i => i.SequencePoint.Document.Url)
                    .Distinct();
                foreach (var sourceFile in sourceFiles)
                {
                    if (!SourceFiles.ContainsKey(sourceFile) && File.Exists(sourceFile))
                    {
                        SourceFiles.Add(sourceFile, File.ReadAllLines(sourceFile));
                    }
                }
            }
        }

        private void LoadSourceCodeForTypes(IEnumerable<TypeDefinition> types, ISymbolReader reader)
        {
            foreach (var typeDefinition in types)
            {
                foreach (var method in typeDefinition.Methods.Where(m => m.HasBody))
                {
                    MethodDefinition capturedMethod = method;
                    reader.Read(capturedMethod.Body,
                        o => capturedMethod.Body.Instructions.FirstOrDefault(i => i.Offset >= o));

                    var sourceFiles = method.Body.Instructions.Where(i => i.SequencePoint != null)
                        .Select(i => i.SequencePoint.Document.Url)
                        .Distinct();
                    foreach (var sourceFile in sourceFiles)
                    {
                        if (!SourceFiles.ContainsKey(sourceFile) && File.Exists(sourceFile))
                        {
                            SourceFiles.Add(sourceFile, File.ReadAllLines(sourceFile));
                        }
                    }
                }
                if (typeDefinition.NestedTypes != null)
                {
                    LoadSourceCodeForTypes(typeDefinition.NestedTypes, reader);
                }
            }
        }

        private ISymbolReader ResolveSymbolReader()
        {
            string symbolLocation = null;
            string pdbLocation = Path.ChangeExtension(AssemblyLocation, "pdb");
            string mdbLocation = AssemblyLocation + ".mdb";
            ISymbolReaderProvider provider = null;
            if (File.Exists(pdbLocation))
            {
                symbolLocation = pdbLocation;
                provider = new PdbReaderProvider();
            }
            else if (File.Exists(mdbLocation))
            {
                symbolLocation = AssemblyLocation;
                provider = new MdbReaderProvider();
            }
            if (provider == null) return null;
            var reader = provider.GetSymbolReader(Definition, symbolLocation);
            return reader;
        }
    }
}
