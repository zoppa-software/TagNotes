using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TagNotes.Services;
using System.Collections.ObjectModel;
using TagNotes.Views;
using TagNotes.Helper;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using ZoppaLoggingExtensions;

namespace TagNotes.Models
{
    /// <summary>リストページモデル。</summary>
    /// <param name="loggerFactory">ロガーファクトリ。</param>
    /// <param name="dbService">データベースサービス。</param>
    /// <param name="searchHistoryService">検索履歴サービス。</param>
    internal sealed partial class ListPageModel(ILoggerFactory loggerFactory, 
                                                DatabaseService dbService, 
                                                SearchHistoryService searchHistoryService) :
        ObservableObject
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<ListPageModel>();

        /// <summary>データベースサービス。</summary>
        private readonly DatabaseService dbService = dbService;

        /// <summary>検索履歴サービス。</summary>
        private readonly SearchHistoryService searchHistoryService = searchHistoryService;

        /// <summary>メモリスト。</summary>
        [ObservableProperty]
        private ObservableCollection<NoteView> noteList = [];

        /// <summary>リストを読み込みます。</summary>
        /// <param name="condition">検索条件。</param>
        /// <param name="navigationMode">ナビゲーションモード。</param>
        /// <returns>通知メッセージリスト。</returns>
        public async Task LoadList(string condition, NavigationMode navigationMode)
        {
            try {
                // 検索履歴を更新
                if (navigationMode == NavigationMode.Back) {
                    this.logger.ZLog(this).LogInformation("検索履歴を更新。{condition}", condition);
                    await this.searchHistoryService.UpdateSearchHistory(condition);
                }

                // リストを読み込む
                this.logger.ZLog(this).LogInformation("リストを読み込みます。");
                var condItems = condition.ParseCondition();
                var list = await this.dbService.GetNotesSync(condItems.SearchWords, condItems.SearchTags, condItems.NotSearchTags);
                this.logger.ZLog(this).LogInformation("リストの件数:{list.Count}", list.Count);

                // リストを更新
                this.NoteList.Rewrite(
                    list.Select(v => new NoteView(v.Index, v.Information, v.NotificationTime, v.IsEveryDay, v.UpdateTime, v.Tags))
                );
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "メモの取得に失敗しました。");
            }
        }

        /// <summary>メモの編集ダイアログを表示して選択、処理します。</summary>
        /// <param name="item">編集対象メモ。</param>
        /// <param name="title">ダイアログタイトル。</param>
        /// <param name="caption">ダイアログキャプション。</param>
        /// <param name="showDialogSync">ダイアログ表示メソッド。</param>
        /// <param name="selectedPrimaryAction">選択ボタン処理。</param>
        /// <param name="selectedSecondaryAction">削除ボタン処理。</param>
        /// <param name="selectedOtherAction">選択ボタン以外の処理。</param>
        /// <returns>非同期タスク。</returns>
        internal async Task EditSelectNote(NoteView item,
                                           string title,
                                           string caption,
                                           Func<NoteDialogModel, Task<ContentDialogResult>> showDialogSync,
                                           Func<NoteDialogModel, Task> selectedPrimaryAction,
                                           Func<NoteDialogModel, Task> selectedSecondaryAction,
                                           Action selectedOtherAction)
        {
            try {
                // メモの編集ダイアログのデータを取得します
                var nofTime = item.NotificationTime ?? DateTime.Now;
                var data = new NoteDialogModel(item.Index) {
                    Title = title,
                    Note = item.Information,
                    ActionCaption = caption,
                    IsNotification = (item.NotificationTime != null),
                    NotificationDate = new DateTimeOffset(nofTime.Date),
                    NotificationTime = nofTime.TimeOfDay,
                    IsEveryDay = item.IsEveryDay,
                    UseDeleteButton = true
                };

                // メモの編集ダイアログを表示、操作を処理します
                var result = await showDialogSync(data);
                switch (result) {
                    case ContentDialogResult.Primary:
                        this.logger.ZLog(this).LogInformation("追加実行。id:{data.Id}, メモ:{data.Note}, 通知する:{data.IsNotification}, 毎日通知する:{data.IsEveryDay}, 通知日:{data.NotificationDate}, 通知時刻:{data.NotificationTime}",
                            data.Id, data.Note, data.IsNotification, data.IsEveryDay, data.NotificationDate, data.NotificationTime);
                        this.dbService.EditNote(
                            data.Id, 
                            data.Note, 
                            data.IsNotification, 
                            data.IsEveryDay, 
                            data.NotificationDate, 
                            data.NotificationTime
                        );
                        await selectedPrimaryAction(data);
                        break;

                    case ContentDialogResult.Secondary:
                        this.logger.ZLog(this).LogInformation("削除実行。id:{data.Id}", data.Id);
                        this.dbService.DeleteNote(data.Id);
                        await selectedSecondaryAction(data);
                        break;

                    default:
                        selectedOtherAction();
                        break;
                }
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "メモの編集に失敗しました。{ex.Message}", ex.Message);
            }
        }
    }
}
