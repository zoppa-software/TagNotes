using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using TagNotes.Helper;
using TagNotes.Models;
using TagNotes.Services;
using ZoppaLoggingExtensions;

namespace TagNotes
{
    // note: 単一インスタンスアプリケーションの実装については以下を参照
    // https://learn.microsoft.com/ja-jp/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance

    // note: アイコン変更
    // https://stackoverflow.com/questions/76662671/how-to-change-remove-window-title-bar-icon-in-winui-3

    // note: 色々
    // https://takusan.negitoro.dev/posts/windows_winui3_installer_and_virtual_desktop/

    /// <summary>エントリプログラム。</summary>
    internal static class Program
    {
        /// <summary>アプリケーション。</summary>
        private static App app;

        /// <summary>エントリポイント。</summary>
        /// <param name="args">引数。</param>
        [STAThread]
        static void Main(string[] args)
        {
            // WinRTの初期化
            WinRT.ComWrappersSupport.InitializeComWrappers();

            // サービスプロバイダの作成
            using var provider = CreateProvider();

            // データベースの初期化
            var db = provider.GetService<DatabaseService>();
            db.Initialize();

            // 通知メッセージサービス起動
            var notif = provider.GetService<NotificationMessageService>();

            // 検索履歴サービス起動
            var search = provider.GetService<SearchHistoryService>();
            search.LoadHistory();

            // アプリケーションの開始
            Microsoft.UI.Xaml.Application.Start((p) => {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                app = new App(provider);
            });

            notif.FinishNotification();
        }

        /// <summary>サービスプロバイダを作成します。</summary>
        /// <returns>サービスプロバイダ。</returns>
        private static ServiceProvider CreateProvider()
        {
            // サービスコレクションの作成
            var services = new ServiceCollection();

            // ログサービスの登録
            services.AddSingleton<ILoggerFactory>(
                (provider) => {
                    var loggerFactory = LoggerFactory.Create(builder => {
                        builder.AddZoppaLogging(
                            (config) => {
                                config.DefaultLogFile = $"{SettingModel.LogFolderPath}\\TagNotes.log";
                                config.MinimumLogLevel = LogLevel.Trace;
                            }
                        );
                        builder.SetMinimumLevel(LogLevel.Trace);
                    });

                    return loggerFactory;
                }
            );

            // データベースサービスの登録
            services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
            services.AddSingleton<DatabaseService>();

            // 通知メッセージサービスの登録
            services.AddSingleton<NotificationMessageService>();

            // 検索履歴サービスの登録
            services.AddSingleton<SearchHistoryService>();

            // モデルの登録
            services.AddTransient<MainWindowModel>();
            services.AddTransient<ListPageModel>();
            services.AddTransient<SettingModel>();
            services.AddTransient<SearchModel>();

            // サービスプロバイダの作成
            return services.BuildServiceProvider();
        }
    }
}
