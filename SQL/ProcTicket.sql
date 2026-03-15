CREATE OR ALTER PROCEDURE [dbo].[USP_RebuildInventory]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 🚩 修正：將 A.Total 改為 (A.RowCount * A.SeatCount)
    -- 🚩 修正：加入 CASE WHEN 防止計算結果變成負數，避免觸發 547 錯誤
    UPDATE A
    SET A.Remaining = CASE 
        WHEN ((A.[RowCount] * A.SeatCount) - ISNULL(T.SoldCount, 0)) < 0 THEN 0 
        ELSE ((A.[RowCount] * A.SeatCount) - ISNULL(T.SoldCount, 0)) 
    END
    FROM TicketsArea A
    LEFT JOIN (
        SELECT TicketsAreaID, COUNT(*) AS SoldCount
        FROM Tickets
        WHERE TicketsStatusID IN ('P', 'S', 'Y', 'A') -- 鎖定、已售、進場、待進場
        GROUP BY TicketsAreaID
    ) T ON A.TicketsAreaID = T.TicketsAreaID;

    PRINT 'Inventory Rebuild Completed (Safety Mode).';
END;
GO

-- 🚩 修改完資料庫後，記得執行一次：
EXEC [dbo].[USP_RebuildInventory];


-- 建立唯一索引，防止 Session + Area + Row + Seat 產生重複紀錄
CREATE UNIQUE INDEX IX_Tickets_Seat_Unique 
ON [dbo].[Tickets] (SessionID, TicketsAreaID, RowIndex, SeatIndex);
GO

-- 1. 清除重複座位，只保留最新的一筆 (TicketsID 最大者)
WITH DuplicateCTE AS (
    SELECT 
        TicketsID, 
        ROW_NUMBER() OVER (
            PARTITION BY SessionID, TicketsAreaID, RowIndex, SeatIndex 
            ORDER BY TicketsID DESC
        ) AS RowNum
    FROM Tickets
)
DELETE FROM DuplicateCTE WHERE RowNum > 1;
GO

SELECT 
    TicketsAreaID, 
    TicketsAreaName, 
    ([RowCount] * SeatCount) AS [容量上限],
    (SELECT COUNT(*) FROM Tickets T 
     WHERE T.TicketsAreaID = A.TicketsAreaID 
     AND T.TicketsStatusID IN ('P', 'S', 'Y', 'A')) AS [目前票數],
    (([RowCount] * SeatCount) - (SELECT COUNT(*) FROM Tickets T WHERE T.TicketsAreaID = A.TicketsAreaID AND T.TicketsStatusID IN ('P', 'S', 'Y', 'A'))) AS [計算出的剩餘數]
FROM TicketsArea A
WHERE ([RowCount] * SeatCount) < (
    SELECT COUNT(*) FROM Tickets T 
    WHERE T.TicketsAreaID = A.TicketsAreaID 
    AND T.TicketsStatusID IN ('P', 'S', 'Y', 'A')
);

SELECT * FROM [Order] O
WHERE NOT EXISTS (
    SELECT 1 
    FROM Tickets T 
    WHERE T.OrderID = O.OrderID
);

DELETE O
FROM [Order] O
WHERE NOT EXISTS (
    SELECT 1 
    FROM Tickets T 
    WHERE T.OrderID = O.OrderID
);

-- 執行完後，看看影響了幾列
PRINT '空殼訂單清理完畢！';