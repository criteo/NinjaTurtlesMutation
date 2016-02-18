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
using System.Reflection;

using NinjaTurtles.TestRunner;

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
    /// This code creates and runs the default set of mutation tests for the
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
    /// be run for each mutation that is found of the code under test. By
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
    /// MutationTestBuilder&lt;ClassUnderTest&gt;.UseRunner&lt;GallioTestRunner&gt;();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest).UseRunner(Of GallioTestRunner)
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;::UseRunner&lt;GallioTestRunner^&gt;();
    /// </code>
    /// </example>
    public static class MutationTestBuilder<T> where T : class
    {
        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="methodName">
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
        public static IMutationTest For(string methodName, Type[] parameterTypes = null)
        {
            return MutationTestBuilder.For(Assembly.GetCallingAssembly(), typeof(T), methodName, parameterTypes);
        }
    }

    /// <summary>
    /// A static class used as the starting point for a fluent definition of
    /// a set of mutation tests.
    /// </summary>
    /// <remarks>
    /// For public classes, the generic <see cref="MutationTestBuilder{T}" />
    /// is to be prefered. See that class for full documentation.
    /// </remarks>
    public static class MutationTestBuilder
    {
        private static Type _testRunner;

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="className">
        /// The namespace-qualified name of the type for which mutation tests
        /// are being defined.
        /// </param>
        /// <param name="methodName">
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
        public static IMutationTest For(string className, string methodName, Type[] parameterTypes = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            Type resolvedType = ResolveTypeFromReferences(callingAssembly, className);
            if (resolvedType == null)
            {
                throw new MutationTestFailureException(
                    string.Format("Type {0} could not be resolved.", className));
            }
            return For(callingAssembly, resolvedType, methodName, parameterTypes);
        }

        private static Type ResolveTypeFromReferences(Assembly assembly, string className)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.FullName == className) return type;
            }
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                var type = ResolveTypeFromReferences(Assembly.Load(reference), className);
                if (type != null) return type;
            }
            return null;
        }

        internal static IMutationTest For(Assembly callingAssembly, Type targetType, string methodName, Type[] parameterTypes)
        {
            var testAssembly = callingAssembly.Location;
            var mutationTest = new MutationTest(targetType, methodName, parameterTypes, testAssembly);
            if (_testRunner != null)
            {
                mutationTest.TestRunner = _testRunner;
            }
            return mutationTest;
        }

        /// <summary>
        /// Sets a default test runner type for mutation testing.
        /// </summary>
        /// <typeparam name="TRunner">
        /// The type of test runner to store as default.
        /// </typeparam>
        /// <example>
        /// This example shows how this might be used in a method flagged with
        /// the MSTest <b>[TestInitialize]</b> attribute:
        /// <code lang="cs">
        /// [TestInitialize]
        /// public void TestInitialize()
        /// {
        ///     MutationTestBuilder&lt;ClassUnderTest&gt;.UseRunner&lt;GallioTestRunner&gt;();
        /// }
        /// </code>
        /// <code lang="vbnet">
        /// &lt;TestInitialize&gt;
        /// Public Sub TestInitialize()
        ///     Call MutationTestBuilder(Of ClassUnderTest).UseRunner(Of GallioTestRunner()
        /// End Sub
        /// </code>
        /// <code lang="cpp">
        /// [TestInitialize]
        /// void TestInitialize()
        /// {
        ///     MutationTestBuilder&lt;ClassUnderTest^&gt;::UseRunner&lt;GallioTestRunner^&gt;();
        /// }
        /// </code>
        /// </example>
        public static void UseRunner<TRunner>() where TRunner : ITestRunner
        {
            _testRunner = typeof(TRunner);
        }

        /// <summary>
        /// Clears any defaults that have been set.
        /// </summary>
        /// <example>
        /// This example shows how this might be used in a method flagged with
        /// the MSTest <b>[TestCleanup]</b> attribute:
        /// <code lang="cs">
        /// [TestCleanup]
        /// public void TestCleanup()
        /// {
        ///     MutationTestBuilder&lt;ClassUnderTest&gt;.Clear();
        /// }
        /// </code>
        /// <code lang="vbnet">
        /// &lt;TestCleanup&gt;
        /// Public Sub TestCleanup()
        ///     Call MutationTestBuilder(Of ClassUnderTest).Clear
        /// End Sub
        /// </code>
        /// <code lang="cpp">
        /// [TestCleanup]
        /// void TestCleanup()
        /// {
        ///     MutationTestBuilder&lt;ClassUnderTest^&gt;::Clear();
        /// }
        /// </code>
        /// </example>
        public static void Clear()
        {
            _testRunner = null;
        }
    }
}
