﻿using System;
using System.Threading;
using Hangfire.Server;
using StackExchange.Redis;

namespace Hangfire.Redis
{
    internal class RedisSubscription : IServerComponent
    {
        internal static string Channel = string.Format("{0}JobFetchChannel", RedisStorage.Prefix);

        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private readonly ISubscriber _subscriber;
        
        public RedisSubscription(ISubscriber subscriber)
        {
            _subscriber = subscriber;
            _subscriber.Subscribe(Channel, (channel, value) => _mre.Set());
        }
        
                
        public void WaitForJob(TimeSpan timeout, CancellationToken cancellationToken)
        {
            _mre.Reset();
            WaitHandle.WaitAny(new[] { _mre, cancellationToken.WaitHandle }, timeout);
        }
        
        void IServerComponent.Execute(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _subscriber.Unsubscribe(Channel);
                _mre.Dispose();
            }
        }
    }
}
