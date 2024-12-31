using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TagNotes.Models;

namespace TagNotes.Views
{
    /// <summary>設定ページ。</summary>
    public sealed partial class SettingPage : Page
    {
        /// <summary>コンストラクタ。</summary>
        public SettingPage()
        {
            this.InitializeComponent();

            // サービスプロバイダからモデルを設定
            var provider = (Application.Current as App)?.Provider;
            this.DataContext = provider?.GetService<SettingModel>();
        }
    }
}
