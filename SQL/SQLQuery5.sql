SELECT 
    TicketsAreaName,
    ([RowCount] * SeatCount) AS TotalSeats,
    ISNULL((SELECT COUNT(*) FROM Tickets WHERE TicketsAreaID = ta.TicketsAreaID AND TicketsStatusID IN ('P','S','Y')), 0) AS SoldCount,
    ([RowCount] * SeatCount) - ISNULL((SELECT COUNT(*) FROM Tickets WHERE TicketsAreaID = ta.TicketsAreaID AND TicketsStatusID IN ('P','S','Y')), 0) AS Calc_Remaining,
    Remaining AS Current_Remaining
FROM TicketsArea ta;


EXEC [dbo].[USP_RebuildInventory]


SELECT TicketsStatusID, COUNT(*) as Count
FROM Tickets
GROUP BY TicketsStatusID


SELECT 
    TicketsAreaName,
    ([RowCount] * SeatCount) AS [總座位],
    ISNULL(t.SoldCount, 0) AS [系統數出的票],
    Remaining AS [目前庫存欄位]
FROM TicketsArea ta
LEFT JOIN (
    SELECT TicketsAreaID, COUNT(*) AS SoldCount
    FROM Tickets
    WHERE TicketsStatusID IN ('P', 'S', 'Y', 'A')
    GROUP BY TicketsAreaID
) t ON ta.TicketsAreaID = t.TicketsAreaID
WHERE ta.TicketsAreaID = '202603030103';