create function funGetProgrammeID ()
	returns nchar(8)
	as
	begin
	 declare @DatePrefix nchar(6),@LastProID int,@NewProgrammeID nchar(8)

	 set @DatePrefix =convert(nvarchar(6),getdate(),112)

	 select @LastProID= isnull (max(cast(right(ProgrammeID,2) as int)),0) 
	 from Programme
	 where left(ProgrammeID,6)=@DatePrefix

	 set @LastProID+=1

	 set @NewProgrammeID=@DatePrefix+RIGHT('00'+ CAST(@LastProID as varchar),2)

	 return @NewProgrammeID
	 end
	 go

	 print dbo.funGetProgrammeID();