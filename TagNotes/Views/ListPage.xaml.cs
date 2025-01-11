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
    /// <summary>���X�g�y�[�W�B</summary>
    public sealed partial class ListPage : Page
    {
        /// <summary>���O�o�͋@�\�B</summary>
        private readonly ILogger? logger;

        /// <summary>���C���E�B���h�E�B</summary>
        private MainWindow? mainWin;

        /// <summary>�\�����̌��������B</summary>
        private string searchCondition = "";

        /// <summary>�R���X�g���N�^�B</summary>
        public ListPage()
        {
            this.InitializeComponent();

            // �T�[�r�X�v���o�C�_���擾
            var provider = (Application.Current as App)?.Provider;

            // ���O�ݒ�
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<ListPage>();

            // �y�[�W���f����ݒ�
            this.DataContext = provider?.GetService<ListPageModel>();
        }

        /// <summary>���X�g��ǂݍ��݂܂��B</summary>
        /// <param name="navigationMode">�i�r�Q�[�V�������[�h�B</param>
        /// <returns>�񓯊��^�X�N�B</returns>
        public async Task<string> ReloadList(NavigationMode navigationMode)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    this.logger?.ZLog(this).LogInformation("���X�g���ēǍ��B����:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, navigationMode);
                    this.mainWin?.SetSearchCondition(this.searchCondition);
                    return this.searchCondition;
                }
            }
            catch (Exception ex) {
                this.logger?.ZLog(this).LogError(ex, "���X�g�ēǍ��G���[:{ex.Message}", ex.Message);
            }
            return "";
        }

        /// <summary>�y�[�W���i�r�Q�[�g���ꂽ�Ƃ��ɌĂяo����܂��B</summary>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                if (this.DataContext is ListPageModel model) {
                    (this.mainWin, this.searchCondition) = ((MainWindow, string))e.Parameter;
                    this.logger?.ZLog(this).LogInformation("���X�g��Ǎ��B����:{searchCondition}", this.searchCondition);
                    await model.LoadList(this.searchCondition, e.NavigationMode);
                    this.mainWin.SetSearchCondition(this.searchCondition);
                }
            }
            catch (Exception ex) {
                this.logger?.ZLog(this).LogError(ex, "���X�g�Ǎ��G���[:{ex.Message}", ex.Message);
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
