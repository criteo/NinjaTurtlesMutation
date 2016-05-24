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
using System.Reflection;

using Mono.Cecil;

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;
using NinjaTurtlesMutation;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class MethodDefinitionResolverLoggingTests : LoggingTestFixture
    {
        [Test]
        public void ResolveMethod_Logs_If_Ambiguous()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            try
            {
                MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences");
            }
            catch (ArgumentException) { }
            AssertLogContains("ERROR|Method \"ResolveTypeFromReferences\" is overloaded.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" with specified parameter types is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" is unrecognised.|");
        }

        [Test]
        public void ResolveMethod_Logs_If_Unambiguous()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(MutationTest).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "MutationTest");
            MethodDefinitionResolver.ResolveMethod(type, "Run");
            AssertLogContains("DEBUG|Resolving method \"Run\" in \"NinjaTurtlesMutation.MutationTest\".|");
            AssertLogContains("DEBUG|Method \"Run\" successfully resolved in \"NinjaTurtlesMutation.MutationTest\".|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" with specified parameter types is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"Leonardo\" is overloaded.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" is unrecognised.|");
        }

        [Test]
        public void ResolveMethod_Logs_If_Overload_Called_With_Null_Parameter_Types()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(MutationTest).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "MutationTest");
            MethodDefinitionResolver.ResolveMethod(type, "Run", (Type[])null);
            AssertLogContains("WARN|\"ResolveMethod\" overload with parameter types called unnecessarily.|");
        }

        [Test]
        public void ResolveMethod_Logs_If_Disambiguated_With_Parameter_Types()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            var parameterTypes = new[] { typeof(Assembly), typeof(string), typeof(IList<string>) };
            MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences", parameterTypes);
            AssertLogContains("DEBUG|Method \"ResolveTypeFromReferences\" successfully resolved in \"NinjaTurtlesMutation.TypeResolver\".|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" with specified parameter types is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"Leonardo\" is overloaded.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" is unrecognised.|");
        }

        [Test]
        public void ResolveMethod_Logs_If_No_Matching_Parameters()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            var parameterTypes = new[] { typeof(Assembly), typeof(string), typeof(ICollection<int>) };
            try
            {
                MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences", parameterTypes);
            }
            catch (ArgumentException) {}
            AssertLogContains("ERROR|Method \"ResolveTypeFromReferences\" with specified parameter types is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"Leonardo\" is overloaded.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" is unrecognised.|");
        }

        [Test]
        public void ResolveMethod_Logs_If_No_Matching_Method_Without_Parameters()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            try
            {
                MethodDefinitionResolver.ResolveMethod(type, "Leonardo");
            }
            catch (ArgumentException) {}
            AssertLogContains("ERROR|Method \"Leonardo\" is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"ResolveTypeFromReferences\" with specified parameter types is unrecognised.|");
            AssertLogDoesNotContain("ERROR|Method \"Leonardo\" is overloaded.|");
        }

    }
}
