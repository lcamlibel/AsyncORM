using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM
{
    internal sealed class AsyncLazy<T>
    {
        private readonly Lazy<Task<T>> _lazyInstance;

        public AsyncLazy(Func<T> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() =>
                                                  {
                                                      var task = Task.Run(factory, cancellationToken);
                                                      task.ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                                                      return task;
                                                  });
        }

        public AsyncLazy(Func<Task<T>> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() =>
                                                  { 
                                                      var task = Task.Run(factory, cancellationToken);
                                                      task.ConfigureAwait(AsyncOrmConfig.ConfigureAwait);
                                                      return task;
                                                  });
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return _lazyInstance.Value.GetAwaiter();
        }
    }
}