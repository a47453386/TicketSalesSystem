SELECT 
    t.TicketsID,
    t.CheckInCode,
    s.SessionID,         -- 這就是你要的場次 ID
    p.ProgrammeName,     -- 活動名稱
    s.StartTime,         -- 場次開始時間
    t.TicketsStatusID    -- 票券狀態 (A 或 Y)
FROM Tickets t
JOIN [Order] o ON t.OrderID = o.OrderID
JOIN [Session] s ON o.SessionID = s.SessionID -- 假設你的訂單表直接連結場次
JOIN Programme p ON s.ProgrammeID = p.ProgrammeID
WHERE t.CheckInCode = '5139EB84616F';

UPDATE Tickets
SET TicketsStatusID = 'A'
WHERE TicketsStatusID = 'S';


UPDATE Tickets
SET TicketsStatusID = 'A',ScannedTime = NULL
WHERE CheckInCode = '5139EB84616F';