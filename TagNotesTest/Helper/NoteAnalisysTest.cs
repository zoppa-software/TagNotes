using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;

namespace TagNotesTest.Helper
{
    public class NoteAnalisysTest
    {
        [Fact]
        public void Test1()
        {
            var a =
@"#TAG1 は検索タグです。
#TAG1 は複数定義できます。".ParseNote();

            Assert.Equal(4, a.Count);
            Assert.Equal(NoteToken.TokenKind.Tag, a[0].Kind);
            Assert.Equal("#TAG1", a[0].Value);
            Assert.Equal(NoteToken.TokenKind.Text, a[1].Kind);
            Assert.Equal(" は検索タグです。\r\n", a[1].Value);
            Assert.Equal(NoteToken.TokenKind.Tag, a[2].Kind);
            Assert.Equal("#TAG1", a[2].Value);
            Assert.Equal(NoteToken.TokenKind.Text, a[3].Kind);
            Assert.Equal(" は複数定義できます。", a[3].Value);
        }

        [Fact]
        public void Test2()
        {
            var a =
@"https://www.bing.com/search?q=github+readme+%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9&qs=n&form=QBRE&sp=-1&lq=0&pq=github+readme+%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9&sc=12-19&sk=&cvid=C2D0D297087C4C94B35979F906CA2C6B&ghsh=0&ghacc=0&ghpl=
https://www.google.co.jp/
file:///G:/TagNotes/DesignDoc.html
G:\source\TagNotes\TagNotes\bin\x86\Debug\net8.0-windows10.0.19041.0\win-x86\db".ParseNote();

            Assert.Equal(7, a.Count);
            Assert.Equal(NoteToken.TokenKind.Uri, a[0].Kind);
            Assert.Equal(@"https://www.bing.com/search?q=github+readme+%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9&qs=n&form=QBRE&sp=-1&lq=0&pq=github+readme+%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9&sc=12-19&sk=&cvid=C2D0D297087C4C94B35979F906CA2C6B&ghsh=0&ghacc=0&ghpl=", a[0].Value);
            Assert.Equal(NoteToken.TokenKind.Text, a[1].Kind);
            Assert.Equal("\r\n", a[1].Value);
            Assert.Equal(NoteToken.TokenKind.Uri, a[2].Kind);
            Assert.Equal(@"https://www.google.co.jp/", a[2].Value);
            Assert.Equal(NoteToken.TokenKind.Text, a[3].Kind);
            Assert.Equal("\r\n", a[3].Value);
            Assert.Equal(NoteToken.TokenKind.Uri, a[4].Kind);
            Assert.Equal(@"file:///G:/TagNotes/DesignDoc.html", a[4].Value);
            Assert.Equal(NoteToken.TokenKind.Text, a[5].Kind);
            Assert.Equal("\r\n", a[5].Value);
            Assert.Equal(NoteToken.TokenKind.Path, a[6].Kind);
            Assert.Equal(@"G:\source\TagNotes\TagNotes\bin\x86\Debug\net8.0-windows10.0.19041.0\win-x86\db", a[6].Value);
        }
    }
}
