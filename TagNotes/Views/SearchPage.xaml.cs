using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagNotes.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TagNotes.Views
{
    /// <summary>検索ページです。</summary>
    public sealed partial class SearchPage : Page
    {
        /// <summary>メインウィンドウ。</summary>
        private MainWindow mainWin;

        /// <summary>コンストラクタ。</summary>
        public SearchPage()
        {
            this.InitializeComponent();

            // サービスプロバイダからモデルを設定
            var provider = (Application.Current as App)?.Provider;
            this.DataContext = provider?.GetService<SearchModel>();
        }

        /// <summary>ロードイベントハンドラです。</summary>
        /// <param name="sender">イベントハンドラです。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                data.LoadHistory();
                data.LoadTags();
            }
        }

        /// <summary>ナビゲーションイベントハンドラです。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.mainWin = (MainWindow)e.Parameter;
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

        /// <summary>検索ボタンクリックイベントハンドラです。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                await data.UpdateSearchHistory();
                this.mainWin.GoListPage(data.SearchCondition);
            }
        }

        /// <summary>キャンセルボタンクリックイベントハンドラです。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainWin.GoBackFrame();
        }
    }
}
