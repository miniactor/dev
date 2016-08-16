using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MiniActor
{
    internal class Mail<TMessage, TResponse> : Mail<TMessage, TResponse, object>
    {

    }
    internal class Mail<TMessage, TState, TResponse>
    {
        public async Task MailDelivery(BlockingCollection<MailMessage<TMessage, TState, TResponse>> queue)
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
                    var retryCount = 0;
                    var handler = new StateHandler<TState>(workItem.Actor.StateInternal.State);
                    try
                    {

                        Func<Task<bool>> a = async () =>
                        {
                            var result = workItem.Work != null
                           ? await workItem.Work(workItem.Command)
                           : await workItem.WorkWithState(workItem.Command, handler);
                            workItem.TaskSource.SetResult(result);

                            return await Task.FromResult(true);
                        };

                        Exception e = null;
                        Supervision supervision=null;
                        do
                        {
                            if (e != null )
                            {
                                if (supervision.RetryBackOffType == RetryBackOffType.Linear)
                                {
                                    await Task.Delay(supervision.RetryBackOffInterval);
                                }
                                else
                                {
                                    for (var i = 0; i < retryCount; i++)
                                    {
                                        await Task.Delay(supervision.RetryBackOffInterval);
                                    }
                                }
                            }
                            e = null;
                            try
                            {
                                var result = await a();
                            }
                            catch (Exception ex)
                            {
                                e = ex;
                                retryCount++;
                                supervision = workItem.Actor.SuperVision(ex)?? new Supervision();
                            }
                           
                           
                           
                        } while (
                        e != null
                        &&supervision.SupervisionStrategy == SupervisionStrategy.Retry
                        && (retryCount <= supervision.MaxRetryCount)
                        );

                        if (e != null &&supervision.SupervisionStrategy == SupervisionStrategy.IgnoreFailure)
                        {
                            throw e;
                        }



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
                    workItem.Actor.StateInternal.State = handler.State;
                }
            }
        }
    }
}