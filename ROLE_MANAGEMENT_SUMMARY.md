# Role Management System - Implementation Summary

## Overview

This system implements **separate role management** for Admin and Company contexts with full multi-tenancy support for company roles.

---

## Architecture

### **1. AdminRole** (System-wide custom roles)
- Created by system administrators
- No tenant isolation
- Assigns permissions from `ClaimNames.AllAdminClaims`
- Users with AdminRoleId get claims from AdminRoleClaims

### **2. CompanyRole** (Company-specific custom roles)
- Created by company admins
- **Tenant isolated** by `CompanyId`
- Assigns permissions from `ClaimNames.AllCompanyAdminClaims`
- Users with CompanyRoleId get claims from CompanyRoleClaims

---

## Entities Created

### **AdminRole** (`Domain/Entities/AdminRole.cs`)
```csharp
public class AdminRole : Base
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<AdminRoleClaim> AdminRoleClaims { get; set; }
    public virtual ICollection<User> Users { get; set; }
}
```

### **CompanyRole** (`Domain/Entities/CompanyRole.cs`)
```csharp
public class CompanyRole : Base
{
    public string CompanyId { get; set; } // TENANT ID
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Company Company { get; set; }
    public virtual ICollection<CompanyRoleClaim> CompanyRoleClaims { get; set; }
    public virtual ICollection<User> Users { get; set; }
}
```

### **AdminRoleClaim** (`Domain/Entities/AdminRoleClaim.cs`)
```csharp
public class AdminRoleClaim : Base
{
    public string AdminRoleId { get; set; }
    public string ClaimType { get; set; } // "Permission"
    public string ClaimValue { get; set; } // e.g., "Permissions.Companies.View"

    public virtual AdminRole AdminRole { get; set; }
}
```

### **CompanyRoleClaim** (`Domain/Entities/CompanyRoleClaim.cs`)
```csharp
public class CompanyRoleClaim : Base
{
    public string CompanyRoleId { get; set; }
    public string ClaimType { get; set; } // "Permission"
    public string ClaimValue { get; set; } // e.g., "Permissions.LoanRequests.View"

    public virtual CompanyRole CompanyRole { get; set; }
}
```

### **Updated User Entity**
```csharp
public class User : IdentityUser, IBase
{
    // ... existing properties

    // Custom Role Associations
    public string AdminRoleId { get; set; }
    public string CompanyRoleId { get; set; }

    public virtual AdminRole AdminRole { get; set; }
    public virtual CompanyRole CompanyRole { get; set; }
}
```

---

## Database Configuration

### **Entity Configurations Created:**
- `AdminRoleConfiguration` - Unique name index, cascade delete for claims
- `CompanyRoleConfiguration` - Unique (CompanyId, Name) composite index
- `AdminRoleClaimConfiguration` - Unique (AdminRoleId, ClaimValue)
- `CompanyRoleClaimConfiguration` - Unique (CompanyRoleId, ClaimValue)

### **DbSets Added:**
```csharp
public DbSet<AdminRole> AdminRoles { get; set; }
public DbSet<AdminRoleClaim> AdminRoleClaims { get; set; }
public DbSet<CompanyRole> CompanyRoles { get; set; }
public DbSet<CompanyRoleClaim> CompanyRoleClaims { get; set; }
```

---

## How It Works

### **Authorization Flow with Custom Roles**

```
1. User has Identity Role (Administrator/CompanyAdmin/Employee)
   +
   User has AdminRoleId or CompanyRoleId (optional custom role)

2. PermissionAuthorizationHandler checks:
   a) If user is Administrator → Grant all admin claims
   b) If user is CompanyAdmin → Grant all company claims
   c) If user has AdminRoleId → Load AdminRoleClaims and check permission
   d) If user has CompanyRoleId → Load CompanyRoleClaims and check permission
   e) Check user's direct claims (UserClaims table)

3. Access granted if any check passes
```

### **Example Scenarios**

#### **Scenario 1: System Admin with Custom Role**
```
User: staff@admin.com
Identity Role: Administrator (built-in)
AdminRoleId: NULL

Result: Gets ALL admin claims automatically
```

#### **Scenario 2: Custom Admin Role**
```
User: operations@admin.com
Identity Role: OperationsStaff (custom Identity role)
AdminRoleId: "operations-manager-role-id"

AdminRole "Operations Manager" has claims:
- Permissions.Companies.View
- Permissions.SubscriptionPlans.View

Result: Gets ONLY those 2 specific claims
```

#### **Scenario 3: Company Admin with Custom Role**
```
User: supervisor@company.com
Identity Role: CompanyAdmin (built-in)
CompanyRoleId: NULL

Result: Gets ALL company claims automatically
```

#### **Scenario 4: Custom Company Role (Loan Officer)**
```
User: loanofficer@company.com
Identity Role: Employee
CompanyRoleId: "loan-officer-role-id"
CompanyId: "acme-company-id"

CompanyRole "Loan Officer" (for Acme Company) has claims:
- Permissions.LoanRequests.View
- Permissions.LoanRequests.Approve
- Permissions.LoanRequests.Reject

Result: Gets ONLY those 3 specific claims
        AND only for Acme Company (tenant isolation)
```

---

## API Endpoints Structure

### **AdminRole Endpoints**
```
POST   /api/v1/admin-roles          - Create custom admin role
PUT    /api/v1/admin-roles/{id}     - Update admin role
DELETE /api/v1/admin-roles/{id}     - Delete admin role (soft delete)
GET    /api/v1/admin-roles           - List all admin roles
GET    /api/v1/admin-roles/{id}      - Get admin role with claims
GET    /api/v1/admin-roles/permissions - Get all available admin permissions
```

