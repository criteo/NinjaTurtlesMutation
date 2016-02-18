using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtles.Tests.Turtles.Tests
{
    public class ClosureClassUnderTest
    {
        public int Dummy()
        {
            return 0;
        }

        public int AddClosure(int left, int right)
        {
            Func<int> f = () => left + right;

            return f();
        }

        public Func<int> ReturnsClosure(int left, int right)
        {
            return () => left + right;
        }

        public int AddDelegate(int left, int right)
        {
            Func<int, int, int> f = (l, r) => l + r;

            return f(left, right);
        }

        public Func<int, int, int> ReturnsDelegate()
        {
            return (l, r) => l + r;
        }


        public int AddMultipleClosures(int left, int right)
        {
            Func<int> f1 = () => left;
            Func<int, int> f2 = (partial) => partial + right;

            return f2(f1());
        }

        public int AddMultipleDelegates(int left, int right)
        {
            Func<int, int> f1 = (i) => i;
            Func<int, int, int> f2 = (partial, i) => partial + i;

            return f2(f1(left), right);
        }
    }
}
