namespace TicketSalesSystem.Service.Images
{
    public interface IImageService
    {
        Task<string> FileUpload(IFormFile photo, string PID);
    }
}
