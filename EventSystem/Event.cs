using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem;

/// <summary>
/// 事件管理器
/// </summary>
/// <typeparam name="TSender">事件操作者类型</typeparam>
/// <typeparam name="TEventArgs">事件参数类型</typeparam>
public sealed class Event<TSender, TEventArgs> : IEventSubscriber<TSender, TEventArgs>, IEventPublisher<TSender, TEventArgs>
{
    /// <summary>
    /// 订阅列表锁
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 订阅列表
    /// </summary>
    private readonly List<EventSubjectsInfo> _subjects = new();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public void Subject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs
    {
        lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
        {
            foreach (var subject in _subjects)
            {
                if (subject.Handler.Equals(handler))
                {
                    return;
                }
            }
            _subjects.Add(new EventSubjectsInfo(typeof(TArgs), handler));
        }
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public void UnSubject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs
    {
        lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
        {
            for (int i = _subjects.Count - 1; i >= 0; i--)
            {
                var subject = _subjects[i];
                var ctype = subject.Type;
                var cHandler = subject.Handler;
                if (ctype.IsAssignableFrom(typeof(TArgs)) && cHandler.Equals(handler))
                {
                    _subjects.Remove(subject);
                }
            }
        }
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="args">发布的事件参数对象</param>
    public void Publish<TArgs>(TSender sender, TArgs args) where TArgs : TEventArgs
    {
        lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
        {
            //try
            //{
            foreach (var subject in _subjects)
            {
                var subjectType = subject.Type;
                var subjectHandler = subject.Handler;
                if (subjectType.IsAssignableFrom(args?.GetType()))
                {
                    subjectHandler.DynamicInvoke(sender, args);
                }
            }
            //}
            //catch (Exception)
            //{
            //    // 异常处理，可以弄个异常日志事件，当然也可以不进行处理，直接抛出
            //}
        }
    }

    /// <summary>
    /// 事件是否已经被订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <returns></returns>
    public bool IsSubscribed<TArgs>() where TArgs : TEventArgs
    {
        lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
        {
            foreach (var subject in _subjects)
            {
                if (subject.Type.IsAssignableFrom(typeof(TArgs)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 事件订阅信息（内部使用）
    /// </summary>
    private sealed class EventSubjectsInfo
    {
        /// <summary>
        /// 订阅事件参数类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 订阅处理程序
        /// </summary>
        public Delegate Handler { get; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="type">订阅事件参数类型</param>
        /// <param name="handler">订阅处理程序</param>
        public EventSubjectsInfo(Type type, Delegate handler)
        {
            Type = type;
            Handler = handler;
        }
    }
}