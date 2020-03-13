using System;
using System.Collections.Generic;
using System.Configuration;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace OnXap.Site
{
    using Core.Data;

    public class MvcApplication : HttpApplicationBase
    {
        class c : IDbConfigurationBuilder
        {
            void IDbConfigurationBuilder.OnConfigureEntityFrameworkCore(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }

            bool IDbConfigurationBuilder.OnConfigureFluentMigrator(IMigrationRunnerBuilder runnerBuilder)
            {
                runnerBuilder.AddSqlServer().WithGlobalConnectionString(ConnectionString);
                return true;
            }

            private string ConnectionString
            {
                get
                {
                    try
                    {
                        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                        if (!Debug.IsDeveloper)
                            connectionString = ConfigurationManager.ConnectionStrings["ServerConnection"].ConnectionString;

                        return connectionString;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("MvcApplication.ConnectionString: " + ex.ToString());
                    }

                    return "";
                }
            }
        }

        protected override void OnAfterApplicationStart()
        {
            try
            {
                var resourceManager = AppCore.Get<Core.Storage.ResourceProvider>();
                var physicalApplicationPath = AppCore.ApplicationWorkingFolder;
                var paths = new List<string>();

                if (AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("OnXap.Binding.AspNetMvc,")).Count() > 0)
                    paths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath, "../../Binding/AspNetMvc/Library")));

                if (AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("OnXap.Standard.AspNetMvc,")).Count() > 0)
                    paths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath, "../../Standard/AspNetMvc/JS/dist")));

                if (AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("OnXap.Standard.AspNetMvc,")).Count() > 0)
                    paths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath, "../../Standard/AspNetMvc/Library")));

                if (AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("OnXap.Standard.AspNetMvc,")).Count() > 0)
                    paths.Add(Path.GetFullPath(Path.Combine(physicalApplicationPath, "../../Standard/AspNetMvc/SourceHelper/bin/ExternalSources")));

                resourceManager.SourceDevelopmentPathList.AddRange(paths);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnAfterApplicationStart.Development: {0}", ex.Message);
                throw;
            }
        }

        protected override void OnBeginRequest()
        {
            base.OnBeginRequest();
            //AppCore.GetUserContextManager().SetCurrentUserContext(AppCore.GetUserContextManager().CreateGuestUserContext());
           // AppCore.GetUserContextManager().SetCurrentUserContext(AppCore.GetUserContextManager().GetSystemUserContext());
        }

        protected override IDbConfigurationBuilder GetDbConfigurationBuilder()
        {
            return new c();
        }
    }
}