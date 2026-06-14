# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MoogleERP is a multi-module enterprise resource planning application built on .NET 10 using **Blazor Server + WebAssembly** (hybrid interactive rendering) with **MudBlazor** as the UI component library. It covers modules for Finance, Healthcare, Inventory, Library, Pharmacy, Resource Management, Tech/System Management, and Event Management.

## Solution Structure

```
MoogleKhErp.sln
├── CoreLibrary/DataLayer/          # Shared data access & domain models (class library)
├── WebApp/MoogleERP/
│   ├── MoogleERP/                  # Blazor Server host app (entry point)
│   └── MoogleERP.Client/           # Blazor WebAssembly client project
├── WebApp/PruIT_CMDB_ITSM/         # Secondary IT/CMDB ERP module
└── MauiApp/MoogleMAUI/             # Cross-platform mobile app (MAUI)
```

## Commands

```bash
# Build the full solution
dotnet build MoogleKhErp.sln

# Run the main web app
cd WebApp/MoogleERP/MoogleERP
dotnet run

# Publish
dotnet publish MoogleKhErp.sln -c Release
```

There are no automated tests in this project.

## Architecture

### Data Layer (`CoreLibrary/DataLayer/`)

**ORM:** Dapper (micro-ORM) — **not** Entity Framework. Uses `Dapper.Contrib` for attribute-based mapping and `Dapper.SqlBuilder` for dynamic query construction.

**Database support:** MSSQL (primary) and PostgreSQL (secondary), with MongoDB and IBM DB2 infrastructure present. Each entity declares both table names:
```csharp
public static string MsSqlTableName => "Person";
public static string PgTableName => "person";   // PostgreSQL uses schema-qualified names e.g. "tsm.laptop"
```

**Entity base class:** All entities extend `AuditObject`, which provides `Id`, `ObjectCode`, `ObjectName`, `IsDeleted` (soft delete), and audit timestamp/user fields.

**Repository pattern with Unit of Work:**
- `IBaseRepos<TEntity>` / `BaseRepos<TEntity>` — generic CRUD base
- `IUowMoogleKhErp` — aggregates all MSSQL repositories
- `IUowMoogleKhErpPg` — aggregates all PostgreSQL repositories
- Both UOW interfaces are registered as **Singletons** in DI

`ConnectionFactory` handles multi-database connection creation based on `DatabaseConfig`.

### Web App (`WebApp/MoogleERP/MoogleERP/`)

**Rendering:** Interactive Server + Interactive WebAssembly (Auto mode). The `.Client` project serves WebAssembly assets; the server project hosts the app.

**Component base classes** (defined in `Components/`):
- `MainPageBase<T>` — base for list/grid pages. Handles paging, searching, row selection, and navigation into CRUD modes. Wires up keyboard shortcuts (Alt+N = new, Ctrl+E = edit).
- `CRUCPageBase<T>` — base for Create/Read/Update/Clone form pages. Handles form validation, pre/post save hooks, and user session cascading.

**Page organization under `Components/Pages/`:**

| Folder | Module |
|--------|--------|
| `Core/` | Persons, Employees, Organizations, Locations, Users |
| `FIN/` | Finance — Currency, Customer, Invoice, Purchase Order |
| `HMS/` | Healthcare — Patient, Doctor, Appointments |
| `HIM/` | Home Inventory — Boardgames, Merchants, Owned Items |
| `LIB/` | Library — Books |
| `PMS/` | Pharmacy — Medicine, Medical Composition |
| `RMS/` | Resource Management — Items, Suppliers |
| `TSM/` | Tech/System — Laptops, CPUs, RAM, Storage |
| `EMS/` | Event Management — Events, Registrations, Organizers |

**Key services registered in `Program.cs`:** MudBlazor, MudExtensions, HotKeys2, SweetAlert2 (Swal), localization, and the two UOW singletons.

### Conventions

- **Soft delete:** use `IsDeleted` flag; never hard-delete audit objects.
- **CRUD modes:** pages operate in `Create`, `Read`, `Update`, or `Clone` mode, controlled by base class state.
- **Cascading parameters:** user session and permission data cascade down the component tree.
- **Dapper attributes:** `[Table]`, `[Computed]`, `[ReadOnly]`, `[Write(false)]` control mapping; `[MaxLength]`, `[StringUnicode]` drive validation and UI hints.
- **Dialogs:** use MudBlazor dialogs located in `Components/Dialogs/`; SweetAlert2 for confirmation prompts.
