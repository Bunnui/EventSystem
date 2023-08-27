# EventSystem

这是发布订阅事件的一个简单实现，它支持订阅父类或抽象类进行统一处理

注意：在发布事件后，如果订阅者处理器出现异常，它不会继续通知下一个订阅者处理器，请自行实现异常的处理

## 测试代码

``` C#
using EventSystem;

var bot = new BotClient();

// 订阅全部基于 EventArgs 的事件
bot.MyEvent.Subject<EventArgs>((s, e) => { Console.WriteLine("[标识1] 操作者：{0} ，事件参数：{1}", s, e); });
// 订阅程序运行程序
bot.MyEvent.Subject<RunEvent>((s, e) => { Console.WriteLine("[标识2] 操作者：{0} ，事件参数：{1}", s, e); });
bot.MyEvent.Subject<StopEvent>((s, e) => { Console.WriteLine("[标识3] 操作者：{0} ，事件参数：{1}", s, e); });
// 注意：在订阅了MessageEventArgs，程序在发布FriendMessageEvent、GroupMessageEvent事件同样也会进行通知，
// 因为它可以进行转换到该类型，这种应为抽象类比较合适，这样做可以统一处理基于该类的实现类
bot.MyEvent.Subject<MessageEvent>((s, e) => { Console.WriteLine("[标识4] 操作者：{0} ，事件参数：{1}", s, e); });
bot.MyEvent.Subject<FriendMessageEvent>((s, e) => { Console.WriteLine("[标识5] 操作者：{0} ，事件参数：{1}", s, e); });
bot.MyEvent.Subject<GroupMessageEvent>((s, e) => { Console.WriteLine("[标识6] 操作者：{0} ，事件参数：{1}", s, e); });
// 模拟运行程序
bot.Run();
// 模拟接收到消息
bot.ReceiveMessages();
// 模拟停止程序
bot.Stop();
// 阻塞一下，防止程序被关闭
Console.ReadLine();

namespace EventSystem
{

    public class BotClient
    {
        // 这里只举例一个，可以自行定义其他事件

        /// <summary>
        /// 内部的对象，如果你不希望发布方法暴露出去，请使用接口进行暴露
        /// </summary>
        internal readonly Event<BotClient, EventArgs> _myEvent = new();

        /// <summary>
        /// 这是面向用户的订阅事件接口
        /// </summary>
        public IEventSubscriber<BotClient, EventArgs> MyEvent => _myEvent;


        public void Run()
        {
            _myEvent.Publish(this, new RunEvent());
        }

        public void Stop()
        {
            _myEvent.Publish(this, new StopEvent());
        }

        public void ReceiveMessages()
        {
            // 模拟接收到信息
            _myEvent.Publish(this, new FriendMessageEvent("你好，世界！"));
            _myEvent.Publish(this, new GroupMessageEvent("你好，世界！"));
        }
    }


    public class RunEvent : EventArgs
    {
        public override string ToString()
        {
            return $"程序正在运行中！";
        }
    }

    public class StopEvent : EventArgs
    {
        public override string ToString()
        {
            return $"程序已经被关闭！";
        }
    }


    public class MessageEvent : EventArgs
    {
        public string Type { get; set; } = "未知";
        public string Message { get; set; } = string.Empty;

        public MessageEvent(string type, string message)
        {
            Type = type;
            Message = message;
        }
        public override string ToString()
        {
            return $"接收到 {Type} 消息，内容：{Message}";
        }
    }


    public class FriendMessageEvent : MessageEvent
    {
        public FriendMessageEvent(string message) : base("好友", message) { }
    }

    public class GroupMessageEvent : MessageEvent
    {
        public GroupMessageEvent(string message) : base("群", message) { }
    }
}

```

## 运行结果

```
预期结果：
每个事件有2个输出，消息事件会有3个，是因为有一个是由消息父类（MessageEvent）订阅者发出

运行结果：
[标识1] 操作者：EventSystem.BotClient ，事件参数：程序正在运行中！
[标识2] 操作者：EventSystem.BotClient ，事件参数：程序正在运行中！
[标识1] 操作者：EventSystem.BotClient ，事件参数：接收到 好友 消息，内容：你好，世界！
[标识4] 操作者：EventSystem.BotClient ，事件参数：接收到 好友 消息，内容：你好，世界！
[标识5] 操作者：EventSystem.BotClient ，事件参数：接收到 好友 消息，内容：你好，世界！
[标识1] 操作者：EventSystem.BotClient ，事件参数：接收到 群 消息，内容：你好，世界！
[标识4] 操作者：EventSystem.BotClient ，事件参数：接收到 群 消息，内容：你好，世界！
[标识6] 操作者：EventSystem.BotClient ，事件参数：接收到 群 消息，内容：你好，世界！
[标识1] 操作者：EventSystem.BotClient ，事件参数：程序已经被关闭！
[标识3] 操作者：EventSystem.BotClient ，事件参数：程序已经被关闭！
```
