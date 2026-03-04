using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSalesSystem.Models;
using TicketSalesSystem.Service.ID;
using TicketSalesSystem.Service.Images;
using TicketSalesSystem.ViewModel.Employee;

namespace TicketSalesSystem.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = "EmployeeScheme", Roles = "S,A,F")]
    public class EmployeesController : Controller
    {
        private readonly TicketsContext _context;
        private readonly IIDService _idService;
        private readonly IFileService _fileService;

        public EmployeesController(TicketsContext context,IIDService idService,IFileService fileService)
        {
            _context = context;
            _idService = idService;
            _fileService = fileService;
        }
        
        // GET: Employees
        public async Task<IActionResult> Index(string searchString)
        {
            //包含關聯資料
            var employees = _context.Employee
                .Include(e => e.AccountStatus)
                .Include(e => e.Role)
                .AsQueryable();//暫時轉成可查詢物件，方便後續加條件

            //實作搜尋邏輯 (姓名或編號)
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(s => s.Name.Contains(searchString)
                                             || s.EmployeeID.Contains(searchString));
            }

            //排序
            var result = await employees.OrderByDescending(e => e.RoleID).ToListAsync();

            //將搜尋字串傳回 View，讓搜尋框保留文字
            ViewData["CurrentFilter"] = searchString;


            return View(result);
        }
       
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.AccountStatus)
                .Include(e => e.Role)
                .Include(e => e.EmployeeLogin)
                .FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            PopulateDropdownLists();
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VMEmployeeCreate vm )
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(vm.AccountStatusID, vm.RoleID);
                return View(vm);
            }

            //檢查帳號重複
            if (await _context.EmployeeLogin.AnyAsync(a => a.Account == vm.Account))
            {
                ModelState.AddModelError("Account", "帳號已被註冊。");
                PopulateDropdownLists(vm.AccountStatusID, vm.RoleID);
                return View(vm);
            }


            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //產生新的 EmployeeID
                    var emID = await _idService.GetNextEmployeeID(vm.RoleID);


                    //處理照片上傳
                    string? fileName = null;
                    if (vm.PhotoFile != null&&vm.PhotoFile.Length>0)
                    {
                        fileName= await _fileService.SaveFileAsync(vm.PhotoFile,emID, "employeePhotos");
                        
                    }

                    //建立 Employee 實體並儲存
                    var emp = new Employee
                    {
                        EmployeeID = emID,
                        Name = vm.Name,
                        HireDate = vm.HireDate,
                        Address = vm.Address,
                        Birthday = vm.Birthday,
                        Tel = vm.Tel,
                        Gender = vm.Gender,
                        NationalID = vm.NationalID,
                        Email = vm.Email,
                        Extension = vm.Extension,
                        Photo = fileName,
                        CreatedTime = DateTime.Now,
                        LastLoginTime = null,
                        RoleID = vm.RoleID,
                        AccountStatusID = "A"//預設為啟用(A)
                    };
                    _context.Employee.Add(emp);

                    //建立 EmployeeLogin 實體並儲存
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
                    var login = new EmployeeLogin
                    {
                        EmployeeID = emp.EmployeeID,
                        Account = vm.Account,
                        Password = hasher.HashPassword(vm.Account, vm.Password),
                    };
                    _context.EmployeeLogin.Add(login);


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "新增失敗，可能原因：帳號重複或系統錯誤。");
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";

                    return Content($"存檔失敗！<br/>" +
                                   $"主錯誤：{ex.Message}<br/>" +
                                   $"底層原因：{innerMessage}");
                }

            }
        }



        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.AccountStatus)
                .Include(e => e.Role)
                .Include(e => e.EmployeeLogin)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }
            
            var vm = new VMEmployeeEdit
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                HireDate = employee.HireDate,
                Address = employee.Address,
                Birthday = employee.Birthday,
                Tel = employee.Tel,
                Gender = employee.Gender,
                NationalID = employee.NationalID,
                Email = employee.Email,
                Extension = employee.Extension,
                Photo = employee.Photo,
                Account = employee.EmployeeLogin?.Account??"",//如果 EmployeeLogin 為 null，則給予空字串
                RoleID = employee.RoleID,
                AccountStatusID = employee.AccountStatusID
            };
            PopulateDropdownLists(employee.AccountStatusID, employee.RoleID);
            return View(vm);
        }

        // POST: Employees/Edit/5
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VMEmployeeEdit vm)
        {
            // 🚩 1. 行內新增的驗證排除邏輯
            if (!string.IsNullOrEmpty(vm.NewRoleID)) ModelState.Remove("RoleID");
            if (!string.IsNullOrEmpty(vm.NewAccountStatusID)) ModelState.Remove("AccountStatusID");

            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(vm.AccountStatusID, vm.RoleID);
                return View(vm);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //取得現有的 Employee 實體
                    var employee = await _context.Employee
                        .Include(e => e.AccountStatus)
                        .Include(e => e.Role)
                        .Include(e => e.EmployeeLogin)
                        .FirstOrDefaultAsync(e => e.EmployeeID == vm.EmployeeID);

                    if(employee == null) return NotFound();


                    //處裡照片更新
                    if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
                    {
                        //刪除舊照片
                        if (!string.IsNullOrEmpty(employee.Photo))
                        {
                            await _fileService.DeleteFileAsync(employee.Photo, "Photos","employeePhotos");
                        }
                        //儲存新照片
                        var newFileName = await _fileService.SaveFileAsync(vm.PhotoFile, employee.EmployeeID, "employeePhotos");
                        employee.Photo = newFileName;
                    }


                    //更新 EmployeeLogin 資料
                    employee.Name = vm.Name;
                    employee.HireDate = vm.HireDate;
                    employee.Address = vm.Address;
                    employee.Birthday = vm.Birthday;
                    employee.Tel = vm.Tel;
                    employee.Gender = vm.Gender;
                    employee.NationalID = vm.NationalID;
                    employee.Email = vm.Email;
                    employee.Extension = vm.Extension;
                    employee.RoleID= vm.RoleID;
                    employee.AccountStatusID = vm.AccountStatusID;


                    //如果有輸入新密碼，則更新密碼
                    if (employee.EmployeeLogin != null)
                    { 
                        if(!string.IsNullOrEmpty(vm.Password))
                        {
                            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
                            employee.EmployeeLogin.Password = hasher.HashPassword(employee.EmployeeLogin.Account, vm.Password);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"更新失敗：{ex.Message}");
                    PopulateDropdownLists(vm.AccountStatusID, vm.RoleID);
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "無底層訊息";
                    return Content($"更新失敗！<br/>" +
                                   $"主錯誤：{ex.Message}<br/>" +
                                   $"底層原因：{innerMessage}");
                    
                }
            }
            
            
        }



        // POST: Employees/Delete/5
        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // 1. 抓出員工資料
                var employee = await _context.Employee.FindAsync(id);
                if (employee == null) return Json(new { success = false, message = "找不到該員工資料。" });

                // 🚩 2. 先把照片檔名備份起來 (很重要，因為 SaveChanges 後物件可能會被釋放)
                string photoToDelete = employee.Photo;

                // 3. 處理關聯的登入帳號 (EmployeeLogin)
                // 刪除員工前，必須先刪除依賴它的帳號資料 (若 DB 未設級聯刪除)
                var login = await _context.EmployeeLogin.FirstOrDefaultAsync(l => l.EmployeeID == id);
                if (login != null)
                {
                    _context.EmployeeLogin.Remove(login);
                }

                // 4. 執行員工主表刪除
                _context.Employee.Remove(employee);

                // 🚩 核心修正：務必加上 await，確保資料庫存檔完成
                await _context.SaveChangesAsync();

                // 🚩 5. 資料庫成功後，再刪除實體檔案
                if (!string.IsNullOrEmpty(photoToDelete))
                {
                    // 依照你的參數：檔名, "Photos", "employeePhotos"
                    await _fileService.DeleteFileAsync(photoToDelete, "Photos", "employeePhotos");
                }

                // 🚩 6. 回傳成功 JSON
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // 捕捉可能的異常 (例如權限不足或資料庫約束)
                return Json(new { success = false, message = "刪除失敗，原因：" + ex.Message });
            }
        }



        private void PopulateDropdownLists(string? selectedStatus = null, string? selectedRole = null)
        {

            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus, "AccountStatusID", "FullDisplayName", selectedStatus);
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "FullDisplayName", selectedRole);
        }

        // 🚩 處理新角色新增邏輯
        [HttpPost]
        public async Task<IActionResult> QuickCreateRole(string id, string name)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "請輸入完整的角色編號與名稱" });

            if (await _context.Role.AnyAsync(r => r.RoleID == id))
                return Json(new { success = false, message = "角色編號已存在" });

            var role = new Role { RoleID = id, RoleName = name };
            _context.Role.Add(role);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = id, name = name });
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string id, string name)
        {
            var role = await _context.Role.FindAsync(id);
            if (role == null) return Json(new { success = false, message = "找不到該角色" });

            role.RoleName = name;
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = id, name = name });
        }

        // 🚩 [POST] 快速新增帳號狀態
        [HttpPost]
        public async Task<IActionResult> QuickCreateStatus(string id, string name)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return Json(new { success = false, message = "請輸入完整的狀態編號與名稱" });

            if (await _context.AccountStatus.AnyAsync(s => s.AccountStatusID == id))
                return Json(new { success = false, message = "狀態編號已存在" });

            var status = new AccountStatus { AccountStatusID = id, AccountStatusName = name };
            _context.AccountStatus.Add(status);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = id, name = name });
        }

        // 🚩 [POST] 快速編輯帳號狀態名稱
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string name)
        {
            var status = await _context.AccountStatus.FindAsync(id);
            if (status == null) return Json(new { success = false, message = "找不到該狀態" });

            status.AccountStatusName = name;
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = id, name = name });
        }

    }
}
