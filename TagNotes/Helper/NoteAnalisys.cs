using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TagNotes.Helper
{
    /// <summary>メモの解析を行います。</summary>
    internal static partial class NoteAnalisys
    {
        /// <summary>URIパターン正規表現。</summary>
        [GeneratedRegex(@"^((http:)|(https:)|(file://))//([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?")]
        private static partial Regex URI_PATTERN();

        /// <summary>パスパターン正規表現。</summary>
        [GeneratedRegex(@"^(([a-zA-Z]:\\)|(\\\\))([^\x01-\x7E]|[\w- .?%&]+)(\\([^\x01-\x7E]|[\w- .?%&\$\(\)])+)*")]
        private static partial Regex PATH_PATTERN();

        /// <summary>メモをトークンに分割します。</summary>
        public static List<NoteToken> ParseNote(this string note)
        {
            // 戻り値のリスト
            var tokens = new List<NoteToken>();

            // 解析対象文字リスト
            var str = note.ToCharArray();

            // 文字バッファ
            var buf = new StringBuilder();

            for (int i = 0; i < str.Length; ++i) {
                var c = str[i];

                switch (c) {
                    case '#':
                        // タグ追加
                        //
                        // 1. バッファに残っている文字列をテキストとして追加
                        // 2. 空白文字が見つかるまで文字列を取得
                        // 3. 取得した文字列をタグとして追加
                        if (buf.Length > 0) {                   // 1                         
                            tokens.Add(new NoteToken(NoteToken.TokenKind.Text, buf.ToString()));
                            buf.Clear();
                        }
                        for (int j = i; j < str.Length; ++j) {  // 2
                            if (!Char.IsWhiteSpace(str[j])) {
                                buf.Append(str[j]);
                            }
                            else {
                                break;
                            }
                        }
                        if (buf.Length > 0) {                   // 3
                            tokens.Add(new NoteToken(NoteToken.TokenKind.Tag, buf.ToString()));
                            i += buf.Length - 1;
                            buf.Clear();
                        }
                        break;

                    default:
                        if ((c == 'h' && note.AsSpan(i).StartsWith("http://")) ||
                            (c == 'h' && note.AsSpan(i).StartsWith("https://")) ||
                            (c == 'f' && note.AsSpan(i).StartsWith("file://"))) {
                            // URI追加
                            //
                            // 1. バッファに残っている文字列をテキストとして追加
                            // 2. URIパターンに一致する文字列を取得
                            // 3. 取得した文字列をURIとして追加
                            if (buf.Length > 0) {                               // 1
                                tokens.Add(new NoteToken(NoteToken.TokenKind.Text, buf.ToString()));
                                buf.Clear();
                            }

                            var path = URI_PATTERN().Match(note[i..]).Value;    // 2

                            if (path.Length > 0) {                              // 3
                                tokens.Add(new NoteToken(NoteToken.TokenKind.Uri, path));
                                i += path.Length - 1;
                            }
                        }
                        else if ((c == '\\' && i < str.Length - 1 && str[i + 1] == '\\') ||
                                 (((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) && i < str.Length - 2 && str[i + 1] == ':' && str[i + 2] == '\\')) {
                            // パス追加
                            //
                            // 1. バッファに残っている文字列をテキストとして追加
                            // 2. パスパターンに一致する文字列を取得
                            // 3. 取得した文字列をパスとして追加
                            if (buf.Length > 0) {                               // 1
                                tokens.Add(new NoteToken(NoteToken.TokenKind.Text, buf.ToString()));
                                buf.Clear();
                            }

                            var path = PATH_PATTERN().Match(note[i..]).Value;   // 2

                            if (path.Length > 0) {                              // 3
                                tokens.Add(new NoteToken(NoteToken.TokenKind.Path, path));
                                i += path.Length - 1;
                            }
                        }
                        else {
                            // テキスト向けに文字をバッファに追加
                            buf.Append(c);
                        }
                        break;
                }
            }

            // バッファに残っている文字列をテキストとして追加
            if (buf.Length > 0) {
                tokens.Add(new NoteToken(NoteToken.TokenKind.Text, buf.ToString()));
            }

            return tokens;
        }
    }
}