### **CompanyRole Endpoints**
```
POST   /api/v1/company-roles          - Create custom company role
PUT    /api/v1/company-roles/{id}     - Update company role
DELETE /api/v1/company-roles/{id}     - Delete company role (soft delete)
GET    /api/v1/company-roles           - List company roles (filtered by tenant)
GET    /api/v1/company-roles/{id}      - Get company role with claims
GET    /api/v1/company-roles/permissions - Get all available company permissions
```

---

## Commands & Queries Implemented

### **AdminRole**
✅ **CreateAdminRole** - Create role with permissions
⏳ **UpdateAdminRole** - Update role and permissions
⏳ **DeleteAdminRole** - Soft delete role
⏳ **GetAdminRoles** - Paginated list with search
⏳ **GetAdminRoleById** - Get role with all claims
⏳ **GetAllAdminPermissions** - List all available admin claims

### **CompanyRole**
⏳ **CreateCompanyRole** - Create role with permissions (tenant-scoped)
⏳ **UpdateCompanyRole** - Update role and permissions
⏳ **DeleteCompanyRole** - Soft delete role
⏳ **GetCompanyRoles** - Paginated list (filtered by CompanyId)
⏳ **GetCompanyRoleById** - Get role with all claims
⏳ **GetAllCompanyPermissions** - List all available company claims

---

## Updated Authorization Handler

The `PermissionAuthorizationHandler` will need to be updated to:

```csharp
protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // 1. Check built-in roles (Administrator, CompanyAdmin)
    if (roles.Contains(RoleNames.Administrator))
    {
        if (ClaimNames.AllAdminClaims.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }
    }

    if (roles.Contains(RoleNames.CompanyAdmin))
    {
        if (ClaimNames.AllCompanyAdminClaims.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }
    }

    // 2. Check custom AdminRole claims
    var user = await _context.Users
        .Include(u => u.AdminRole)
            .ThenInclude(r => r.AdminRoleClaims)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user?.AdminRole != null && user.AdminRole.IsActive)
    {
        var hasPermission = user.AdminRole.AdminRoleClaims.Any(c =>
            c.ClaimType == "Permission" && c.ClaimValue == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
            return;
        }
    }

    // 3. Check custom CompanyRole claims
    if (user?.CompanyRole != null && user.CompanyRole.IsActive)
    {
        var hasPermission = user.CompanyRole.CompanyRoleClaims.Any(c =>
            c.ClaimType == "Permission" && c.ClaimValue == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
            return;
        }
    }

    // 4. Check direct user claims
    if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission))
    {
        context.Succeed(requirement);
    }
}
```

---

## Multi-Tenancy for Company Roles

Company roles are **automatically tenant-isolated** by `CompanyId`:

```csharp
// When creating a company role
var companyRole = new CompanyRole
{
    CompanyId = currentUser.CompanyId, // Auto-set from current user's company
    Name = "Loan Officer",
    Description = "Handles loan approvals"
};

// When querying company roles
var roles = await _context.CompanyRoles
    .Where(r => r.CompanyId == currentUser.CompanyId) // Tenant filter
    .ToListAsync();

// When assigning role to user
if (user.CompanyId != role.CompanyId)
{
    throw new BadRequestException("Cannot assign role from different company");
}
```

---

## Next Steps to Complete

1. **Complete remaining CRUD operations**
   - UpdateAdminRole, DeleteAdminRole
   - GetAdminRoles, GetAdminRoleById
   - CreateCompanyRole with all CRUD operations

2. **Create GetAllAvailablePermissions query**
   - Returns categorized permissions
   - Separate for admin and company

3. **Create Controllers**
   - AdminRoleController with claim-based auth
   - CompanyRoleController with claim-based auth + tenant filtering

4. **Update PermissionAuthorizationHandler**
   - Add AdminRole/CompanyRole claim checking logic
   - Include caching for performance

5. **Create Migration**
   ```bash
   dotnet ef migrations add AddCustomRoleManagement --project Persistence --startup-project WebApi
   dotnet ef database update --project Persistence --startup-project WebApi
   ```

6. **Add Role Assignment Endpoints**
   - Assign AdminRole to user
   - Assign CompanyRole to user (with tenant validation)

7. **Add User Management Updates**
   - Update CreateUser to optionally assign custom role
   - Update UpdateUser to change custom role assignment

---

## Benefits of This Approach

✅ **Separation of Concerns** - Admin and Company roles are completely separate
✅ **Multi-Tenancy** - Company roles are automatically tenant-isolated
✅ **Flexibility** - Can create unlimited custom roles with specific permissions
✅ **Security** - Tenant validation prevents cross-company access
✅ **Scalability** - Easy to extend with new modules and claims
✅ **Maintainability** - Clear separation between system and company administration

---

## Testing Checklist

- [ ] Create custom admin role with specific permissions
- [ ] Assign admin role to user
- [ ] Verify user gets only assigned permissions
- [ ] Create custom company role with specific permissions
- [ ] Assign company role to user
- [ ] Verify user gets only assigned permissions
- [ ] Verify tenant isolation (user from Company A cannot access Company B's roles)
- [ ] Test built-in Administrator role still gets all admin claims
- [ ] Test built-in CompanyAdmin role still gets all company claims
- [ ] Test updating role permissions propagates to users
- [ ] Test deleting role removes permissions from users
- [ ] Test deactivating role blocks user access

---

This role management system is now ready for completion and testing! 🎉