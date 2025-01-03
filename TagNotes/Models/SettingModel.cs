using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Input;
using TagNotes.Helper;
using TagNotes.Services;
using ZoppaLoggingExtensions;

namespace TagNotes.Models
{
    /// <summary>設定ページモデル。</summary>
    /// <remarks>コンストラクタ。</remarks>
    /// <param name="loggerFactory">ロガーファクトリ。</param>
    internal sealed class SettingModel(ILoggerFactory loggerFactory)
    {
        /// <summary>ログフォルダパスを取得します。</summary>
        public static string LogFolderPath => $"{TipsHelper.ExePath}\\log";

        /// <summary>ログ出力機能。</summary>
        private readonly ILogger logger = loggerFactory.CreateLogger<SettingModel>();

        /// <summary>ログフォルダを開くコマンド。</summary>
        public ICommand OpenLogFolder => new RelayCommand(() => {
            try {
                System.Diagnostics.Process.Start("explorer.exe", LogFolderPath);
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "ログフォルダを開けませんでした。");
            }
        });

        /// <summary>データベースフォルダを開くコマンド。</summary>
        public ICommand OpenDatabaseFolder => new RelayCommand(() => {
            try {
                System.Diagnostics.Process.Start("explorer.exe", DatabaseConnectionService.DatabaseFolderPath);
            }
            catch (Exception ex) {
                this.logger.ZLog(this).LogError(ex, "データベースフォルダを開けませんでした。");
            }
        });
    }
}
