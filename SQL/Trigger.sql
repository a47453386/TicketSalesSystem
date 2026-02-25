ALTER TRIGGER [dbo].[TR_Tickets_ReturnStock]
ON [dbo].[Tickets]
AFTER UPDATE, DELETE  -- 🚩 增加處理刪除的情況
AS
BEGIN
    SET NOCOUNT ON;

    -- 處理更新 (狀態從有效變為無效)
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        UPDATE ta
        SET ta.Remaining = ta.Remaining + cancelled.Count
        FROM [TicketsArea] ta
        INNER JOIN (
            SELECT i.TicketsAreaID, COUNT(*) AS [Count]
            FROM inserted i
            JOIN deleted d ON i.TicketsID = d.TicketsID
            WHERE i.TicketsStatusID IN ('C', 'N')       -- 新狀態是取消/逾期
              AND d.TicketsStatusID NOT IN ('C', 'N')  -- 舊狀態是正常的(P或Y)
            GROUP BY i.TicketsAreaID
        ) AS cancelled ON ta.TicketsAreaID = cancelled.TicketsAreaID;
    END
    -- 處理刪除 (如果直接刪掉 P 狀態的票券也該還庫存)
    ELSE
    BEGIN
        UPDATE ta
        SET ta.Remaining = ta.Remaining + deleted_counts.Count
        FROM [TicketsArea] ta
        INNER JOIN (
            SELECT d.TicketsAreaID, COUNT(*) AS [Count]
            FROM deleted d
            WHERE d.TicketsStatusID NOT IN ('C', 'N') -- 只有有效的票被刪除才還
            GROUP BY d.TicketsAreaID
        ) AS deleted_counts ON ta.TicketsAreaID = deleted_counts.TicketsAreaID;
    END
END