using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TagNotes.Models;
using TagNotes.Views;

#nullable enable

namespace TagNotes.Views
{
    /// <summary>検索ダイアログです。</summary>
    public sealed partial class SearchDialog : ContentDialog
    {
        /// <summary>コンストラクタ。</summary>
        public SearchDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>ロードイベントハンドラです。</summary>
        /// <param name="sender">イベントハンドラです。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                data.LoadHistory();
            }
        }

        /// <summary>リストビューの選択イベントハンドラです。</summary>
        /// <param name="sender">イベントハンドラです。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView &&
                listView.SelectedItem is SearchView item &&
                this.DataContext is SearchModel data) {
                // 選択された項目をクリア
                listView.SelectedItem = null;

                // 検索条件を設定
                data.SearchCondition = item.Command;
            }
        }
    }
}
