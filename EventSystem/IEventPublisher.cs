namespace EventSystem;

/// <summary>
/// 事件发布者接口
/// </summary>
/// <typeparam name="TEventSender">事件的操作者对象</typeparam>
/// <typeparam name="TEventArgs">事件的参数对象</typeparam>
public interface IEventPublisher<TEventSender, TEventArgs>
{
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TArgs">事件的参数类型</typeparam>
    /// <param name="sender">事件的操作者对象</param>
    /// <param name="args">发布的事件参数对象</param>
    public void Publish<TArgs>(TEventSender sender, TArgs args) where TArgs : TEventArgs;
}
