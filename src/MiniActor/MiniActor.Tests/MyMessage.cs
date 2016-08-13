namespace MiniActor.Tests
{
    public class MyMessage
    {
        public MyMessage(string name)
        {
            Name = name;
        }

        public string Name { private set; get; }
    }
}