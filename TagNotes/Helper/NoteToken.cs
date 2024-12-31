namespace TagNotes.Helper
{
    /// <summary>メモトークン。</summary>
    /// <remarks>コンストラクタ。</remarks>
    /// <param name="kd">トークン種類。</param>
    /// <param name="value">トークン文字列。</param>
    internal readonly struct NoteToken(NoteToken.TokenKind kd, string value)
    {
        /// <summary>トークンの種類。</summary>
        internal enum TokenKind
        {
            /// <summary>テキスト。</summary>
            Text,

            /// <summary>タグ。</summary>
            Tag,

            /// <summary>URI。</summary>
            Uri,

            /// <summary>パス。</summary>
            Path
        }

        /// <summary>トークンの種類を取得します。</summary>
        public TokenKind Kind { get; } = kd;

        /// <summary>トークン文字列を取得します。</summary>
        public string Value { get; } = value;
    }
}
