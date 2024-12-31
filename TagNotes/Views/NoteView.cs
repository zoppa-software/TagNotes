using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;
using TagNotes.Services;

namespace TagNotes.Views
{
    /// <summary>メモの表示を行うビュー。</summary>
    /// <param name="index">メモのインデックス。</param>
    /// <param name="information">メモの内容。</param>
    /// <param name="notificationTime">通知する日時。</param>
    /// <param name="updateTime">更新日時。</param>
    internal sealed class NoteView(long index, string information, DateTime? notificationTime, DateTime updateTime, List<DatabaseService.Tag> tags)
    {
        /// <summary>メモのインデックスを取得します。</summary>
        public long Index { get; } = index;

        /// <summary>メモの内容を取得します。</summary>
        public string Information { get; } = information;

        /// <summary>通知する日時を取得します。</summary>
        public DateTime? NotificationTime { get; } = notificationTime;

        /// <summary>更新日時を取得します。</summary>
        public DateTime UpdateTime { get; } = updateTime;

        /// <summary>タグリストを取得します。</summary>
        public List<string> Tags { get; } = tags.Select(v => v.TagString).ToList();

        /// <summary>メモを解析してInlineのリストを取得します。</summary>
        public ObservableCollection<Inline> Inlines {
            get {
                // 戻り値のリスト
                var res = new ObservableCollection<Inline>();

                // メモを解析してトークンに分解
                var tokens = NoteAnalisys.ParseNote(this.Information);

                // トークンを表示要素に変換
                foreach (var token in tokens) {
                    switch (token.Kind) {
                        case NoteToken.TokenKind.Text:
                            // テキスト要素
                            res.Add(new Run { Text = token.Value });
                            break;

                        case NoteToken.TokenKind.Tag:
                            // タグ要素
                            res.Add(new Run { 
                                Text = token.Value, 
                                Foreground = Application.Current.Resources["tagColor"] as Brush
                            });
                            break;

                        case NoteToken.TokenKind.Uri:
                            // URI要素
                            res.Add(new Hyperlink { 
                                NavigateUri = new Uri(token.Value), 
                                Inlines = { new Run { Text = token.Value } } 
                            });
                            break;

                        case NoteToken.TokenKind.Path:
                            // パス要素
                            // 1. パス文字列
                            // 2. パス文字列をクリックしたときの処理を設定
                            var fpath = new Hyperlink {     // 1
                                Inlines = { new Run { Text = token.Value } }
                            };
                            fpath.Click += (s, e) => {      // 2
                                var path = token.Value;
                                if (System.IO.File.Exists(path)) {
                                    System.Diagnostics.Process.Start(path);
                                }
                                else {
                                    System.Diagnostics.Process.Start("explorer.exe", path);
                                }
                            };
                            res.Add(fpath);
                            break;
                    }
                }
                return res;
            }
        }
    }
}
