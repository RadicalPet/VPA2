namespace VPA2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class clients : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        clientId = c.Int(nullable: false, identity: true),
                        firstName = c.String(nullable: false),
                        lastName = c.String(nullable: false),
                        email = c.String(),
                        message = c.String(),
                        uniqueToken = c.String(),
                    })
                .PrimaryKey(t => t.clientId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        clientId = c.Int(nullable: false),
                        documentName = c.String(),
                        documentExtension = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Documents");
            DropTable("dbo.Clients");
        }
    }
}
