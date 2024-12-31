using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TagNotes.Models;
using TagNotes.Views;

#nullable enable

namespace TagNotes.Views
{
    /// <summary>�����_�C�A���O�ł��B</summary>
    public sealed partial class SearchDialog : ContentDialog
    {
        /// <summary>�R���X�g���N�^�B</summary>
        public SearchDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>���[�h�C�x���g�n���h���ł��B</summary>
        /// <param name="sender">�C�x���g�n���h���ł��B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                data.LoadHistory();
            }
        }

        /// <summary>���X�g�r���[�̑I���C�x���g�n���h���ł��B</summary>
        /// <param name="sender">�C�x���g�n���h���ł��B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView &&
                listView.SelectedItem is SearchView item &&
                this.DataContext is SearchModel data) {
                // �I�����ꂽ���ڂ��N���A
                listView.SelectedItem = null;

                // ����������ݒ�
                data.SearchCondition = item.Command;
            }
        }
    }
}
