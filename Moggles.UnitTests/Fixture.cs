using Microsoft.EntityFrameworkCore;
using Moggles.Data.SQL;

namespace Moggles.UnitTests
{
    public class Fixture
    {
        public Fixture(){}

        public static TogglesContext GetTogglesContext(string testName)
        {
            var options = new DbContextOptionsBuilder<TogglesContext>()
                .UseInMemoryDatabase(databaseName: testName)
                .Options;
            var context = new TogglesContext(options);
            return context;
        }

        public static DbContextOptions<TogglesContext> GetOptions(string testName)
        {
            return new DbContextOptionsBuilder<TogglesContext>()
                .UseInMemoryDatabase(testName)
                .Options;
        }
    }

}