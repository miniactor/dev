using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MiniActor
{
    public class MiniActor<TMessageType, TResponseType> : AMiniActor<TMessageType, TResponseType>
    {
        public MiniActor(int workerCount = 1, Func<Exception, SuperVision> superVision = null) : base( workerCount, superVision)
        {
        }
        public MiniActor( Func<Exception, SuperVision> superVision , int workerCount = 1) : base(workerCount, superVision)
        {
        }
    }

    public class MiniActor<TMessageType, TState, TResponseType> : AMiniActor<TMessageType, TState, TResponseType>
    {
        public MiniActor( int workerCount=1, Func<Exception, SuperVision> superVision = null) : base(workerCount, superVision)
        {
        }
        public MiniActor( Func<Exception, SuperVision> superVision , int workerCount = 1) : base(workerCount, superVision)
        {
        }
    }
    public abstract class AMiniActor<TMessage, TResponse> : AMiniActor<TMessage, object, TResponse>
    {
        protected AMiniActor(int workerCount, Func<Exception, SuperVision> superVision) : base(workerCount, superVision)
        {
        }
    }
    public abstract class AMiniActor<TMessage, TState, TResponse> : IDisposable
    {
        internal   SuperVision DefaultSupervision = new SuperVision();
    
        public Func<Exception, SuperVision> SuperVision;
        internal InternalState<TState> StateInternal = new InternalState<TState>();
        private readonly BlockingCollection<MailMessage<TMessage, TState, TResponse>> _mailBox = new BlockingCollection<MailMessage<TMessage, TState, TResponse>>();
        
        protected AMiniActor(int workerCount, Func<Exception, SuperVision> superVision)
        {
            SuperVision = superVision;
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