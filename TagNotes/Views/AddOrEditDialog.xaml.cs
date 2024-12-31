using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TagNotes.Models;

namespace TagNotes.Views
{
    /// <summary>�����ǉ��_�C�A���O�B</summary>
    public sealed partial class AddOrEditDialog : 
        ContentDialog
    {
        /// <summary>�R���X�g���N�^�B</summary>
        public AddOrEditDialog()
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

            if (!(args.NewValue as NoteDialogModel).UseDeleteButton) {
                this.SecondaryButtonText = "";
            }
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
