--SessionID
create function funGetSessionID (@ProID nchar(8))
	returns nchar(10)
	as
	begin
	 declare @CurrentMax int,@NewSessionID nchar(10)

	 --從 Session 表中抓取該活動目前的最後兩位數最大值
	 select @CurrentMax =max(cast(right(SessionID,2)as int))
	 from [Session]
	 where SessionID like @ProID+'%';

	Set @NewSessionID = @ProID + RIGHT('00'+ CAST(isnull(@CurrentMax,0)+1 as varchar),2)

	return @NewSessionID;
	end

	go
----------------------------------------------------------------------------------------------------------
--TicketsAreaID
	create function funGetTicketsAreaID (@Sess nchar(10))
	returns nchar(12)
	as
	begin
	 declare @CurrentMax int,@NewTicketsAreaID nchar(12)

	 --從 Session 表中抓取該活動目前的最後兩位數最大值
	 select @CurrentMax =max(cast(right(TicketsAreaID,2)as int))
	 from TicketsArea
	 where TicketsAreaID like @Sess+'%';

	Set @NewTicketsAreaID = @Sess + RIGHT('00'+ CAST(isnull(@CurrentMax,0)+1 as varchar),2)

	return @NewTicketsAreaID;
	end

	 go
	

	 print dbo.funGetTicketsAreaID(2026020101);
	 go

----------------------------------------------------------------------------------------------------------
--FAQTypeF1~F9

--create function funGetFAQTypeID ()
--	returns nchar(2)
--	as
--	begin
--	 declare @DatePrefix nchar(2),@LastOID int,@NewFAQTypeID nchar(2)

	 

--	 select @LastOID= isnull (max(cast(right(FAQTypeID,1) as int)),0) 
--	 from FAQType
--	 where FAQTypeID like 'F%';

--	 set @LastOID+=1

--	 set @NewFAQTypeID='F'+cast(@LastOID as nchar(1))

--	 return @NewFAQTypeID
--	 go

--	 SELECT dbo.funGetFAQTypeID();

--FAQTypeA1~Z9

ALTER FUNCTION funGetFAQTypeID ()
RETURNS nchar(2)
AS
BEGIN
    DECLARE @TargetLetter char(1)
    DECLARE @NewNum int
    DECLARE @NewFAQTypeID nchar(2)
    DECLARE @CurrentAscii int = 65 -- 從 'A' (ASCII 65) 開始檢查

    -- 1. 外部迴圈：從 A 跑到 Z
    WHILE @CurrentAscii <= 90 
    BEGIN
        SET @TargetLetter = CHAR(@CurrentAscii)

        -- 2. 內部檢查：在當前字母下，找 1-9 號第一個沒人用的數字
        -- 我們利用一個虛擬的 1-9 表來跟資料庫比對
        SELECT TOP 1 @NewNum = t1.Num
        FROM (
            SELECT 1 AS Num UNION SELECT 2 UNION SELECT 3 UNION 
            SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION 
            SELECT 7 UNION SELECT 8 UNION SELECT 9
        ) AS t1
        LEFT JOIN (
            -- 找出資料庫中「該字母」開頭且符合「字母+數字」格式的編號
            SELECT TRY_CAST(RIGHT(RTRIM(FAQTypeID), 1) AS int) AS UsedNum
            FROM FAQType
            WHERE FAQTypeID LIKE @TargetLetter + '[1-9]'
        ) AS t2 ON t1.Num = t2.UsedNum--請把左邊的數字，拿去跟右邊已使用的數字對對看。
        WHERE t2.UsedNum IS NULL -- 只要 UsedNum 是 NULL，代表這個數字沒人用
        ORDER BY t1.Num ASC -- 確保拿最小的那個數字 (例如缺 2 和 5，先拿 2)

        -- 3. 如果在這個字母找到了可用數字
        IF @NewNum IS NOT NULL
        BEGIN
            SET @NewFAQTypeID = @TargetLetter + CAST(@NewNum AS nchar(1))
            BREAK -- 找到目前全球最小的可用編號了，直接收工跳出迴圈
        END

        -- 4. 如果這個字母 1-9 全滿，移動到下一個字母 (例如 A 全滿 -> 檢查 B)
        SET @CurrentAscii = @CurrentAscii + 1
    END

    -- 5. 如果從 A 到 Z 都滿了 (總共 234 筆資料)
    IF @NewFAQTypeID IS NULL
    BEGIN
        RETURN '!!' -- 代表溢位
    END

    RETURN @NewFAQTypeID
END
GO

	 SELECT dbo.funGetFAQTypeID();

     ----------------------------------------------------------------------------------------------------------
--QuestionTypeIDA1~Z9

