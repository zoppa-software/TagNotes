using TagNotes.Helper;

namespace TagNotesTest
{
    public class ConditionAnalisysTest
    {
        [Fact]
        public void Test1()
        {
            var a = "\"1 2 3\" #tag -#tag1 \"#111\"".ParseCondition();

            Assert.Equal("1 2 3", a.SearchWords[0]);
            Assert.Equal("#111", a.SearchWords[1]);
            Assert.Equal("tag", a.SearchTags[0]);
            Assert.Equal("tag1", a.NotSearchTags[0]);
        }
    }
}