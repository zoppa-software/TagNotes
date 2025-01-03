using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TagNotes.Models
{
    /// <summary>ダイアログモデル。</summary>
    internal sealed partial class NoteDialogModel(long id) : 
        ObservableObject
    {
        /// <summary>識別子を取得します。。</summary>
        public long Id => id;

        /// <summary>タイトル。</summary>
        [ObservableProperty]
        private string title = "title";

        /// <summary>実行ボタンテキスト。</summary>
        [ObservableProperty]
        private string actionCaption = "";

        /// <summary>メモ。</summary>
        [ObservableProperty]
        private string note = "note";

        /// <summary>通知を行うスイッチ。</summary>
        [ObservableProperty]
        private bool isNotification;

        /// <summary>毎日通知を行うスイッチ。</summary>
        [ObservableProperty]
        private bool isEveryDay;

        /// <summary>通知日時。</summary>
        [ObservableProperty]
        private DateTimeOffset notificationDate;

        /// <summary>通知時刻。</summary>
        [ObservableProperty]
        private TimeSpan notificationTime;

        /// <summary>削除をするスイッチ。</summary>
        [ObservableProperty]
        private bool useDeleteButton = false;
    }
}
