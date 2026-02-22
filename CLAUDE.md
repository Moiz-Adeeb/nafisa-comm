# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TRO API is an Equipment Inspection Management System built with ASP.NET Core 8.0. It manages equipment inspections for various equipment types (Reach Stackers, Empty Handlers, Forklifts, Terminal Tractors, and Trailers) in a port/warehouse environment. The system follows Clean Architecture with CQRS pattern and includes real-time notifications via SignalR.

## Solution Structure

The solution follows **Clean Architecture** with 7 projects organized in layers:

```
Src/
├── Core/
│   ├── Domain/              # Entities, enums, interfaces, constants
│   └── Application/         # Business logic, CQRS handlers, services
├── Infrastructure/
│   ├── Persistence/         # EF Core DbContext, configurations, migrations
│   └── Infrastructure/      # Email, PDF, Excel, Image services
├── Common/                  # Shared utilities, constants, extensions
└── Presentation/
    └── WebApi/             # Controllers, filters, SignalR hubs
```

### Key Architectural Patterns

- **CQRS** via MediatR - Commands and Queries separated per feature
- **Clean Architecture** - Dependency inversion, layers isolated
- **Repository Pattern** - Via EF Core DbContext
- **Pipeline Behaviors** - Logging, validation (FluentValidation), performance monitoring
- **Soft Delete** - All entities implement `IBase.IsDeleted` with global query filter

## Technology Stack

- **ASP.NET Core 8.0** (net8.0)
- **SQL Server** with EF Core 8.0.18
- **OpenIddict 5.8.0** - OAuth 2.0 / OpenID Connect server (password & refresh token flows)
- **ASP.NET Core Identity** - User management
- **MediatR 12.4.1** - Mediator pattern for CQRS
- **FluentValidation 11.10.0** - Request validation
- **SignalR** - Real-time notifications
- **NSwag 14.1.0** - OpenAPI/Swagger
- **NLog** - Structured logging
- **iText 9.2.0** - PDF generation
- **MailKit 4.13.0** - Email (SMTP)

## Build, Run & Test Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build InspectionApi.sln

# Run the API (Development)
dotnet run --project WebApi/WebApi.csproj

# Watch mode (auto-reload on changes)
dotnet watch --project WebApi/WebApi.csproj

# Build for production
dotnet build -c Release
```

### API URLs
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger (when enabled in appsettings.json)

### Database Initialization

The database is **automatically initialized** on application startup via `DatabaseInitializer.cs` with:
- Role seeding (Administrator, Supervisor, Inspector, Operator)
- Default users (admin, supervisor, inspectors)
- Equipment types (RS, EH, FL, TT, Rmq)
- Categories (Pneumatiques, Mécaniques, Électriques, Ergonomiques)
- Sample equipment records
- Questionnaires with 40+ inspection questions per equipment type

**Connection String**: `Server=localhost\\SQLEXPRESS;Database=tro;Trusted_Connection=True;`

## Architecture Details

### API Structure

All API endpoints follow the pattern: `/api/v1/[controller]`

**Key Endpoints**:
- `/connect/token` - OpenIddict token endpoint (authentication)
- `/notification` - SignalR hub endpoint (real-time notifications)
- `/api/v1/users`, `/api/v1/equipments`, `/api/v1/inspections`, etc.

### Request Flow

1. **HTTP Request** → Controller action
2. **Controller** → Sends MediatR request via `Mediator.Send()`
3. **Pipeline Behaviors** execute:
   - Request logging
   - FluentValidation (auto-validation)
   - Performance monitoring
4. **Request Handler** → Executes business logic
5. **DbContext** → Database operations (EF Core)
6. **Response** → Serialized JSON with custom DateTime converters

### Controllers

All controllers inherit from `BaseController` which provides:
- Lazy-loaded `IMediator` property
- Minimal logic (thin controllers)
- All business logic delegated to Application layer handlers

### CQRS Organization

Each feature in `Application/Services/` has:
```
[Feature]/
├── Commands/
│   └── Create[Feature]/
│       ├── Create[Feature]RequestModel.cs
│       ├── Create[Feature]RequestValidator.cs
│       └── Create[Feature]RequestHandler.cs
└── Queries/
    └── Get[Feature]/
        ├── Get[Feature]RequestModel.cs
        └── Get[Feature]RequestHandler.cs
