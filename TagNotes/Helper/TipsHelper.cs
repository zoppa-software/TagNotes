using System.Collections.Generic;
using System.Reflection;

namespace TagNotes.Helper
{
    /// <summary>ヘルパー機能です。</summary>
    internal static class TipsHelper
    {
        /// <summary>実行ファイルのパスを取得します。</summary>
        public static string ExePath => System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>引数のリストで対象リストを上書きします。</summary>
        /// <typeparam name="T">対象データ。</typeparam>
        /// <param name="destList">上書きするリスト。</param>
        /// <param name="sourceList">元のリスト。</param>
        public static void Rewrite<T>(this IList<T> destList, IEnumerable<T> sourceList)
        {
            destList.Clear();
            foreach (var his in sourceList) {
                destList.Add(his);
            }
        }
    }
}
