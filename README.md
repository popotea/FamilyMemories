# Family Memories - 家庭回憶應用

這是一個使用 .NET 8.0 和 ASP.NET Core 開發的家庭回憶記錄應用程式，使用者可以記錄重要的家庭時刻，包含文字描述和照片。

## 專案技術架構

### 後端框架
- **ASP.NET Core 8.0**: Web 應用程式框架
- **Entity Framework Core 8.0**: ORM 框架
- **SQLite**: 輕量級資料庫
- **Razor Pages**: MVC 模式實作

### 前端技術
- **HTML/CSS**: Bootstrap 框架
- **JavaScript**: jQuery 和 jQuery Validation
- **Razor Syntax**: 後端模板引擎

## 專案結構

```
FamilyMemories/
├── Data/
│   └── ApplicationDbContext.cs      # 資料庫上下文
├── Models/
│   └── Memory.cs                    # 記憶資料模型
├── Pages/
│   ├── Index.cshtml                 # 首頁 (修改過)
│   ├── Privacy.cshtml
│   ├── Error.cshtml
│   └── Memories/
│       ├── Index.cshtml             # 記憶列表頁面
│       ├── Index.cshtml.cs          # 列表頁面後端邏輯
│       ├── Create.cshtml            # 新增記憶頁面
│       └── Create.cshtml.cs         # 新增頁面後端邏輯
├── wwwroot/
│   ├── css/site.css                 # 自訂 CSS
│   ├── js/site.js                   # 自訂 JavaScript
│   ├── lib/                         # 第三方庫
│   └── uploads/                     # 上傳圖片存放目錄
├── appsettings.json                 # 應用程式設定 (包含資料庫連線字串)
├── Program.cs                       # 應用程式入口點
└── FamilyMemories.csproj            # 專案設定檔
```

## 如何運行專案

### 必要條件
- .NET 8.0 SDK (下載自: https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio Code 或任何 C# 編輯器

### 步驟
1. **複製本專案到本地**
   ```bash
   cd c:\code\FamilyMemories\FamilyMemories
   ```

2. **確保依賴已安裝**
   ```bash
   dotnet restore
   ```

3. **運行專案**
   ```bash
   dotnet run
   ```

4. **開啟瀏覽器訪問**
   - 應用程式運行在: `http://localhost:5227`
   - **首頁**: 直接進入美觀的相片相簿介面
   - **登入**: `http://localhost:5227/popo/login` 或 `http://localhost:5227/Identity/Account/Login`

## 主要功能

### 記憶管理
- **新增記憶**: 在 `/Memories/Create` 頁面可以輸入標題、描述、日期，並上傳圖片
- **查看記憶**: `/Memories/Index` 顯示所有記憶列表，包含小型圖片預覽
- **未來可擴展**: 編輯、刪除、搜尋功能

### 圖片上傳
- 支援常見的圖片格式 (通過 HTML5 file input)
- 檔案儲存到 `wwwroot/uploads/` 目錄
- 圖片路徑儲存到資料庫中的 `ImagePath` 欄位

## 資料庫配置

### SQLite 設定
- **資料庫檔案**: `familyapp.db` (位於專案根目錄)
- **連線字串**: 在 `appsettings.json` 中配置
- **遷移**: 使用 EF Core Migrations 管理資料庫結構

### 資料模型 (Memory)
```csharp
public class Memory
{
    public int Id { get; set; }                    // 主鍵
    [Required][StringLength(100)]
    public string Title { get; set; }              // 標題 (必填, 最多100字元)
    [StringLength(500)]
    public string Description { get; set; }        // 描述 (最多500字元)
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }            // 記憶日期
    public string ImagePath { get; set; }         // 圖片檔案名稱
}
```

## 開發與測試規則

### 編碼規範
- 使用 C# 8.0 或更高版本特性
- 遵循 ASP.NET Core 最佳實務
- 使用 async/await 進行非同步操作
- 註解關鍵邏輯和複雜函數

### 測試範圍
1. **Web 頁面渲染**: 確保 Razor Pages 正確顯示
2. **資料庫操作**: 新增記憶能正確儲存和查詢
3. **圖片上傳**: 檔案能正確上傳和顯示
4. **表單驗證**: 必填欄位和資料格式檢查

### 加入新功能
1. 在 `Models/` 新增或修改資料模型
2. 執行 `dotnet ef migrations add [遷移名稱]` 建立資料庫遷移
3. 執行 `dotnet ef database update` 應用遷移
4. 在 `Pages/` 新增對應的 Razor Pages
5. 在 `wwwroot/` 新增必要的靜態檔案

## AI 開發指南

### 修改專案時請遵守：
1. **維持一致性**: 使用現有的命名慣例和程式碼風格
2. **資料庫文檔**: 任何資料模型變更都要在 README 更新
3. **安全性考量**: 圖片上傳要驗證檔案類型和大小
4. **錯誤處理**: 在控制器和頁面中加入適當異常處理
5. **效能考量**: 使用 Entity Framework 的最佳實務 (Eager Loading, N+1 問題避免)

### 擴展建議：
- **使用者認證**: 加入 ASP.NET Core Identity
- **分類標籤**: 為記憶加入分類功能
- **搜尋功能**: 實作全文搜尋和過濾器
- **資料匯入匯出**: JSON/XML 格式支援
- **雲端儲存**: 將圖片儲存到 Azure Blob Storage 或 AWS S3

## 聯絡方式
如果有任何問題或建議，請在專案中提出 Issue 或 Pull Request。

## Cloudflare R2 (S3 相容) 使用說明（範例）

如果你想把圖片儲存在 Cloudflare R2（建議用於生產），以下是快速操作指南。

1. 在 Cloudflare 後台建立 R2 bucket，記下 Bucket 名稱與 Account ID。
2. 建立 Access Key / Secret Key（請妥善保管），不要把它們直接放入 Git。
3. 在 `appsettings.Development.json` 設定 `CloudflareR2:ServiceUrl` 與 `CloudflareR2:Bucket`，但請把金鑰放到環境變數或 secret 管理器：

   Windows PowerShell 範例（臨時環境變數，重啟終端會失效）：

   ```powershell
   $env:CloudflareR2__AccessKeyId = "<your-access-key>"
   $env:CloudflareR2__SecretAccessKey = "<your-secret-key>"
   ```

   注意：在 .NET 中，`:` 在環境變數會被 `__` 取代（例如 `CloudflareR2:AccessKeyId`->`CloudflareR2__AccessKeyId`）。

4. 本專案已實作 `IStorageService` 與 `CloudflareR2StorageService`，並在啟動時（Program.cs）若偵測到 cloud 設定就會自動註冊。

5. 本機測試：啟動專案後可使用 Postman 或 curl 測試上傳：

   - API: POST /api/memories
   - 表單欄位 (form-data): title, description, date, file (type=file)

6. 注意事項：
   - 若尚未提供 Cloudflare R2 設定，應用會回落到本地 `wwwroot/uploads/`，保持向後相容。
   - 若你使用私有 bucket，請將上傳改為不公開，並使用 `GeneratePresignedUrlAsync` 產生短期存取連結。

7. 刪除：當你刪除記憶（DELETE /api/memories/{id}）時，系統會嘗試刪除對應的雲端物件或本地檔案。

如果要我為你把 README 裡的範例命令再補上更完整的 CI / 部署範例，或直接幫你在伺服器上測試，回覆我想要的選項即可。
