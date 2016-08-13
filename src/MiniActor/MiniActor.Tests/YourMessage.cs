namespace MiniActor.Tests
{
    public class YourMessage
    {
        public YourMessage(string name)
        {
            Name = name;
        }

        public string Name { private set; get; }
    }
}