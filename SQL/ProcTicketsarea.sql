--庫存與狀態重整成功

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 如果已經存在就先刪除
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_RebuildInventory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[USP_RebuildInventory]
GO

create PROCEDURE [dbo].[USP_RebuildInventory]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. 全量校正庫存 (使用 CTE 確保語法嚴謹)
        ;WITH CTE_Inventory AS (
            SELECT 
                ta.Remaining,
                ta.[RowCount], -- 使用中括號保護
                ta.[SeatCount],
                ISNULL(t.SoldCount, 0) AS CalculatedSoldCount
            FROM [dbo].[TicketsArea] AS ta
            LEFT JOIN (
                SELECT TicketsAreaID, COUNT(*) AS SoldCount
                FROM [dbo].[Tickets]
                WHERE TicketsStatusID IN ('P', 'S', 'Y','A')
                GROUP BY TicketsAreaID
            ) AS t ON ta.TicketsAreaID = t.TicketsAreaID
        )
        UPDATE CTE_Inventory
        SET Remaining = ([RowCount] * [SeatCount]) - CalculatedSoldCount;

        -- 2. 全量校正票區狀態
        UPDATE [dbo].[TicketsArea]
        SET TicketsAreaStatusID = CASE 
            WHEN Remaining <= 0 THEN 'O' 
            ELSE 'I' 
        END
        WHERE TicketsAreaStatusID IN ('I', 'O');

        COMMIT TRANSACTION;
        PRINT '庫存與狀態重整成功！';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT '庫存重整失敗，原因：' + @ErrorMessage;
    END CATCH
END
GO

EXEC [dbo].[USP_RebuildInventory];