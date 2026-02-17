using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TicketSalesSystem.Models;

namespace TicketSalesSystem.ValidationAttributes
{
    public class MyValidator
    {
        private readonly TicketsContext _context;
        public MyValidator(TicketsContext context)
        {
            _context = context;
        }
        //台灣身分證驗證器
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

        //帳號重複驗證器
        public class AccountDuplicateCheck : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                // 1. 取得資料庫上下文
                var _context = (TicketsContext)validationContext.GetService(typeof(TicketsContext))!;

                string account = value?.ToString() ?? "";

                // 2. 檢查資料庫是否已存在該帳號
                // 🚩 注意：如果是編輯 (Edit)，要排除掉「自己」原本的帳號，但新增 (Create) 不需要
                bool isExist = _context.EmployeeLogin.Any(x => x.Account == account);

                if (isExist)
                {
                    return new ValidationResult("此帳號已被使用，請更換其他名稱。");
                }

                return ValidationResult.Success;
            }
        }

        //會員手機號碼重複驗證器
        public class MemberTelDuplicateCheckAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                //取得輸入的手機號碼
                string? tel = value?.ToString();
                if (string.IsNullOrEmpty(tel))
                {
                    return ValidationResult.Success; // 由 [Required] 負責檢查，這裡跳過
                }

                //從 validationContext 取得資料庫實例 (DbContext)
                var _context = (TicketsContext?)validationContext.GetService(typeof(TicketsContext));

                if (_context == null)
                {
                    throw new Exception("無法取得資料庫連線實例");
                }

                // 檢查邏輯：判斷資料庫是否已有相同手機號碼
                bool isExist = _context.Member.Any(m => m.Tel == tel);

                if (isExist)
                {
                    // 驗證失敗，回傳錯誤訊息
                    return new ValidationResult("此手機號碼已被註冊，請換一個。");
                }

                // 驗證通過
                return ValidationResult.Success;
            }
        }





    }
}
