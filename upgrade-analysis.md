# ASP.NET MVC 升級至 .NET 8 分析報告

## 1. 專案架構分析

### 1.1 主要專案結構

#### Web 應用程式
- `HRPortal` (主要Web專案)
  - 使用傳統的 ASP.NET MVC 架構
  - 包含 Web.config 配置
  - 使用 OWIN 中介軟體
- `HRPortal.Mvc.DDMC_PFA`
  - 特定功能的 MVC 模組

#### API 和服務層
- `HRPortal.ApiAdapter`
- `HRPortal.Services.DDMC_PFA`
- `HRPortal.WebAPI`

#### 資料存取層
- `HRPortal.DBEntities`
- `HRPortal.DBEntities.DDMC_PFA`
- `YoungCloud.Databases`

#### 背景服務
- `HRPortal.BackgroundService.BambooHRIntegration`
- `HRPortal.EffectiveDateSchedule`
- `HRPortal.SendMailService`

#### 共用元件
- `HRPortal.Core`
- `HRPortal.Models`
- `YoungCloud.Extensions`
- `YoungCloud.Security`

### 1.2 技術堆疊分析

#### 框架與函式庫
- ASP.NET MVC 5
- Entity Framework 6.1.3
- Microsoft.Owin.Security.OAuth
- AutoMapper 3.3.1
- Newtonsoft.Json
- NLog

#### 架構特性
- OWIN 中介軟體處理認證
- WebAPI 使用 AttributeRouting
- packages.config 套件管理
- 使用 Global.asax 啟動配置
- 傳統 Web.config 配置方式

## 2. 升級策略

### 2.1 分階段升級計劃

#### 第一階段：升級至 .NET Framework 4.8
1. 更新所有專案的目標框架
2. 更新 NuGet 套件到最新的 .NET Framework 相容版本
3. 修復任何相容性問題
4. 完整測試確保功能正常

#### 第二階段：轉換至 .NET 8
1. 從較小的專案開始轉換：
   - `HRPortal.Core`
   - `HRPortal.Models`
   - `HRPortal.ApiAdapter`
2. 逐步轉換較大的專案
3. 最後轉換主要的 Web 專案

### 2.2 需要注意的重點

#### 架構變更
- 移除 OWIN 中介軟體，改用 ASP.NET Core 中介軟體
- 更新認證機制至 ASP.NET Core Identity
- 轉換 Entity Framework 6 到 EF Core
- 重構 Global.asax 至 Program.cs 與 Startup.cs

#### 配置轉換
- Web.config 轉換至 appsettings.json
- 移除 packages.config，改用 SDK-style 專案格式
- 更新 DI 容器配置

#### 程式碼更新
- 更新控制器基底類別
- 修改 Action 方法簽章
- 更新 Razor 視圖語法
- 調整靜態檔案處理方式

## 3. 準備工作清單

### 3.1 環境準備
- [ ] 安裝 .NET 8 SDK
- [ ] 安裝升級助手 (upgrade-assistant)
- [ ] 安裝 API 分析工具 (apiport)
- [ ] 建立源碼版控分支

### 3.2 評估工作
#### HRPortal.Core 分析結果
- [x] 完整的相依性分析
  - 已發現 7 個需要處理的問題
  - 4 個強制性變更
  - 2 個選擇性變更
  - 29 個潛在變更點
- [x] 識別不相容的第三方套件
  - EntityFramework 6.1.3 -> Microsoft.EntityFrameworkCore 8.0.0
  - LinqKit 1.1.2 -> LinqKit.Core 1.2.4
  - Newtonsoft.Json 6.0.8 -> 13.0.3
  - NLog 3.2.0 -> 5.2.5
- [ ] 評估資料庫相容性
- [ ] 評估需要重寫的功能

#### HRPortal.Core 升級進度
1. [x] 轉換專案檔至 SDK-style 格式
   - 移除舊的專案格式
   - 新增 SDK 參考 (Microsoft.NET.Sdk)
   - 更新目標框架至 .NET 8
   - 更新 NuGet 套件參考
