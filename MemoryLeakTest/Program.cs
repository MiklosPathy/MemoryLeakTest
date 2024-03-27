using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFMemoryLeakTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dbname = "TestDB";
            string dbfile = $"{dbname}.sqlite";
            string connectionstring = $"Data Source={dbfile}";

            //CreateSqliteTestDataBase(dbfile, connectionstring);
            CreateMemoryTestDataBase(dbname);

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<TestHostedService>();
            //builder.Services.AddDbContextFactory<TestBDContext>((ob) => ob.UseSqlite(connectionstring));
            builder.Services.AddDbContextFactory<TestBDContext>((ob) => ob.UseInMemoryDatabase(databaseName: dbname));

            IHost host = builder.Build();
            host.Run();
        }

        private static void CreateSqliteTestDataBase(string dbfile, string connectionstring)
        {
            if (!File.Exists(dbfile))
            {
                var options = new DbContextOptionsBuilder<TestBDContext>().UseSqlite(connectionstring).Options;
                var dbContext = new TestBDContext(options);
                dbContext.Database.Migrate();

                PopulateDB(dbContext, 100000);
            }
        }

        private static void CreateMemoryTestDataBase(string dbname)
        {
            var options = new DbContextOptionsBuilder<TestBDContext>().UseInMemoryDatabase(databaseName: dbname).Options;
            var dbContext = new TestBDContext(options);
            PopulateDB(dbContext, 100000);
        }

        private static void PopulateDB(TestBDContext dbContext, int recordnumber)
        {
            Faker faker = new Faker();

            Console.WriteLine("Generating Test DataBase.");
            Console.WriteLine();

            for (int i = 1; i <= recordnumber; i++)
            {
                dbContext.TestEntities.Add(new TestEntity { Id = i, Name = faker.Name.FullName(), Description = faker.Lorem.Text() });
                if (i % 1000 == 0)
                {
                    dbContext.SaveChanges();
                    Console.Write($"\r{i}/{recordnumber}");
                }
            }
            Console.WriteLine();
            dbContext.SaveChanges();
        }
    }
}
