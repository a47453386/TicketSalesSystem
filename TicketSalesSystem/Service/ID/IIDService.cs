namespace TicketSalesSystem.Service.ID
{
    public interface IIDService
    {
            Task<string> GetNextProgrammeID();
            Task<string> GetNextSessionID(string pid);
            Task<string> GetNextTicketsAreaID(string sid);
            Task<string> GetNextFAQTypeID();
            Task<string> GetQuestionTypeID();
            Task<string> GetNextEmployeeID(string roleID);

    }
}
