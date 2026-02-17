
namespace TicketSalesSystem.Service.Sms
{
    public class MockSmsService : ISmsService
    {
        public Task SendVerificationCodeAsync(string phone, string code)
        {
            System.Diagnostics.Debug.WriteLine($"===============================");
            System.Diagnostics.Debug.WriteLine($"[學生專題模擬簡訊發送系統]");
            System.Diagnostics.Debug.WriteLine($"發送時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"目標手機：{phone}");
            System.Diagnostics.Debug.WriteLine($"驗證代碼：{code}");
            System.Diagnostics.Debug.WriteLine($"==============================="); 
            return Task.CompletedTask;
        }
    }
}
