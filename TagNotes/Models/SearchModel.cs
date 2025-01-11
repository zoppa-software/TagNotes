using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TagNotes.Helper;
using TagNotes.Services;
using TagNotes.Views;
using ZoppaLoggingExtensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TagNotes.Models
{
    /// <summary>検索モデル。</summary>
    /// <param name="loggerFactory">ログ出力機能。</param>
    /// <param name="databaseService">データベースサービス。</param>
    /// <param name="searchHistoryService">検索履歴サービス。</param>
    internal sealed partial class SearchModel(ILoggerFactory loggerFactory,
                                              DatabaseService databaseService,
                                              SearchHistoryService searchHistoryService) :
        ObservableObject
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<SearchModel>();

        /// <summary>データベースサービス。</summary>
        private readonly DatabaseService dbService = databaseService;

        /// <summary>検索履歴サービス。</summary>
        private readonly SearchHistoryService searchHistoryService = searchHistoryService;

        /// <summary>検索ワード。</summary>
        [ObservableProperty]
        private string searchCondition = "";

        /// <summary>タグ文字列。</summary>
        [ObservableProperty]
        private string tagsString = "";

        /// <summary>検索リスト。</summary>
        [ObservableProperty]
        private ObservableCollection<SearchView> searchList = [];

        /// <summary>検索履歴を読み込む。</summary>
        public void LoadHistory()
        {
            try {
                // 検索履歴を読み込む
                var list = this.searchHistoryService.SearchConditionHistory;
                this.logger.ZLog(this).LogInformation("履歴件数:{list.Count}", list.Count);

                // 検索履歴を更新
                this.SearchList.Rewrite(list.Select(v => new SearchView(v.IndexNo, v.Command)));

                // 検索履歴の最新の検索条件を取得
                this.SearchCondition = this.searchHistoryService.LatestSearchCondition.Command;
                this.logger.ZLog(this).LogInformation("最新検索条件:{SearchCondition}", SearchCondition);
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "検索履歴の取得に失敗しました。");
            }
        }

        /// <summary>タグを読み込む。</summary>
        public void LoadTags()
        {
            try {
                // タグを読み込む
                var list = this.dbService.LoadTags();
                this.logger.ZLog(this).LogInformation("タグ件数:{list.Count}", list.Count);

                // 検索履歴の最新の検索条件を取得
                this.TagsString = string.Join(" ", list.Select(v => "#" + v.TagString));
                this.logger.ZLog(this).LogInformation("タグ文字列:{TagsString}", this.TagsString);
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "タグの取得に失敗しました。");
            }
        }

        /// <summary>検索履歴を更新します。</summary>
        internal async Task UpdateSearchHistory()
        {
            try {
                // 検索履歴を更新
                await this.searchHistoryService.UpdateSearchHistory(this.SearchCondition);
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "検索履歴の更新に失敗しました。");
            }
        }
    }
}
