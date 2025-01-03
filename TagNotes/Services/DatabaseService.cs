using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagNotes.Helper;
using ZoppaDSql;
using ZoppaLoggingExtensions;

namespace TagNotes.Services
{
    /// <summary>データベースサービス。</summary>
    internal sealed class DatabaseService
    {
        /// <summary>ログ出力サービス。</summary>
        private readonly ILogger logger;

        /// <summary>データベース接続サービス。</summary>
        private readonly IDatabaseConnectionService connection;

        /// <summary>コンストラクタ。</summary>
        /// <param name="loggerFactory">ログファクトリー。</param>
        /// <param name="connection">データベース接続サービス。</param>
        public DatabaseService(ILoggerFactory loggerFactory, IDatabaseConnectionService connection)
        {
            this.logger = loggerFactory.CreateLogger<DatabaseService>();
            this.logger.ZLog(this).LogInformation("create database connection");
            this.connection = connection;

            loggerFactory.SetZoppaDSqlLogFactory();
        }

        /// <summary>初期化を行います。</summary>
        public async Task Initialize()
        {
            if (this.connection.IsNewDatabase()) {
                this.logger.ZLog(this).LogInformation("create new database");

                await Task.Run(() => {
                    using var con = this.connection.GetDbConnection();
                    con.Open();

                    var createCommand =
@"CREATE TABLE [ShortItem] (
    [ino] INTEGER,
    [information] TEXT NOT NULL,
    [notification] TIMESTAMP,
    [every_day] INTEGER NOT NULL DEFAULT '0',
    [update_dt] TIMESTAMP NOT NULL,
    PRIMARY KEY([ino])
);
CREATE TABLE[ShortTag] (
    [no] INTEGER DEFAULT '0' UNIQUE,
    [ino] INTEGER NOT NULL REFERENCES[ShortItem]([ino]),
    [tag] TEXT NOT NULL,
    PRIMARY KEY([no])
);
CREATE TABLE [ShortNotification] (
    [ino] INTEGER,
    [timing] TIMESTAMP NOT NULL,
    PRIMARY KEY([ino],[timing])
);
CREATE TABLE [Condition] (
    [ino] INTEGER PRIMARY KEY AUTOINCREMENT,
    [command] TEXT NOT NULL,
    [update_dt] TIMESTAMP NOT NULL
);";
                    con.ExecuteQuery(createCommand);
                });
            }
            else {
                this.logger.ZLog(this).LogInformation("created database");
            }
        }

        /// <summary>メモを追加します。</summary>
        /// <param name="note">メモ。</param>
        /// <param name="isNotification">通知フラグ。</param>
        /// <param name="isEveryDay">毎日通知フラグ。</param>
        /// <param name="notificationDate">通知日。</param>
        /// <param name="notificationTime">通知時刻。</param>
        internal void AddNote(string note, 
                              bool isNotification,
                              bool isEveryDay,
                              DateTimeOffset notificationDate, 
                              TimeSpan notificationTime)
        {
            // 現在日時を取得
            var now = DateTime.Now;

            // 通知日時を取得
            var notifTime = new DateTime(
                notificationDate.Year,
                notificationDate.Month,
                notificationDate.Day,
                notificationTime.Hours,
                notificationTime.Minutes,
                0
            );

            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 最大のインデックスNoを取得
            long newIndex = con.SetTransaction(tran).ExecuteDatas<long>("select max(ck.ino) as ino from ShortItem ck").First() + 1;
            this.logger.ZLog(this).LogInformation("新しい項目index:{newIndex}", newIndex);

            // 項目を追加する
            con.SetTransaction(tran).ExecuteQuery(
@"insert into ShortItem(
    ino, 
    information, 
    notification, 
    every_day, 
    update_dt
) values(
    @ino, 
    @information, 
    @notification, 
    @every_day,
    @update_dt
)",
                new { 
                    ino = newIndex, 
                    information = note, 
                    notification = (DateTime?)(isNotification ? notifTime : null),
                    every_day = (isEveryDay ? 1 : 0),
                    update_dt = now 
                }
            );

            // タグを追加する
            var tags = note.ParseNote().Where(tkn => tkn.Kind == NoteToken.TokenKind.Tag);
            if (tags.Any()) {
                con.SetTransaction(tran).ExecuteQuery(
                    "insert into ShortTag (ino, tag) values(@ino, @tag)",
                    tags.Select(tkn => new { ino = newIndex, tag = tkn.Value[1..] })
                );
            }

