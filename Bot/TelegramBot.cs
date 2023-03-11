namespace Sfan.Bot;

using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Sfan.Util;

public class TelegramBot
{
    private static readonly TelegramBot instance = new TelegramBot();

    /// <summary>
    /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
    /// </summary>
    static TelegramBot()
    {
    }

    private TelegramBot()
    {
    }

    public static TelegramBot Instance
    {
        get { return instance; }
    }

    AuthConfig? config;
    BotClient? api;

    public void Run(AuthConfig cnf)
    {
        this.config = cnf;
        var client = new BotClient(cnf.BotToken);
        this.api = client;
        ThreadPool.QueueUserWorkItem(state =>
        {
            SendMessage("start bot:" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
            while (true)
            {
                try
                {
                    Update(client);
                }
                catch (Exception err)
                {
                    Log.SaveException(err, null, "bot_");
                    try
                    {
                        client.Close();
                        Thread.Sleep(10000);
                        client = new BotClient(cnf.BotToken);
                        this.api = client;
                        //SendMessage("restart bot:" + DateTime.Now.ToString("yy-MM-dd HH:mm:ss"));
                    }
                    catch
                    {
                    }
                }
            }
        });
    }

    static void Update(BotClient client)
    {
        var updates = client.GetUpdates();
        while (true)
        {
            if (updates.Any())
            {
                foreach (var update in updates)
                {
                    if (update.Message != null && update.Message.Chat != null)
                    {
                        long chatId = update.Message.Chat.Id; // Target chat Id
                        try
                        {
                            client.SendMessage(chatId, "ok" + chatId.ToString());
                        }
                        catch
                        {
                        }
                    }
                }

                var offset = updates.Last().UpdateId + 1;
                updates = client.GetUpdates(offset);
            }
            else
            {
                updates = client.GetUpdates();
                Thread.Sleep(1000);
            }
        }
    }

    private void SendMessage(string msg)
    {
        if (api == null || config == null)
        {
            return;
        }

        lock (api)
        {
            foreach (var charId in config.ChatIds)
            {
                api.SendMessage(charId, msg);
            }
        }
    }

    public static void SendMsg(string msg)
    {
        lock (instance)
        {
            try
            {
                instance.SendMessage(msg);
            }
            catch
            {
            }
        }
    }
}
