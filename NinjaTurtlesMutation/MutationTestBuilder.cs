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
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NinjaTurtlesMutation.ServiceTestRunnerLib;

namespace NinjaTurtlesMutation
{
    public static class MutationTestBuilder
    {
        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="callingAssemblyLocation">The test assembly for most cases</param>
        /// <param name="targetClass">
        /// The namespace-qualified name of the type for which mutation tests
        /// are being defined.
        /// </param>
        /// <param name="targetMethod">
        /// The name of the method to mutate.
        /// </param>
        /// <param name="dispatcher">
        /// The dispatcher used by MutationTest to test mutated assembly
        /// </param>
        /// <param name="parameterTypes">
        /// Optional parameter specifying an array of parameter types used to
        /// identify a particular method overload.
        /// </param>
        /// <returns>
        /// An <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        internal static IMutationTest For(string callingAssemblyLocation, string targetClass, string targetMethod, TestsDispatcher dispatcher, Type[] parameterTypes = null)
        {
            var callingAssembly = Assembly.LoadFrom(callingAssemblyLocation);
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return new MutationTest(callingAssemblyLocation, resolvedType, targetMethod, parameterTypes, dispatcher);
        }

        internal static IMutationTest For(string callingAssemblyLocation, string targetClass, string returnType, string targetMethod, GenericParameter[] methodGenerics, TestsDispatcher dispatcher, Type[] parameterTypes = null)
        {
            var callingAssembly = Assembly.LoadFrom(callingAssemblyLocation);
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return new MutationTest(callingAssemblyLocation, resolvedType, returnType, targetMethod, methodGenerics, parameterTypes, dispatcher);
        }

        internal static IMutationTest For(string callingAssemblyLocation, string targetClass, string returnType, string targetMethod, GenericParameter[] methodGenerics, TestsDispatcher dispatcher, TypeReference[] parameterTypes)
        {
            var callingAssembly = Assembly.LoadFrom(callingAssemblyLocation);
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return new MutationTest(callingAssemblyLocation, resolvedType, returnType, targetMethod, methodGenerics, parameterTypes, dispatcher);
        }
	}
}

