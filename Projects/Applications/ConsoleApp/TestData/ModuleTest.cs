using Microsoft.EntityFrameworkCore;
using OnUtils.Architecture.AppCore;
using OnXap.Core;
using OnXap.Core.Db;
using OnXap.Core.MetadataObject;
using OnXap.Core.MetadataObject.Db;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace ConsoleApp.TestData
{
    using OnXap.Modules.Realty.DB;

    class context : MetadataObjectDbContextBase
    {
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                //.AddFilter((category, level) =>
                //    category == DbLoggerCategory.Database.Command.Name
                //    && level == LogLevel.Information)
                .AddConsole();
        });

        protected override void OnContextConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(MyLoggerFactory);
        }

        public DbSet<Realty> Test { get; set; }
    }

    class ModuleTest : CoreComponentBase, IComponentSingleton, IAutoStart
    {
        protected override void OnStarted()
        {
            try
            {
                using (var ctx = AppCore.Create<context>())
                {
                    var dd = ctx.Test.
                        WithMetadataObjectProperties().
                        AsNoTracking().
                        Take(2).
                        ToList();

                    var ddd = new Realty();
                    ctx.Test.Attach(ddd);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
