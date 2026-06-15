# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Moogle ERP — a personal/enterprise multi-module ERP system with a companion IT CMDB/ITSM application, both built on .NET 10 Blazor. The solution also includes a .NET MAUI cross-platform mobile app.

## Build & Run

All commands run from the repo root where `MoogleKhErp.sln` lives.

```bash
# Restore & build entire solution
dotnet restore
dotnet build MoogleKhErp.sln

# Run the main ERP web app (http://localhost:5191 / https://localhost:7215)
dotnet run --project WebApp/MoogleERP/MoogleERP/MoogleERP.csproj

# Run the PruIT CMDB/ITSM web app
dotnet run --project WebApp/PruIT_CMDB_ITSM/PruIT_CMDB_ITSM/PruIT_CMDB_ITSM.csproj
```

No test projects exist yet. Format check:
```bash
dotnet format MoogleKhErp.sln --verify-no-changes
```

## Solution Structure

```
CoreLibrary/DataLayer/        # Shared data access layer (used by all web apps)
WebApp/MoogleERP/             # Main ERP — Blazor Server host + WebAssembly client
WebApp/PruIT_CMDB_ITSM/       # IT CMDB/ITSM app — same pattern as MoogleERP
MauiApp/MoogleMAUI/           # Cross-platform mobile app (.NET MAUI)
```

## Architecture

### DataLayer (CoreLibrary/DataLayer/)

Shared across all web apps. Key concepts:

