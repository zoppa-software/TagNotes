using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;

namespace TagNotesTest.Models
{
    public class ListPageModelTest
    {
        private Mock<ILoggerFactory> mock = new Mock<ILoggerFactory>();

        public ListPageModelTest() 
        { 
        }

        [Fact]
        public void Test1()
        {
            /*
            var a = "\"1 2 3\" #tag -#tag1 \"#111\"".ParseCondition();

            Assert.Equal(2, a.SearchWords.Count);
            Assert.Equal("1 2 3", a.SearchWords[0]);
            Assert.Equal("#111", a.SearchWords[1]);

            Assert.Single(a.SearchTags);
            Assert.Equal("tag", a.SearchTags[0]);

            Assert.Single(a.NotSearchTags);
            Assert.Equal("tag1", a.NotSearchTags[0]);
            */
        }
    }
}
