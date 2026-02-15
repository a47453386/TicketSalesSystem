[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Question question, IFormFile? upload)
{
    // 1. 補足後端自動生成的資料
    question.QuestionID = Guid.NewGuid().ToString();
    question.CreatedTime = DateTime.Now;

    // 2. 處理檔案上傳
    if (upload != null && upload.Length > 0)
    {
        // 呼叫你的 FileService
        // 它會自動根據副檔名存到 Photos/Questions 或 Docs/Questions
        // 並回傳路徑如 "Docs/Questions/guid.pdf"
        string dbPath = await _fileService.SaveFileAsync(upload, "Questions");
        question.UploadFile = dbPath;
    }

    // 3. 移除不需要驗證的欄位（因為 ID 和時間是後端產生的）
    ModelState.Remove("QuestionID");
    ModelState.Remove("CreatedTime");
    ModelState.Remove("MemberID"); // 假設從 Session 抓

    if (ModelState.IsValid)
    {
        _context.Add(question);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyList));
    }

    // 如果失敗，重新載入下拉選單
    ViewBag.QuestionTypeID = new SelectList(_context.QuestionType, "QuestionTypeID", "QuestionTypeName", question.QuestionTypeID);
    return View(question);
}