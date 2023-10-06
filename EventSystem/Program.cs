using EventSystem;

var bot = new BotClient();
// 订阅父类事件 - 全局基类
var cancel1 = bot.MyEvent.Subject<EventArgs>((s, e) => { Console.WriteLine("[标识1] [操作者]：{0} [事件参数]：{1}", s, e); });
// 订阅父类事件 - 消息基类（这是继承自EventArgs类，所以EventArgs订阅者也会输出该内容）
var cancel2 = bot.MyEvent.Subject<MessageEvent>((s, e) => { Console.WriteLine("[标识2] [操作者]：{0} [事件参数]：{1}", s, e); });
// 订阅子类事件
var cancel3 = bot.MyEvent.Subject<RunEvent>((s, e) => { Console.WriteLine("[标识3] [操作者]：{0} [事件参数]：{1}", s, e); });
var cancel4 = bot.MyEvent.Subject<StopEvent>((s, e) => { Console.WriteLine("[标识4] [操作者]：{0} [事件参数]：{1}", s, e); });
var cancel5 = bot.MyEvent.Subject<FriendMessageEvent>((s, e) => { Console.WriteLine("[标识5] [操作者]：{0} [事件参数]：{1}", s, e); });
var cancel6 = bot.MyEvent.Subject<GroupMessageEvent>((s, e) => { Console.WriteLine("[标识6] [操作者]：{0} [事件参数]：{1}", s, e); });

// 模拟触发事件
{
    bot.Run();
    bot.ReceiveMessages();
    bot.Stop();
}

// 取消事件测试
{
    Console.WriteLine();
    Console.WriteLine("中途取消事件测试");

    cancel1.Dispose();
    cancel2.Dispose();

    cancel3.Dispose();
    cancel4.Dispose();
    cancel5.Dispose();
    cancel6.Dispose();

    // 模拟触发事件
    bot.Run();
    bot.ReceiveMessages();
    bot.Stop();
}

Console.ReadLine();


namespace EventSystem
{
    public class BotClient
    {
        /// <summary>
        /// 内部的对象，如果你不希望发布方法暴露出去，请使用接口进行暴露
        /// </summary>
        internal readonly EventBus<BotClient, EventArgs> _myEvent = new();

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
            _myEvent.Publish(this, new FriendMessageEvent("你好，世界！"));
            _myEvent.Publish(this, new GroupMessageEvent("你好，世界！"));
        }

        public override string ToString()
        {
            return "这是机器人客户端";
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
