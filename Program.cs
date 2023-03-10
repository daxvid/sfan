namespace Sfan;

using Sfan.Bot;

class Program
{
    static void Main(string[] args)
    {
        var cnf = AppConfig.FromYamlFile("app.yaml");
        var authCnf = AuthConfig.FromYamlFile("auth.yaml");

        TelegramBot.Instance.Run(authCnf);

        while (true)
        {
            using var client = new SfanClient(cnf, authCnf);
            try
            {
                client.Run();
            }
            catch (Exception err)
            {
                client.SaveException(err);
            }
            Thread.Sleep(30 * 1000);
        }
    }
}