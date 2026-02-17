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
                    //處理角色新增
                    if(!string.IsNullOrEmpty(vm.NewRoleID) && !string.IsNullOrEmpty(vm.NewRoleName))
                    {
                        // 檢查重複 (避免編號衝突)
                        if (await _context.Role.AnyAsync(r => r.RoleID == vm.NewRoleID))
                        {
                            ModelState.AddModelError("NewRoleID", "此角色編號已存在");
                            throw new Exception("Duplicate RoleID");
                        }
                        _context.Role.Add(new Role { RoleID = vm.NewRoleID, RoleName = vm.NewRoleName });
                        vm.RoleID = vm.NewRoleID; // 🚩 強制將員工關聯到新角色
                    }

                    //處理帳號狀態新增
                    if (!string.IsNullOrEmpty(vm.NewAccountStatusID) && !string.IsNullOrEmpty(vm.NewAccountStatusName))
                    {
                        if (await _context.AccountStatus.AnyAsync(s => s.AccountStatusID == vm.NewAccountStatusID))
                        {
                            ModelState.AddModelError("NewAccountStatusID", "此狀態編號已存在");
                            throw new Exception("Duplicate StatusID");
                        }
                        _context.AccountStatus.Add(new AccountStatus { AccountStatusID = vm.NewAccountStatusID, AccountStatusName = vm.NewAccountStatusName });
                        vm.AccountStatusID = vm.NewAccountStatusID; // 🚩 強制將員工關聯到新狀態
                    }
                    // 先儲存主檔變動，確保外鍵關聯正確
                    await _context.SaveChangesAsync();

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

                    //新增角色
                    if (!string.IsNullOrEmpty(vm.NewRoleID) && !string.IsNullOrEmpty(vm.NewRoleName))
                    {
                        if (!await _context.Role.AnyAsync(r => r.RoleID == vm.NewRoleID))
                        {
                            _context.Role.Add(new Role { RoleID = vm.NewRoleID, RoleName = vm.NewRoleName });
                            vm.RoleID = vm.NewRoleID;
                        }
                    }

                    // 新增狀態
                    if (!string.IsNullOrEmpty(vm.NewAccountStatusID) && !string.IsNullOrEmpty(vm.NewAccountStatusName))
                    {
                        if (!await _context.AccountStatus.AnyAsync(s => s.AccountStatusID == vm.NewAccountStatusID))
                        {
                            _context.AccountStatus.Add(new AccountStatus { AccountStatusID = vm.NewAccountStatusID, AccountStatusName = vm.NewAccountStatusName });
                            vm.AccountStatusID = vm.NewAccountStatusID;
                        }
                    }

                    await _context.SaveChangesAsync();

                    //處裡照片更新
                    if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
                    {
                        //刪除舊照片
                        if (!string.IsNullOrEmpty(employee.Photo))
                        {
                            await _fileService.DeleteFileAsync(employee.Photo, "employeePhotos");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();

            try 
            {
                var login = await _context.EmployeeLogin.FirstOrDefaultAsync(l => l.EmployeeID == id);
                if (login != null)
                {
                    _context.EmployeeLogin.Remove(login);
                }

                _context.Employee.Remove(employee);
                _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(employee.Photo))
                {
                    await _fileService.DeleteFileAsync(employee.Photo, "employeePhotos");
                }
                
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return RedirectToAction(nameof(Index), new { error = "刪除失敗" });
            }

            
        }

     

        private void PopulateDropdownLists(string? selectedStatus = null, string? selectedRole = null)
        {

            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus, "AccountStatusID", "FullDisplayName", selectedStatus);
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "FullDisplayName", selectedRole);
        }


    }
}
