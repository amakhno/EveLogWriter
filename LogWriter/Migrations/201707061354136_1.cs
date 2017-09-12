namespace LogWriter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.LogPositions");

            CreateTable(
                "dbo.LogPositions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        CharacterName = c.String(),
                        OreType = c.String(),
                        Count = c.Int(nullable: false),
                        UploadTime = c.DateTime(nullable: false),
                        AdminName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LogPositions");
        }
    }
}
