namespace ApiFox.Migrations
{
    using ApiFox.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApiFox.DAL.ApifoxContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ApiFox.DAL.ApifoxContext";
        }

        protected override void Seed(ApiFox.DAL.ApifoxContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            var apiUrl = "testare";
            var apiName = "testul";

            var apiUrl2 = "hailatest";
            var apiName2 = "Hai la test";

            context.Apis.AddOrUpdate(p => p.ApiUrl, new Apis[]{
                new Apis { ApiName = apiName, ApiUrl = apiUrl, Owner = "victorantos@gmail.com" },
                new Apis { ApiName = apiName2, ApiUrl = apiUrl2, Owner = "victorantos@gmail.com" },
            });

            context.ApiRequests.AddOrUpdate(new ApiRequests[]{
              new ApiRequests { ApiUrl = apiUrl, DateCreated = DateTime.UtcNow.AddMinutes(-15), Api = context.Apis.Where(a => a.ApiUrl == apiUrl).SingleOrDefault() },
              new ApiRequests { ApiUrl = apiUrl2,  DateCreated = DateTime.UtcNow.AddMinutes(-27), Api = context.Apis.Where(a => a.ApiUrl == apiUrl2).SingleOrDefault() },
              new ApiRequests { ApiUrl = apiUrl2,  DateCreated = DateTime.UtcNow.AddMinutes(-1), Api = context.Apis.Where(a => a.ApiUrl == apiUrl2).SingleOrDefault() },
              new ApiRequests { ApiUrl = apiUrl, DateCreated = DateTime.UtcNow.AddDays(-1).AddMinutes(-35), Api = context.Apis.Where(a => a.ApiUrl == apiUrl).SingleOrDefault() },
              new ApiRequests { ApiUrl = apiUrl2,  DateCreated = DateTime.UtcNow.AddDays(-1).AddMinutes(-50), Api = context.Apis.Where(a => a.ApiUrl == apiUrl2).SingleOrDefault() }
            
            });
            context.SaveChanges();
        }
    }
}
