using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagNotes.Helper;
using TagNotes.Services;

namespace TagNotesTest.Services
{
    public class DatabaseServiceTest
    {
        public DatabaseServiceTest()
        {
            if (System.IO.File.Exists("test.db")) {
                System.IO.File.Delete("test.db");
            }
        }

        [Fact]
        public async void Test1()
        {
            // モックの設定
            var mock = new Mock<IDatabaseConnectionService>();
            mock.Setup(
                method => method.GetDbConnection()
            ).Returns(
                () => new SQLiteConnection("Data Source=test.db")
            );
            mock.Setup(
                method => method.IsNewDatabase()
            ).Returns(
                () => true
            );

            // サービスの作成
            var service = new DatabaseService(new TestLoggerFactory(), mock.Object);

            // 初期化
            await service.Initialize();
            Assert.True(System.IO.File.Exists("test.db"));

            // 2回目の初期化
            mock.Setup(
                method => method.IsNewDatabase()
            ).Returns(
                () => false
            );
            await service.Initialize();
            Assert.True(System.IO.File.Exists("test.db"));
        }
    }
}
