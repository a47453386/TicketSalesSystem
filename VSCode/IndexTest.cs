// 修改後的入口 Action
public async Task<IActionResult> IndexTest()
{
    // 取得所有可選的場次
    var sessions = await _context.Session.ToListAsync();
    
    // 預設不抓票區，等使用者選了場次再透過 AJAX 抓，或者直接全部抓出來
    ViewBag.Sessions = sessions;
    
    return View();
}

// 新增一個 API：根據場次抓票區
[HttpGet]
public async Task<IActionResult> GetAreasBySession(string sessionId)
{
    var areas = await _context.TicketsArea
        .Where(a => a.SessionID == sessionId)
        .Select(a => new { a.TicketsAreaID, a.AreaName, a.Price, a.VenueID })
        .ToListAsync();
    return Json(areas);
}