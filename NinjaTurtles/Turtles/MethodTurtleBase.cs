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
using System.Globalization;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles
{
    /// <summary>
    /// An abstract base class for implementations of
    /// <see cref="IMethodTurtle" />.
    /// </summary>
    public abstract class MethodTurtleBase : IMethodTurtle
    {
        internal void MutantComplete(MutantMetaData metaData)
        {
            metaData.TestDirectory.Dispose();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> of detailed descriptions
        /// of mutations, having first carried out the mutation in question and
        /// saved the modified assembly under test to disk.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <param name="originalOffsets">
        /// An array of the original IL offsets before macros were expanded.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        public IEnumerable<MutantMetaData> Mutate(MethodDefinition method, Module module, int[] originalOffsets)
        {
            var ret = MutateMethod(method, module, originalOffsets);

            ret = ret.Concat(MutateEnumerableGenerators(method, module));
            
            ret = ret.Concat(MutateClosures(method, module));

            ret = ret.Concat(MutateAnonymousDelegates(method, module));

            return ret;
        }

        private IEnumerable<MutantMetaData> MutateEnumerableGenerators(MethodDefinition method, Module module)
        {
            var nestedType =
                method.DeclaringType.NestedTypes.FirstOrDefault(
                    t => t.Name.StartsWith(string.Format("<{0}>", method.Name))
                    && t.Interfaces.Any(i => i.Name == "IEnumerable`1"));
            if (nestedType == null)
                return Enumerable.Empty<MutantMetaData>();
            
            var nestedMethod = nestedType.Methods.First(m => m.Name == "MoveNext");
            var originalOffsets = nestedMethod.Body.Instructions.Select(i => i.Offset).ToArray();
            return MutateMethod(nestedMethod, module, originalOffsets);
        }

        private IEnumerable<MutantMetaData> MutateClosures(MethodDefinition method, Module module)
        {
            var ret = Enumerable.Empty<MutantMetaData>();

            var nestedType =
                method.DeclaringType.NestedTypes.FirstOrDefault(
                    t => t.Name.StartsWith("<>c__DisplayClass")
                        && t.Methods.Any(m => m.Name.StartsWith(string.Format("<{0}>", method.Name)))
                    );
            if (nestedType == null)
                return ret;

            var closureMethods = nestedType.Methods.Where(m => m.Name.StartsWith(string.Format("<{0}>", method.Name)));
            foreach (var closureMethod in closureMethods) { 
                var originalOffsets = closureMethod.Body.Instructions.Select(i => i.Offset).ToArray();
                ret = ret.Concat(MutateMethod(closureMethod, module, originalOffsets));
            }

            return ret;
        }

        private IEnumerable<MutantMetaData> MutateAnonymousDelegates(MethodDefinition method, Module module)
        {
            var delegateMethods = method.DeclaringType.Methods.Where(m => m.Name.StartsWith(string.Format("<{0}>", method.Name)));

            var ret = Enumerable.Empty<MutantMetaData>();
            foreach (var delegateMethod in delegateMethods)
            {
                var originalOffsets = delegateMethod.Body.Instructions.Select(i => i.Offset).ToArray();
                ret = ret.Concat(MutateMethod(delegateMethod, module, originalOffsets));
            }

            return ret;
        }

        private IEnumerable<MutantMetaData> MutateMethod(MethodDefinition method, Module module, int[] originalOffsets)
        {
                //leave as a yield-return, so that we don't optimize macros again until we stop enumerating.
            method.Body.SimplifyMacros();
            foreach (var mutation in CreateMutant(method, module, originalOffsets))
            {
                yield return mutation;
            }
            method.Body.OptimizeMacros();
        }

        /// <summary>
        /// Gets a description of the current turtle.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Performs the actual code mutations, returning each with
        /// <c>yield</c> for the calling code to use.
        /// </summary>
        /// <remarks>
        /// Implementing classes should yield the result obtained by calling
        /// the <see mref="DoYield" /> method.
        /// </remarks>
        /// <param name="method">
        ///     A <see cref="MethodDefinition" /> for the method on which mutation
        ///     testing is to be carried out.
        /// </param>
        /// <param name="module">
        ///     A <see cref="Module" /> representing the main module of the
        ///     containing assembly.
        /// </param>
        /// <param name="originalOffsets"></param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        protected abstract IEnumerable<MutantMetaData> CreateMutant(MethodDefinition method, Module module, int[] originalOffsets);

        /// <summary>
        /// A helper method that copies the test folder, and saves the mutated
        /// assembly under test into it before returning an instance of
        /// <see cref="MutantMetaData" />.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <param name="description">
        /// A description of the mutation that has been applied.
        /// </param>
        /// <param name="index">
        /// The index of the (first) IL instruction at which the mutation was
        /// applied.
        /// </param>
        /// <returns></returns>
        protected MutantMetaData DoYield(MethodDefinition method, Module module, string description, int index)
        {
            var toCopy = new List<string>() { method.DeclaringType.Module.ToString() };
            var testDirectory = new TestDirectory(Path.GetDirectoryName(module.AssemblyLocation), toCopy);
            testDirectory.SaveAssembly(module);
            return new MutantMetaData(
                module,
                description,
                method,
                index,
                testDirectory
            );
        }

        /// <summary>
        /// A helper method that copies the test folder, and saves the mutated
        /// assembly under test into it before returning an instance of
        /// <see cref="MutantMetaData" />.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <param name="description">
        /// A description of the mutation that has been applied.
        /// </param>
        /// <param name="genericDescription">
        /// A generic description of the mutation that has been applied.
        /// </param>
        /// <param name="index">
        /// The index of the (first) IL instruction at which the mutation was
        /// applied.
        /// </param>
        /// <returns></returns>
        protected MutantMetaData DoYield(MethodDefinition method, Module module, string description, string genericDescription, int index)
        {
            var toCopy = new List<string>() { method.DeclaringType.Module.ToString() };
            var testDirectory = new TestDirectory(Path.GetDirectoryName(module.AssemblyLocation), toCopy);
            testDirectory.SaveAssembly(module);
            return new MutantMetaData(
                module,
                description,
                genericDescription,
                method,
                index,
                testDirectory
            );
        }
    }
}
