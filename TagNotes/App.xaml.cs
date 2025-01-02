using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using TagNotes.Helper;
using Microsoft.Windows.AppLifecycle;
using Microsoft.UI.Dispatching;
using System.Diagnostics;

// note: アプリ通知の処理については以下を参照
// https://learn.microsoft.com/ja-jp/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/toast-notifications?tabs=appsdk

namespace TagNotes
{
    /// <summary>プリケーションクラスです。</summary>
    /// <remarks>
    /// デフォルトのアプリケーションクラスを補完するために、アプリケーション固有の動作を提供します。
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>サービスプロバイダーを取得します。</summary>
        internal ServiceProvider Provider { get; private set; }

        /// <summary>コンストラクタ。</summary>
        /// <remarks>
        /// シングルトンアプリケーションオブジェクトを初期化します。
        /// これは、作成されたコードの中で最初に実行される行であり、論理的には main() または WinMain() に相当します。
        /// </remarks>
        public App(ServiceProvider provider)
        {
            this.InitializeComponent();

            // サービスプロバイダーを設定します
            this.Provider = provider;
        }

        /// <summary>アプリケーションが起動されたときに呼び出されます。</summary>
        /// <param name="args">起動リクエストとプロセスの詳細。</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // メインウィンドウを生成します
            this.m_window = new MainWindow();

            // 通知イベントを受信するようにアプリを登録します
            AppNotificationManager notificationManager = AppNotificationManager.Default;
            notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;
            notificationManager.Register();

            // ウィンドウの起動/アクティブ化コードを専用の LaunchAndBringToForegroundIfNeeded ヘルパー メソッドにリファクタリングして、
            // 複数の場所から呼び出すことができるようにします
            var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            var activationKind = activatedArgs.Kind;
            if (activationKind != ExtendedActivationKind.AppNotification) {
                LaunchAndBringToForegroundIfNeeded();
            }
            else {
                HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);
            }
        }

        /// <summary>ウィンドウの起動/アクティブ化コードを実行します。</summary>
        private void LaunchAndBringToForegroundIfNeeded()
        {
            if (m_window == null) {
                // ウィンドウがまだ作成されていない場合は、作成してアクティブ化します
                m_window = new MainWindow();
                m_window.Activate();

                // さらに、アプリの通知を通じてアクティベートされた場合、
                // ウィンドウが正しくアクティベートされないため、ヘルパーを実行します。
                WindowHelper.ShowWindow(m_window);
            }
            else {
                // ウィンドウが既に作成されている場合は、前面に表示します
                WindowHelper.ShowWindow(m_window);
            }
        }

        /// <summary>通知イベント処理ハンドラです。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private void NotificationManager_NotificationInvoked(AppNotificationManager sender, 
                                                             AppNotificationActivatedEventArgs args)
        {
            HandleNotification(args);
        }

        /// <summary>通知を処理します。</summary>
        /// <param name="args">イベントオブジェクト。</param>
        private void HandleNotification(AppNotificationActivatedEventArgs args)
        {
            // ウィンドウにディスパッチャーが存在する場合はそれを使用し、
            // そうでない場合はアプリのディスパッチャーを使用する。
            var dispatcherQueue = m_window?.DispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            // ディスパッチャーに処理をキューイングします
            dispatcherQueue.TryEnqueue(
                delegate {
                    switch (args.Arguments["action"]) {
                        case "sendMessage":
                            // バックグラウンドメッセージを送信する
                            string message = args.UserInput["textBox"].ToString();

                            // UIアプリが開いていない場合、完了したので閉じる
                            if (m_window == null) {
                                Process.GetCurrentProcess().Kill();
                            }
                            break;

                        case "viewMessage":
                            // 表示メッセージを送信する
                            // ウィンドウを前面に表示/前面に持ってくる
                            LaunchAndBringToForegroundIfNeeded();
                            break;
                }
            });
        }

        /// <summary>メインウィンドウ。</summary>
        private Window m_window;
    }
}
