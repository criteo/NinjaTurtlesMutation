using System;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTurtlesMutation.AppDomainIsolation
{
    /// <summary>
    /// Isolated class provide a way to easily instantiate object
    /// in a different AppDomain than the calling code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Isolated<T> : IDisposable where T : Adaptor.Adaptor
    {
        private const int INSTANCE_WAIT_FOR_EXIT_TIME_MS = 20;
        private const int APPDOMAIN_UNLOADING_TIMELIMT_MS = 50;

        private AppDomain _domain;
        private T _instance;

        /// <summary>
        /// Instantiate a new isolated T in a AppDomain defined with a guid
        /// </summary>
        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(), null,
                AppDomain.CurrentDomain.SetupInformation);
            var instanceType = typeof (T);
            _instance = (T) _domain.CreateInstanceAndUnwrap(instanceType.Assembly.FullName, instanceType.FullName);
        }

        /// <summary>
        /// Instantiate a new isolated T in a AppDomain defined with the idx parameter
        /// </summary>
        public Isolated(int idx)
        {
            _domain = AppDomain.CreateDomain("Isolated:" + idx, null,
                AppDomain.CurrentDomain.SetupInformation);
            var instanceType = typeof(T);
            _instance = (T)_domain.CreateInstanceAndUnwrap(instanceType.Assembly.FullName, instanceType.FullName);
        }

        /// <summary>
        /// Instantiate a new isolated T in a AppDomain defined with the string parameter
        /// </summary>
        public Isolated(string id)
        {
            _domain = AppDomain.CreateDomain("Isolated:" + id + " " + Guid.NewGuid(), null,
                AppDomain.CurrentDomain.SetupInformation);
            var instanceType = typeof(T);
            _instance = (T)_domain.CreateInstanceAndUnwrap(instanceType.Assembly.FullName, instanceType.FullName);
        }

        /// <summary>
        /// Access the internal T instance
        /// </summary>
        public T Instance
        {
            get { return _instance; }
        }

        private void DisposeCore()
        {
            if (_domain != null)
            {
                if (!_instance.IsCompleted())
                    _instance.WaitForExit(INSTANCE_WAIT_FOR_EXIT_TIME_MS);
                try { AppDomain.Unload(_domain); }
                catch (Exception ex) { Console.Error.WriteLine("A AppDomain unloading goes wrong:\n" + ex); }
                _domain = null;
            }
        }

        /// <summary>
        /// Unload the associated AppDomain
        /// </summary>
        public void Dispose()
        {
            var unloadTask = new Task(DisposeCore);
            unloadTask.Start();
            Thread.Sleep(APPDOMAIN_UNLOADING_TIMELIMT_MS);
        }
    }
}
