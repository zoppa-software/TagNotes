using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TagNotes.Services;
using ZoppaLoggingExtensions;

namespace TagNotes.Models
{
    /// <summary>メインウィンドウモデル。</summary>
    /// <param name="provider">サービスプロバイダ。</param>
    /// <param name="loggerFactory">ロガーファクトリ。</param>
    /// <param name="dbService">データベースサービス。</param>
    /// <param name="searchHistoryService">検索履歴サービス。</param>
    internal sealed partial class MainWindowModel(IServiceProvider provider,
                                                  ILoggerFactory loggerFactory, 
                                                  DatabaseService dbService,
                                                  SearchHistoryService searchHistoryService) :
        ObservableObject
    {
        /// <summary>サービスプロバイダ。</summary>
        private readonly IServiceProvider provider = provider;

        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<MainWindowModel>();

        /// <summary>データベースサービス。</summary>
        private readonly DatabaseService dbService = dbService;

        /// <summary>検索履歴サービス。</summary>
        private readonly SearchHistoryService searchHistoryService = searchHistoryService;

        /// <summary>最新の検索条件を取得します。</summary>
        public string LatestSearchCondition {
            get {
                return this.searchHistoryService.LatestSearchCondition.Command;
            }
        }

        /// <summary>検索ダイアログを表示して選択、処理します。</summary>
        /// <param name="showDialogSync">ダイアログ表示メソッド。</param>
        /// <param name="selectedPrimaryAction">選択ボタン処理。</param>
        /// <param name="selectedOtherAction">選択ボタン以外の処理。</param>
        /// <returns>非同期タスク。</returns>
        internal async Task SelectBySearchPage(Func<SearchModel, Task<ContentDialogResult>> showDialogSync, 
                                               Action<SearchModel> selectedPrimaryAction, 
                                               Action selectedOtherAction)
        {
            try {
                // 検索ダイアログのデータを取得します
                var data = this.provider.GetService<SearchModel>();

                // 検索ダイアログを表示、操作を処理します
                var result = await showDialogSync(data);
                switch (result) {
                    case ContentDialogResult.Primary:
                        this.logger.ZLog(this).LogInformation("検索実行。条件:{data.SearchCondition}", data.SearchCondition);
                        await this.searchHistoryService.UpdateSearchHistory(data.SearchCondition);
                        selectedPrimaryAction(data);
                        break;

                    default:
                        this.logger.ZLog(this).LogInformation("検索キャンセル");
                        selectedOtherAction();
                        break;
                }
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "メモの検索に失敗しました。");
            }
        }

        /// <summary>メモの追加ダイアログを表示して選択、処理します。</summary>
        /// <param name="title">ダイアログタイトル。</param>
        /// <param name="caption">ダイアログキャプション。</param>
        /// <param name="showDialogSync">ダイアログ表示メソッド。</param>
        /// <param name="selectedPrimaryAction">選択ボタン処理。</param>
        /// <param name="selectedOtherAction">選択ボタン以外の処理。</param>
        /// <returns>非同期タスク。</returns>
        internal async Task SelectByAddPage(string title, 
                                            string caption, 
                                            Func<NoteDialogModel, Task<ContentDialogResult>> showDialogSync, 
                                            Action<NoteDialogModel> selectedPrimaryAction, 
                                            Action selectedOtherAction)
        {
            try {
                // メモの追加ダイアログのデータを取得します
                var data = new NoteDialogModel(0) {
                    Title = title,
                    Note = string.Empty,
                    ActionCaption = caption,
                    NotificationDate = new DateTimeOffset(DateTime.Now.Date),
                    NotificationTime = TimeSpan.Zero,
                    IsEveryDay = false
                };

                // メモの追加ダイアログを表示、操作を処理します
                var result = await showDialogSync(data);
                switch (result) {
                    case ContentDialogResult.Primary:
                        this.logger.ZLog(this).LogInformation("追加実行。メモ:{data.Note}, 通知する:{data.IsNotification}, 毎日通知する:{data.IsEveryDay}, 通知日:{data.NotificationDate}, 通知時刻:{data.NotificationTime}", 
                            data.Note, data.IsNotification, data.IsEveryDay, data.NotificationDate, data.NotificationTime);
                        this.dbService.AddNote(
                            data.Note, 
                            data.IsNotification, 
                            data.IsEveryDay,
                            data.NotificationDate, 
                            data.NotificationTime
                        );
                        selectedPrimaryAction(data);
                        break;

                    default:
                        this.logger.ZLog(this).LogInformation("追加キャンセル");
                        selectedOtherAction();
                        break;
                }
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "メモの追加に失敗しました。");
            }
        }
    }
}
