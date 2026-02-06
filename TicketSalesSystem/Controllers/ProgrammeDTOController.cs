using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSalesSystem.DTOs;
using TicketSalesSystem.Helpers;
using TicketSalesSystem.Models;
using TicketSalesSystem.ViewModel.CreateProgramme.CreateProgrammeStep;
using TicketSalesSystem.ViewModel.CreateProgramme.Item;

namespace TicketSalesSystem.Controllers
{
    public class ProgrammeDTOController : Controller
    {

        //private static ProgrammeDTO ItemProgramme(Programme programme)
        //{
        //    ProgrammeDTO p = new ProgrammeDTO()
        //    {
        //        ProgrammeName = programme.ProgrammeName,
        //        ProgrammeDescription = programme.ProgrammeDescription,
        //        LimitPerOrder = programme.LimitPerOrder,
        //        OnShelfTime = programme.OnShelfTime,
        //    };
        //    return p;
        //}
        private void MapStep1ToDo(VMProgrammeStep1 vm, ProgrammeDTO dto)
        {
            dto.ProgrammeName = vm.ProgrammeName;
            dto.ProgrammeDescription = vm.ProgrammeDescription;
            dto.LimitPerOrder = vm.LimitPerOrder;
            dto.OnShelfTime = vm.OnShelfTime;
        }
        private void MapStep1ToVM( ProgrammeDTO dto,VMProgrammeStep1 vm)
        {
            vm.ProgrammeName = dto.ProgrammeName;
            vm.ProgrammeDescription = dto.ProgrammeDescription;
            vm.LimitPerOrder = dto.LimitPerOrder;
            vm.OnShelfTime = dto.OnShelfTime;
        }
        private ProgrammeDTO GetCurrentDTO()
        { 
            return HttpContext.Session.GetObject<ProgrammeDTO>("programme") ?? new ProgrammeDTO();
        }
        private void SaveDTO(ProgrammeDTO dto)
        {
             HttpContext.Session.SetObject("programme", dto);
        }
        

        [HttpGet]
        public IActionResult CreateStep1()
        {
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();

            //建立一個空白的 VM
            var vm = new VMProgrammeStep1();

            //將 DTO 的資料映射到 VM
            MapStep1ToVM(dto,vm);

            //將 VM 傳遞給 View
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep1(VMProgrammeStep1 vm)
        {
            //驗證輸入
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();
            //將 VM 的資料映射到 DTO
            MapStep1ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep2");
        }



        private void MapStep2ToDo(VMProgrammeStep2 vm, ProgrammeDTO dto)
        {
            dto.Sessions= vm.Sessions
                .Select(s => new SessionDTO
                {
                SaleStartTime=s.SaleStartTime,
                SaleEndTime=s.SaleEndTime,
                StartTime=s.StartTime
                })
                .ToList();
        }
        private void MapStep2ToVM(ProgrammeDTO dto, VMProgrammeStep2 vm)
        {
            vm.Sessions = dto.Sessions
                .Select(s => new VMSessionItem
                {
                    SaleStartTime = s.SaleStartTime,
                    SaleEndTime = s.SaleEndTime,
                    StartTime = s.StartTime
                })
                .ToList();
        }


        [HttpGet]
        public IActionResult CreateStep2()
        {
            //取得現有的 DTO (使用你之前抽出來的 GetCurrentDto)
            var dto = GetCurrentDTO();

            //建立 Step 2 的 VM
            var vm = new VMProgrammeStep2();
            if (dto.Sessions != null && dto.Sessions.Any())
            {
                MapStep2ToVM(dto, vm);
            }
            else
            {
                // 如果是第一次進來，預設給一筆空白場次，方便前端產生第一個輸入框
                var vmF = new VMSessionItem
                {
                    StartTime = DateTime.Now,
                    SaleStartTime = DateTime.Now,
                    SaleEndTime = DateTime.Now
                };
                vm.Sessions.Add(vmF);
            }           

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep2(VMProgrammeStep2 vm)
        {
            //驗證輸入
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();
            //將 VM 的資料映射到 DTO
            if(vm.Sessions == null || !vm.Sessions.Any())
            {
                ModelState.AddModelError("", "請至少新增一個場次");
                return View(vm);
            }
            MapStep2ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep3");
        }

        private void MapStep3ToDo(VMProgrammeStep3 vm, ProgrammeDTO dto)
        {
            dto.TicketsAreas = vm.TicketsAreas
                .Select(s => new TicketsAreaDTO
                {
                    TicketsAreaName = s.TicketsAreaName,
                    RowCount = s.RowCount,
                    SeatCount = s.SeatCount,
                    Price = s.Price,
                })
                .ToList();
        }
        private void MapStep3ToVM(ProgrammeDTO dto, VMProgrammeStep3 vm)
        {
            vm.TicketsAreas = dto.TicketsAreas
                .Select(s => new VMTicketsAreaItem
                {
                    TicketsAreaName = s.TicketsAreaName,
                    RowCount = s.RowCount,
                    SeatCount = s.SeatCount,
                    Price = s.Price,
                })
                .ToList();
        }
        [HttpGet]
        public IActionResult CreateStep3()
        {
            //取得現有的 DTO (使用你之前抽出來的 GetCurrentDto)
            var dto = GetCurrentDTO();

            //建立 Step 3 的 VM
            var vm = new VMProgrammeStep3();
            if (dto.TicketsAreas != null && dto.TicketsAreas.Any())
            {
                MapStep3ToVM(dto, vm);
            }
            else
            {
                // 如果是第一次進來，預設給一筆空白場次，方便前端產生第一個輸入框
                var vmF = new VMTicketsAreaItem
                {
                    TicketsAreaName = "",
                    RowCount = 0,
                    SeatCount = 0,
                    Price = 0
                };
                vm.TicketsAreas.Add(vmF);
            }

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep3(VMProgrammeStep3 vm)
        {
            //驗證輸入
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            //從 Session 取出 DTO，如果沒有就建立一個新的
            var dto = GetCurrentDTO();
            //將 VM 的資料映射到 DTO
            if (vm.TicketsAreas == null || !vm.TicketsAreas.Any())
            {
                ModelState.AddModelError("", "請至少新增一個票區");
                return View(vm);
            }
            MapStep3ToDo(vm, dto);
            //將 DTO 存回 Session
            SaveDTO(dto);
            //導向下一步
            return RedirectToAction("CreateStep4");
        }
    }
}
