namespace TicketSalesSystem.Service.Images
{
    public class ImageService
    {
       public async Task<string> FileUpload(IFormFile photo, string PID,string folderName)
        {
            var extension = Path.GetExtension(photo.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                return "";
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos",folderName);

            var fileName = $"{PID}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
