using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiniActor.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void basic_ask()
        {
            var actor = new MiniActor<MyMessage, YourMessage>();
            var ran = 0;
            var task = actor.Ask(new MyMessage("me"), async (myMessage, state) =>
            {
                ran++;
                return await Task.FromResult(new YourMessage("you"));
            });
            Task.WaitAll(task);
            Assert.AreEqual(ran, 1);
            Assert.AreEqual(task.Result.Name, "you");
        }
        [TestMethod]
        public void basic_ask_with_state1()
        {
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var ran = 0;
            var task = actor.Ask(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you"));
            });
            Task.WaitAll(task);
            Assert.AreEqual(ran, 1);
            Assert.AreEqual(task.Result.Name, "you");
        }
        [TestMethod]
        public void basic_ask_with_state2()
        {
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var ran = 0;
            var task1 = actor.Ask(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you1"));
            });
            Task.WaitAll(task1);
            var task2 = actor.Ask(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you2"));
            });
            Task.WaitAll(task2);
            Assert.AreEqual(ran, 2);
            Assert.AreEqual(task1.Result.Name, "you1");
            Assert.AreEqual(task2.Result.Name, "you2");
        }
        [TestMethod]
        public void basic_tell()
        {
            var actor = new MiniActor<MyMessage, YourMessage>();
            var ran = 0;
            var task = actor.Tell(new MyMessage("me"), async (myMessage, state) =>
            {
                ran++;
                return await Task.FromResult(new YourMessage("you"));
            });
            Task.WaitAll(task);
            Assert.IsTrue(task.Result);
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(ran, 1);
        }


        [TestMethod]
        public void basic_tell_with_state1()
        {
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var ran = 0;
            var task = actor.Tell(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you"));
            });
            Task.WaitAll(task);
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(ran, 1);
            Assert.IsTrue(task.Result);
        }
        [TestMethod]
        public void basic_tell_with_state2()
        {
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var ran = 0;
            var task1 = actor.Tell(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you1"));
            });
            Task.WaitAll(task1);
            var task2 = actor.Tell(new MyMessage("me"), async (myMessage, stateHandler) =>
            {
                var state = stateHandler.GetState();
                state = state ?? new MyState();
                state.StateNumber++;
                stateHandler.SetState(state);
                ran++;
                Assert.AreEqual(ran, state.StateNumber);
                return await Task.FromResult(new YourMessage("you2"));
            });
            Task.WaitAll(task2);
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(ran, 2);
            Assert.IsTrue(task1.Result);
            Assert.IsTrue(task2.Result);
        }



        [TestMethod]
        public void basic_tell_with_state_iteration()
        {
            var ran = 0;
            const int total = 1000000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();

            Enumerable.Range(1, total).ToList().ForEach(async x =>
               {
                   var result = await actor.Tell(new MyMessage("me"), async (myMessage, stateHandler) =>
                   {
                       var state = stateHandler.GetState();
                       state = state ?? new MyState();
                       state.StateNumber++;
                       stateHandler.SetState(state);
                       ran++;
                       Assert.AreEqual(ran, state.StateNumber);
                       return await Task.FromResult(new YourMessage("you"));
                   });
                   await Task.Delay(TimeSpan.FromMilliseconds(100));

                   Assert.IsTrue(result);
               });
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(ran, total);
        }

        [TestMethod]
        public void basic_tell_with_state_iteration_keeping_order()
        {
            const int total = 1000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var expectedString = "";
            range.ForEach(x => expectedString += x);

            var finalString = "";

            range.ForEach(async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage, stateHandler) =>
                {
                    done = true;
                    finalString += myMessage.Name;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done);
                Assert.IsTrue(result);
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(expectedString, finalString);
        }


        [TestMethod]
        public void basic_tell_with_state_iteration_parallel_no_retry()
        {
            const int total = 100000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            Parallel.ForEach( range, new ParallelOptions { MaxDegreeOfParallelism = total }, async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage, stateHandler) =>
                {
                    counter++;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done);
                Assert.IsTrue(result);
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(total, counter);
        }


        [TestMethod]
        public void basic_ask_with_state_iteration_keeping_order()
        {
            const int total = 1000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var expectedString = "";
            range.ForEach(x => expectedString += x);
            var finalString = "";

            Task.WaitAll(range.Select(x => actor.Ask(new MyMessage(x.ToString()), async (myMessage, stateHandler) =>
            {
                finalString += myMessage.Name;
                return await Task.FromResult(new YourMessage("you"));
            })).ToArray());
            Assert.AreEqual(expectedString, finalString);
        }
    }

    public class MyState
    {
        public int StateNumber { set; get; }
    }

    public class MyMessage
    {
        public MyMessage(string name)
        {
            Name = name;
        }

        public string Name { private set; get; }
    }
    public class YourMessage
    {
        public YourMessage(string name)
        {
            Name = name;
        }

        public string Name { private set; get; }
    }

}
