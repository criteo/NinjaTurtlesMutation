using System;
using System.Net.Mime;

namespace PrimeFinderMutationPlayground
{
    public interface IDummy { }

    public class Dummy : IDummy { }

    public class PrimeFinder
    {
        public static bool IsPrime(int n)
        {
            return IsPrimed(n, new Dummy());
        }

        public static bool IsPrimed(int n, IDummy dummy)
        {
            if (dummy == null)
                throw new Exception("null dummy");

            if (n <= 1)
                throw new Exception("n should be greater than 1");

            if (n == 2)
                return true;
            if (n % 2 == 0)
                return false;

            int upperBound = (int)Math.Sqrt(n);
            for (int i = 3; i <= upperBound; i += 2)
            {
                if (n % i == 0)
                    return false;
            }
            return true;
        }
    }
}
