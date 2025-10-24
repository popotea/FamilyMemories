# 功能需求和待辦事項 - Requirements & Backlog

這份檔案記錄 Family Memories 應用程式的功能需求、使用者故事和開發待辦事項。當您需要請 AI 協助開發新功能時，請先在此檔案中詳細描述您的需求，然後貼上需求給 AI 檢視。

## 如何使用這份檔案

### 1. **宣告需求格式**
每個新功能請按照以下格式描述：

```markdown
### 功能名稱: [建立使用者權限維護及角色]

#### 描述
[這功能要能設定每個使用者能讀取那些功能權限獲選單,角色也是]

#### 使用者故事 (User Story)
As a [全部用戶], I want [能有切割使用者權限] so that [價值/目的]

#### 接受標準 (Acceptance Criteria)
- [具體條件1]
- [具體條件2]
- [具體條件3]

#### 技術考量
- [前端/後端要求]
- [資料庫變更]
- [UI/UX 需求]
- [安全性考量]

#### 優先順序
[高/中/低]
```

### 2. **請 AI 協助的步驟**
1. 在此檔案中新增需求 (使用上述格式)
2. 將需求文字複製貼上給 AI
3. 說明專案的脈絡：".NET Core 應用，使用 Razor Pages，請參考 README.md 了解專案架構"
4. 如果需要修改現有功能，請指出具體的檔案路徑

### 3. **更新追蹤**
每個功能完成後，請在此檔案中將狀態改為 "✅ 已完成"

---

## 🎯 目前待辦功能

### 基本功能 (已完成 ✅)
- ✅ 記憶列表顯示
- ✅ 新增記憶功能 (含圖片上傳)
- ✅ SQLite 資料庫設定

### 待開發功能 🔄

### 功能名稱: 使用者權限維護及角色系統 - User Permission Maintenance and Role System

#### 描述
建立完整的使用者認證和授權系統，讓系統可以管理多個使用者帳號，為每個使用者設定不同的角色和權限，控制他們可以存取的功能和選單項目。

#### 使用者故事 (User Story)
As a 管理員, I want 能建立和管理使用者帳號 so that 我可以控制誰可以使用系統。

As a 全部用戶, I want 能有切割使用者權限 so that 不同使用者有不同的功能存取權限，保護資料安全性。

#### 接受標準 (Acceptance Criteria)
- 可以使用者帳號密碼登入系統
- 可以註冊新使用者帳號
- 管理員可以為使用者指派角色 (Admin, User, Guest 等)
- 不同角色有不同的功能權限設定
- 未登入使用者只能看到登入頁面和公開頁面
- 登入後根據權限顯示不同選單項目
- 可以變更密碼和個人資訊
- 安全性：密碼加密儲存、防止常見攻擊

#### 技術考量
- 新增 ASP.NET Core Identity 套件
- 建立 IdentityUser 和 IdentityRole 擴展模型
- 更新 DbContext 繼承 IdentityDbContext
- 修改 Program.cs 新增身分驗證服務
- 建立登入頁面 `/Identity/Account/Login`
- 建立註冊頁面 `/Identity/Account/Register`
- 建立使用者管理頁面 (僅管理員)
- 新增 [Authorize] 屬性到需要保護的頁面
- 角色-based 授權檢查
- UI 根據使用者角色動態顯示/隱藏選單

#### 優先順序
高


### 功能名稱: 編輯現有記憶 - Edit Existing Memory

#### 描述
允許使用者編輯已存在的家庭回憶，修改標題、描述、日期，或更換圖片。

#### 使用者故事 (User Story)
As a 使用者, I want 編輯已存在的回憶 so that 我可以修正或更新重要的家庭時刻。

#### 接受標準 (Acceptance Criteria)
- 在記憶列表中每個項目有 "編輯" 連結
- 編輯頁面預先填入現有資料
- 可以上傳新圖片替換舊圖片
- 編輯完成後重新導向到列表頁面
- 如果未上傳新圖片，保留原有圖片
- 表單驗證確保必填欄位

#### 技術考量
- 新增 `/Pages/Memories/Edit.cshtml` 和 `.cs`
- 修改 Index.cshtml 加入編輯連結
- 在 Edit.cshtml.cs 中處理圖片替換邏輯
- Entity Framework 更新操作
- 檔案系統: 刪除舊圖片檔案 (如果替換)

#### 優先順序
高

### 功能名稱: 刪除記憶功能 - Delete Memory Feature

#### 描述
提供刪除功能，讓使用者可以移除不需要的回憶記錄和相關圖片檔案。

