using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;


var sender = new Sender();
var eventManager = new EventManager<Sender>();
eventManager.Subject<EventArgs>((s, e) => { Console.WriteLine("[EventArgs] 操作者：{0} ，事件参数：{1}", s, e); });   // 订阅全部基于EventArgs的事件
eventManager.Subject<EventArgsTest1>((s, e) => { Console.WriteLine("[EventArgsTest1] 操作者：{0} ，事件参数：{1}", s, e); });
eventManager.Subject<EventArgsTest2>((s, e) => { Console.WriteLine("[EventArgsTest2] 操作者：{0} ，事件参数：{1}", s, e); });
eventManager.Subject<EventArgsTest3>((s, e) => { Console.WriteLine("[EventArgsTest3] 操作者：{0} ，事件参数：{1}", s, e); });
eventManager.Publish(sender, new EventArgsTest1());
eventManager.Publish(sender, new EventArgsTest2());
eventManager.Publish(sender, new EventArgsTest3());
Console.ReadLine();
// 运行结果
// [EventArgs] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest1 事件
// [EventArgsTest1] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest1 事件
// [EventArgs] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest3 事件
// [EventArgsTest2] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest3 事件
// [EventArgsTest3] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest3 事件
// [EventArgs] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest2 事件
// [EventArgsTest2] 操作者：这是 Sender 操作者 ，事件参数：这是 EventArgsTest2 事件


public class Sender
{
    public override string ToString()
    {
        return $"这是 {nameof(Sender)} 操作者";
    }
}

public class EventArgsTest1 : EventArgs
{
    public override string ToString()
    {
        return $"这是 {nameof(EventArgsTest1)} 事件";
    }
}

public class EventArgsTest2 : EventArgs
{
    public override string ToString()
    {
        return $"这是 {nameof(EventArgsTest2)} 事件";
    }
}

public class EventArgsTest3 : EventArgsTest2    // 注意：如果是订阅者订阅了 EventArgsTest2 在调用发布 EventArgsTest3 事件后，EventArgsTest2订阅者也会收到事件通知，因为EventArgsTest3继承了EventArgsTest2
{
    public override string ToString()
    {
        return $"这是 {nameof(EventArgsTest3)} 事件";
    }
}


/// <summary>
/// 事件参数
/// </summary>
public class EventArgs
{
    /// <summary>
    /// 事件发送日期
    /// </summary>
    public DateTime EventDateTime { get; } = DateTime.Now;
}

/// <summary>
/// 事件处理程序
/// </summary>
/// <typeparam name="TSender">事件操作者类型</typeparam>
/// <typeparam name="TArgs">事件参数类型</typeparam>
/// <param name="sender">发生事件的操作者</param>
/// <param name="args">发生事件的对象</param>
public delegate void EventHandler<TSender, TArgs>(TSender sender, TArgs args) where TArgs : EventArgs;


/// <summary>
/// 事件管理器
/// </summary>
public sealed class EventManager<TSender>
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
    /// <typeparam name="TEventArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public void Subject<TEventArgs>(EventHandler<TSender, TEventArgs> handler) where TEventArgs : EventArgs
    {
        Task.Factory.StartNew(() =>
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
                _subjects.Add(new EventSubjectsInfo(typeof(TEventArgs), handler));
            }
        });
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TEventArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    public void UnSubject<TEventArgs>(EventHandler<TSender, TEventArgs> handler) where TEventArgs : EventArgs
    {
        Task.Factory.StartNew(() =>
        {
            lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
            {
                for (int i = _subjects.Count - 1; i >= 0; i--)
                {
                    var subject = _subjects[i];
                    var ctype = subject.Type;
                    var cHandler = subject.Handler;
                    if (ctype.IsAssignableFrom(typeof(TEventArgs)) && cHandler.Equals(handler))
                    {
                        _subjects.Remove(subject);
                    }
                }
            }
        });
    }

    /// <summary>
    /// 发布事件（为了更好二次开发利用事件管理器，所以将可访问性设置为公开）
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="args">发布的事件参数对象</param>
    public void Publish<TArgs>(TSender sender, TArgs args) where TArgs : EventArgs
    {
        Task.Factory.StartNew(() =>
        {
            lock (_lock)    // 避免遍历过程中，原列表有订阅或取消订阅移除列表，导致遍历不安全，所以加锁
            {
                try
                {
                    foreach (var subject in _subjects)
                    {
                        var subjectType = subject.Type;
                        var subjectHandler = subject.Handler;
                        if (subjectType.IsAssignableFrom(args.GetType()))
                        {
                            subjectHandler.DynamicInvoke(sender, args);
                        }
                    }
                }
                catch (Exception)
                {
                    // 异常处理，可以弄个异常日志事件，当然也可以不进行处理，直接抛出
                }
            }
        });
    }

    /// <summary>
    /// 事件是否已经被订阅
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    /// <returns></returns>
    internal bool IsSubscribed<TArgs>() where TArgs : EventArgs
    {
        foreach (var subject in _subjects)
        {
            if (subject.Type.IsAssignableFrom(typeof(TArgs)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Konata 事件订阅信息
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


