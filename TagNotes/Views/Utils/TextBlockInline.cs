using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TagNotes.Views.Utils
{
    /// <summary>TextBlockのInlineプロパティをバインド可能にします。</summary>
    internal sealed class TextBlockInline
    {
        /// <summary>TextBlockのInlineプロパティのバインドをサポートするためのインターフェース。</summary>
        public interface ITextBlockInline
        {
            /// <summary>表示する項目情報を取得する。</summary>
            ObservableCollection<Inline> Inlines { get; }
        }

        /// <summary>取得プロパティを実装します。</summary>
        /// <param name="element">対象エレメント。</param>
        /// <returns>プロパティの値。</returns>
        /// <exception cref="ArgumentNullException">対象エレメントが nullで発生。</exception>
        public static object GetInlines(DependencyObject element)
        {
            if (element != null) {
                return element.GetValue(InlinesProperty);
            }
            else {
                throw new ArgumentNullException("GetInlines null element:" + element);
            }
        }

        /// <summary>設定プロパティを実装します。</summary>
        /// <param name="element">対象エレメント。</param>
        /// <param name="value">設定する値。</param>
        /// <exception cref="ArgumentNullException">対象エレメントが nullで発生。</exception>
        public static void SetInlines(DependencyObject element, object value)
        {
            if (element != null) {
                element.SetValue(InlinesProperty, value);
            }
            else {
                throw new ArgumentNullException("SetInlines null element:" + element);
            }
        }

        /// <summary>依存関係プロパティの登録。</summary>
        public static DependencyProperty InlinesProperty = 
            DependencyProperty.RegisterAttached(
                "Inlines", 
                typeof(object), 
                typeof(TextBlockInline), 
                new PropertyMetadata(null, Inlines_ChangedCallback)
            );

        /// <summary>Inlineプロパティの変更時に呼び出されるコールバック。</summary>
        /// <param name="depen">依存対象オブジェクト。</param>
        /// <param name="ev">変更イベントデータ。</param>
        public static void Inlines_ChangedCallback(DependencyObject depen, DependencyPropertyChangedEventArgs ev)
        {
            var tb = depen as TextBlock;
            if (tb != null) {
                // 依存対象のTextBlockのInlineをクリア
                tb.Inlines.Clear();

                // 新しいInlineを追加
                if (ev.NewValue != null) {
                    foreach (var v in (ev.NewValue as IList<Inline> ?? [])) {
                        tb.Inlines.Add(v);
                    }
                }
            }
        }
    }
}
