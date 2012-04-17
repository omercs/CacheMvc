using System;
using System.Data.Entity;

namespace MvcApplication1.Models
{
    /// <summary>
    /// Define scripts...dont forget to remove Go statements. You need to execute each command that requires go statement separately.
    /// This is for DbContext API (EFv4.1) or forward
    /// 
    /// We will just start from scratch for each launch of app
    /// </summary>
    public class MyInitializer : DropCreateDatabaseIfModelChanges<MarketContext>
    {


        public static string TableCacheTrack =
            @"  
CREATE TABLE [dbo].[CacheTrack](
	[CacheTrackId] [int] IDENTITY(1,1) NOT NULL primary key,
	[TableName] [nvarchar](50) NOT NULL,
	[UpdateTime] [datetime] NOT NULL,
 )
 ";
        #region ErrorLog Related
            
        public static string TableErrorLog =
            @" 
CREATE TABLE [dbo].[ErrorLog](
	[id] [int] IDENTITY(1,1) NOT NULL primary key,
	[UserName] [nvarchar](50) NULL,
	[ErrorNumber] [int] NULL,
	[ErrorSeverity] [int] NULL,
	[ErrorState] [int] NULL,
	[ErrorProcedure] [nvarchar](50) NULL,
	[ErrorLine] [int] NULL,
	[ErrorMessage] [nvarchar](2000) NULL )  
";

        public static string ProcPrintError = @"
 
CREATE PROCEDURE [dbo].[uspPrintError] 
AS
BEGIN
    SET NOCOUNT ON;
 
    -- Print error information. 
    PRINT 'Error ' + CONVERT(varchar(50), ERROR_NUMBER()) +
          ', Severity ' + CONVERT(varchar(5), ERROR_SEVERITY()) +
          ', State ' + CONVERT(varchar(5), ERROR_STATE()) + 
          ', Procedure ' + ISNULL(ERROR_PROCEDURE(), '-') + 
          ', Line ' + CONVERT(varchar(5), ERROR_LINE());
    PRINT ERROR_MESSAGE();
END;";
        public static string ProcLogError = @" 
 CREATE PROCEDURE [dbo].[uspLogError] 
    @ErrorLogID [int] = 0 OUTPUT  -- Contains the ErrorLogID of the row inserted
                                  -- by uspLogError in the ErrorLog table.

AS
BEGIN
    SET NOCOUNT ON;

    -- Output parameter value of 0 indicates that error 
    -- information was not logged.
    SET @ErrorLogID = 0;

    BEGIN TRY
        -- Return if there is no error information to log.
        IF ERROR_NUMBER() IS NULL
            RETURN;

        -- Return if inside an uncommittable transaction.
        -- Data insertion/modification is not allowed when 
        -- a transaction is in an uncommittable state.
        IF XACT_STATE() = -1
        BEGIN
            PRINT 'Cannot log error since the current transaction is in an uncommittable state. ' 
                + 'Rollback the transaction before executing uspLogError in order to successfully log error information.';
            RETURN;
        END;

        INSERT [dbo].[ErrorLog] 
            (
            [UserName], 
            [ErrorNumber], 
            [ErrorSeverity], 
            [ErrorState], 
            [ErrorProcedure], 
            [ErrorLine], 
            [ErrorMessage]
            ) 
        VALUES 
            (
            CONVERT(sysname, CURRENT_USER), 
            ERROR_NUMBER(),
            ERROR_SEVERITY(),
            ERROR_STATE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_MESSAGE()
            );

        -- Pass back the ErrorLogID of the row inserted
        SELECT @ErrorLogID = @@IDENTITY;
    END TRY
    BEGIN CATCH
        PRINT 'An error occurred in stored procedure uspLogError: ';
        EXECUTE [dbo].[uspPrintError];
        RETURN -1;
    END CATCH
END;  
";
        #endregion

        /// <summary>
        /// Script to create delete trigger for our column
        /// </summary>
        public static string TriggerDeleteMarketEvents =
            @" 
create TRIGGER [dbo].[dMarketEventsCode] ON [dbo].[MarketEvents] 
AFTER delete  AS 
BEGIN
    DECLARE @Count int;
--===================================================
--	After Trigger for delete
--  Trigger type is after so this will happen after all update operation is done.
--===================================================
    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;

    BEGIN TRY
        
        -- Insert record into CacheTrack if not exists, otherwise update that         
        If NOt exists (select CacheTrackId from CacheTrack where tablename = 'MarketEvents')
         
