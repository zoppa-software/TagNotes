using System.Data;

namespace TagNotes.Services
{
    /// <summary>データベース接続サービスインターフェース。</summary>
    internal interface IDatabaseConnectionService
    {
        /// <summary>データベース接続を取得します。</summary>
        IDbConnection GetDbConnection();

        /// <summary>新規データベースかどうかを取得します。</summary>
        bool IsNewDatabase();
    }
}
