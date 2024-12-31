using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TagNotes.Models;

namespace TagNotes.Views
{
    /// <summary>メモ追加ダイアログ。</summary>
    public sealed partial class AddOrEditDialog : 
        ContentDialog
    {
        /// <summary>コンストラクタ。</summary>
        public AddOrEditDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>バインドしたデータが変更されたときを処理します。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private void ContentDialog_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.Calendar.SelectedDates.Clear();
            this.Calendar.SelectedDates.Add((args.NewValue as NoteDialogModel).NotificationDate);

            if (!(args.NewValue as NoteDialogModel).UseDeleteButton) {
                this.SecondaryButtonText = "";
            }
        }

        /// <summary>日付選択時の処理を行います。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private void DatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            this.Calendar.SelectedDates.Clear();
            var selDt = args.NewDate ?? new DateTimeOffset();
            this.Calendar.SelectedDates.Add(selDt);
            this.Calendar.SetDisplayDate(selDt);
        }

        /// <summary>カレンダーの日付選択時の処理を行います。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="args">イベントオブジェクト。</param>
        private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (args.AddedDates.Count > 0 && (this.DataContext as NoteDialogModel).NotificationDate.Date.Date != args.AddedDates[0].Date.Date) {
                (this.DataContext as NoteDialogModel).NotificationDate = args.AddedDates[0];
                this.CalendarFlyout.Hide();
            }
        }
    }
}
