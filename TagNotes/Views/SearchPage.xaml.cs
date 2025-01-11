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
    /// <summary>�����y�[�W�ł��B</summary>
    public sealed partial class SearchPage : Page
    {
        /// <summary>���C���E�B���h�E�B</summary>
        private MainWindow mainWin;

        /// <summary>�R���X�g���N�^�B</summary>
        public SearchPage()
        {
            this.InitializeComponent();

            // �T�[�r�X�v���o�C�_���烂�f����ݒ�
            var provider = (Application.Current as App)?.Provider;
            this.DataContext = provider?.GetService<SearchModel>();
        }

        /// <summary>���[�h�C�x���g�n���h���ł��B</summary>
        /// <param name="sender">�C�x���g�n���h���ł��B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                data.LoadHistory();
                data.LoadTags();
            }
        }

        /// <summary>�i�r�Q�[�V�����C�x���g�n���h���ł��B</summary>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.mainWin = (MainWindow)e.Parameter;
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

        /// <summary>�����{�^���N���b�N�C�x���g�n���h���ł��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SearchModel data) {
                await data.UpdateSearchHistory();
                this.mainWin.GoListPage(data.SearchCondition);
            }
        }

        /// <summary>�L�����Z���{�^���N���b�N�C�x���g�n���h���ł��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainWin.GoBackFrame();
        }
    }
}
