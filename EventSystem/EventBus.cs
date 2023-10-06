using System.Collections.Concurrent;
using System.Reflection;

namespace EventSystem;

/// <summary>
/// 事件总线
/// </summary>
/// <typeparam name="TSender">事件操作者类型</typeparam>
/// <typeparam name="TEventArgs">事件参数类型</typeparam>
public sealed class EventBus<TSender, TEventArgs> : IDisposable, IEventSubscriber<TSender, TEventArgs>, IEventPublisher<TSender, TEventArgs> where TEventArgs : notnull
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();   // 线程安全字典

    /// <inheritdoc/>
    public IDisposable Subject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs
    {
        var type = typeof(TArgs);
        var delegates = _subscribers.GetOrAdd(type, new List<Delegate>());
        if (!delegates.Contains(handler))
        {
            delegates.Add(handler);
        }
        return new EventHandlerDisposable<TArgs>(this, handler);
    }

    /// <inheritdoc/>
    public void UnSubject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs
    {
        Type key = typeof(TArgs);
        _subscribers.AddOrUpdate(key, new List<Delegate>(), (t, list) =>
        {
            list.RemoveAll(@delegate => @delegate.Equals(handler));
            return list;
        });

        if (_subscribers.TryGetValue(key, out var value) && value.Count == 0)
        {
            _subscribers.TryRemove(key, out value);
        }
    }

    /// <inheritdoc/>
    public IDisposable Subject(EventHandler<TSender, TEventArgs> handler) => Subject<TEventArgs>(handler);

    /// <inheritdoc/>
    public void UnSubject(EventHandler<TSender, TEventArgs> handler) => Subject<TEventArgs>(handler);

    /// <inheritdoc/>
    public void Publish<TArgs>(TSender sender, TArgs args) where TArgs : TEventArgs
    {
        foreach (var subscriber in _subscribers)
        {
            var type = subscriber.Key;
            if (type.IsAssignableFrom(args.GetType()))
            {
                var handlers = subscriber.Value;
                foreach (var handler in handlers)
                {
                    //TODO: 这边没有对订阅者处理器进行异常捕获默认处理，请根据不同项目的情况进行调整！
                    handler.DynamicInvoke(sender, args);
                }
            }
        }
    }

    /// <summary>
    /// 事件是否已经被订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <returns></returns>
    public bool IsSubscribed<TArgs>() where TArgs : TEventArgs
    {
        foreach (var subscriber in _subscribers)
        {
            var type = subscriber.Key;
            if (type.IsAssignableFrom(typeof(TArgs)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _subscribers.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 可释放的事件处理程序
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    public sealed class EventHandlerDisposable<TArgs> : IDisposable where TArgs : TEventArgs
    {
        private readonly EventBus<TSender, TEventArgs> _eventBus;
        private readonly EventHandler<TSender, TArgs> _handler;

        public EventHandlerDisposable(EventBus<TSender, TEventArgs> eventBus, EventHandler<TSender, TArgs> handler)
        {
            _eventBus = eventBus;
            _handler = handler;
        }

        public void Dispose()
        {
            _eventBus.UnSubject(_handler);
            GC.SuppressFinalize(this);
        }
    }
}