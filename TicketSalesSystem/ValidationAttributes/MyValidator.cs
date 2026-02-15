using System.ComponentModel.DataAnnotations;

namespace TicketSalesSystem.ValidationAttributes
{
    public class MyValidator
    {
        public class TaiwanIDAttribute: ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                // 如果是空值，讓 [Required] 標籤去處理，這裡回傳 Success
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return ValidationResult.Success;
                }

                string id = value.ToString()!.ToUpper(); // 轉大寫確保一致

                try
                {
                    //權重計算 (Checksum)
                    // 字母對應數值表 (A=10, B=11... 這裡要考慮特殊對應)
                    int[] letterWeights = { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    int letterIndex = id[0] - 'A';
                    int letterNum = letterWeights[letterIndex];

                    int n1 = letterNum / 10; // 十位數
                    int n2 = letterNum % 10; // 個位數

                    // 加權公式: n1*1 + n2*9 + d1*8 + d2*7 + d3*6 + d4*5 + d5*4 + d6*3 + d7*2 + d8*1 + d9*1
                    int sum = n1 + (n2 * 9);
                    int[] weights = { 8, 7, 6, 5, 4, 3, 2, 1 };

                    for (int i = 0; i < 8; i++)
                    {
                        sum += (id[i + 1] - '0') * weights[i];
                    }

                    // 最後一碼 (檢查碼)
                    sum += (id[9] - '0');

                    // 總和必須能被 10 整除
                    if (sum % 10 != 0)
                    {
                        return new ValidationResult("無效的身分證字號(檢查碼錯誤)");
                    }


                }
                catch(Exception e) 
                {
                    // 如果前面的標籤沒擋住導致噴錯，這裡回傳格式錯誤
                    return new ValidationResult("身分證字號格式錯誤");
                }
                

                return ValidationResult.Success;
            }
        }






    }
}
