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
        public class SomeActor : MiniActor
        {
            //todo how to persist state within an actor
            private string State { set; get; }
            public SomeActor()
            {
               
                Receive<string>(message =>
                {
                    State = State ?? "";
                    State += message;
                });
                Receive<int>(message =>
                {
                    State = State ?? "";
                    State += message;
                });
            }
            public override Superkision Supervision(Exception ex)
            {
                return base.Supervision(ex);
            }
        }

      
        //minimally tested
        [TestMethod]
        public void actor_test()
        {
            var messanger = new MiniActorMessanger();
            messanger.Tell<SomeActor>("message");
            messanger.Tell<SomeActor>(5);

            var messanger1 = new MiniActorMessanger();
            messanger1.Tell<SomeActor>("message");

            var messanger2 = new MiniActorMessanger();
            messanger2.Tell<SomeActor>(5);
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(1000)));
        }

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
        public void basic_ask_no_state()
        {
            var actor = new MiniActor<MyMessage, YourMessage>();
            var ran = 0;
            var task = actor.Ask(new MyMessage("me"), async (myMessage) =>
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
        public void basic_tell_no_state()
        {
            var actor = new MiniActor<MyMessage, YourMessage>();
            var ran = 0;
            var task = actor.Tell(new MyMessage("me"), async (myMessage) =>
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
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(400)));
            Assert.AreEqual(expectedString, finalString);
            actor.Dispose();
        }



        [TestMethod]
        public void basic_tell_no_state_iteration_keeping_order()
        {
            const int total = 500;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var expectedString = "";
            range.ForEach(x => expectedString += x);

            var finalString = "";

            range.ForEach(async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage) =>
                {
                    done = true;
                    finalString += myMessage.Name;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done);
                Assert.IsTrue(result);
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(400)));
            Assert.AreEqual(expectedString, finalString);
        }
        [TestMethod]
        public void basic_tell_no_state_iteration_no_order_with_many_workers()
        {
            Enumerable.Range(2, 10).ToList().ForEach(w =>
            {
                const int total = 500;
                var actor = new MiniActor<MyMessage, MyState, YourMessage>(w);
                var range = Enumerable.Range(1, total).ToList();
                var expectedString = "";
                range.ForEach(x => expectedString += x);

                var finalString = "";

                range.ForEach(async x =>
                {
                    var done = false;
                    var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage) =>
                    {
                        done = true;
                        finalString += myMessage.Name;
                        return await Task.FromResult(new YourMessage("you"));
                    });
                    Assert.IsFalse(done);
                    Assert.IsTrue(result);
                });
                Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(300)));
                Assert.AreNotEqual(expectedString, finalString);
                actor.Dispose();
            });
        }

        [TestMethod]
        public void basic_tell_with_state_iteration_parallel_no_retry()
        {
            const int total = 100000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = total }, async x =>
           {
               var done = false;
               var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage, stateHandler) =>
               {
                   counter++;
                   return await Task.FromResult(new YourMessage("you"));
               });
               Assert.IsFalse(done, "tell is expected to complete before execution");
               Assert.IsTrue(result, "tell is expected to succeed");
           });
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreEqual(total, counter);
        }


        [TestMethod]
        public void basic_tell_with_no_state_iteration_parallel_no_retry2()
        {
            const int total = 100000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = total }, async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage) =>
                {
                    counter++;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done, "tell is expected to complete before execution");
                Assert.IsTrue(result, "tell is expected to succeed");
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreEqual(total, counter);
        }



        [TestMethod]
        public void basic_tell_with_no_state_iteration_parallel_no_retry3_test_collision()
        {
            const int total = 100;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>();
            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            bool isWorking = false;
            Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = total }, async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage) =>
                {
                    if (isWorking)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        isWorking = true;
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(5));
                    counter++;
                    isWorking = false;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done, "tell is expected to complete before execution");
                Assert.IsTrue(result, "tell is expected to succeed");
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(total, counter);
        }



        [TestMethod]
        public void basic_tell_with_no_state_iteration_parallel_no_retry2_supervised()
        {
            var exceptions = new List<Exception>();
            const int total = 1000;
            var actor = new MiniActor<MyMessage, MyState, YourMessage>(
                (exception) =>
                {
                    exceptions.Add(exception);
                    return new Superkision(SupervisionStrategy.Retry, 10, TimeSpan.FromMilliseconds(1),
                        RetryBackOffType.Linear);
                });
            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = total }, async x =>
            {
                var done = false;
                var result = await actor.Tell(new MyMessage(x.ToString()), async (myMessage, handler) =>
                {
                    var state = handler.GetState();
                    if (state == null)
                    {
                        handler.SetState(new MyState());
                        throw new Exception();
                    }
                    counter++;
                    return await Task.FromResult(new YourMessage("you"));
                });
                Assert.IsFalse(done, "tell is expected to complete before execution");
                Assert.IsTrue(result, "tell is expected to succeed");
            });
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreEqual(total, counter);
            Assert.AreEqual(1, exceptions.Count);
        }
        //becomes eventually consistent
        [TestMethod]
        public void basic_tell_with_no_state_iteration_parallel_no_retry2_supervised2()
        {
            var exceptions = new List<Exception>();
            const int total = 4;

            var range = Enumerable.Range(1, total).ToList();
            var counter = 0;
            Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = total }, x =>
           {
               var done = false;
               var actor = new MiniActor<MyMessage, MyState, YourMessage>(
              (exception) =>
              {
                  exceptions.Add(exception);
                  return new Superkision(SupervisionStrategy.Retry, 10, TimeSpan.FromMilliseconds(1),
                      RetryBackOffType.Linear);
              });
               var result = actor.Tell(new MyMessage(x.ToString()), async (myMessage, handler) =>
               {
                   var state = handler.GetState();
                   if (state == null)
                   {
                       handler.SetState(new MyState());
                       throw new Exception();
                   }
                   counter++;
                   return await Task.FromResult(new YourMessage("you"));
               });
               Assert.IsFalse(done, "tell is expected to complete before execution");
               Task.WaitAll(result);
               Assert.IsTrue(result.Result, "tell is expected to succeed");
           });
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(5)));
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(total, counter);
            Assert.AreEqual(total, exceptions.Count);
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
}
