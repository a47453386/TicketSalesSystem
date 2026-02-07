create function funGetSessionID (@ProID nchar(8))
	returns nchar(10)
	as
	begin
	 declare @CurrentMax int,@NewSessionID nchar(10)

	 --眖 Session いъ赣笆ヘ玡程ㄢ计程
	 select @CurrentMax =max(cast(right(SessionID,2)as int))
	 from [Session]
	 where SessionID like @ProID+'%';

	Set @NewSessionID = @ProID + RIGHT('00'+ CAST(isnull(@CurrentMax,0)+1 as varchar),2)

	return @NewSessionID;
	end

	go

	create function funGetTicketsAreaID (@Sess nchar(10))
	returns nchar(12)
	as
	begin
	 declare @CurrentMax int,@NewTicketsAreaID nchar(12)

	 --眖 Session いъ赣笆ヘ玡程ㄢ计程
	 select @CurrentMax =max(cast(right(TicketsAreaID,2)as int))
	 from TicketsArea
	 where TicketsAreaID like @Sess+'%';

	Set @NewTicketsAreaID = @Sess + RIGHT('00'+ CAST(isnull(@CurrentMax,0)+1 as varchar),2)

	return @NewTicketsAreaID;
	end

	 go
	

	 print dbo.funGetTicketsAreaID(2026020101);
	 go