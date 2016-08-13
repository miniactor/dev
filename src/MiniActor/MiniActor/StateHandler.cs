namespace MiniActor
{
    public class StateHandler<TState>
    {
        internal TState State { set; get; }

        public StateHandler(TState state)
        {
            State = state;
        }

        public void SetState(TState state)
        {
            State = state;
        }
        public TState GetState()
        {
            return State;
        }
    }
}