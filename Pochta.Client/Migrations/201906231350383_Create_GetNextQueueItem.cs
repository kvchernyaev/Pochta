namespace Pochta.Client.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Create_GetNextQueueItem : DbMigration
    {
        public override void Up()
        {
            string sql = @"
create or alter proc GetNextQueueItem as
begin
	; with cte as
	(
		select top 1 t.*
		from QueueItems t with(xlock, rowlock) /*так защищаемся от параллельных выборок - гаратированно возьмется только одним клиентом*/
		where (isnull(t.SuccessfullySent,0)=0) and 
			(t.LastRetryDate is null or datediff(second, t.LastRetryDate, getdate()) > 10)
		order by t.TakedCount asc /*nulls first*/, t.LastRetryDate asc,
			t.id asc
	)
	update cte
	set TakedCount = isnull(TakedCount,0) + 1, LastRetryDate = getdate()
	output inserted.*
end
";
            this.Sql(sql);
        }
        
        public override void Down()
        {
            this.Sql("DROP PROCEDURE dbo.GetNextQueueItem ");
        }
    }
}
