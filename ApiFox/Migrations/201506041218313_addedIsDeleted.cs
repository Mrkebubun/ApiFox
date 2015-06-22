namespace ApiFox.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedIsDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Apis", "IsDeleted", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Apis", "IsDeleted");
        }
    }
}
