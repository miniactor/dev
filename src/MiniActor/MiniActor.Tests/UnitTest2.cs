using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiniActor.Tests
{
    [TestClass]
    public class UnitTest2
    {
        private static int _counterAct = 0;

        public class SomeActor : MiniActor
        {
            public SomeActor()
            {
                Receive<Guid>(m =>
                {
                    _counterAct++;
                });
            }
        }

        [TestMethod]
        public void actor_test2()
        {
            var messanger = new MiniActorMessanger();
            const int total = 100000;
            foreach (var i in Enumerable.Range(0, total))
            {
                messanger.Tell<SomeActor>(Guid.NewGuid());
            }
            var counter = 0;
            foreach (var i in Enumerable.Range(0, total))
            {
                counter++;
                try
                {
                    Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(1)));
                    Assert.AreEqual(total, _counterAct);
                    break;
                }
                catch (Exception)
                {

                }
            }
            Assert.IsTrue(counter < total);

        }
    }
}