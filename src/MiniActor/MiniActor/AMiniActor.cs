namespace MiniActor
{
    public abstract class AMiniActor<TMessage, TResponse> : AMiniActor<TMessage, object, TResponse>
    {
        protected AMiniActor(int workerCount) : base(workerCount)
        {
        }
    }
}