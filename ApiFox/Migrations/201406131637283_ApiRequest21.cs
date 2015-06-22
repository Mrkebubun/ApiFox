namespace ApiFox.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApiRequest21 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApiRequests", "Ip", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApiRequests", "Ip");
        }
    }
}
