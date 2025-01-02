using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;

namespace TagNotesTest.Helper
{
    public class TipsHelperTest
    {
        [Fact]
        public void Test1()
        {
            var a = TipsHelper.ExePath;
            Assert.True(System.IO.Directory.Exists(a));
        }

        [Fact]
        public void Test2()
        {
            var lst1 = new List<int> { 1, 2, 3 };
            var lst2 = new List<int> { 4, 5, 6 };

            lst1.Rewrite(lst2);

            Assert.Equal(3, lst1.Count);
            Assert.Equal(4, lst1[0]);
            Assert.Equal(5, lst1[1]);
            Assert.Equal(6, lst1[2]);
        }
    }
}
