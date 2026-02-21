CREATE TRIGGER [dbo].[TR_Tickets_ReturnStock]
ON [dbo].[Tickets]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    --只有當 TicketsStatusID 欄位被更新時才執行
    IF UPDATE(TicketsStatusID)
    BEGIN
        -- 更新 TicketsArea 表的剩餘票數
        UPDATE ta
        SET ta.Remaining = ta.Remaining + cancelled.Count--cancelled庫存的回收站
        FROM [TicketsArea] ta
        INNER JOIN (
            -- 統計「新狀態為 C 或 N」且「舊狀態不是 C 或 N」的票數
            -- 這樣可以避免重複加回庫存
            SELECT i.TicketsAreaID, COUNT(*) AS [Count]
            FROM inserted i
            JOIN deleted d ON i.TicketsID = d.TicketsID
            WHERE i.TicketsStatusID IN ('C', 'N')  -- 新狀態是取消或逾期
              AND d.TicketsStatusID NOT IN ('C', 'N') -- 舊狀態是有效的
            GROUP BY i.TicketsAreaID
        ) AS cancelled ON ta.TicketsAreaID = cancelled.TicketsAreaID;
    END
END