# 實體類別完成報告

## 📅 完成日期
2025年8月6日

## ✅ 已完成的實體類別

### 1. 基礎實體類別 (`BaseEntity.cs`)
- **BaseEntity**: 基本ID、時間戳記
- **BaseEntityWithSoftDelete**: 支援軟刪除
- **BaseEntityWithConcurrency**: 支援樂觀並發控制

### 2. 組織架構實體
- **Company** (`Company.cs`): 公司實體
- **Department** (`Department.cs`): 部門實體，支援階層結構
- **Employee** (`Employee.cs`): 員工實體

### 3. 表單實體
- **Form** (`Form.cs`): 表單基底類別
- **LeaveForm** (`LeaveForm.cs`): 請假表單
- **OvertimeForm** (`OvertimeForm.cs`): 加班表單
- **PatchForm** (`PatchForm.cs`): 補卡表單

### 4. 權限管理實體
- **User** (`User.cs`): 使用者實體
- **Role** (`Role.cs`): 角色實體
- **UserRole** (`Role.cs`): 使用者角色對應
- **Menu** (`Menu.cs`): 選單實體
- **RoleMenu** (`Role.cs`): 角色選單對應

### 5. 系統管理實體
- **SystemSetting** (`SystemEntities.cs`): 系統設定
- **MailAccount** (`SystemEntities.cs`): 郵件帳號
- **MailMessage** (`SystemEntities.cs`): 郵件訊息

## 🔧 設計特點

### 繼承結構
```
BaseEntity
├── BaseEntityWithSoftDelete
│   └── BaseEntityWithConcurrency
│       ├── Company
│       ├── Department
│       ├── Employee
│       ├── Form (abstract)
│       │   ├── LeaveForm
│       │   ├── OvertimeForm
│       │   └── PatchForm
│       ├── User
│       ├── MailAccount
│       └── MailMessage
├── Role
├── Menu
└── SystemSetting
```

### 枚舉類型
- **FormStatus**: 表單狀態
- **LeaveType**: 請假類型
- **PatchType**: 補卡類型
- **MailStatus**: 郵件狀態

### 關聯設計
- **一對多關聯**: Company → Departments, Company → Employees
- **自參考關聯**: Department → SubDepartments, Menu → Children
- **多對多關聯**: User ↔ Role, Role ↔ Menu
- **繼承關聯**: Form → LeaveForm/OvertimeForm/PatchForm

### 業務邏輯方法
- **LeaveForm**: `CalculateDays()` - 計算請假天數
- **OvertimeForm**: `CalculateHours()`, `UpdateHours()` - 計算加班時數
- **PatchForm**: `GetFullPatchDateTime()` - 取得完整補卡時間
- **User**: `IsLockedOut()`, `ResetFailedLoginCount()` - 帳號鎖定管理
- **MailMessage**: `GetRecipientList()`, `MarkAsSent()` - 郵件管理

## 📊 統計資訊
- **實體類別總數**: 15 個
- **枚舉類型**: 4 個
- **關聯類型**: 2 個 (UserRole, RoleMenu)
- **業務方法**: 10+ 個

## 🎯 下一步工作
1. [ ] 建立 DbContext
2. [ ] 配置實體關聯 (Fluent API)
3. [ ] 實作資料存取層 (Repository)
4. [ ] 建立服務層介面

---
**狀態**: ✅ 完成  
**品質**: 高品質，遵循 DDD 原則
