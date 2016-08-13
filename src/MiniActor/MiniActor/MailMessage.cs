using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniActor
{
    internal class MailMessage<TMessage, TState, TResponse>
    {
        public readonly TMessage Command;
        public readonly TaskCompletionSource<TResponse> TaskSource;
        public readonly Func<TMessage, Task<TResponse>> Work;
        public readonly Func<TMessage, StateHandler<TState>, Task<TResponse>> WorkWithState;
        public readonly CancellationToken? CancelToken;
        internal AMiniActor<TMessage, TState, TResponse> Actor;

        public MailMessage(
            TMessage command,
            TaskCompletionSource<TResponse> taskSource,
            Func<TMessage, Task<TResponse>> action,
            CancellationToken? cancelToken, AMiniActor<TMessage, TState, TResponse> actor)
        {
            Command = command;
            TaskSource = taskSource;
            Work = action;
            CancelToken = cancelToken;
            // State = state;
            Actor = actor;
        }
        public MailMessage(
            TMessage command,
            TaskCompletionSource<TResponse> taskSource,
            Func<TMessage, StateHandler<TState>, Task<TResponse>> actionWithState,
            CancellationToken? cancelToken,  AMiniActor<TMessage, TState, TResponse> actor)
        {
            Command = command;
            TaskSource = taskSource;
            WorkWithState = actionWithState;
            CancelToken = cancelToken;
            //  State = state;
            Actor = actor;
        }

        // public InternalState<TState> State { get; set; }
    }
}