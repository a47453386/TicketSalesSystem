namespace TicketSalesSystem.Service.Sms
{
    public interface ISmsService
    {
        // 發送驗證碼簡訊
        Task SendVerificationCodeAsync(string phone, string code);

    }
}
