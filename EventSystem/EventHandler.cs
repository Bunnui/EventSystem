using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem;

/// <summary>
/// 事件处理程序
/// </summary>
/// <typeparam name="TSender">事件操作者类型</typeparam>
/// <typeparam name="TEventArgs">事件参数类型</typeparam>
/// <param name="sender">发生事件的操作者</param>
/// <param name="args">发生事件的对象</param>
public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs args);