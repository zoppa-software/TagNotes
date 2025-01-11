using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Threading.Tasks;
using TagNotes.Models;
using ZoppaLoggingExtensions;

#nullable enable

namespace TagNotes.Views
{
    /// <summary>リストページ。</summary>
    public sealed partial class ListPage : Page
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger? logger;

        /// <summary>メインウィンドウ。</summary>
        private MainWindow? mainWin;

        /// <summary>表示中の検索条件。</summary>
        private string searchCondition = "";

        /// <summary>コンストラクタ。</summary>
        public ListPage()
        {
            this.InitializeComponent();

            // サービスプロバイダを取得
            var provider = (Application.Current as App)?.Provider;

            // ログ設定
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<ListPage>();

            // ページモデルを設定
            this.DataContext = provider?.GetService<ListPageModel>();
        }

        /// <summary>リストを読み込みます。</summary>
        /// <param name="navigationMode">ナビゲーションモード。</param>
        /// <returns>非同期タスク。</returns>
        public async Task<string> ReloadList(NavigationMode navigationMode)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.logger?.ZLog(this).LogInformation("リストを再読込。条件:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, navigationMode);
                    this.mainWin?.SetSearchCondition(this.searchCondition);
                    return this.searchCondition;
                }
            }
            catch (Exception ex) {
                this.logger?.ZLog(this).LogError(ex, "リスト再読込エラー:{ex.Message}", ex.Message);
            }
            return "";
        }

        /// <summary>ページがナビゲートされたときに呼び出されます。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    (this.mainWin, this.searchCondition) = ((MainWindow, string))e.Parameter;
                    this.logger?.ZLog(this).LogInformation("リストを読込。条件:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, e.NavigationMode);
                    this.mainWin.SetSearchCondition(this.searchCondition);
                }
            }
            catch (Exception ex) {
                this.logger?.ZLog(this).LogError(ex, "リスト読込エラー:{ex.Message}", ex.Message);
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
