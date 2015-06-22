namespace ApiFox.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApiRequest2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApiRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApiId = c.Int(nullable: false),
                        ApiUrl = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Apis", t => t.ApiId, cascadeDelete: true)
                .Index(t => t.ApiId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApiRequests", "ApiId", "dbo.Apis");
            DropIndex("dbo.ApiRequests", new[] { "ApiId" });
            DropTable("dbo.ApiRequests");
        }
    }
}