create FUNCTION funGetQuestionTypeID ()
RETURNS nchar(2)
AS
BEGIN
    DECLARE @TargetLetter char(1)
    DECLARE @NewNum int
    DECLARE @NewQuestionTypeID nchar(2)
    DECLARE @CurrentAscii int = 65 -- 從 'A' (ASCII 65) 開始檢查

    -- 1. 外部迴圈：從 A 跑到 Z
    WHILE @CurrentAscii <= 90 
    BEGIN
        SET @TargetLetter = CHAR(@CurrentAscii)

        -- 2. 內部檢查：在當前字母下，找 1-9 號第一個沒人用的數字
        -- 我們利用一個虛擬的 1-9 表來跟資料庫比對
        SELECT TOP 1 @NewNum = t1.Num
        FROM (
            SELECT 1 AS Num UNION SELECT 2 UNION SELECT 3 UNION 
            SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION 
            SELECT 7 UNION SELECT 8 UNION SELECT 9
        ) AS t1
        LEFT JOIN (
            -- 找出資料庫中「該字母」開頭且符合「字母+數字」格式的編號
            SELECT TRY_CAST(RIGHT(RTRIM(QuestionTypeID), 1) AS int) AS UsedNum
            FROM QuestionType
            WHERE QuestionTypeID LIKE @TargetLetter + '[1-9]'
        ) AS t2 ON t1.Num = t2.UsedNum--請把左邊的數字，拿去跟右邊已使用的數字對對看。
        WHERE t2.UsedNum IS NULL -- 只要 UsedNum 是 NULL，代表這個數字沒人用
        ORDER BY t1.Num ASC -- 確保拿最小的那個數字 (例如缺 2 和 5，先拿 2)

        -- 3. 如果在這個字母找到了可用數字
        IF @NewNum IS NOT NULL
        BEGIN
            SET @NewQuestionTypeID = @TargetLetter + CAST(@NewNum AS nchar(1))
            BREAK -- 找到目前全球最小的可用編號了，直接收工跳出迴圈
        END

        -- 4. 如果這個字母 1-9 全滿，移動到下一個字母 (例如 A 全滿 -> 檢查 B)
        SET @CurrentAscii = @CurrentAscii + 1
    END

    -- 5. 如果從 A 到 Z 都滿了 (總共 234 筆資料)
    IF @NewQuestionTypeID IS NULL
    BEGIN
        RETURN '!!' -- 代表溢位
    END

    RETURN @NewQuestionTypeID
END
GO

	 SELECT dbo.funGetQuestionTypeID();

----------------------------------------------------------------------------------------------------------
--EmployeeID F21025

create function funGetEmployeeID(@roleID nchar(1))
returns nchar(6)
as
begin 
    declare @currentYear nchar(2),@lastseq int,@newID nchar(6)

    --取得當年度後2碼，YEAR(GETDATE()) 會抓到 2026
    set @currentYear=right(CAST(Year(Getdate()) as nchar(4)),2)

    --取得該角色在今年度最大流水號
    select @lastseq= ISNULL(MAX(CAST(RIGHT(EmployeeID, 3) AS int)), 0)
    from Employee
    where EmployeeID like @roleID+@currentYear+'%'--F21%

    --流水號加1
    set @lastseq+=1

    --組合編號
    set @newID=@roleID+@currentYear+RIGHT('000' + CAST(@lastSeq AS varchar), 3)

    return @newID

end

SELECT dbo.funGetEmployeeID('A');

----------------------------------------------------------------------------------------------------------
--PlaceID A

create function funGetPlaceID()
returns nchar(1)
as
begin 
   DECLARE @LastID VARCHAR(1),@NextID VARCHAR(1)
    

    -- 取得目前最後一筆 ID
    SELECT TOP 1 @LastID = [PlaceID] FROM [Place] ORDER BY [PlaceID] DESC

    IF @LastID IS NULL
        SET @NextID = 'A'
    ELSE
        -- 將字母轉為 ASCII 碼 + 1 再轉回字元
        SET @NextID = CHAR(ASCII(@LastID) + 1)

    RETURN @NextID

end

SELECT dbo.funGetPlaceID();

----------------------------------------------------------------------------------------------------------
--EmployeeID F21025

create function funGetVenueID(@PlaceID nchar(1))
returns nchar(3)
as
begin 
    DECLARE @LastID VARCHAR(3),@NextID VARCHAR(3), @Seq INT

   

     select @Seq =isnull(max(cast(right(VenueID,2)as int)),0)
	 from Venue
	 where PlaceID like @PlaceID+'%';

     set @Seq+=1

    -- 組合成 A + 兩位數流水號 (例如 A02)
    SET @NextID = @PlaceID + RIGHT('00' + CAST(@Seq AS VARCHAR), 2)

    RETURN @NextID

end

SELECT dbo.funGetVenueID('C');