			INSERT INTO dbo.CacheTrack
				(TableName
				,UpdateTime                
				)
			SELECT 
				'MarketEvents'               
				,Getutcdate() ;               
         
        ELSE
			UPDATE dbo.CacheTrack
			SET	UpdateTime =    Getutcdate()            
			where TableName = 'MarketEvents';
         
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;
";

        public static string TriggerUpdateMarketEvents =
            @" 
CREATE TRIGGER [dbo].[uMarketEventsCode] ON [dbo].[MarketEvents] 
AFTER UPDATE,INSERT  AS 
BEGIN
    DECLARE @Count int;
--===================================================
--	After Trigger to detect changes for Campaign. we need to detect those to populate cache
--  Trigger type is after so this will happen after all update operation is done.
--===================================================
    SET @Count = @@ROWCOUNT;
    IF @Count = 0 
        RETURN;

    SET NOCOUNT ON;
	print 'uMarketEventsCode fired';
	
    BEGIN TRY
		--DEBUG
		----select 'inserted',* from inserted
		----select 'deleted',* from deleted
		----select i.Campaign  FROM Inserted i
		----	LEFT JOIN Deleted d ON i.MarketEventId = d.MarketEventId        
		----	WHERE ISNULL(i.Campaign, '<some null value>') != ISNULL(d.Campaign, '<some null value>')
			
		--Update only if column data changes
		--Update function does not care if column data changed or not,It returns true if statement contains this column
		--We can check previous value to make sure it is really a change on this column. If it is an insert op, it does not have deleted record
		--Dont forget that trigger's inserted or deleted may contain multiple rows. It fires for all the inserts,updates..
        IF UPDATE(Campaign) and          
           exists(select i.Campaign  FROM Inserted i
		 	LEFT JOIN Deleted d ON i.MarketEventId = d.MarketEventId        
			WHERE ISNULL(i.Campaign, '<some null value>') != ISNULL(d.Campaign, '<some null value>'))
		 	
			
        -- Insert record into CacheTrack if not exists, otherwise update that
        BEGIN
        print 'uMarketEventsCode add record';
            If NOt exists (select CacheTrackId from CacheTrack where tablename = 'MarketEvents')
             
				INSERT INTO dbo.CacheTrack
					(TableName
					,UpdateTime                
					)
				SELECT 
					'MarketEvents'               
					,Getutcdate() ;               
             
            ELSE
				UPDATE dbo.CacheTrack
				SET	UpdateTime =    Getutcdate()            
				where TableName = 'MarketEvents';
             

        
        END;
    END TRY
    BEGIN CATCH
        EXECUTE [dbo].[uspPrintError];

        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [dbo].[uspLogError];
    END CATCH;
END;";

        public static string ProcGetCacheUpdateTime =
            @"
 
create procedure [dbo].[sp_GetCacheUpdateTime]
@tablename nvarchar(50)
as

  select UpdateTime from dbo.CacheTrack
  where tablename = @tablename
";
        protected override void Seed(MarketContext context)
        {
            //some setup
            context.Database.ExecuteSqlCommand("SET ANSI_NULLS ON");
            context.Database.ExecuteSqlCommand("SET QUOTED_IDENTIFIER ON");
            context.Database.ExecuteSqlCommand("SET NOCOUNT ON");

            //add company
            context.Companys.Add(new Company() {Name = "ABC Inc"});
            //add user
            context.Users.Add(new User()
                                  {
                                      CreatedOn = DateTime.UtcNow,
                                      LoginMethod = 0,
                                      ModifiedOn = DateTime.UtcNow,
                                      RefKey = null,
                                      Username = "tes"
                                  });

            context.SaveChanges();

            try
            {
                 

                //first err log related
                context.Database.ExecuteSqlCommand(TableErrorLog);
                context.Database.ExecuteSqlCommand(ProcPrintError);
                context.Database.ExecuteSqlCommand(ProcLogError);

                //Ok we will create our cache track table first
                context.Database.ExecuteSqlCommand(TableCacheTrack);

                //triggers on our marketevent table
                context.Database.ExecuteSqlCommand(TriggerDeleteMarketEvents);
                context.Database.ExecuteSqlCommand(TriggerUpdateMarketEvents);
                //proc to get cache update time
                context.Database.ExecuteSqlCommand(ProcGetCacheUpdateTime);
            }
            catch (Exception ex)
            {
                //delete that now if fails. instead of cleaning manually
                context.Database.Delete();
                throw ex;
            }
            
        }
    }
}