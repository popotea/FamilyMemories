# Family Memories 應用程式 - MVC 架構規劃

根據您的要求，我將規劃如何將現有的 Razor Pages 應用程式重構為更清晰的 MVC (Model-View-Controller) 架構，並加入 ViewModel 分層，提升程式碼的可讀性和維護性。

## 🎯 當前架構分析

### ✅ 現有優點：
- **Razor Pages**: 簡單快速開發
- **內建驗證**: 自動模型綁定
- **頁面導向**: 邏輯與視圖整合

### ❌ 現有缺點：
- **關注點混合**: PageModel 同時處理邏輯和資料轉換
- **重用性低**: 視圖模型緊耦合PageModel
- **測試困難**: 業務邏輯與UI邏輯混合

---

## 📁 建議 MVC 架構規劃

### 1. 資料夾結構

```
FamilyMemories/
├── Controllers/                    # 📂 新增 - 控制器層
│   ├── HomeController.cs          # 首頁控制器
│   ├── MemoriesController.cs      # 回憶管理控制器
│   ├── AccountController.cs       # 帳號管理控制器 (取代 Identity Pages)
│   └── AdminController.cs         # 管理員功能控制器
│
├── Models/                        # ✅ 現有 - 資料模型
│   ├── Domain/                    # 📂 新增 - 領域模型
│   │   ├── Memory.cs             # 核心業務模型
│   │   └── User.cs               # (如果需要區分)
│   ├── ViewModels/               # 📂 新增 - 視圖模型
│   │   ├── Home/
│   │   │   └── IndexViewModel.cs
│   │   ├── Memories/
│   │   │   ├── IndexViewModel.cs
│   │   │   ├── CreateViewModel.cs
│   │   │   ├── EditViewModel.cs
│   │   │   └── DetailsViewModel.cs
│   │   ├── Account/
│   │   │   ├── LoginViewModel.cs
│   │   │   ├── RegisterViewModel.cs
│   │   │   └── ProfileViewModel.cs
│   │   └── Admin/
│   │       └── UserManagementViewModel.cs
│   └── DTOs/                      # 📂 新增 - 資料傳輸物件
│       ├── MemoryDto.cs
│       └── UserDto.cs
│
├── Views/                         # 📂 新增 - MVC 視圖
│   ├── Shared/                    # 共用視圖
│   │   ├── _Layout.cshtml        # 主版面頁面
│   │   ├── _LoginPartial.cshtml  # 登入狀態顯示
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── Home/                      # 首頁視圖
│   │   └── Index.cshtml          # 首頁
│   ├── Memories/                  # 回憶管理視圖
│   │   ├── Index.cshtml          # 列表頁面
│   │   ├── Create.cshtml         # 新增頁面
│   │   ├── Edit.cshtml           # 編輯頁面
│   │   ├── Delete.cshtml         # 刪除確認頁面
│   │   └── Details.cshtml        # 詳細頁面
│   ├── Account/                   # 帳號管理視圖
│   │   ├── Login.cshtml          # 登入頁面
│   │   ├── Register.cshtml       # 註冊頁面
│   │   ├── Logout.cshtml         # 登出頁面
│   │   └── AccessDenied.cshtml   # 存取拒絕
│   └── Admin/                     # 管理員視圖
│       └── Index.cshtml          # 用戶管理
│
├── Services/                      # 📂 新增 - 業務邏輯層
│   ├── Interfaces/
│   │   ├── IMemoryService.cs
│   │   └── IUserService.cs
│   ├── Implementations/
│   │   ├── MemoryService.cs      # 回憶業務邏輯
│   │   └── UserService.cs        # 用戶業務邏輯
│   └── AutoMapperProfiles.cs     # 映射設定
│
├── Data/                         # ✅ 現有 - 資料存取層
│   ├── ApplicationDbContext.cs   # 資料庫上下文
│   ├── Repositories/             # 📂 新增 - 儲存庫模式
│   │   ├── Interfaces/
│   │   │   ├── IMemoryRepository.cs
│   │   └── Implementations/
│   │       └── MemoryRepository.cs
│   └── Migrations/               # 資料庫遷移
│
├── Infrastructure/               # 📂 新增 - 基礎設施
│   ├── FileStorage.cs            # 檔案儲存服務
│   ├── EmailService.cs           # 郵件服務
│   └── SecurityService.cs        # 安全性服務
│
├── wwwroot/                      # ✅ 現有 - 靜態檔案
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── uploads/                  # 圖片儲存
│
├── Properties/                   # ✅ 現有
├── appsettings.json              # ✅ 現有 - 配置檔案
├── Program.cs                    # ✅ 現有 - 應用程式進入點
└── FamilyMemories.csproj         # ✅ 現有 - 專案設定
```

---

## 🔄 從 Pages 到 MVC 的遷移步驟

### 第1階段: 基礎 MVC 架構建立
1. **安裝 MVC 套件**
   ```bash
   dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
   ```

2. **更新 Program.cs (MVC 支援)**
   ```csharp
   builder.Services.AddControllersWithViews(); // 加入 MVC 支援
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}"
   );
   ```

3. **建立 Controllers 資料夾與基礎控制器**

### 第2階段: ViewModel 層建立
1. **建立 ViewModels 資料夾結構**
2. **將現有 PageModel 屬性搬移到 ViewModel**
3. **實作 AutoMapper 進行模型映射**

### 第3階段: 服務層分離
1. **建立 Services 資料夾**
2. **將業務邏輯從 Controllers 移出**
3. **實作依賴注入**

### 第4階段: 視圖遷移
1. **將 Pages 遷移為 Views**
2. **移除 @page 指示詞**
3. **更新視圖路徑**

### 第5階段: 最佳化與測試
1. **實作儲存庫模式**
2. **加入單元測試**
3. **效能優化**

---

## 📊 MVC 分層優點

### 🏗️ **關注點分離 (Separation of Concerns)**
```
控制器 (Controller) - 處理請求與回應
├── 接收 HTTP 請求
├── 調用業務邏輯
└── 返回適當的視圖

服務層 (Services) - 業務邏輯
├── 資料驗證
├── 業務規則
└── 工作流程

資料存取層 (Repositories) - 資料庫操作
├── CRUD 操作
├── 查詢邏輯
└── 資料轉換

視圖模型 (ViewModel) - UI 資料
├── 資料格式化
├── UI 狀態
└── 驗證訊息
```

### 🔧 **程式碼重用性**
- ViewModel 可在不同的控制器中重用
- Service 可被多個 Controller 使用
- Repository 提供一致的資料存取介面

### 🧪 **易於測試**
- 各層可獨立測試
- 相依性容易模擬
- 單元測試涵蓋率更高

### 📚 **維護性提升**
- 修改一層不會影響其他層
- 新功能開發更加結構化
- 程式碼定位更加精確

---

## 🚀 立即實作建議

如果您想要開始實作這個架構，我建議從以下步驟開始：

1. **先保留現有 Pages 結構**
2. **建立新的 Controllers 和 ViewModels**
3. **漸進式遷移** - 從最簡單的功能開始
4. **平行運行** - 兩套架構同時存在，直到完全遷移完成

您希望我開始實作這個 MVC 架構嗎？我可以從建立基本的 Controllers 和 ViewModels 開始。
