using System;
using NUnit.Framework;
using PrimeFinderMutationPlayground;

namespace PrimeFinder.UTest
{
    class T_PrimeMutationPlayground
    {
        [TestCase(1)]
        [TestCase(0)]
        [TestCase(-1)]
        public void ShouldThrowIfLowerThanTwo(int n)
        {
            Assert.Catch(() => PrimeFinderMutationPlayground.PrimeFinder.IsPrime(n));
        }

        //[TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        [TestCase(5, true)]
        [TestCase(27, false)]
        [TestCase(31, true)]
        [TestCase(121, false)]
        public void ShouldDetectPrimes(int n, bool isPrime)
        {
            Assert.AreEqual(isPrime, PrimeFinderMutationPlayground.PrimeFinder.IsPrime(n));
        }
    }
}
