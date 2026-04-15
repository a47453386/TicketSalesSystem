 🎫 票務管理系統 

一套完整的票務管理平台後端系統，提供 RESTful API 供
Android App 串接，並包含完整的管理後台介面。

## 🎬 功能展示影片
👉 [https://canva.link/1odpz69pbqo58w6]

## 🛠 使用技術

### 後端
- C# / ASP.NET Core 8
- Entity Framework Core
- SQL Server（含自訂函數 dbo.funGetOrderID）
- RESTful API 設計（多模組 ApiController）
- JWT 雙 Scheme 驗證機制（會員／管理員）
- Service Layer 架構（關注點分離）
- Repository Pattern
- Transaction 處理（確保訂單資料一致性）
- Queue 排隊機制（應對高併發購票場景）
- 自動配位演算法（SeatHelper.FindBestSeats）

### 前端
- Razor Views / HTML / CSS / JavaScript
- jQuery AJAX（動態互動）
- Bootstrap

### 工具
- Visual Studio 2022
- Git / GitHub

## ✨ 主要功能

### 使用者端 API
- 會員註冊／登入／個人資料編輯
- 活動瀏覽與場次查詢
- 手動選位與自動配位購票
- 訂單查詢與 QR Code 票券顯示
- 公告瀏覽
- FAQ 提問與查詢

### 管理後台
- 活動與場次管理
- 座位區域設定
- 訂單管理與狀態追蹤
- 會員管理
- FAQ 分類管理（AJAX 動態新增）
- 公告發布與管理

## 🏗 系統架構
<img width="1024" height="559" alt="image" src="https://github.com/user-attachments/assets/7ccf57e9-5373-4ad9-88bf-f3102614762f" />


## 🔗 相關專案
- 使用者購票 Android App：[https://github.com/a47453386/TicketSalesSystemAPP.git]
- 票券驗證 Android App：[https://github.com/a47453386/TicketVerifyApp.git]

## 💻 本機執行方式
1. Clone 此專案至 Visual Studio 2022
2. 修改 `appsettings.json` 中的 SQL Server 連線字串
3. 執行 Entity Framework Core Migration
4. 啟動專案（預設 Port：請確認 launchSettings.json）

