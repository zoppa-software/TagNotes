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
    /// <summary>���C���E�B���h�E�ł��B</summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>���O�o�͋@�\�B</summary>
        private readonly ILogger? logger;

        /// <summary>�R���X�g���N�^�B</summary>
        public MainWindow()
        {
            // ������
            this.InitializeComponent();

            // �E�B���h�E�A�C�R���̐ݒ�
            if (AppWindowTitleBar.IsCustomizationSupported() is true) {
                IntPtr hWnd = WindowNative.GetWindowHandle(this);
                WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(wndId);
                appWindow.SetIcon(@"app.ico");
            }

            this.ExtendsContentIntoTitleBar = true;

            // �T�[�r�X�v���o�C�_���擾
            var provider = (Application.Current as App)?.Provider;

            // ���O�ݒ�
            this.logger = provider?.GetService<ILoggerFactory>()?.CreateLogger<MainWindow>();

            // ���C���E�B���h�E���f�����擾�A�ݒ�
            var model = provider?.GetService<MainWindowModel>();
            this.NavView.DataContext = model;

            // �E�B���h�E�T�C�Y��ݒ�
            double dpiScale = this.GetDpiScale();
            AppWindow.ResizeClient(new SizeInt32(
                (int)(1024 * dpiScale),
                (int)(768 * dpiScale))
            );
        }

        /// <summary>�E�B���h�E�̃��[�h�����ł��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            try {
                // �ݒ荀�ڂ̃L���v�V������ݒ�
                if (sender is NavigationView navView &&
                    navView?.SettingsItem is NavigationViewItem settingsItem) {
                    var resourceLoader = new ResourceLoader();
                    settingsItem.Content = resourceLoader.GetString("SETTING_CAPTION");
                }

                // �ŐV�̌��������Ō������A���X�g�y�[�W��\��
                if (this.NavView.DataContext is MainWindowModel model) {
                    this.ContentFrame.Navigate(typeof(ListPage), model.LatestSearchCondition ?? "");
                }
            }
            catch (Exception ex) {
                this.logger?.LogError(ex, "�E�B���h�E�̃��[�h�Ɏ��s���܂����B");
            }
        }

        /// <summary>�����̍ŐV�y�[�W���猟���������擾���܂��B</summary>
        /// <returns>���������B</returns>
        private string GetSearchConditionFromHistoryPage()
        {
            var page = this.ContentFrame.BackStack.LastOrDefault(v => v.SourcePageType == typeof(ListPage));
            return page?.Parameter?.ToString() ?? "";
        }

        /// <summary>���X�g�y�[�W�ɕύX���܂��B</summary>
        private void ChangeListPage()
        {
            if (this.ContentFrame.CurrentSourcePageType != typeof(ListPage)) {
                // ���݂̃y�[�W�����X�g�y�[�W�łȂ��ꍇ�A���X�g�y�[�W��\��
                this.ContentFrame.Navigate(typeof(ListPage), this.GetSearchConditionFromHistoryPage());
            }
            else {
                // ���X�g�y�[�W���ēǂݍ���
                (this.ContentFrame.Content as ListPage)?.ReloadList(NavigationMode.New);
                this.NavView.SelectedItem = this.ListPageItem;
            }
        }

        /// <summary>�i�r�Q�[�V�����r���[�̑I��ύX�C�x���g�n���h���B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="args">�C�x���g�I�u�W�F�N�g�B</param>
        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected) {
                this.ContentFrame.Navigate(typeof(SettingPage));
            }
            else if (args.SelectedItem is NavigationViewItem item) {
                switch (item.Tag) {
                    case "SearchPage": {
                            // �����y�[�W��\�����A���X�g��\��
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
                            // �����ǉ��_�C�A���O��\�����A���X�g��\��
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

        /// <summary>�o�b�N�{�^���������ꂽ�Ƃ��ɌĂяo����܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="args">�C�x���g�I�u�W�F�N�g�B</param>
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (this.ContentFrame.CanGoBack) {
                this.ContentFrame.GoBack();   
            }
        }

        /// <summary>�i�r�Q�[�V���������������Ƃ��ɌĂяo����܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="e">�C�x���g�I�u�W�F�N�g�B</param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // �i�r�Q�[�V�����r���[�̑I����ύX
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

            // �o�b�N�{�^���̗L��������ݒ�
            this.NavView.IsBackEnabled = this.ContentFrame.CanGoBack;
        }
    }
}
