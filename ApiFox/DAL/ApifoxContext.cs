using ApiFox.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;

namespace ApiFox.DAL
{
    public class ApifoxContext : DbContext
    {
        public ApifoxContext():base("DefaultConnection")
        {
            
        }
        public DbSet<ImportedFile> ImportedFiles { get; set; }
        public DbSet<Apis> Apis { get; set; }
        public DbSet<ApiRequests> ApiRequests { get; set; }

        public static ApifoxContext Create()
        {
            return new ApifoxContext();
        }
    }
}