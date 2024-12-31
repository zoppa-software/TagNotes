using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TagNotes.Models;

namespace TagNotes.Views
{
    /// <summary>�ݒ�y�[�W�B</summary>
    public sealed partial class SettingPage : Page
    {
        /// <summary>�R���X�g���N�^�B</summary>
        public SettingPage()
        {
            this.InitializeComponent();

            // �T�[�r�X�v���o�C�_���烂�f����ݒ�
            var provider = (Application.Current as App)?.Provider;
            this.DataContext = provider?.GetService<SettingModel>();
        }
    }
}
