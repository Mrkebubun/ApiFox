namespace ApiFox.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Apis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApiName = c.String(),
                        ApiUrl = c.String(),
                        Owner = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        FileSource_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ImportedFiles", t => t.FileSource_Id)
                .Index(t => t.FileSource_Id);
            
            CreateTable(
                "dbo.ImportedFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Ip = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Apis", "FileSource_Id", "dbo.ImportedFiles");
            DropIndex("dbo.Apis", new[] { "FileSource_Id" });
            DropTable("dbo.ImportedFiles");
            DropTable("dbo.Apis");
        }
    }
}
