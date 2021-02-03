using Grillbot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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
            try
            {
                using var repository = CreateRepository();

                var configs = repository.GlobalConfigRepository.GetAllItems().ToList();
                Data = configs.ToDictionary(o => o.Item1, o => o.Item2);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private IGrillBotRepository CreateRepository()
        {
            var builder = new DbContextOptionsBuilder<GrillBotContext>();
            builder.UseSqlServer(ConnectionString);

            var context = new GrillBotContext(builder.Options);
            return new GrillBotRepository(context);
        }
    }
}
