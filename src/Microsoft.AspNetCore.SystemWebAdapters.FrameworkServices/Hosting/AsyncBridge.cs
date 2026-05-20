// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

internal static class AsyncBridge
{
    public static void Run(Func<Task> asyncMethod)
    {
        if (asyncMethod is null)
        {
            throw new ArgumentNullException(nameof(asyncMethod));
        }

        var previousContext = SynchronizationContext.Current;
        using var context = new SingleThreadSynchronizationContext();

        try
        {
            SynchronizationContext.SetSynchronizationContext(context);

            var task = asyncMethod() ?? throw new InvalidOperationException("No task provided.");
            task.ContinueWith(_ => context.Complete(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            context.RunOnCurrentThread();
            task.GetAwaiter().GetResult();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object?>> _queue = new();

        public override void Post(SendOrPostCallback d, object? state)
        {
            if (d is null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            try
            {
                if (_queue.TryAdd(new KeyValuePair<SendOrPostCallback, object?>(d, state)))
                {
                    return;
                }
            }
            catch (InvalidOperationException)
            {
            }

            ThreadPool.QueueUserWorkItem(s => d(s), state);
        }

        public override void Send(SendOrPostCallback d, object? state)
            => throw new NotSupportedException("Synchronously sending is not supported.");

        public void RunOnCurrentThread()
        {
            foreach (var workItem in _queue.GetConsumingEnumerable())
            {
                workItem.Key(workItem.Value);
            }
        }

        public void Complete() => _queue.CompleteAdding();

        public void Dispose() => _queue.Dispose();
    }
}
