select *from [Tickets] order by CreatedTime desc


UPDATE T
SET T.TicketsStatusID = 'A'
FROM Tickets T
INNER JOIN [Session] S ON T.SessionID = S.SessionID
WHERE T.TicketsStatusID = 'S' -- 只有目前是鎖定狀態的才處理
  AND S.StartTime <= DATEADD(day, 15, GETDATE()); -- 演出日期在 15 天內




  --票券方法 (更新票券狀態)
  CREATE PROCEDURE sp_UnlockTicketsBySchedule
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 執行解鎖更新
    UPDATE T
    SET T.TicketsStatusID = 'A'
    FROM Tickets T
    INNER JOIN [Session] S ON T.SessionID = S.SessionID
    WHERE T.TicketsStatusID = 'S'
      AND S.StartTime <= DATEADD(day, 15, GETDATE());
      
    -- (選填) 也可以順便記錄一下這次更新了多少張票
    PRINT '解鎖成功，影響行數：' + CAST(@@ROWCOUNT AS VARCHAR);
END