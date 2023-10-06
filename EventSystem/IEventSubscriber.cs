namespace EventSystem;

/// <summary>
/// 事件订阅者接口
/// </summary>
/// <typeparam name="TEventSender">事件的操作者对象</typeparam>
/// <typeparam name="TEventArgs">事件的参数对象</typeparam>
public interface IEventSubscriber<TEventSender, TEventArgs>
{
    #region 方法泛型类型（方法泛型是基于接口泛型为基础）

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    IDisposable Subject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs;

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    void UnSubject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs;

    #endregion

    #region 默认类型（基于接口泛型）

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    IDisposable Subject(EventHandler<TEventSender, TEventArgs> handler);

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型</typeparam>
    /// <param name="handler">事件处理程序</param>
    void UnSubject(EventHandler<TEventSender, TEventArgs> handler);

    #endregion
}
