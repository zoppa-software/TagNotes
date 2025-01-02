using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;

namespace TagNotesTest
{
    public class ConditionAnalisysTest
    {
        [Fact]
        public void Test1()
        {
            var a = "\"1 2 3\" #tag -#tag1 \"#111\"".ParseCondition();

            Assert.Equal(2, a.SearchWords.Count);
            Assert.Equal("1 2 3", a.SearchWords[0]);
            Assert.Equal("#111", a.SearchWords[1]);

            Assert.Single(a.SearchTags);
            Assert.Equal("tag", a.SearchTags[0]);

            Assert.Single(a.NotSearchTags);
            Assert.Equal("tag1", a.NotSearchTags[0]);
        }

        [Fact]
        public void Test2()
        {
            var a = "testword".ParseCondition();

            Assert.Single(a.SearchWords);
            Assert.Equal("testword", a.SearchWords[0]);

            Assert.Empty(a.SearchTags);
            Assert.Empty(a.NotSearchTags);
        }

        [Fact]
        public void Test3()
        {
            var a = @"""1 \""2 3""".ParseCondition();

            Assert.Single(a.SearchWords);
            Assert.Equal("1 \"2 3", a.SearchWords[0]);

            Assert.Empty(a.SearchTags);
            Assert.Empty(a.NotSearchTags);
        }
    }
}