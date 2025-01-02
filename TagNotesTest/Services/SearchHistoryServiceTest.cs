using Moq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Services;

namespace TagNotesTest.Services
{
    public class SearchHistoryServiceTest
    {
        [Fact]
        public async void Test1()
        {
            System.IO.File.Copy("Resources\\db\\search_test.db", "Resources\\db\\search_test_tmp.db", true);

            // モックの設定
            var mock = new Mock<IDatabaseConnectionService>();
            mock.Setup(
                method => method.GetDbConnection()
            ).Returns(
                () => new SQLiteConnection("Data Source=Resources\\db\\search_test_tmp.db")
            );

            // サービスの作成
            var dbService = new DatabaseService(new TestLoggerFactory(), mock.Object);
            var service = new SearchHistoryService(new TestLoggerFactory(), dbService);

            foreach (var cond in new string[] {
                "cond1", "cond2", "cond3", "cond4", "cond5",
                "cond6", "cond7", "cond8", "cond9", "cond10"
                }) {
                await service.UpdateSearchHistory(cond);
            }

            // 検索履歴の取得
            var lastest1 = service.LatestSearchCondition;
            Assert.Equal("cond10", lastest1.Command);

            // 比較1
            var history1 = service.SearchConditionHistory;
            Assert.Equal(10, history1.Count);
            Assert.Equal(
                new string[] { 
                    "cond10", "cond9", "cond8", "cond7", "cond6", 
                    "cond5", "cond4", "cond3", "cond2", "cond1"
                }, 
                service.SearchConditionHistory.Select(v => v.Command).ToArray()
            );

            // 比較2
            await service.UpdateSearchHistory("");
            var history2 = service.SearchConditionHistory;
            Assert.Equal(10, history2.Count);
            Assert.Equal(
                new string[] {
                    "cond10", "cond9", "cond8", "cond7", "cond6",
                    "cond5", "cond4", "cond3", "cond2", "cond1"
                },
                service.SearchConditionHistory.Select(v => v.Command).ToArray()
            );

            // 比較3
            await service.UpdateSearchHistory("cond3");
            var history3 = service.SearchConditionHistory;
            Assert.Equal(10, history3.Count);
            Assert.Equal(
                new string[] {
                    "cond3", "cond10", "cond9", "cond8", "cond7", 
                    "cond6", "cond5", "cond4", "cond2", "cond1"
                },
                service.SearchConditionHistory.Select(v => v.Command).ToArray()
            );

            // 比較4
            await service.UpdateSearchHistory("cond11");
            var history4 = service.SearchConditionHistory;
            Assert.Equal(10, history4.Count);
            Assert.Equal(
                new string[] {
                    "cond11", "cond3", "cond10", "cond9", "cond8", 
                    "cond7", "cond6", "cond5", "cond4", "cond2"
                },
                service.SearchConditionHistory.Select(v => v.Command).ToArray()
            );
        }
    }
}
