namespace ApiFox.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewFileStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ImportedFiles", "IsRejected", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportedFiles", "IsRejected");
        }
    }
}
