using OnXap;
using System;
using OnXap.Core.Data;
using Microsoft.EntityFrameworkCore;
using FluentMigrator.Runner;

namespace ConsoleApp
{
    class app : OnXApplication
    {
        class c : IDbConfigurationBuilder
        {
            void IDbConfigurationBuilder.OnConfigureEntityFrameworkCore(DbContextOptionsBuilder optionsBuilder)
            {
             //   optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=TestWeb;Integrated Security=true;");
            }

            bool IDbConfigurationBuilder.OnConfigureFluentMigrator(IMigrationRunnerBuilder runnerBuilder)
            {
                runnerBuilder.AddSqlServer().WithGlobalConnectionString("Data Source=localhost;Initial Catalog=TestWeb;Integrated Security=true;");
                return true;
            }
        }

        public app(): base(Environment.CurrentDirectory, new c())
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new app();
                app.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}
