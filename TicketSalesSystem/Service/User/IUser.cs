using TicketSalesSystem.ViewModel.Programme;

namespace TicketSalesSystem.Service.User
{
    public interface IUser
    {
        Task<List<VMProgramme>> GetProgrammesALL();

        string GetImageFullUrl(string fileName);
    }
}
