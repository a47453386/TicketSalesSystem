alter function funGetOrderID ()
	returns nvarchar(12)
	as
	begin
	 declare @DatePrefix nchar(6),@LastOID int,@NewOrderID nchar(12)

	 set @DatePrefix =convert(nvarchar(6),getdate(),112)

	 select @LastOID= isnull (max(cast(right(OrderID,6) as int)),0) 
	 from [Order]
	 where left(OrderID,6)=@DatePrefix

	 set @LastOID+=1

	 set @NewOrderID=cast(@DatePrefix as nchar(6))+RIGHT('000000'+ CAST(@LastOID as varchar(6)),6)

	 return @NewOrderID
	 end
	 go

	 print dbo.funGetOrderID();
	 go