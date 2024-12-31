using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using TagNotes.Helper;
using TagNotes.Models;
using TagNotes.Views;
using Windows.ApplicationModel.Resources;
using Windows.Graphics;
using WinRT.Interop;

#nullable enable

namespace TagNotes
{
    /// <summary>メインウィンドウです。</summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>ログ出力機能。</summary>
        private readonly ILogger? logger;

        /// <summary>コンストラクタ。</summary>
        public MainWindow()
        {
            // 初期化
            this.InitializeComponent();

            // ウィンドウアイコンの設定
            if (AppWindowTitleBar.IsCustomizationSupported() is true) {
                IntPtr hWnd = WindowNative.GetWindowHandle(this);
                WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(wndId);
                appWindow.SetIcon(@"app.ico");
            }

            this.ExtendsContentIntoTitleBar = true;

            // サービスプロバイダを取得
            var provider = (Application.Current as App)?.Provider;

            // ログ設定
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<MainWindow>();

            // メインウィンドウモデルを取得、設定
            var model = provider?.GetService<MainWindowModel>();
            this.NavView.DataContext = model;

            // ウィンドウサイズを設定
            double dpiScale = this.GetDpiScale();
            AppWindow.ResizeClient(new SizeInt32(
                (int)(1024 * dpiScale),
                (int)(768 * dpiScale))
            );
        }

        /// <summary>ウィンドウのロード処理です。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            try {
                // 設定項目のキャプションを設定
                if (sender is NavigationView navView &&
                    navView?.SettingsItem is NavigationViewItem settingsItem) {
                    var resourceLoader = new ResourceLoader();
                    settingsItem.Content = resourceLoader.GetString("SETTING_CAPTION");
                }

                // 最新の検索条件で検索し、リストページを表示
                if (this.NavView.DataContext is MainWindowModel model) {
                    this.ContentFrame.Navigate(typeof(ListPage), model.LatestSearchCondition ?? "");
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "ウィンドウのロードに失敗しました。");
            }
        }

        /// <summary>履歴の最新ページから検索条件を取得します。</summary>
        /// <returns>検索条件。</returns>
        private string GetSearchConditionFromHistoryPage()
        {
            var page = this.ContentFrame.BackStack.LastOrDefault(v => v.SourcePageType == typeof(ListPage));
            return page?.Parameter?.ToString() ?? "";
        }

        /// <summary>リストページに変更します。</summary>
        private void ChangeListPage()
        {
            if (this.ContentFrame.CurrentSourcePageType != typeof(ListPage)) {
                // 現在のページがリストページでない場合、リストページを表示
                this.ContentFrame.Navigate(typeof(ListPage), this.GetSearchConditionFromHistoryPage());
            }
            else {
                // リストページを再読み込み
                (this.ContentFrame.Content as ListPage)?.ReloadList(NavigationMode.New);
                this.NavView.SelectedItem = this.ListPageItem;
            }
        }

        /// <summary>ナビゲーションビューの選択変更イベントハンドラ。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected) {
                this.ContentFrame.Navigate(typeof(SettingPage));
            }
            else if (args.SelectedItem is NavigationViewItem item) {
                switch (item.Tag) {
                    case "SearchPage": {
                            // 検索ページを表示し、リストを表示
                            if (this.NavView.DataContext is MainWindowModel model) {
                                await model.SelectBySearchPage(
                                    async (data) => {
                                        var dialog = new SearchDialog {
                                            XamlRoot = this.Content.XamlRoot,
                                            DataContext = data
                                        };
                                        return await dialog.ShowAsync();
                                    },
                                    (data) => this.ContentFrame.Navigate(typeof(ListPage), data.SearchCondition),
                                    () => this.ChangeListPage()
                                );
                            }
                        }
                        break;

                    case "AddPage": {
                            // メモ追加ダイアログを表示し、リストを表示
                            if (this.NavView.DataContext is MainWindowModel model) {
                                var resourceLoader = new ResourceLoader();
                                await model.SelectByAddPage(
                                    resourceLoader.GetString("ADD_DIALOG_TITLE"),
                                    resourceLoader.GetString("ADD_ACTION_CAPTION"),
                                    async (data) => {
                                        var dialog = new AddOrEditDialog {
                                            XamlRoot = this.Content.XamlRoot,
                                            DataContext = data
                                        };
                                        return await dialog.ShowAsync();
                                    },
                                    (data) => this.ChangeListPage(),
                                    () => this.ChangeListPage()
                                );
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>バックボタンが押されたときに呼び出されます。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (this.ContentFrame.CanGoBack) {
                this.ContentFrame.GoBack();   
            }
        }

        /// <summary>ナビゲーションが完了したときに呼び出されます。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // ナビゲーションビューの選択を変更
            switch (e.SourcePageType.Name) {
                case "ListPage":
                    this.NavView.SelectedItem = this.ListPageItem;
                    break;

                case "SettingPage":
                    this.NavView.SelectedItem = this.NavView.SettingsItem;
                    break;

                default:
                    this.NavView.SelectedItem = this.NavView.MenuItems.OfType<NavigationViewItem>().First(p => p.Tag.Equals(e.SourcePageType.Name));
                    break;
            }

            // バックボタンの有効無効を設定
            this.NavView.IsBackEnabled = this.ContentFrame.CanGoBack;
        }
    }
}
