using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;

namespace ILoggerSample;


// https://stackoverflow.com/questions/38706959/net-core-console-applicatin-configuration-xml
// https://kuttsun.blogspot.com/2017/09/net-core_20.html

internal class Program
{

    static void Main(string[] args)
    {
        // DI を使った Application クラスを例として挙げる

        // IServiceCollection に対してフレームワークが提供する拡張メソッド を使いながら依存性を定義してゆき、
        // ActivatorUtilities などを使って依存関係が解決されたインスタンスを取得するのが大まかな流れ
        // add メソッドでサービスを追加すると、DI として構成される
        IServiceCollection serviceCollection = new ServiceCollection();

        // ConfigureServices で DI の準備を行う
        ConfigureServices(serviceCollection);

        // var app = new Application(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        // DI サービスコンテナから指定した型のサービスを取得する
        var app = serviceProvider.GetService<Application>();

        // 実行
        app?.Run();
        Console.ReadKey();
    }


    private static void ConfigureServices(IServiceCollection services)
    {
        // ロギングの設定
        NLog.LogManager.LoadConfiguration("NLog.config");
        ILoggerFactory loggerFactory = new NLogLoggerFactory();
        
        // DI サービスコンテナに Singleton ライフサイクルにてオブジェクトを登録する
        // Singleton ライフサイクルでは Dependency インスタンスを一つ生成し、そのインスタンスをアプリケーションで共有する
        services.AddSingleton(loggerFactory);
        // AddLogging メソッドを呼び出すことで ILoggerFactory と ILogger<T> が DI 経由で扱えるようになる
        services.AddLogging();

        // IConfigurationBuilder で設定を選択
        // IConfigurationBuilder.Build() で設定情報を確定し、IConfigurationRoot を生成する
        IConfigurationRoot configuration = new ConfigurationBuilder()
            // 基準となるパスを設定
            .SetBasePath(Directory.GetCurrentDirectory())
            // ここでどの設定元を使うか指定
            // 同じキーが設定されている場合、後にAddしたものが優先される
            .AddJsonFile(path: "appsettings.json", optional: true)
            // ここでは JSON より環境変数を優先している
            .AddEnvironmentVariables(prefix: "DOTNET_")
            // 上記の設定を実際に適用して構成読み込み用のオブジェクトを得る
            .AddUserSecrets<Program>(optional: true)
            .Build();

        // Logger と同じく DI サービスコンテナに Singleton ライフサイクルにてオブジェクトを登録する
        services.AddSingleton(configuration);

        // オプションパターンを有効にすることで、構成ファイルに記述した階層構造データを POCO オブジェクトに読み込めるようにする
        services.AddOptions();

        // Configure<T> を使ってオプションを初期化する
        // IConfigurationRoot から GetSection 及び GetChildren で個々の設定の取り出しができる
        // ここでは "MyOptions" セクションの内容を MyOptions として登録
        services.Configure<AppConfig>(configuration.GetSection("appConfig"));

        // Application を DI サービスコンテナに登録する
        // AddTransient はインジェクション毎にインスタンスが生成される
        services.AddTransient<Application>();
    }


}






