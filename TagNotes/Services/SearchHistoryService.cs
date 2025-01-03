using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagNotes.Helper;
using TagNotes.Views;
using ZoppaLoggingExtensions;

namespace TagNotes.Services
{
    /// <summary>検索履歴サービス。</summary>
    internal sealed class SearchHistoryService(ILoggerFactory loggerFactory,
                                               DatabaseService databaseService)
    {
        /// <summary>検索履歴の最大数。</summary>
        private const int HISTORY_MAX = 10;

        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<SearchHistoryService>();

        /// <summary>データベースサービス。</summary>
        private readonly DatabaseService dbService = databaseService;

        /// <summary>検索履歴リスト。</summary>
        private readonly List<SearchView> searchHistory = [];

        /// <summary>最新の検索条件を取得します。</summary>
        public SearchView LatestSearchCondition {
            get {
                return this.searchHistory.Count > 0 ? this.searchHistory[0] : new SearchView(0, "");
            }
        }

        /// <summary>検索履歴リストを取得します。</summary>
        public List<SearchView> SearchConditionHistory {
            get {
                return this.searchHistory.Where(v => v.Command != "").ToList();
            }
        }

        /// <summary>検索履歴を読み込む。</summary>
        /// <returns>非同期タスク。</returns>
        public void LoadHistory()
        {
            try {
                // 検索履歴を読み込む
                this.logger.ZLog(this).LogInformation("検索履歴を読み込みます。");
                var list = this.dbService.GetHistories();
                this.logger.ZLog(this).LogInformation("履歴の件数:{list.Count}", list.Count);

                // 検索履歴を更新
                this.searchHistory.Rewrite(list.Select(v => new SearchView(v.IndexNo, v.Command)));

                // 検索履歴の最大数を超えた場合は削除
                var maxCount = HISTORY_MAX + this.searchHistory.Where(v => string.IsNullOrEmpty(v.Command)).Count();
                while (this.searchHistory.Count > maxCount) {
                    var delhis = this.searchHistory[maxCount];
                    this.logger.ZLog(this).LogInformation("最大件数を超えた検索条件を削除します。{delhis.Command}", delhis.Command);
                    this.dbService.DeleteHistory(delhis.IndexNo);
                    this.searchHistory.RemoveAt(maxCount);
                }
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "検索履歴の取得に失敗しました。");
            }
        }

        /// <summary>検索履歴リストに追加します。</summary>
        /// <param name="searchCondition">検索ワード。</param>
        internal async Task UpdateSearchHistory(string searchCondition)
        {
            var hit = this.searchHistory.FirstOrDefault(x => x.Command == searchCondition);
            if (hit != null) {
                this.searchHistory.Remove(hit);
                this.searchHistory.Insert(0, hit);
                this.dbService.UpdateConditionTime(hit.IndexNo);
            }
            else {
                this.dbService.InsertHistory(searchCondition);
                await Task.Run(() => this.LoadHistory());
            }
        }
    }
}
