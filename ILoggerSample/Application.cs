using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ILoggerSample;


public class Application
{
    ILogger logger;
    AppConfig settings;

    // コンストラクタの引数として ILogger や IOptions 型の引数を定義すると、.NET Core の DI 機能によりオブジェクトが注入される
    // (コンストラクタインジェクション)
    // 設定を取得する際には IOptions<T> を経由して DI から値を受け取る
    public Application(ILogger<Application> logger, IOptions<AppConfig> settings)
    {
        this.logger = logger;
        // ここで受け取れるオブジェクトは、オブジェクト自体ではなくアクセサオブジェクトであるため、Value プロパティを参照している
        this.settings = settings.Value;
    }

    public void Run()
    {
        logger.LogCritical("Log Critical");
        logger.LogError("Log Error");
        logger.LogWarning("Log Warning");
        logger.LogInformation("Log Information");
        // 以下の２つはデフォルトでは出力されない
        logger.LogDebug("Log Debug");
        logger.LogTrace("Log Trace");

        try
        {
            logger.LogInformation($"This is a console application for {settings?.Name}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }
}