            // 通知日時を追加する
            if (isNotification) {
                var param = GetNotificationTime(notifTime, isEveryDay)
                    .Where(timing => now < timing)
                    .Select(tim => new { ino = newIndex, timing = tim })
                    .ToArray();

                if (param.Length != 0) {
                    con.SetTransaction(tran).ExecuteQuery(
                        "insert into ShortNotification (ino, timing) values(@ino, @timing)", param
                    );
                }
            }

            tran.Commit();
        }

        /// <summary>メモを編集します。</summary>
        /// <param name="index">メモ識別子。</param>
        /// <param name="note">メモ。</param>
        /// <param name="isNotification">通知フラグ。</param>
        /// <param name="isEveryDay">毎日通知フラグ。</param>
        /// <param name="notificationDate">通知日。</param>
        /// <param name="notificationTime">通知時刻。</param>
        internal void EditNote(long index, 
                               string note, 
                               bool isNotification, 
                               bool isEveryDay, 
                               DateTimeOffset notificationDate, 
                               TimeSpan notificationTime)
        {
            // 現在日時を取得
            var now = DateTime.Now;

            // 通知日時を取得
            var notifTime = new DateTime(
                notificationDate.Year,
                notificationDate.Month,
                notificationDate.Day,
                notificationTime.Hours,
                notificationTime.Minutes,
                0
            );

            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 項目を追加する
            con.SetTransaction(tran).ExecuteQuery(
@"update ShortItem 
set
    information = @information,
    notification = @notification,
    every_day = @every_day,
    update_dt = @update_dt
where
    ino = @ino",
                new { 
                    ino = index, 
                    information = note, 
                    notification = (DateTime?)(isNotification ? notifTime : null),
                    every_day = (isEveryDay ? 1 : 0),
                    update_dt = now
                }
            );

            // タグを追加する
            var tags = note.ParseNote().Where(tkn => tkn.Kind == NoteToken.TokenKind.Tag);
            if (tags.Any()) {
                con.SetTransaction(tran).ExecuteQuery(
                    "delete from ShortTag where ino = @ino",
                    new { ino = index }
                );

                con.SetTransaction(tran).ExecuteQuery(
                    "insert into ShortTag (ino, tag) values(@ino, @tag)",
                    tags.Select(tkn => new { ino = index, tag = tkn.Value[1..] })
                );
            }

            // 通知日時を追加する
            con.SetTransaction(tran).ExecuteQuery(
                "delete from ShortNotification where ino = @ino",
                new { ino = index }
            );
            if (isNotification) {
                var param = GetNotificationTime(notifTime, isEveryDay)
                    .Where(timing => now < timing)
                    .Select(tim => new { ino = index, timing = tim})
                    .ToArray();

                if (param.Length != 0) {
                    con.SetTransaction(tran).ExecuteQuery(
                        "insert into ShortNotification (ino, timing) values(@ino, @timing)", param
                    );
                }
            }

            tran.Commit();
        }

        /// <summary>通知時間を取得します。</summary>
        /// <param name="isEveryDay">毎日通知するならば真。</param>
        /// <returns>通知時間リスト。</returns>
        private static DateTime[] GetNotificationTime(DateTime notifTime, bool isEveryDay)
        {
            return (
                isEveryDay ?
                [
                    AjustNextNotificationTime(notifTime.Subtract(TimeSpan.FromMinutes(5))),
                    AjustNextNotificationTime(notifTime)
                ] :
                [
                    notifTime.Subtract(TimeSpan.FromDays(3)),
                    notifTime.Subtract(TimeSpan.FromDays(1)),
                    notifTime.Subtract(TimeSpan.FromHours(4)),
                    notifTime.Subtract(TimeSpan.FromHours(1)),
                    notifTime.Subtract(TimeSpan.FromMinutes(30)),
                    notifTime.Subtract(TimeSpan.FromMinutes(15)),
                    notifTime.Subtract(TimeSpan.FromMinutes(5)),
                    notifTime
                ]
            );
        }

        /// <summary>次の通知時間を取得します。</summary>
        /// <param name="notif">通知時間。</param>
        /// <returns>次の通知時間。</returns>
        private static DateTime AjustNextNotificationTime(DateTime notif)
        {
            var res = DateTime.Now.Date.Add(notif.TimeOfDay);
            return (res < DateTime.Now) ? res.AddDays(1) : res;
        }

        /// <summary>通知メッセージを取得します。</summary>
        /// <returns>通知メッセージリスト。</returns>
        internal List<Notification> GetNotificationMessages()
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // 通知メッセージを取得
            return con.ExecuteRecords<Notification>(
@"select
    notif.ino, 
    notif.timing, 
    sitm.every_day,
    sitm.information, 
    sitm.notification
from
    ShortNotification notif
inner join ShortItem sitm on 
    notif.ino = sitm.ino
order by
    notif.timing,
    sitm.notification");
        }

        internal void UpdateNotificationMessage(Notification updateMsg)
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 通知メッセージを削除
            con.SetTransaction(tran).ExecuteQuery(
                "delete from ShortNotification where ino = @ino and timing = @timing",
                new { ino = updateMsg.Index, timing = updateMsg.Timing }
            );

            // 通知メッセージを追加
            con.SetTransaction(tran).ExecuteQuery(
                "insert into ShortNotification (ino, timing) values(@ino, @timing)",
                new { ino = updateMsg.Index, timing = updateMsg.Timing.AddDays(1) }
            );

            tran.Commit();
        }

        /// <summary>通知メッセージを削除します。</summary>
        /// <param name="notifications">削除リスト。</param>
        internal void DeleteNotificationMessage(List<Notification> notifications)
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // 通知メッセージを削除
            con.ExecuteQuery(
                "delete from ShortNotification where ino = @ino and timing = @timing",
                notifications.Select(x => new { ino = x.Index, timing = x.Timing })
            );
        }

        /// <summary>通知メッセージ。</summary>
        /// <param name="Index">メッセージID。</param>
        /// <param name="Timing">表示タイミング。</param>
        /// <param name="EveryDay">毎日ならば真。</param>
        /// <param name="Message">メッセージ。</param>
        /// <param name="NotificationTime">通知時間。</param>
        public record Notification(long Index, DateTime Timing, long EveryDay, string Message, DateTime? NotificationTime);

        /// <summary>通知メッセージリストを取得します。</summary>
        /// <param name="searchWords">検索ワードリスト。</param>
        /// <param name="searchTags">検索タグリスト。</param>
        /// <param name="notSearchTags">非対象タグリスト。</param>
        /// <returns>通知メッセージリスト。</returns>
        internal async Task<List<Note>> GetNotesSync(List<string> searchWords, List<string> searchTags, List<string> notSearchTags)
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // 検索ワードを取得
            var filterWords = searchWords.Where(s => s.Trim() != "").Select(s => s.Replace("'", "''"));

            // メモを取得
            return await con.ExecuteCreateRecordsSync<Note>(
@"with hitTags as (
    select distinct
        si.ino
    from
        ShortItem as si
    left outer join ShortTag st on
        si.ino = st.ino
    {trim}
    where
        {trim both}
        {if hasSearchTags}st.tag in (#{searchTags}){/if} or
        {if hasSearchWords}({trim}{foreach word in searchWords}si.information like '%!{word}%' or {/for}{/trim}){/if}
        {/trim}
    {/trim}
)
select
    si.ino,
    si.information,
    si.notification,
    si.every_day,
    si.update_dt,
    st.[no],
    st.tag
from
    ShortItem as si
left outer join ShortTag st on
    si.ino = st.ino
where
    si.ino in hitTags
    {if hasNotSearchTags}and (st.tag is null or not st.tag in (#{notSearchTags})){/if}
order by
    si.update_dt desc,
    st.[no]",
                new {
                    searchWords = filterWords,
                    hasSearchWords = filterWords.Any(),
                    searchTags,
                    hasSearchTags = (searchTags.Count != 0),
                    notSearchTags,
                    hasNotSearchTags = (notSearchTags.Count != 0)
                },
                [],
                (param) => [param[0]],
                (note, param) => {
                    if (param[5] != null) {
                        note.Tags.Add(new Tag((long)param[5], param[5].ToString()));
                    }
                },
                (param) => {
                    var note = new Note((long)param[0], param[1].ToString(), (DateTime?)param[2], (param[3] != null && (long)param[3] != 0), (DateTime)param[4]);
                    if (param[5] != null) {
                        note.Tags.Add(new Tag((long)param[5], param[6].ToString()));
                    }
                    return note;
                }
            );
        }

        /// <summary>検索履歴を取得します。</summary>
        /// <returns>検索履歴リスト。</returns>
        internal List<Condition> GetHistories()
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // 検索履歴を取得
            return con.ExecuteRecords<Condition>(
@"select
    ino, 
    command
from
    Condition
order by
    update_dt desc");
        }

        /// <summary>検索履歴を更新します。</summary>
        /// <param name="indexNo">検索インデックス。</param>
        internal void UpdateConditionTime(long indexNo)
        {
            // 現在日時を取得
            var now = DateTime.Now;

            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 項目を追加する
            con.SetTransaction(tran).ExecuteQuery(
@"update Condition 
set
    update_dt = @update_dt
where
    ino = @ino",
                new { ino = indexNo, update_dt = now }
            );

            tran.Commit();
        }

        /// <summary>検索履歴を追加します。</summary>
        /// <param name="searchCondition">検索条件。</param>
        internal void InsertHistory(string searchCondition)
        {
            // 現在日時を取得
            var now = DateTime.Now;

            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 項目を追加する
            con.SetTransaction(tran).ExecuteQuery(
@"insert into Condition(
    command, 
    update_dt
)
values (
    @command,
    @update_dt
)",
                new { command = searchCondition, update_dt = now }
            );

            tran.Commit();
        }

        /// <summary>検索履歴を削除します。</summary>
        /// <param name="indexNo">検索情報インデックス。</param>
        internal void DeleteHistory(long indexNo)
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 項目を追加する
            con.SetTransaction(tran).ExecuteQuery(
@"delete from Condition
where
    ino = @ino",
                new { ino = indexNo }
            );

            tran.Commit();
        }

        /// <summary>メモを削除します。</summary>
        /// <param name="id">メモのインデックス。</param>
        internal void DeleteNote(long id)
        {
            // 接続を開く
            using var con = this.connection.GetDbConnection();
            con.Open();

            // トランザクションの取得
            using var tran = con.BeginTransaction();

            // 項目を削除する
            con.SetTransaction(tran).ExecuteQuery(
                "delete from ShortItem where ino = @ino",
                new { ino = id }
            );

            // タグを削除する
            con.SetTransaction(tran).ExecuteQuery(
                "delete from ShortTag where ino = @ino",
                new { ino = id }
            );

            // 通知日時を削除する
            con.SetTransaction(tran).ExecuteQuery(
                "delete from ShortNotification where ino = @ino",
                new { ino = id }
            );

            tran.Commit();
        }

        /// <summary>メモ情報。</summary>
        /// <param name="index">メモインデックス。</param>
        /// <param name="information">メモ。</param>
        /// <param name="notificationTime">通知時刻。</param>
        /// <param name="isEveryDay">毎日通知フラグ。</param>
        /// <param name="updateTime">更新日時・</param>
        public sealed class Note(long index, string information, DateTime? notificationTime, bool isEveryDay, DateTime updateTime)
        {
            /// <summary>メモインデックスを取得します。</summary>
            public long Index { get; } = index;

            /// <summary>メモを取得します。</summary>
            public string Information { get; } = information;

            /// <summary>通知時刻を取得します。</summary>
            public DateTime? NotificationTime { get; } = notificationTime;

            /// <summary>毎日通知フラグを取得します。</summary>
            public bool IsEveryDay { get; } = isEveryDay;

            /// <summary>更新日時を取得します。</summary>
            public DateTime UpdateTime { get; } = updateTime;

            /// <summary>タグリストを取得します。</summary>
            public List<Tag> Tags { get; } = [];
        };

        /// <summary>タグ情報。</summary>
        /// <param name="TagNo">タグNo。</param>
        /// <param name="TagString">タグ文字列。</param>
        public record class Tag(long TagNo, string TagString);

        /// <summary>検索条件情報。</summary>
        /// <param name="IndexNo">検索インデックス。</param>
        /// <param name="Command">検索条件。</param>
        public record class Condition(long IndexNo, string Command);
    }
}
