using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Threading.Tasks;
using TagNotes.Models;

#nullable enable

namespace TagNotes.Views
{
    /// <summary>リストページ。</summary>
    public sealed partial class ListPage : Page
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger? logger;

        /// <summary>表示中の検索条件。</summary>
        private string searchCondition = "";

        /// <summary>コンストラクタ。</summary>
        public ListPage()
        {
            this.InitializeComponent();

            // サービスプロバイダを取得
            var provider = (Application.Current as App)?.Provider;

            // ログ設定
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<MainWindow>();

            // ページモデルを設定
            this.DataContext = provider?.GetService<ListPageModel>();
        }

        /// <summary>リストを読み込みます。</summary>
        /// <param name="navigationMode">ナビゲーションモード。</param>
        /// <returns>非同期タスク。</returns>
        public async Task ReloadList(NavigationMode navigationMode)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.logger?.LogInformation("リストを再読込。条件:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, navigationMode);
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "リスト再読込エラー:{ex.Message}", ex.Message);
            }
        }

        /// <summary>ページがナビゲートされたときに呼び出されます。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.searchCondition = e.Parameter?.ToString() ?? "";
                    this.logger?.LogInformation("リストを読込。条件:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, e.NavigationMode);
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "リスト読込エラー:{ex.Message}", ex.Message);
            }
        }

        /// <summary>リストビューの選択が変更されたときに呼び出されます。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 選択された項目を取得
            NoteView? listItem = null;
            var listView = sender as ListView;
            if (listView?.SelectedItem != null) {
                listItem = listView.SelectedItem as NoteView;
                listView.SelectedItem = null;
            }

            // 選択された項目がある場合、詳細ダイアログを表示
            if (listItem != null && 
                this.DataContext is ListPageModel model) {
                var resourceLoader = new ResourceLoader();

                await model.EditSelectNote(
                    listItem,
                    resourceLoader.GetString("EDIT_DIALOG_TITLE"),
                    resourceLoader.GetString("EDIT_ACTION_CAPTION"),
                    async (data) => {
                        var dialog = new AddOrEditDialog {
                            XamlRoot = this.Content.XamlRoot,
                            DataContext = data
                        };
                        return await dialog.ShowAsync();
                    },
                    async (data) => {
                        await model.LoadList(this.searchCondition, NavigationMode.Refresh);
                    },
                    async (data) => {
                        await model.LoadList(this.searchCondition, NavigationMode.Refresh);
                    },
                    () => { }
                );
            }
        }
    }
}
