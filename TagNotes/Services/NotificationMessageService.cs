using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TagNotes.Services
{
    /// <summary>通知メッセージサービス。</summary>
    internal sealed class NotificationMessageService
    {
        /// <summary>ログ出力サービス。</summary>
        private readonly ILogger logger;

        /// <summary>データベースサービス。</summary>
        private readonly DatabaseService dbService;

        /// <summary>監視フラグ。</summary>
        private bool observed = false;

        /// <summary>コンストラクタ。</summary>
        /// <param name="loggerFactory">ログファクトリー。</param>
        /// <param name="dbService">データベースサービス。</param>
        public NotificationMessageService(ILoggerFactory loggerFactory, DatabaseService dbService)
        {
            // ログ出力サービスとデータベースサービスを保持
            this.logger = loggerFactory.CreateLogger<NotificationMessageService>();
            this.dbService = dbService;

            // 通知メッセージの監視を開始
            this.logger.LogInformation("create notification message service");
            Task.Run(() => {
                this.observed = true;
                this.Observe();
            });
        }

        /// <summary>通知メッセージの監視を行います。</summary>
        private void Observe()
        {
            while (true) {
                // 通知メッセージを取得
                List<DatabaseService.Notification> messages = null;
                try {
                    messages = this.dbService.GetNotificationMessages();
                }
                catch (Exception ex) {
                    this.logger.LogError(ex, "通知メッセージの取得に失敗しました。");
                }

                // 通知メッセージを表示
                foreach (var group in messages.Where(x => x.Timing <= DateTime.Now).GroupBy(x => x.Index)) {
                    var toastMsg = group.OrderByDescending(x => x.Timing).First();
                    try {
                        this.logger.LogInformation("show notification message : {toastMsg.message}", toastMsg.Message);

                        // 通知メッセージを表示
                        ShowToast(toastMsg.Timing, toastMsg.Message, toastMsg.NotificationTime);

                        // 通知メッセージを削除
                        switch(toastMsg.EveryDay) {
                            case 1:
                                this.dbService.UpdateNotificationMessage(toastMsg);
                                break;
                            default:
                                this.dbService.DeleteNotificationMessage(group.ToList());
                                break;
                        }
                    }
                    catch (Exception ex) {
                        this.logger.LogError(ex, "通知メッセージの表示に失敗しました。");
                    }
                }

                // 15秒待機
                for (int i = 0; i < 3; ++i) {
                    Task.Delay(5 * 1000).Wait();
                    if (!this.observed) {
                        return;
                    }
                }
            }
        }

        /// <summary>通知メッセージを表示します。</summary>
        /// <param name="timing">表示タイミング。</param>
        /// <param name="information">通知メッセージ。</param>
        /// <param name="notificationTime">通知時間。</param>
        private static void ShowToast(DateTime timing, string information, DateTime? notificationTime)
        {
            // メッセージを取得する
            var resourceLoader = new ResourceLoader();
            string msgTmplate = resourceLoader.GetString("AGO_TEMPLATE");
            string message;

            // メッセージを組み立てる
            var subspan = notificationTime?.Subtract(timing) ?? TimeSpan.MaxValue;
            if (subspan.TotalMinutes < 5) {
                message = resourceLoader.GetString("NOW_NOTIF");
            }
            else if (subspan.TotalMinutes < 15) {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_5M"));
            }
            else if (subspan.TotalMinutes < 30) {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_15M"));
            }
            else if (subspan.TotalHours < 1) {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_30M"));
            }
            else if (subspan.TotalHours < 4) {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_1H"));
            }
            else if (subspan.TotalDays < 1) {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_4H"));
            }
            else {
                message = string.Format(msgTmplate, resourceLoader.GetString("AGO_1D"));
            }

            // 通知を表示する
            var msg = information.Length > 40 ?
                        string.Concat(information.AsSpan(0, 40), "...") :
                        information;
            var toast = new AppNotificationBuilder()
                .AddText(message, new AppNotificationTextProperties().SetMaxLines(1))
                .AddText(msg);
            AppNotificationManager.Default.Show(toast.BuildNotification());
        }

        /// <summary>通知メッセージの監視を終了します。</summary>
        public void FinishNotification()
        {
            this.observed = false;
        }
    }
}
