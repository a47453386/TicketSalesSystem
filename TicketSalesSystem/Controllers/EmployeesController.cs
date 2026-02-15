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
using TicketSalesSystem.ViewModel;

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
        public async Task<IActionResult> Index()
        {
            var ticketsContext = _context.Employee.Include(e => e.AccountStatus).Include(e => e.Role);
            return View(await ticketsContext.ToListAsync());
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
        public async Task<IActionResult> Create(EmployeeCreateVM vm )
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(vm);
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
                         await _fileService.SaveFileAsync(vm.PhotoFile,emID, "employeePhotos");
                        
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

                PopulateDropdownLists(vm);
                return View(vm);
            }
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus, "AccountStatusID", "AccountStatusID", employee.AccountStatusID);
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "RoleID", employee.RoleID);
            return View(employee);
        }

        // POST: Employees/Edit/5
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EmployeeID,Name,HireDate,Address,Birthday,Tel,Gender,NationalID,Email,Extension,Photo,CreatedTime,LastLoginTime,RoleID,AccountStatusID")] Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus, "AccountStatusID", "AccountStatusID", employee.AccountStatusID);
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "RoleID", employee.RoleID);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.AccountStatus)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employee.Any(e => e.EmployeeID == id);
        }


        private void PopulateDropdownLists(EmployeeCreateVM p = null)
        {
            ViewData["AccountStatusID"] = new SelectList(_context.AccountStatus, "AccountStatusID", "AccountStatusName", p?.AccountStatusID);
            ViewData["RoleID"] = new SelectList(_context.Role, "RoleID", "RoleName", p?.RoleID);
        }


    }
}
