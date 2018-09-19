﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveAgent.Agents.Dataflow;

namespace ReactiveAgent.Agents
{
    public interface IAgent<TMessage>
    {
        Task Send(TMessage message);
        void Post(TMessage message);

        // TODO :
        //      Add an AsObservable property to access the IObservable<'R>
        //      The type 'R is the result of a projection over the type TMessage
    }

    public interface IReplyAgent<TMessage, TReply> : IAgent<TMessage>
    {
        Task<TReply> Ask(TMessage message);
    }

    public static class Agent
    {
        public static IAgent<TMessage> Start<TMessage>(Action<TMessage> action, CancellationTokenSource cts = null)
            => new StatelessDataflowAgent<TMessage>(action, cts);
        public static IAgent<TMessage> Start<TMessage>(Func<TMessage, Task> action, CancellationTokenSource cts = null)
            => new StatelessDataflowAgent<TMessage>(action, cts);

        public static IAgent<TMessage> Start<TState, TMessage>(TState initialState, Func<TState, TMessage, Task<TState>> action, CancellationTokenSource cts = null)
            => new StatefulDataflowAgent<TState, TMessage>(initialState, action, cts);
        public static IAgent<TMessage> Start<TState, TMessage>(TState initialState, Func<TState, TMessage, TState> action, CancellationTokenSource cts = null)
            => new StatefulDataflowAgent<TState, TMessage>(initialState, action, cts);

        public static IReplyAgent<TMessage, TReply> Start<TState, TMessage, TReply>(TState initialState, Func<TState, TMessage, Task<TState>> projection, Func<TState, TMessage, Task<(TState, TReply)>> ask, CancellationTokenSource cts = null)
            => new StatefulReplyDataflowAgent<TState, TMessage, TReply>(initialState, projection, ask, cts);
        public static IReplyAgent<TMessage, TReply> Start<TState, TMessage, TReply>(TState initialState, Func<TState, TMessage, TState> projection, Func<TState, TMessage, (TState, TReply)> ask, CancellationTokenSource cts = null)
            => new StatefulReplyDataflowAgent<TState, TMessage, TReply>(initialState, projection, ask, cts);

        public static IReplyAgent<TMessage, TReply> Start<TMessage, TReply>(Func<TMessage, TReply> ask, CancellationTokenSource cts = null)
            => new StatefulReplyDataflowAgent<int, TMessage, TReply>(0, (cnt, msg) => cnt + 1, (cnt, msg) => (cnt + 1, ask(msg)), cts);
    }
}
