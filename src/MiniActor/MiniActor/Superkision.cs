using System;

namespace MiniActor
{
    public class Superkision
    {
        public Superkision( SupervisionStrategy supervisionStrategy= SupervisionStrategy.Fail, int maxRetryCount=0, TimeSpan? retryBackOffInterval=null, RetryBackOffType retryBackOffType= RetryBackOffType.Exponential)
        {
            RetryBackOffInterval = retryBackOffInterval?? TimeSpan.FromMilliseconds(500);
            RetryBackOffType = retryBackOffType;
            SupervisionStrategy = supervisionStrategy;
            MaxRetryCount = maxRetryCount;
        }

        public SupervisionStrategy SupervisionStrategy { private set; get; }
        public int MaxRetryCount { private set; get; }
        public TimeSpan RetryBackOffInterval { get; private set; }
        public  RetryBackOffType RetryBackOffType { get; private set; }
     
    }
}