```

### Authentication & Authorization

**Authentication Flow**:
1. Login via `POST /connect/token` with grant_type=password
2. Returns access token (1 day) + refresh token (30 days)
3. Token includes claims: UserId, UserName, FullName, Email, Roles
4. Use Bearer token in Authorization header

**Default Users** (seeded on first run):
- Administrator: admin@example.com (password in AppConstant)
- Supervisor: supervisor@example.com / Supervisor@123
- Inspectors: inspector1-3@example.com / Inspector@123

**Authorization**:
- Role-based: `[Authorize(Roles = "Administrator")]`
- Custom filter: `CustomAuthorizeFilter`
- SignalR groups: Role-based and user-specific channels

### Entity Framework Core

**DbContext**: `ApplicationDbContext` in `Persistence/Context/`

**Important Features**:
- **Global Query Filter** - Automatically filters `IsDeleted = true`
- **Snake case naming** - Via EFCore.NamingConventions
- **Cascade Restrictions** - All relationships use `DeleteBehavior.Restrict`
- **Audit fields** - CreatedDate, UpdatedDate auto-populated
- **Entity Configurations** - Fluent API in `Persistence/Configurations/`

**Core Entities**:
- Users (ASP.NET Identity)
- Equipment, EquipmentType
- Questionnaire, Question, Answer
- Inspection
- Category, Shift
- AppNotification, AuditLog

### Dependency Injection

**Singleton Services**: ImageService, SmtpService, PdfService, ExcelService, WordService, CsvService, AlertService, BackgroundTaskQueueService

**Scoped Services**: ApplicationDbContext, MediatR handlers, Identity services

**Transient Services**: SessionService, MediatR pipeline behaviors

### Error Handling

- **Global exception filter** - `CustomExceptionFilterAttribute` catches all exceptions
- **Custom exceptions**: `BadRequestException`, `AlreadyExistsException`
- **Structured error responses** - Consistent JSON error format
- **NLog integration** - All errors logged

### Real-time Notifications

- **SignalR Hub** - `NotificationHub` at `/notification`
- **Role-based groups** - Broadcast to Administrator, Supervisor, Inspector
- **User-specific channels** - Personal notifications via user ID
- **Connection tracking** - Concurrent dictionary for active connections

### Background Tasks

- **Queue-based processing** - `IBackgroundTaskQueueService`
- **Hosted service** - `QueuedHostedService` processes queued tasks
- **Use case**: Async audit logging

### Validation

- **FluentValidation** - All request models have validators
- **Automatic validation** - Via `ValidationActionFilter` in pipeline
- **Custom validators** - Email, password strength, max length
- **ModelState suppressed** - Using FluentValidation exclusively

### Localization

- **Bilingual support** - French & English throughout
- **Entity properties**: `Title` / `TitleFr`
- **Audit logs**: `Description` / `DescriptionFr`
- **UI assets**: `wwwroot/images/` organized by category with bilingual naming

## Development Workflow

### Adding a New Feature

1. **Create entity** in `Domain/Entities/`
2. **Add DbSet** in `ApplicationDbContext.cs`
3. **Create entity configuration** in `Persistence/Configurations/`
4. **Generate migration**: `dotnet ef migrations add Add[Feature] --project Persistence --startup-project WebApi`
5. **Update database**: `dotnet ef database update --project Persistence --startup-project WebApi`
6. **Create DTOs** in `Application/Services/[Feature]/Models/`
7. **Implement Commands/Queries** in `Application/Services/[Feature]/`
8. **Add FluentValidation validators**
9. **Add controller** in `WebApi/Controllers/V1/`
10. **Test via Swagger UI**

### Modifying an Existing Entity

1. Update entity class in `Domain/Entities/`
2. Update entity configuration in `Persistence/Configurations/` if needed
3. Create migration: `dotnet ef migrations add Update[Feature] --project Persistence --startup-project WebApi`
4. Apply migration: `dotnet ef database update --project Persistence --startup-project WebApi`
5. Update affected DTOs, validators, and handlers

### Code Naming Conventions

- **DTOs**: `[Feature]Dto` (e.g., `UserDto`)
- **Requests**: `[Action][Feature]RequestModel` (e.g., `CreateUserRequestModel`)
- **Responses**: `[Action][Feature]ResponseModel` (e.g., `CreateUserResponseModel`)
- **Validators**: `[Request]Validator` (e.g., `CreateUserRequestValidator`)
- **Handlers**: `[Request]Handler` (e.g., `CreateUserRequestHandler`)
- **One file per handler**: Request, Validator, Handler typically in same file

### Configuration Files

**appsettings.json** contains:
- Connection strings
- SMTP settings (Email, Password, Server, Port)
- Swagger enabled/disabled
- URLs configuration
- CORS allowed origins
- Logging levels

**Environment-specific**: `appsettings.Development.json` overrides for development

### Static Assets

**wwwroot/images/** - SVG images for inspection questions organized by category:
- `pneumatique.*` - Tire/wheel inspection items
- `mechanique.*` - Mechanical inspection items
- `electrique.*` - Electrical inspection items
- `ergonomic.*` - Safety/ergonomic inspection items

## Important Notes

- **Database auto-migration** - Database is automatically created and seeded on first run
- **Soft deletes** - Never hard delete entities; set `IsDeleted = true`
- **Query filters** - All queries automatically exclude deleted records
- **Cascade restrictions** - Manual cleanup required before deleting parent entities
- **Token lifetimes** - Access: 1 day, Refresh: 30 days (configurable in Program.cs)
- **Password requirements** - Minimum 4 characters, must contain digit
- **Case-insensitive email** - Usernames are email addresses, case-insensitive
- **Bilingual content** - Always provide both English and French text for user-facing content


## 1. Document Overview

This PRD defines the complete requirements for a comprehensive Employee Loan Management System with separate modules for Admin, Company Admin, and Employee roles. The system enables companies to offer customized loan products to their employees with flexible terms and easy approval workflows.

---

## 2. System Architecture

The system consists of three main modules:

1. **Admin Module** - System administration and global settings
2. **Company Module** - Company administration and employee management
3. **Employee Module** - Employee loan applications and management

---

## 3. Admin Module

### 3.1 Subscription Plans Management

Administrators can create and manage tiered subscription plans for companies.

**Fields:**
- **Plan Name** - Unique identifier for subscription plan
- **Billing Cycle** - Monthly or Yearly options
- **Price (CFA)** - Cost in CFA currency
- **Max Users Allowed** - Maximum employees allowed on plan
- **Branches** - Min 1, Max N branches allowed per plan
- **Features** - Text area with line breaks describing plan features
- **IsActive** - Boolean flag to enable/disable plan

---

### 3.2 Company Management

Administrators manage company registrations, approvals, and subscription assignments.

**Company Registration Fields:**
- First Name
- Last Name
- Email
- Phone Number
- Password
- Confirm Password
- Business Registration Certificate (File Upload: PNG, JPG, PDF)
- VAT Certificate (File Upload: PNG, JPG, PDF)
- Authorization ID Proof (File Upload: PNG, JPG, PDF)

**Company Approval Workflow:**
1. Company submits registration with required documents
2. Admin reviews and approves company account
3. Admin sends approval email notification to company
4. Admin selects subscription plan for company
5. Invoice is auto-generated with Accept/Reject options
6. Company can upload invoice or payment proof for subscription request
7. Upon activation, record time is tracked

---

### 3.3 Admin Staff Management

Create and manage system admin staff accounts.

**Staff Fields:**
- First Name
- Last Name
- Email
- Phone Number
- RoleId - Role assignment
- Active flag - When toggled inactive, logs out all sessions via SignalR and Firebase

---

### 3.4 Admin Roles & Permissions

Create custom roles with fine-grained permission claims.

**Role Configuration:**
- Role Name
- Description
- Permission Lists (Claims-based access control)

---

### 3.5 Global App Settings

Configure system-wide loan parameters:
- Global Loan Interest Rate - Applied to all loan offers
- Maximum Loan Limit
- Minimum Loan Amount

---

## 4. Company Admin Module

### 4.1 Company Roles & Permissions

Company admins can create custom roles with claims-based permissions.

**Role Configuration:**
- Role Name
- Description
- Permission Lists (Claims)

---

### 4.2 Branch Management

Create and manage company branches.

**Branch Fields:**
- Name
- Code - Unique branch identifier
- Address
- Phone Number
- Active flag - When toggled inactive, logs out all branch users via SignalR and Firebase

---

### 4.3 Company Staff Management

Create and manage employee accounts within company branches.

**Staff Fields:**
- First Name
- Last Name
- Email
- Phone Number
- Branch ID - Assign employee to specific branch
- RoleId - Role assignment
- Salary - Employee salary
- PIN (4-digit)
- Confirm PIN (4-digit)
- Profile Picture (File Upload: PNG, JPG)
- ID Card (File Upload: PNG, JPG)
- Active flag - When toggled inactive, logs out user sessions via SignalR and Firebase

---

### 4.4 Loan Offers Management

Create predefined loan products with customizable terms.

**Offer Fields:**
- Title - Offer name
- Interest Rate - Read-only, pulled from Global App Settings
- Loan Min - Minimum loan amount
- Loan Max - Maximum loan amount
- Durations - 6 Months, 12 Months, 24 Months (checkboxes or multi-select)
- Description - Loan offer details
- Active - Boolean flag

---

### 4.5 Payroll Generation

Allow company admin to generate employee salaries for specific branches and months.

**Functionality:**
- View all employees of a branch
- Select month for payroll
- Generate salaries for each employee
- For each employee/month combination:
    - Create new payroll record
    - User approves by uploading screenshot proof
    - Track approval status

---

### 4.6 Loan Requests Management

Manage employee loan requests with approval/rejection workflow.

**Loan Request Fields:**
- Employee - Link to employee record
- Title - Loan name/title
- Amount - Requested amount
- Duration - Loan period (6, 12, or 24 months)
- Rate - Interest rate
- Total Amount Pay Back - Calculated field
- Monthly Pay Back - Calculated field
- Purpose - Reason for loan
- Status - Approve/Reject options

**Workflow:**
- When Loan Approved: Send notification to Employee
- When Loan Rejected: Send notification to Employee

**Dashboard Information:**
- Display Loan History
- Employee Salary
- Active Loans
- Loan Balance
- Payment History
- Payment Status

---

### 4.7 App Settings (Company Level)

Configure company-specific loan parameters:
- Set Loan Interest Rate
- Maximum Limit of Loan
- Minimum Loan Amount

---

## 5. Employee Module (Mobile - Staff with Custom Roles)

### 5.1 Employee Login

**Login Method:**
- Phone Number
- PIN (4-digit)

**Authentication:**
- Custom roles and claims-based access control
- Secure session management

---

### 5.2 Forget Password / PIN Recovery

**Recovery Method:**
- Twilio OTP (One-Time Password)
- Verification and reset workflow

---

### 5.3 Employee Dashboard

**Dashboard Displays:**
- Total Pending Loans - Count and details
- Active Loans - List with details
- Quick access to loan services

---

### 5.4 Loan Request - Standard Offers

**Functionality:**
- Display all available Offers from company
- Employee can select from predefined offers
- Apply for loan with offer terms

---

### 5.5 Custom Loan Request

**Create Custom Loan:**
- Custom Name - Employee-defined loan name
- Loan Period - Select 6, 12, or 24 months
- Amount Needed - Enter desired amount
- Upload Document - PDF or Image support
- Purpose - Reason for loan

**Auto-Calculation:**
- Interest Rate = Global Rate (Example: 2.5%)
- Total Payback = Amount + (Amount × Interest Rate)
- Duration = Selected period
- Monthly Payment = Total Payback ÷ Duration

**Example:**
- Amount needed = 10,000 CFA
- Interest Rate = 2.5%
- Duration = 12 months
- Total Payback = 10,000 + (10,000 × 2.5%) = 10,250 CFA
- Monthly Payment = 10,250 ÷ 12 = 854.17 CFA

---

### 5.6 Advance Salary Request

**Create Advance Salary:**
- Custom Name - Auto-filled as "Advance Salary"
- Loan Period - Select 6, 12, or 24 months
- Amount Needed - Select from employee salary
- Purpose - Pre-filled or employee can customize

**Calculation:**
- Same interest rate and calculation as custom loans
- Amount is limited to employee's current salary

---

### 5.7 Pay Off / Loan Repayment

**Pay Off Functionality:**
- Select Month - Choose payment month
- Select Active Loan - Choose which loan to pay
- View payment due
- Process payment
- Receive confirmation

---

## 6. Real-Time Notifications

**SignalR and Firebase Integration:**

When status changes occur:
- Branch deactivation: All branch users logged out immediately
- Staff deactivation: User session terminated immediately
- Loan approval: Real-time notification to employee
- Loan rejection: Real-time notification to employee
- Payroll updates: Notification to relevant staff
- Payment confirmations: Notification to employee

---

## 7. Security & Access Control

**Authentication:**
- Role-based access control (RBAC)
- Claims-based authorization
- PIN protection for employees
- Secure password management

**Data Protection:**
- File upload validation (size, format, virus scanning)
- Secure storage for certificates and documents
- Encrypted communication channels

---

## 8. Calculations & Business Logic

### 8.1 Loan Calculation Formula

```
Total Payback = Principal Amount + (Principal Amount × Interest Rate / 100)
Monthly Payment = Total Payback / Duration in months
Remaining Balance = Total Payback - (Monthly Payment × Months Paid)
```

### 8.2 Salary Advance Constraints

- Maximum advance: Employee's monthly salary
- Interest rates apply same as regular loans
- Automatically deducted from salary

---

## 9. File Upload Requirements

**Supported Formats:**
- Images: PNG, JPG, JPEG
- Documents: PDF

**File Size Limits:**
- To be determined during implementation
- Recommend: Max 5MB per file

**Required Uploads:**
- Business Registration Certificate
- VAT Certificate
- Authorization ID Proof
- Profile Picture
- ID Card
- Payroll Approval Screenshots
- Loan Request Documents

---

## 10. Reporting & Analytics

**Admin Reports:**
- Company application status
- Subscription usage
- User activity logs

**Company Reports:**
- Active loans per branch
- Payroll reports
- Loan approval rates
- Employee loan balances

**Employee Reports:**
- Personal loan history
- Payment schedule
- Remaining balance

---

## 11. API Endpoints Overview

**Authentication:**
- POST /auth/login
- POST /auth/forgot-pin
- POST /auth/verify-otp

**Admin Endpoints:**
- CRUD operations for plans, companies, staff, roles, settings

**Company Endpoints:**
- CRUD operations for branches, employees, offers, loans
- Payroll management and approval

**Employee Endpoints:**
- Loan applications and management
- Dashboard data retrieval
- Payment processing

---

## 12. Technology Stack Recommendations

**Backend:**
- ASP.NET Core
- Entity Framework Core
- SQL Server

**Frontend (Web):**
- React or Angular
- TypeScript

**Mobile (Employee):**
- React Native or Flutter

**Real-Time Communication:**
- SignalR (web notifications)
- Firebase (push notifications)

**Payment Processing:**
- Integration ready for Twilio SMS
- Payment gateway integration (to be specified)

---

## 13. Success Criteria

- All CRUD operations function correctly
- Real-time notifications deliver within 2 seconds
- File uploads validate and store securely
- Calculations are accurate to 2 decimal places
- Mobile app supports iPhone and Android
- System handles 10,000+ concurrent users
- 99.5% uptime SLA

---