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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Mono.Cecil;

using NinjaTurtles.TestRunners;

namespace NinjaTurtles
{
    /// <summary>
    /// A static class used as the starting point for a fluent definition of
    /// a set of mutation tests.
    /// </summary>
    /// <typeparam name="T">
    /// The type to be tested.
    /// </typeparam>
    /// <example>
    /// <para>
    /// This code creates and runs mutation tests for the 
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->Run();
    /// </code>
    /// <para>
    /// When this code is included in a test, it causes the matching tests to
    /// be run for each mutation that is found of the code under test. The
    /// matching tests are determined automatically by NinjaTurtles. By
    /// default, NinjaTurtles assumes it is running under NUnit, and thus uses
    /// an NUnit runner to run the suite against the mutated code. This can be
    /// changed using the fluent interface:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .UsingRunner&lt;GallioTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .UsingRunner(Of GallioTestRunner)() _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->UsingRunner&lt;GallioTestRunner^&gt;()
    ///     ->Run();
    /// </code>
    /// <para>
    /// Alternatively, this option can be set across all tests in a fixture by
    /// including this line in the test fixture's setup method:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder.UseRunner&lt;GallioTestRunner&gt;();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder.UseRunner(Of GallioTestRunner)
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder::UseRunner&lt;GallioTestRunner^&gt;();
    /// </code>
    /// </example>
    public static class MutationTestBuilder<T>
	{
        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="targetMethod">
        /// The name of the method to mutate.
        /// </param>
        /// <param name="parameterTypes">
        /// Optional parameter specifying an array of parameter types used to
        /// identify a particular method overload.
        /// </param>
        /// <returns>
        /// An <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        public static IMutationTest For(string targetMethod, Type[] parameterTypes = null)
		{
			var callingAssembly = Assembly.GetCallingAssembly();
			return MutationTestBuilder.For(callingAssembly.Location, typeof(T), targetMethod, parameterTypes);
		}

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method that is
        /// passed in via an expression.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <returns>An <see cref="IMutationTest" /> instance to allow fluent method chaining.</returns>
        /// <remarks>
        ///     contributed by Gordon Burgett
        ///     https://ninjaturtles.codeplex.com/workitem/6
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IMutationTest For<TRet>(Expression<Func<T, TRet>> method)
        {
            var call = method.Body as MethodCallExpression;
            if (call == null) throw new ArgumentException("Expression body must be a method call");

            return MutationTestBuilder.For(call.Method);
        }

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular void method that is
        /// passed in via an expression.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <returns>An <see cref="IMutationTest" /> instance to allow fluent method chaining.</returns>
        /// <remarks>
        ///     contributed by Gordon Burgett
        ///     https://ninjaturtles.codeplex.com/workitem/6
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IMutationTest For(Expression<Action<T>> method)
        {
            var call = method.Body as MethodCallExpression;
            if (call == null) throw new ArgumentException("Expression body must be a method call");

            return MutationTestBuilder.For(call.Method);
        }
	}

    /// <summary>
    /// A static class used as the starting point for a fluent definition of
    /// a set of mutation tests.
    /// </summary>
    /// <remarks>
    /// For public classes, the generic <see cref="MutationTestBuilder" />
    /// is to be prefered. See that class for full documentation.
    /// </remarks>
    public static class MutationTestBuilder
    {
        internal static Type TestRunner { get; set; }

        static MutationTestBuilder()
        {
            TestRunner = typeof(NUnitTestRunner);
        }

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="targetClass">
        /// The namespace-qualified name of the type for which mutation tests
        /// are being defined.
        /// </param>
        /// <param name="targetMethod">
        /// The name of the method to mutate.
        /// </param>
        /// <param name="parameterTypes">
        /// Optional parameter specifying an array of parameter types used to
        /// identify a particular method overload.
        /// </param>
        /// <returns>
        /// An <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        public static IMutationTest For(string targetClass, string targetMethod, Type[] parameterTypes = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return For(callingAssembly.Location, resolvedType, targetMethod, parameterTypes);
        }

        internal static IMutationTest For(string targetClass, string targetMethod, TypeReference[] parameterTypes)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return For(callingAssembly.Location, resolvedType, targetMethod, parameterTypes);
        }

        internal static IMutationTest For(string callingAssemblyLocation, string targetClass, string targetMethod, Type[] parameterTypes = null)
        {
            var callingAssembly = Assembly.LoadFrom(callingAssemblyLocation);
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return new MutationTest(callingAssemblyLocation, resolvedType, targetMethod, parameterTypes);
        }

        internal static IMutationTest For(string callingAssemblyLocation, string targetClass, string targetMethod, TypeReference[] parameterTypes)
        {
            var callingAssembly = Assembly.LoadFrom(callingAssemblyLocation);
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

            return new MutationTest(callingAssemblyLocation, resolvedType, targetMethod, parameterTypes);
        }

        internal static IMutationTest For(string callingAssemblyLocation, Type targetType, string targetMethod, Type[] parameterTypes)
        {
            return new MutationTest(callingAssemblyLocation, targetType, targetMethod, parameterTypes);
        }

        internal static IMutationTest For(string callingAssemblyLocation, Type targetType, string targetMethod, TypeReference[] parameterTypes)
        {
            return new MutationTest(callingAssemblyLocation, targetType, targetMethod, parameterTypes);
        }

        /// <summary>
        /// Specifies the implementation of <see cref="ITestRunner" /> to be
        /// used to run the test suite for each mutant. By default, this will
        /// be the <see cref="NUnitTestRunner" />. This can still be overridden
        /// on a per-test basis using the
        /// <see mref="IMutationTest.UsingRunner" /> method.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="ITestRunner" />.
        /// </typeparam>
        public static void UseRunner<T>() where T : ITestRunner
        {
            TestRunner = typeof(T);
        }

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method that is
        /// passed in via an expression.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <typeparam name="T">A type that implements <see cref="ITestRunner" />.</typeparam>
        /// <returns>An <see cref="IMutationTest" /> instance to allow fluent method chaining.</returns>
        /// <remarks>
        ///     contributed by Gordon Burgett
        ///     https://ninjaturtles.codeplex.com/workitem/6
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IMutationTest For<T>(Expression<Func<T>> method)
        {
            var call = method.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException("Expression body must be a method call");

            return For(call.Method);
        }

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular void method that is
        /// passed in via an expression.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <returns>An <see cref="IMutationTest" /> instance to allow fluent method chaining.</returns>
        /// <remarks>
        ///     contributed by Gordon Burgett
        ///     https://ninjaturtles.codeplex.com/workitem/6
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static IMutationTest For(Expression<Action> method)
        {
            var call = method.Body as MethodCallExpression;
            if (call == null) throw new ArgumentException("Expression body must be a method call");

            return For(call.Method);
        }

        // contributed by Gordon Burgett
        // https://ninjaturtles.codeplex.com/workitem/6
        internal static IMutationTest For(MethodInfo method)
        {
            // ReSharper disable once PossibleNullReferenceException
            return For(method.DeclaringType.Assembly.Location, method.DeclaringType, method.Name, method.GetParameters().Select(pi => pi.ParameterType).ToArray());
        }
	}
}

