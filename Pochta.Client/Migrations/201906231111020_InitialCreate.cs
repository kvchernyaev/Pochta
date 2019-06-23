namespace Pochta.Client.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QueueItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Message = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        LastRetryDate = c.DateTime(),
                        TakedCount = c.Int(),
                        SuccessfullySent = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.QueueItems");
        }
    }
}