- **Infrastructure/** — `DatabaseConfig.cs` (config model with computed `ConnectionString`), `DbContext.cs` (returns `IDbConnection` for MSSQL or PostgreSQL based on config), `ConnectionFactory.cs`.
- **Repos/** — one folder per business module. Each repo follows `{EntityName}Repos.cs` / `I{EntityName}Repos` naming. A generic `BaseRepo<T>` handles standard Dapper CRUD with soft-delete (`Delete`) and hard-delete (`HardDelete`).
- **Unit of Work** — `Uow{AppName}.cs` / `IUow{AppName}` exposes all repos as properties. Injected as a singleton. There are separate UoW implementations per database type: `UowMoogleKhErp` (MSSQL), `UowMoogleKhErpPg` (PostgreSQL), `UowPruIT`.
- **Models/** — organized by module (see modules below). Non-persistent DTOs (pagination, dropdowns, response wrappers) live alongside entity models.
- **GlobalConstant/** — `GC_{Module}.cs` files per module (e.g. `GC_EMS.cs`, `GC_FIN.cs`).
- **AuxComponents/Extensions/** — `{TypeName}Ext.cs` extension methods (String, DateTime, Decimal, Type).

### Business Modules

| Prefix | Domain |
|--------|--------|
| EMS | Event Management System |
| FIN | Finance (invoicing, customers, currencies, taxes) |
| HMS | Healthcare Management System |
| PMS | Pharmacy Management System |
| RMS | Retail Management System |
| LIB | Library Management |
| Pru | Prudential IT (IT assets, CMDB, vendors) |
| SysCore | System Core — shared entities (employees, locations, object state, audit) |
| Hobby / HomeInventory / Music | Personal tracking modules |

### Blazor Web Apps (MoogleERP & PruIT_CMDB_ITSM)

Both apps share the same structure:

- **Server project** (`MoogleERP/` or `PruIT_CMDB_ITSM/`) — hosts the app, owns `Program.cs`, `appsettings.json`, and `Components/`.
- **Client project** (`.Client/`) — Blazor WebAssembly for interactive components. Minimal `Program.cs`.
- **Rendering** — Hybrid: server-side interactive by default, WebAssembly interactive for client-heavy pages.

**Component naming conventions:**
- `{Entity}_MainPage.razor` — list/grid view
- `{Entity}_CRUCPage.razor` — Create / Read / Update / Cancel form
- `{Entity}_*Diag.razor` — modal dialogs (kept in `Components/Dialogs/`)
- Pages live under `Components/Pages/{Module}/`

**Services registered in `Program.cs`:**  
MudBlazor, MudExtensions, SweetAlert2, HotKeys2, localization (Resources path), `DatabaseConfig` (per connection), and the appropriate `IUow*` singletons.

### CRUCPage UI Conventions

`ITAssetHw_CRUCPage.razor` is the canonical reference for all `_CRUCPage` markup. When creating or updating any CRUCPage, match these patterns exactly:

**Header banner**
```razor
<EditForm ... style="height:100%; display:flex; flex-direction:column">
<MudPaper Class="pru-ui-mudcard-hdr-banner px-5 py-4" Width="100%" Elevation="0" Outlined="true">
  <MudStack Row="true" ...>
    <MudStack Row="true" ... Spacing="0">
      <MudText Typo="Typo.h3" Color="Color.Secondary">@HeaderTitle</MudText>
      <MudChip Color="..." Size="Size.Small" Variant="Variant.Outlined" Class="pru-ui-mudchip-banner-status" T="string" Style="margin-top:-10px">VIEW</MudChip>
    </MudStack>
    <!-- buttons -->
  </MudStack>
</MudPaper>
```

**Button colors** — Save/Save&Close: `Color.Secondary`. Cancel: `Color.Default`. Close (view): `Color.Secondary`.

**Tab structure**
```razor
<MudTabs Class="pru-mudtab mt-5" Elevation="0" ActiveTabClass="pru-mudtab-active"
         TabHeaderClass="pru-mudtab-hdr" TabButtonsClass="pru-mudtab-btn" TabPanelsClass="pru-mudtab-tabpanel">
  <MudTabPanel Class="pru-mudtabpanel" PanelClass="pru-mudtabpanel-panel" Style="min-width:120px">
    <MudPaper Class="pru-ui-mudpaper-cruc-tab-content px-5 pt-3 pb-4 overflow-y-auto"
              Square="true" Height="calc(100% - var(--pru-cruc-pg-status-bar-height))" Width="100%" Elevation="0">
      <MudGrid Spacing="4" Justify="Justify.SpaceEvenly"> ... </MudGrid>
    </MudPaper>
    <!-- status bar — always a separate MudPaper, never absolute positioning -->
    <MudPaper Class="pru-mudpaper-status-bar" Elevation="0" Height="var(--pru-cruc-pg-status-bar-height)">
      @if (CurrentObject.Id > 0) { <!-- Created By / Modified By using MudText Typo.subtitle2 --> }
    </MudPaper>
  </MudTabPanel>
</MudTabs>
```

**Form field CSS classes**

| Component | Class | Additional required props |
|-----------|-------|--------------------------|
| `MudTextField` | `pru-ui-mudform-input` | `Immediate="false"`, `ClearIcon="material-symbols-rounded/close"`, `IconSize="Size.Small"` |
| `MudSelect` | `pru-ui-mudform-input` | `PopoverClass="pru-ui-mudselect-popover"` `InputClass="pru-ui-mudselect-input"` `ListClass="pru-ui-mudselect-list"` `OuterClass="pru-ui-mudselect-outer"` `AdornmentIcon="material-symbols-rounded/keyboard_arrow_down"` `AdornmentColor="Color.Primary"` `IconSize="Size.Small"` `Immediate="false"` |
| `MudAutocomplete` | `pru-ui-mudform-input` | `PopoverClass="pru-ui-mudautocomplete-popover"` `InputClass="pru-ui-mudautocomplete-input"` `ListClass="pru-ui-mudautocomplete-list"` `ListItemClass="pru-ui-mudautocomplete-listitem"` `AdornmentIcon="material-symbols-rounded/keyboard_arrow_down"` `AdornmentColor="Color.Primary"` `IconSize="Size.Small"` |
| `MudComboBox` | `pru-ui-combobox` | `PopoverClass="pru-ui-combobox-popover"` `InputClass="pru-ui-combobox-input"` `HighlightClass="pru-ui-combobox-input"` `TemplateClass="pru-ui-combobox-input"` `ChipClass="pru-ui-combobox-chip"` `AdornmentIcon="material-symbols-rounded/keyboard_arrow_down"` `AdornmentColor="Color.Primary"` `Adornment="Adornment.End"` `HasAdornmentEnd="true"` `SearchBox="true"` `SearchBoxAutoFocus="true"` `SearchBoxClearable="true"` |
| `MudDatePicker` | `pru-ui-muddatepicker` | `AdornmentIcon="material-symbols-rounded/calendar_month"` `AdornmentColor="Color.Primary"` |

**Audit trail datagrid (History tab)**
- `EditMode="DataGridEditMode.Form"` `SortMode="SortMode.Multiple"` `ColumnResizeMode="ResizeMode.Column"`
- `EditDialogOptions=@(new(){ BackgroundClass="pru-ui-muddatagrid-edit-diag-bg", BackdropClick=false, MaxWidth=MaxWidth.Medium, FullWidth=true })`
- All edit form fields go in the **first column's `<EditTemplate>`** as a single `MudGrid`. All other columns have empty `<EditTemplate></EditTemplate>`.
- Action buttons: `Class="pru-ui-mudiconbtn-datagrid"` with `material-symbols-rounded/edit` and `material-symbols-rounded/delete` icons.

**Audit trail `@code` requirements**
- `bool _isEditingItem = false;` — track editing state
- `StartedEditingItem`: set `_isEditingItem = true`
- `CanceledEditingItem`: reset `_tempItemToAdd = null` then `_isEditingItem = false`
- `CommittedItemChanges`: reset both, call `StateHasChanged()`, return `DataGridEditFormAction.Close`
- `RemoveItem`: soft-delete with `i.IsDeleted = true` when `i.Id > 0`, otherwise `AuditTrails.Remove(i)`
- "Add Audit Trail" button: `Disabled=_isEditingItem` (not `_tempItemToAdd != null`)

### MAUI App (MauiApp/MoogleMAUI/)

Targets Android, iOS, Windows, and macOS Catalyst. Uses CommunityToolkit.Mvvm for MVVM, Syncfusion.Maui.Toolkit for UI, and SQLite for local storage.

## Configuration

Database connections are in `appsettings.json` under `DatabaseConnectionConfig`. The app supports MSSQL and PostgreSQL simultaneously — the `DatabaseConfig` model resolves the connection string at runtime. Override for local dev in `appsettings.Development.json`.

Development-specific settings also include:
- `UserMediaUploadDirectory` — local path for user file uploads
- `UserMediaUploadUrlPrefix` — corresponding URL prefix
- `DetailedErrors: true`
