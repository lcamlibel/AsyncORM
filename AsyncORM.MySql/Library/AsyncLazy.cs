using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncORM.MySql.Library
{
    internal sealed class AsyncLazy<T>
    {
        private readonly Lazy<Task<T>> _lazyInstance;

        public AsyncLazy(Func<T> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() =>
                                                  {
                                                      Task<T> task = Task.Run(factory, cancellationToken);
                                                      task.ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                                                      return task;
                                                  });
        }

        public AsyncLazy(Func<Task<T>> factory, CancellationToken cancellationToken)
        {
            _lazyInstance = new Lazy<Task<T>>(() =>
                                                  {
                                                      Task<T> task = Task.Run(factory, cancellationToken);
                                                      task.ConfigureAwait(MySqlAsyncOrmConfig.ConfigureAwait);
                                                      return task;
                                                  });
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return _lazyInstance.Value.GetAwaiter();
        }
    }
}