using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM
{
    public sealed class AsyncLazy<T>
    {        
        private readonly Lazy<Task<T>> _lazyInstance;

        public AsyncLazy(Func<T> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() => Task.Run(factory,cancellationToken));
        }
        public AsyncLazy(Func<Task<T>> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() => Task.Run(factory,cancellationToken));
        }
        public TaskAwaiter<T> GetAwaiter()
        {
            return _lazyInstance.Value.GetAwaiter();
        }
    }
}
