# HRPortal .NET 8 升級專案

## 🏗️ 架構總覽

這是一個**進行中的 .NET Framework 4.8 → .NET 8 遷移**專案，涵蓋 36 個專案的大型企業人資系統。程式碼展示了多層架構設計，採用 Repository/UoW 模式與 Entity Framework Core。

### 核心架構層級
- **`HRPortal.Core.Contracts`**: 介面定義與實體模型（無相依性基礎層）
- **`HRPortal.Core`**: 商業邏輯、資料存取層、服務層（EF Core 實作）  
- **`HRPortal`**: ASP.NET Core 8 MVC 網頁應用程式
- **`YoungCloud.*`**: 共用工具程式庫（配置、安全、擴充功能）

### 關鍵相依性流向
```
HRPortal (Web) → HRPortal.Core → HRPortal.Core.Contracts (interfaces/entities)
```
**絕對不要**從 `HRPortal.Core.Contracts` 參考 `HRPortal.Core` - 這會造成循環相依性問題。

## 🔧 開發工作流程

### 建置指令 (PowerShell)
```powershell
# 建置特定專案（快速迭代）
dotnet build HRPortal.Core.Contracts/HRPortal.Core.Contracts.csproj --verbosity minimal

# 建置完整解決方案
dotnet build HRPortal.sln

# 檢查升級進度中的編譯錯誤
dotnet build HRPortal.Core/HRPortal.Core.csproj --verbosity minimal
```

### 遷移進度追蹤
- 監控 `upgrade-status-report-2025-08-05.md` 了解當前詳細狀態與錯誤分析  
- 參考 `upgrade-pause-report.md` 了解暫停狀況與重啟指引
- **當前狀態：73 個編譯錯誤**（從原始 179 個改善 59%）

## 📋 程式碼模式與慣例

### 實體設計模式
所有實體都繼承自 `HRPortal.Core.Contracts.Entities` 中的 `BaseEntityWithConcurrency`：
```csharp
public class Employee : BaseEntityWithConcurrency
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsManager { get; set; }
    // 永遠使用 Guid ID，絕不使用 int
}
```

### Repository 模式實作
- **介面**: `HRPortal.Core.Contracts.Repositories.I*Repository`
- **實作**: `HRPortal.Core.Repositories.Implementations.*Repository`
- 所有 repository 都繼承自 `GenericRepository<T>` 並實作特殊化介面

範例：
```csharp
public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(DbContext context) : base(context) { }
    
    public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber && e.CompanyId == companyId && !e.IsDeleted, cancellationToken);
    }
}
```

### 服務層模式
`HRPortal.Core.Services.Implementations` 中的服務遵循此模式：
```csharp
public class EmployeeService : BaseService<Employee>, IEmployeeService
{
    private readonly IHRPortalUnitOfWork _unitOfWork;
    
    public EmployeeService(IHRPortalUnitOfWork unitOfWork, ILogger<EmployeeService> logger) 
        : base(unitOfWork.Employees, unitOfWork, logger)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### 相依性注入註冊
使用 `ServiceCollectionExtensions.cs` 進行 DI 設定：
```csharp
public static IServiceCollection AddHRPortalCore(this IServiceCollection services)
{
    // Repository 註冊
    services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    services.AddScoped<ICompanyRepository, CompanyRepository>();
    services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    
    // Unit of Work 註冊
    services.AddScoped<IHRPortalUnitOfWork, HRPortalUnitOfWork>();
    
    // Service 註冊
    services.AddScoped<IEmployeeService, EmployeeService>();
    
    return services;
}
```

## 🚨 **緊急狀況：關鍵檔案遺失**

### ⚠️ 阻塞性問題
**`HRPortal.Core/Repositories/Implementations/CompanyRepository.cs`** 檔案已被清空，導致：
- `CS0246: 找不到類型或命名空間名稱 'CompanyRepository'` (3個錯誤)
- ServiceCollectionExtensions 無法註冊 CompanyRepository
- HRPortalUnitOfWork 無法實例化 Companies 屬性

### 🚀 立即修復步驟
```csharp
// 重建 CompanyRepository.cs
public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(DbContext context) : base(context) { }
    
    public async Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted, cancellationToken);
    }
    
    public async Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.TaxID == taxId && !c.IsDeleted, cancellationToken);
    }
    
    // ... 其他必要方法
}
```

## 🎯 **當前遷移狀態 (2025年8月5日 最新更新)**

**階段**: Phase 1 ✅ 完成 → Phase 2 🎯 進行中  
**編譯錯誤**: **71 個**（從 73 個改善 2.7%）  
**當前狀態**: CompanyRepository.cs 重建完成，準備 P1 Service 層修復

### 🔍 當前錯誤分析 (71個)
```
✅ 已修復 - P0 阻塞性 (3個)
├── CompanyRepository 類型缺失 (3個) ✅ 已解決

