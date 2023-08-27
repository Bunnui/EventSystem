using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem;

/// <summary>
/// 事件发布者接口
/// </summary>
/// <typeparam name="TSender">事件的操作者对象</typeparam>
/// <typeparam name="TEventArgs">事件的参数对象</typeparam>
public interface IEventSubscriber<TSender, TEventArgs>
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    void Subject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs;

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    void UnSubject<TArgs>(EventHandler<TSender, TArgs> handler) where TArgs : TEventArgs;
}
