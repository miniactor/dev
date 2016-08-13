namespace MiniActor
{
    public class MiniActor<TMessageType, TResponseType> : AMiniActor<TMessageType, TResponseType>
    {
        public MiniActor(int workerCount=1) : base(workerCount)
        {
        }
    }

    public class MiniActor<TMessageType,TState, TResponseType> : AMiniActor<TMessageType, TState, TResponseType>
    {
        public MiniActor(int workerCount = 1) : base(workerCount)
        {
        }
    }
}