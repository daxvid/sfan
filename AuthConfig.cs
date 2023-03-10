namespace Sfan;

using YamlDotNet.Serialization;

public class AuthConfig
{
    public string Home { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string GoogleKey { get; set; } = string.Empty;

    // Telegram
    public string BotToken { get; set; } = string.Empty;

    // 所有接收通知的聊天ID
    public List<long> ChatIds = new List<long>();

    // redis连接
    public string Redis { get; set; } = string.Empty;
    
    // # my=meiying, yr=yiren 
    public string Platform { get; set; } = string.Empty;
    
    public static AuthConfig FromYamlFile(string path)
    {
        var yml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var cnf = deserializer.Deserialize<AuthConfig>(yml);
        return cnf;
    }
}