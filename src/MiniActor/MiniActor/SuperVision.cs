using System;

namespace MiniActor
{
    public class SuperVision
    {
        public SuperVision( SuperVisionStrategy superVisionStrategy= SuperVisionStrategy.Fail, int maxRetryCount=0, TimeSpan? retryBackOffInterval=null, RetryBackOffType retryBackOffType= RetryBackOffType.Exponential)
        {
            RetryBackOffInterval = retryBackOffInterval?? TimeSpan.FromMilliseconds(500);
            RetryBackOffType = retryBackOffType;
            SuperVisionStrategy = superVisionStrategy;
            MaxRetryCount = maxRetryCount;
        }

        public SuperVisionStrategy SuperVisionStrategy { private set; get; }
        public int MaxRetryCount { private set; get; }
        public TimeSpan RetryBackOffInterval { get; private set; }
        public  RetryBackOffType RetryBackOffType { get; private set; }
    }
}