using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniActor
{
    public abstract class MiniActor
    {
        protected MiniActor()
        {
            Actor = new MiniActor<object, Exception>(1, Supervision);
            MessageActions=new Dictionary<Type, Action<object>>();
        }
        internal static ConcurrentDictionary<Type, object> ActorObjects =new ConcurrentDictionary<Type, object>();
        
        internal Dictionary<Type, Action<object>> MessageActions { set; get; }
        
        internal MiniActor<object,Exception> Actor { set; get; }

        public void Receive<T>(Action<T> messageAction)
        {
            MessageActions.Add(typeof(T), Convert(messageAction));
        }

        public void ReceiveAny(Action<object> messageAction)
        {
            if (!MessageActions.ContainsKey(typeof (object)))
            {
               MessageActions.Add(typeof(object), messageAction);
            }
        }
        internal  static Action<object> Convert<T>(Action<T> myActionT)
        {
            if (myActionT == null) return null;
            else return new Action<object>(o => myActionT((T)o));
        }
        public virtual Superkision Supervision( Exception ex)
        {
            return new Superkision();
        }
    }

    public class MiniActor<TMessageType> : AMiniActor<TMessageType, object>
    {
        public MiniActor(int workerCount = 1, Func<Exception, Superkision> superVision = null) : base(workerCount, superVision)
        {

        }
        public void Tell<TActor>(TMessageType command
         , CancellationToken? cancelToken = null
         ) where TActor : MiniActor, new()
        {

            Func<TMessageType, StateHandler<object>, Task<object>> work = async (message, stateHandler) =>
            {
                TActor actor = null;
                var actorType = typeof(TActor);
                if (MiniActor.ActorObjects.ContainsKey(actorType))
                {
                    actor = MiniActor.ActorObjects[actorType] as TActor;
                    if (actor == null)
                    {
                        object add;
                        MiniActor.ActorObjects.TryRemove(actorType, out add);
                    }
                }

                if (actor == null)
                {
                    actor = new TActor();
                    MiniActor.ActorObjects.GetOrAdd(actorType, actor);
                }
                await actor.Actor.Tell(message, async (m, s) =>
                {
                    Exception exception = null;
                    try
                    {
                        Action<object> messageAction = null;
                        foreach (var keyValuePair in actor.MessageActions.Where(keyValuePair => keyValuePair.Key.IsInstanceOfType(m) && messageAction == null))
                        {
                            messageAction = keyValuePair.Value;
                        }

                        if (messageAction != null)
                        {
                            messageAction(m);
                        }
                        else
                        {
                            //todo dead letter
                        }
                        return await Task.FromResult<Exception>(null);
                    }
                    catch (Exception e)
                    {
                        //todo supervision
                        exception = e;
                        throw;
                    }
                    return await Task.FromResult<Exception>(exception);
                }, cancelToken);

                return await Task.FromResult(new object());
            };
            PrepareMail(command, work, cancelToken);
        }


    }

    public class MiniActor<TMessageType, TResponseType> : AMiniActor<TMessageType, TResponseType>
    {
        public MiniActor(int workerCount = 1, Func<Exception, Superkision> superVision = null) : base(workerCount, superVision)
        {
        }
        public MiniActor(Func<Exception, Superkision> superVision, int workerCount = 1) : base(workerCount, superVision)
        {
        }
    }

    public class MiniActor<TMessageType, TState, TResponseType> : AMiniActor<TMessageType, TState, TResponseType>
    {
        public MiniActor(int workerCount = 1, Func<Exception, Superkision> superVision = null) : base(workerCount, superVision)
        {
        }
        public MiniActor(Func<Exception, Superkision> superVision, int workerCount = 1) : base(workerCount, superVision)
        {
        }
    }
    public abstract class AMiniActor<TMessage, TResponse> : AMiniActor<TMessage, object, TResponse>
    {
        protected AMiniActor(int workerCount, Func<Exception, Superkision> superVision) : base(workerCount, superVision)
        {
        }
    }

}