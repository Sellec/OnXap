using OnXap;
using System;
using System.IO;
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
                var path = Path.Combine(Environment.CurrentDirectory, "mydb.db");
                optionsBuilder.UseSqlite("Data Source=" + path + "");
            }

            bool IDbConfigurationBuilder.OnConfigureFluentMigrator(IMigrationRunnerBuilder runnerBuilder)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "mydb.db");
                runnerBuilder.AddSQLite().WithGlobalConnectionString("Data Source=" + path + "");
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
