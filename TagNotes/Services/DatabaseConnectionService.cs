using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SQLite;
using TagNotes.Helper;

namespace TagNotes.Services
{
    /// <summary>データベース接続サービス。</summary>
    internal sealed class DatabaseConnectionService :
        IDatabaseConnectionService
    {
        /// <summary>ファイル定数。</summary>
        private const string FILE_NAME = "sqlite.db";

        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger;

        /// <summary>接続文字列。</summary>
        private readonly string connectionString;

        /// <summary>データベースフォルダパス。</summary>
        public static string DatabaseFolderPath => $"{TipsHelper.ExePath}\\db";

        /// <summary>コンストラクタ。</summary>
        /// <param name="loggerFactory">ログファクトリー。</param>
        public DatabaseConnectionService(ILoggerFactory loggerFactory)
        {
            // ロガーを作成
            this.logger = loggerFactory.CreateLogger<DatabaseConnectionService>();
            this.logger.LogInformation("create database connection");

            // データベースフォルダが存在しない場合は作成
            var dbFolder = new System.IO.DirectoryInfo(DatabaseFolderPath);
            if (!dbFolder.Exists) {
                dbFolder.Create();
            }

            // 接続文字列を作成
            var conBuilder = new SQLiteConnectionStringBuilder() {
                DataSource = $"{DatabaseFolderPath}\\{FILE_NAME}"
            };
            this.connectionString = conBuilder.ConnectionString;
            this.logger.LogInformation("connection string : {connectionString}", this.connectionString);
        }

        /// <summary>データベース接続を取得します。</summary>
        public IDbConnection GetDbConnection()
        {
            this.logger.LogInformation("open connection");
            return new SQLiteConnection(this.connectionString);
        }

        /// <summary>新規データベースかどうかを取得します。</summary>
        public bool IsNewDatabase()
        {
            return !System.IO.File.Exists($"{DatabaseFolderPath}\\{FILE_NAME}");
        }
    }
}