🚨 高優先級 - Service 層問題 (12個) 🎯 當前目標
├── IUnitOfWork vs IHRPortalUnitOfWork 不匹配 (10個)
└── Repository 介面轉換失敗 (2個)

⚠️ 中優先級 - 方法簽名 (35個)  
├── Repository 方法多載不匹配 (15個)
├── GetByIdAsync 參數數量錯誤 (10個)
└── 專用方法缺失 (10個): GetByCodeAsync, IsCodeExistsAsync, CanDeleteAsync

🔧 低優先級 - 類型轉換 (21個)
├── GUID vs int 類型衝突 (16個)
└── 條件運算式類型不匹配 (5個)

🛠️ 技術債務 - 非阻塞 (3個)
├── DataCache.cs dynamic 類型問題 (1個)
├── LinqExtensions switch 表達式 (1個)
└── ISoftDeletable 命名空間衝突 (1個)
```

### 📊 專案結構狀況 (更新)
```
HRPortal.Core.Contracts/    ✅ 完成 (0 錯誤)
├── Entities/               ✅ Company, Department, Employee, User, Form 等
├── Repositories/           ✅ ICompanyRepository, IDepartmentRepository 等
├── Common/                 ✅ PagedResult, 共用類型完成
└── UnitOfWork/             ✅ IHRPortalUnitOfWork 介面完成

HRPortal.Core/              🔄 改善中 (71 錯誤，從 73 改善)
├── Repositories/Impl./     ✅ CompanyRepository.cs 重建完成
├── Services/Impl./         🎯 需要 IUnitOfWork → IHRPortalUnitOfWork 替換
├── UnitOfWork/             ✅ HRPortalUnitOfWork 實作正常
└── Extensions/             ✅ ServiceCollectionExtensions 已解除阻塞
```

### 🚀 修復優先順序 (更新: P0 完成)
```
✅ Phase 1: P0 緊急修復 (已完成 - 5分鐘)
├── ✅ CompanyRepository.cs 重建完成
└── ✅ 編譯錯誤從 73 → 71 個

🎯 Phase 2: P1 Service 層修復 (當前目標 - 10分鐘)
├── 替換 IUnitOfWork → IHRPortalUnitOfWork (所有 Service 層)
└── 修復 Repository 介面轉換失敗

Phase 3: P2 介面對齊 (10分鐘)
├── 修復 Repository 介面實作不匹配
└── 補充缺失的專用方法

Phase 4: P3 類型統一 (8分鐘)  
├── 解決 GUID/int 衝突
└── 修復方法簽名參數不匹配

Phase 5: 最終清理 (2分鐘)
└── 解決技術債務問題
```

### 🔧 快速診斷指令
```powershell
# 檢查當前錯誤數（應該顯示 73 個錯誤）
dotnet build HRPortal.Core/HRPortal.Core.csproj --verbosity minimal

# 檢查 Contracts 專案（應該成功編譯，0 錯誤）
dotnet build HRPortal.Core.Contracts/HRPortal.Core.Contracts.csproj --verbosity minimal

