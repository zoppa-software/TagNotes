using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TagNotes.Helper;
using TagNotes.Services;
using TagNotes.Views;
using ZoppaLoggingExtensions;

namespace TagNotes.Models
{
    /// <summary>検索モデル。</summary>
    internal sealed partial class SearchModel(ILoggerFactory loggerFactory, 
                                              SearchHistoryService searchHistoryService) :
        ObservableObject
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<SearchModel>();

        /// <summary>検索履歴サービス。</summary>
        private readonly SearchHistoryService searchHistoryService = searchHistoryService;

        /// <summary>検索ワード。</summary>
        [ObservableProperty]
        private string searchCondition = "";

        /// <summary>検索リスト。</summary>
        [ObservableProperty]
        private ObservableCollection<SearchView> searchList = [];

        /// <summary>検索履歴を読み込む。</summary>
        public void LoadHistory()
        {
            try {
                // 検索履歴を読み込む
                var list = this.searchHistoryService.SearchConditionHistory;
                this.logger.ZLog(this).LogInformation("履歴件数:{SearchCondition}", list.Count);

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
    }
}
