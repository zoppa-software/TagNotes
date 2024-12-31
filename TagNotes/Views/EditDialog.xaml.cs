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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagNotes.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditDialog : 
        ContentDialog
    {
        public EditDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>�o�C���h�����f�[�^���ύX���ꂽ�Ƃ����������܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="args">�C�x���g�I�u�W�F�N�g�B</param>
        private void ContentDialog_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.Calendar.SelectedDates.Clear();
            this.Calendar.SelectedDates.Add((args.NewValue as NoteDialogModel).NotificationDate);
        }

        /// <summary>���t�I�����̏������s���܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="args">�C�x���g�I�u�W�F�N�g�B</param>
        private void DatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            this.Calendar.SelectedDates.Clear();
            var selDt = args.NewDate ?? new DateTimeOffset();
            this.Calendar.SelectedDates.Add(selDt);
            this.Calendar.SetDisplayDate(selDt);
        }

        /// <summary>�J�����_�[�̓��t�I�����̏������s���܂��B</summary>
        /// <param name="sender">�C�x���g���s���B</param>
        /// <param name="args">�C�x���g�I�u�W�F�N�g�B</param>
        private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (args.AddedDates.Count > 0 && (this.DataContext as NoteDialogModel).NotificationDate.Date.Date != args.AddedDates[0].Date.Date) {
                (this.DataContext as NoteDialogModel).NotificationDate = args.AddedDates[0];
                this.CalendarFlyout.Hide();
            }
        }
    }
}