# 檢查 CompanyRepository 檔案狀態
Get-Content "HRPortal.Core/Repositories/Implementations/CompanyRepository.cs"
```

### 遷移專用指南

#### 命名空間遷移規則
- **舊**: `HRPortal.Core.Entities` → **新**: `HRPortal.Core.Contracts.Entities`
- **舊**: `HRPortal.Core.Repositories` → **新**: `HRPortal.Core.Contracts.Repositories` （介面）
- **舊**: `IUnitOfWork` → **新**: `IHRPortalUnitOfWork`

#### 常見編譯問題與修復
1. **檔案遺失**: 檢查關鍵實作檔案是否存在且非空
2. **介面不匹配**: 確保實作類別正確繼承並實作所有介面方法
3. **依賴注入類型錯誤**: Service 層必須使用 `IHRPortalUnitOfWork`，不是 `IUnitOfWork`

### 📝 重要文檔
- **`upgrade-status-report-2025-08-05.md`**: ⭐ 最新詳細狀況報告
- **`upgrade-pause-report.md`**: 暫停狀況與重啟指引
- **`issue-tracking.md`**: 問題優先級追蹤
- **`update-log.md`**: 📜 文件更新歷程追蹤
- **`upgrade-analysis.md`**: 整體升級分析與技術決策記錄

## ⚡ 立即行動項目
1. **檢查 CompanyRepository.cs** - 如果檔案為空，立即重建
2. **修復 Service 依賴注入** - 替換所有 IUnitOfWork → IHRPortalUnitOfWork  
3. **驗證編譯錯誤數** - 確認實際為 73 個，不是 76 個

**目標**: 25 分鐘內達到編譯成功，恢復系統可用性。

---

## 📊 **執行狀況追蹤 (實時更新)**

### 最後檢查時間: 2025年8月5日
- **CompanyRepository.cs 狀態**: ❌ **檔案為空** (0 行)
- **當前編譯錯誤**: 73 個 (已確認)
- **阻塞等級**: 🚨 **高** - 無法繼續開發直到修復
- **預估修復時間**: 25 分鐘

### 下次執行檢查清單
- [ ] 執行快速診斷: `dotnet build HRPortal.Core/HRPortal.Core.csproj --verbosity minimal`
- [ ] 確認錯誤數是否仍為 73 個
- [ ] 檢查 CompanyRepository.cs 是否仍為空
- [ ] 開始 Phase 1 緊急修復

### 🔧 快速參考指令
```powershell
# 檢查檔案存在性
Test-Path "HRPortal.Core/Repositories/Implementations/CompanyRepository.cs"

# 檢查檔案內容長度
(Get-Content "HRPortal.Core/Repositories/Implementations/CompanyRepository.cs").Count

# 快速編譯測試
dotnet build HRPortal.Core/HRPortal.Core.csproj --verbosity minimal --no-restore
```

---

## 📚 **文檔管理系統**

### 🗂️ 文檔結構與用途
```
HRPortal 升級文檔系統/
├── 📋 主要指令
│   └── .github/copilot-instructions.md      ⭐ 本檔案 - 開發指令與規範
├── 📊 狀態報告  
│   ├── upgrade-status-report-2025-08-05.md  📈 當前詳細狀況 (最新)
│   ├── upgrade-pause-report.md              ⏸️ 暫停狀況與重啟指引  
│   └── upgrade-analysis.md                  📋 整體分析與技術決策
├── 🔍 問題追蹤
│   └── issue-tracking.md                    🎯 問題優先級與解決狀況
└── 📜 維護記錄
    └── update-log.md                        📝 文件更新歷程與變更日誌
```

### 📖 文檔使用指南

#### **開發時優先查看**
1. **`.github/copilot-instructions.md`** (本檔案) - 編碼規範與當前指令
2. **`upgrade-status-report-2025-08-05.md`** - 最新錯誤分析與修復計劃
3. **`issue-tracking.md`** - 當前優先級問題清單

#### **問題排查時參考**
1. **`upgrade-analysis.md`** - 技術決策與架構分析
2. **`upgrade-pause-report.md`** - 歷史狀況與重啟指引  
3. **`update-log.md`** - 變更歷程與文檔一致性檢查

#### **文檔維護指引**
- **狀態變更時**: 更新 `upgrade-status-report-2025-08-05.md` 和 `issue-tracking.md`
- **新問題發現時**: 記錄於 `issue-tracking.md` 並更新主指令檔
- **文檔修改時**: 記錄於 `update-log.md` 確保一致性

### 🔄 文檔同步檢查
```powershell
# 檢查所有升級相關文檔是否存在
ls upgrade-*.md, issue-tracking.md, update-log.md -ErrorAction SilentlyContinue | Select-Object Name

# 快速查看所有 markdown 檔案
ls *.md | Select-Object Name, LastWriteTime | Format-Table

# 檢查關鍵檔案最後更新時間
@("upgrade-status-report-2025-08-05.md", "issue-tracking.md", ".github\copilot-instructions.md") | 
ForEach-Object { if (Test-Path $_) { Get-Item $_ | Select-Object Name, LastWriteTime } }

# 文檔完整性檢查 - 驗證所有必要檔案存在
ls upgrade-status-report-2025-08-05.md, upgrade-analysis.md, issue-tracking.md, update-log.md

# 檢查特定檔案是否存在
Test-Path "upgrade-status-report-2025-08-05.md"
Test-Path "HRPortal.Core/Repositories/Implementations/CompanyRepository.cs"
```
