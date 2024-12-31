using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TagNotes.Services.DatabaseService;

namespace TagNotes.Helper
{
    /// <summary>条件分析機能です。</summary>
    internal static class ConditionAnalisys
    {
        /// <summary>条件解析を行います。</summary>
        /// <param name="condition">条件文字列。</param>
        /// <returns>解析結果。</returns>
        public static ConditionItems ParseCondition(this string condition)
        {
            // 項目のリスト
            var items = new List<(string token, bool isesc)>();

            // 解析対象文字リスト
            var str = condition.ToCharArray();

            // 文字バッファ
            var buf = new StringBuilder();

            // エスケープフラグ
            var esc = false;
            var isesc = false;

            // 解析対象文字を処理
            for (int i = 0; i < str.Length; i++) {
                var c = str[i];
                switch (c) {
                    case '"':
                        if (esc) {
                            esc = false;
                        }
                        else if (buf.Length <= 0) {
                            esc = true;
                            isesc = true;
                        }
                        buf.Append(c);
                        break;

                    case '\\':
                        if (i < str.Length - 1 && str[i + 1] == '"') {
                            buf.Append('"');
                            i++;
                        }
                        break;

                    default:
                        if (esc || !char.IsWhiteSpace(c)) {
                            buf.Append(c);
                        }
                        else {
                            if (buf.Length > 0) {
                                items.Add((buf.ToString(), isesc));
                                isesc = false;
                            }
                            buf.Clear();
                        }
                        break;
                }
            }

            if (buf.Length > 0) {
                items.Add((buf.ToString(), isesc));
            }

            // 戻り値のリスト
            List<string> searchWords = [];
            List<string> searchTags = [];
            List<string> notSearchTags = [];

            foreach (var item in items) {
                if (item.isesc) {
                    if (item.token.Length > 2 && item.token[0] == '"' && item.token[^1] == '"') {
                        searchWords.Add(item.token[1..^1]);
                    }
                }
                else if (item.token.StartsWith('#')) {
                    searchTags.Add(item.token[1..]);
                }
                else if (item.token.StartsWith("-#")) {
                    notSearchTags.Add(item.token[2..]);
                }
                else {
                    searchWords.Add(item.token);
                }
            }

            // 条件項目を返す
            return new ConditionItems(searchWords, searchTags, notSearchTags);
        }

        /// <summary>条件項目です。</summary>
        /// <param name="SearchWords">検索ワードリスト。</param>
        /// <param name="SearchTags">検索タグリスト。</param>
        /// <param name="NotSearchTags">非検索タグリスト。</param>
        public record class ConditionItems(List<string> SearchWords, List<string> SearchTags, List<string> NotSearchTags);
    }
}
