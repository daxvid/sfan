namespace Sfan.Util;

using System;
using StackExchange.Redis;

public class Cache
{
    static ConnectionMultiplexer? _redis;
    static IDatabase? _db;
    private static readonly object _locker = new object();
    static string _platform = string.Empty;

    public static void Init(string strConn = "localhost", string platform = "")
    {
        _platform = platform;
        lock (_locker)
        {
            _redis = ConnectionMultiplexer.Connect(strConn);
            _db = _redis.GetDatabase();
        }
    }

    public static void SaveBank(string card, string msg)
    {
        var key = "b." + card;
        lock (_locker)
        {
            _db!.StringSet(key, msg, TimeSpan.FromDays(30));
        }
    }

    public static string? GetBank(string card)
    {
        var key = "b." + card;
        lock (_locker)
        {
            string? value = _db!.StringGet(key);
            return value;
        }
    }

    public static void SaveOrder(string orderId, string msg)
    {
        var key = _platform + ".o." + orderId;
        lock (_locker)
        {
            _db!.StringSet(key, msg, TimeSpan.FromDays(3));
        }
    }

    public static string? GetOrder(string orderId)
    {
        var key = _platform + ".o." + orderId;
        lock (_locker)
        {
            string? value = _db!.StringGet(key);
            return value;
        }
    }

    public static void SaveRecharge(string card, string msg)
    {
        var key = _platform + ".r." + card;
        lock (_locker)
        {
            _db!.StringSet(key, msg, TimeSpan.FromDays(60));
        }
    }

    public static string? GetRecharge(string card)
    {
        var key = _platform + ".r." + card;
        lock (_locker)
        {
            string? value = _db!.StringGet(key);
            return value;
        }
    }


    public static void SaveGameBind(string card, string msg)
    {
        var key = _platform + ".gb." + card;
        lock (_locker)
        {
            _db!.StringSet(key, msg, TimeSpan.FromDays(2));
        }
    }

    public static string? GetGameBind(string card)
    {
        var key = _platform + ".gb." + card;
        lock (_locker)
        {
            string? value = _db!.StringGet(key);
            return value;
        }
    }
}
