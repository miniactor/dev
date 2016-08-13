using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MiniActor
{
    public abstract class AMiniActor<TMessage, TResponse> : AMiniActor<TMessage, object, TResponse>
    {
        protected AMiniActor(int workerCount) : base(workerCount)
        {
        }
    }
    public abstract class AMiniActor<TMessage, TState, TResponse> : IDisposable
    {

        internal InternalState<TState> _internalState = new InternalState<TState>();
        private readonly BlockingCollection<MailMessage<TMessage, TState, TResponse>> _mailBox = new BlockingCollection<MailMessage<TMessage, TState, TResponse>>();

        protected AMiniActor(int workerCount)
        {
            for (var i = 0; i < workerCount; i++)
            {
                Task.Run(async () => await BeginDelivery());
            }
        }

        public async Task<bool> Tell(TMessage command
           , Func<TMessage, StateHandler<TState>, Task<TResponse>> work
           , CancellationToken? cancelToken = null
           )
        {
            PrepareMail(command, work, cancelToken);
            return await Task.FromResult(true);
        }
        public async Task<bool> Tell(TMessage command
          , Func<TMessage, Task<TResponse>> work
          , CancellationToken? cancelToken = null
          )
        {
            PrepareMail(command, work, cancelToken);
            return await Task.FromResult(true);
        }

        public async Task<TResponse> Ask(TMessage command
          , Func<TMessage, StateHandler<TState>, Task<TResponse>> work
          , CancellationToken? cancelToken = null
          )
        {
            var tcs = PrepareMail(command, work, cancelToken);
            return await tcs.Task;
        }
        public async Task<TResponse> Ask(TMessage command
         , Func<TMessage, Task<TResponse>> work
         , CancellationToken? cancelToken = null
         )
        {
            var tcs = PrepareMail(command, work, cancelToken);
            return await tcs.Task;
        }


        private TaskCompletionSource<TResponse> PrepareMail(TMessage command, Func<TMessage, StateHandler<TState>, Task<TResponse>> work, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<TResponse>(command);
            var mail = new MailMessage<TMessage, TState, TResponse>(command, tcs, work, cancelToken, this)
            ;
            _mailBox.Add(mail);

            return tcs;
        }
        private TaskCompletionSource<TResponse> PrepareMail(TMessage command, Func<TMessage, Task<TResponse>> work, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<TResponse>(command);
            var mail = new MailMessage<TMessage, TState, TResponse>(command, tcs, work, cancelToken, this)
            ;
            _mailBox.Add(mail);

            return tcs;
        }



        private async Task BeginDelivery()
        {
            await new Mail<TMessage, TState, TResponse>().MailDelivery(_mailBox);
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _mailBox.CompleteAdding();
            }
            _disposed = true;
        }

        ~AMiniActor()
        {
            Dispose(false);
        }

        #endregion
    }

}