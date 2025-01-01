namespace TagNotes.Views
{
    /// <summary>検索の表示を行うビュー。</summary>
    internal sealed class SearchView(long indexNo, string command)
    {
        /// <summary>検索のインデックスを取得します。</summary>
        public long IndexNo { get; } = indexNo;

        /// <summary>検索条件を取得します。</summary>
        public string Command { get; } = command;
    }
}
