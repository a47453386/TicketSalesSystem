namespace TicketSalesSystem.Helpers
{
    public class StringHelper
    {
        // 使用 GUID 產生指定長度的核銷碼

        public static string GenerateCheckInCode(int length=12)
        {
            // 產生一個新的 GUID，並移除連字號
            string guidString = Guid.NewGuid().ToString("N"); // "N" 格式會移除連字號
            // 如果指定的長度大於 GUID 字串的長度，則回傳整個 GUID 字串
            if (length >= guidString.Length)
            {
                return guidString;
            }
            // 否則，回傳指定長度的子字串

            return guidString.Substring(0, length).ToUpper(); 
        }
    }
}
