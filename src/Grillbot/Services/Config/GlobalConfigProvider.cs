using Grillbot.Database;
using Grillbot.Database.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Grillbot.Services.Config
{
    public class GlobalConfigProvider : ConfigurationProvider
    {
        private string ConnectionString { get; }

        public GlobalConfigProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public override void Load()
        {
            using var repository = CreateRepository();

            var configs = repository.GetAllItems().ToList();
            Data = configs.ToDictionary(o => o.Item1, o => o.Item2);
        }

        private GlobalConfigRepository CreateRepository()
        {
            var builder = new DbContextOptionsBuilder<GrillBotContext>();
            builder.UseSqlServer(ConnectionString);

            var context = new GrillBotContext(builder.Options);
            return new GlobalConfigRepository(context);
        }
    }
}
