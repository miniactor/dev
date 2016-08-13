using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MiniActor
{
    internal class Mail<TMessage, TState, TResponse>
    {
        
      
        public  async Task MailDelivery(BlockingCollection<MailMessage<TMessage, TState, TResponse>> queue)
        {
            foreach (var workItem in queue.GetConsumingEnumerable())
            {
                if (workItem.CancelToken.HasValue &&
                    workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                {
                    try
                    {
                        var handler=new StateHandler< TState>(workItem.Actor._internalState.State);
                        var result = workItem.Work != null
                            ? await workItem.Work(workItem.Command)
                            : await workItem.WorkWithState(workItem.Command, handler);

                     workItem.Actor._internalState.State=   handler.State;

                        workItem.TaskSource.SetResult(result);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
                }
            }
        }
    }
}