2. [ ] 更新源碼相容性
   - 需要更新的檔案：
     * `Utilities/DataCache.cs` (快取機制可能需要更新)
     * `Utilities/AllPropsContractResolver.cs` (JSON序列化相關)
     * `Utilities/Utility.cs` (通用工具類)
     * `Extensions/LinqExtensions.cs` (LINQ擴展方法)
     * `CultureHelper.cs` (文化資訊相關)
   - 需要移除的檔案：
     * `Properties/AssemblyInfo.cs` (SDK專案自動處理)
   - 主要更新項目：
     * 更新 using 語句
     * 更新已過時的API呼叫
     * 新增檔案頂端的 nullable 參考
     * 更新快取實作從 `System.Runtime.Caching` 到 `Microsoft.Extensions.Caching`
3. [ ] 更新單元測試
4. [ ] 執行完整性測試

#### 今日工作日誌 (2025/08/04)
1. [x] 完成專案檔轉換
   - 建立新的 SDK-style 專案檔
   - 更新套件參考至最新版本
   - 保留原始專案檔備份 (.csproj.bak)
2. [x] 完成源碼檔案清查
   - 識別需要更新的檔案
   - 標記需要特別注意的區域
3. [x] 開始更新源碼檔案
   - 完成 DataCache.cs 的更新:
     * 從 System.Runtime.Caching 遷移到 Microsoft.Extensions.Caching.Memory
     * 加入 Nullable 參考支援
     * 優化字串處理和比較
     * 改進錯誤處理和空值檢查
     * 使用新的 pattern matching 特性
   - 完成 AllPropsContractResolver.cs 的更新:
     * 使用檔案範圍的命名空間宣告
     * 添加 XML 文件註解
     * 實作 CamelCaseNamingStrategy
     * 優化 LINQ 查詢和 pattern matching
     * 改進程式碼可讀性
   - 完成 Utility.cs 的更新:
     * 加入完整的 XML 文件註解
     * 改進參數驗證和錯誤處理
     * 優化正則表達式實作
     * 添加位元序檢查
     * 實作 null 檢查和超時控制
   - 完成 LinqExtensions.cs 的更新:
     * 遷移到 EF Core SQL Server 功能
     * 使用現代 C# 特性增強程式碼可讀性
     * 改進錯誤處理和類型安全性
   - 完成 CultureHelper.cs 的更新:
     * 使用現代 CultureInfo API
     * 改進字符串比較效能
     * 增強參數驗證
     * 添加完整的 XML 文件註解
     * 使用新的 C# 功能（nullable 參考、expression-bodied members）全性
     * 優化 LINQ 表達式建構
     * 添加完整的文件註釋
   - 待處理:
     * CultureHelper.cs (文化資訊相關)

### 3.3 文件準備
- [ ] 建立詳細的升級計劃文件
- [ ] 準備回滾計劃
- [ ] 準備測試計劃
- [ ] 建立問題追蹤機制

## 4. 風險評估

### 4.1 主要風險
1. 第三方套件相容性問題
2. 資料存取層轉換複雜度
3. 認證機制重構風險
4. 效能影響評估

### 4.2 緩解策略
1. 建立完整的測試套件
2. 準備替代方案
3. 分階段部署策略
4. 詳細的監控計劃

## 5. 時程規劃

### 5.1 準備階段 (2週)
- 環境設置
- 詳細分析
- 文件準備

### 5.2 第一階段：.NET Framework 4.8 (3週)
- 更新目標框架
- 更新套件
- 測試與修復

### 5.3 第二階段：.NET 8 (8週)
- 小型專案轉換 (2週)
- 中型專案轉換 (3週)
- 主要 Web 專案轉換 (3週)

### 5.4 測試與部署 (3週)
- 整合測試
- 效能測試
- 安全性測試
- 分階段部署

## 6. 後續追蹤

### 6.1 監控指標
- 應用程式效能
- 錯誤率
- 回應時間
- 資源使用率

### 6.2 維護計劃
- 定期更新相依套件
- 效能優化
- 程式碼重構
- 文件更新

---
*此文件將根據專案進展持續更新*
