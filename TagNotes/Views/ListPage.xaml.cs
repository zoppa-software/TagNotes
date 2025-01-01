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
    /// <summary>���X�g�y�[�W�B</summary>
    public sealed partial class ListPage : Page
    {
        /// <summary>���O�o�͋@�\�B</summary>
        private readonly ILogger? logger;

        /// <summary>�\�����̌��������B</summary>
        private string searchCondition = "";

        /// <summary>�R���X�g���N�^�B</summary>
        public ListPage()
        {
            this.InitializeComponent();

            // �T�[�r�X�v���o�C�_���擾
            var provider = (Application.Current as App)?.Provider;

            // ���O�ݒ�
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<MainWindow>();

            // �y�[�W���f����ݒ�
            this.DataContext = provider?.GetService<ListPageModel>();
        }

        /// <summary>���X�g��ǂݍ��݂܂��B</summary>
        /// <param name="navigationMode">�i�r�Q�[�V�������[�h�B</param>
        /// <returns>�񓯊��^�X�N�B</returns>
        public async Task ReloadList(NavigationMode navigationMode)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.logger?.LogInformation("���X�g���ēǍ��B����:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, navigationMode);
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "���X�g�ēǍ��G���[:{ex.Message}", ex.Message);
            }
        }

        /// <summary>�y�[�W���i�r�Q�[�g���ꂽ�Ƃ��ɌĂяo����܂��B</summary>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.searchCondition = e.Parameter?.ToString() ?? "";
                    this.logger?.LogInformation("���X�g��Ǎ��B����:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, e.NavigationMode);
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "���X�g�Ǎ��G���[:{ex.Message}", ex.Message);
            }
        }

        /// <summary>���X�g�r���[�̑I�����ύX���ꂽ�Ƃ��ɌĂяo����܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // �I�����ꂽ���ڂ��擾
            NoteView? listItem = null;
            var listView = sender as ListView;
            if (listView?.SelectedItem != null) {
                listItem = listView.SelectedItem as NoteView;
                listView.SelectedItem = null;
            }

            // �I�����ꂽ���ڂ�����ꍇ�A�ڍ׃_�C�A���O��\��
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