#### 使用者故事 (User Story)
As a 使用者, I want 刪除回憶記錄 so that 我能清理過時或不需要的內容。

#### 接受標準 (Acceptance Criteria)
- 在記憶列表中每個項目有 "刪除" 連結
- 刪除動作需要確認對話框防止意外刪除
- 刪除時同時移除資料庫記錄和圖片檔案
- 刪除成功後顯示成功訊息並重新導向列表

#### 技術考量
- 新增 `/Pages/Memories/Delete.cshtml` 和 `.cs` (或使用 OnPostDelete)
- JavaScript 確認對話框
- Entity Framework 刪除操作
- 檔案系統清理

#### 優先順序
高

### 功能名稱: 搜尋和過濾功能 - Search and Filter Functionality

#### 描述
讓使用者能透過關鍵字搜尋回憶標題和描述，或依日期範圍篩選。

#### 使用者故事 (User Story)
As a 使用者, I want 搜尋特定回憶 so that 當有許多記錄時能快速找到想看的內容。

#### 接受標準 (Acceptance Criteria)
- 索引頁面有搜尋欄位
- 支援標題、描述全文搜尋
- 提供日期範圍篩選器
- 顯示搜尋結果數量
- 有清除搜尋的按鈕

#### 技術考各考量
- 修改 Index.cshtml.cs 新增搜尋邏輯
- Entity Framework LINQ 查詢
- 介面: 搜尋框和篩選選擇器

#### 優先順序
中

### 機能名稱: 記憶詳細頁面 - Memory Details Page

#### 描述
提供專門的詳細頁面顯示單一回憶內容，包含大圖預覽和完整資訊。

#### 使用者故事 (User Story)
As a 使用者, I want 查看完整回憶內容 so that 能詳細欣賞圖片和閱讀完整描述。

#### 接受標準 (Acceptance Criteria)
- 從列表點擊 "詳情" 或標題連結到詳細頁面
- 顯示大圖片、全尺寸檢視
- 顯示所有欄位資訊
- 提供回到列表的連結

#### 技術考量
- 新增 `/Pages/Memories/Details.cshtml` 和 `.cs`
- 更新 Index.cshtml 加入詳細連結
- 反對善 Entity Framework 單一記錄查詢

#### 優先順序
中

### 功能名稱: 分類標籤系統 - Category Tag System

#### 描述
允許為記憶加上分類標籤，如 "子女成長"、"家庭旅行"、"節慶"等，讓回憶更有組織性。

#### 使用者故事 (User Story)
As a 使用者, I want 為回憶加上分類 so that 更好地組織和管理我的家庭記錄。

#### 接受標準 (Acceptance Criteria)
- 可以為每筆回憶選擇/建立分類
- 支援多個標籤
- 在列表中顯示標籤
- 可按標籤過濾
- 預設分類建議 (子女、親戚、活動等)

#### 技術考量
- 需新增 Category 模型
- 多對多關聯 (Memory-Category)
- 更新資料庫結構
- 修改 UI 新增標籤選擇器

#### 優先順序
低

### 功能名稱: 資料匯出功能 - Data Export Feature

#### 描述
允許使用者將回憶資料匯出到 JSON 或 CSV 格式以備份或共享。

#### 使用者故事 (User Story)
As a 使用者, I want 匯出我的回憶資料 so that 我能備份資料或在其他系統中使用。

#### 接受標準 (Acceptance Criteria)
- 支援 JSON 和 CSV 格式匯出
- 包含圖片檔案下載選項
- 下載檔案以時間戳記命名
- 成功匯出訊息

#### 技術考量
- 新增匯出控制器或頁面
- JSON 序列化
- ZIP 壓縮圖片檔案
- 檔案下載實作

#### 優先順序
低

---

## 🔄 開發狀態
- ✅ 已完成: 基礎 CRUD (Create, Read)
- 🔄 進行中: 無
- 📋 待辦: Edit, Delete, Search, Details, Categories, Export

## 📝 新需求範例

如果您需要新功能，請參考上述格式在此處新增：

### 功能名稱: [新功能名稱]

#### 描述
[詳細描述]

#### 使用者故事
As a [角色], I want [功能] so that [價值]

#### 接受標準
- [條件1]
- [條件2]

#### 技術考量
- [前端/後端/資料庫需求]

#### 優先順序
[高/中/低]

---

## 💡 給 AI 的開發提示

當您請 AI 開發時，請包含：
1. 此要求檔案的相關段落
2. 專案架構參考 (README.md)
3. 具體的檔案路徑
4. 任何特殊技術要求

這將幫助 AI 更精確地理解您的需求和專案脈絡。
