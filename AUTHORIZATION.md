# Authorization System Documentation

## Overview

This system implements a **hybrid role-based and claims-based authorization** system that allows:

1. **Administrator role** - Has full access to all admin module features
2. **CompanyAdmin role** - Has full access to all company module features
3. **Employee role** - Has access based on assigned claims
4. **Custom roles** - Admin and Company can create custom roles with specific claim permissions

---

## Architecture Components

### 1. ClaimNames (`Domain/Constant/ClaimNames.cs`)

Defines all permission claims organized by module:

- **Admin Module Claims**: Subscription plans, companies, admin staff, roles, settings
- **Company Module Claims**: Branches, company staff, loans, payroll, settings
- **Employee Module Claims**: Dashboard, loan requests, payments, profile
- **System Claims**: Audit logs, reports, notifications

### 2. RoleNames (`Domain/Constant/RoleNames.cs`)

Defines system roles:

```csharp
public const string Administrator = "Administrator";     // Full admin access
public const string CompanyAdmin = "CompanyAdmin";       // Full company access
public const string Employee = "Employee";               // Claim-based access
```

### 3. PermissionRequirement (`WebApi/Authorization/PermissionRequirement.cs`)

Authorization requirement that represents a single permission claim.

### 4. PermissionAuthorizationHandler (`WebApi/Authorization/PermissionAuthorizationHandler.cs`)

The core authorization logic:

```csharp
protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
{
    // 1. Check if user is Administrator
    if (roles.Contains(RoleNames.Administrator))
    {
        // Grant access to all admin claims
        if (ClaimNames.AllAdminClaims.Contains(requirement.Permission))
            context.Succeed(requirement);
    }

    // 2. Check if user is CompanyAdmin
    if (roles.Contains(RoleNames.CompanyAdmin))
    {
        // Grant access to all company claims
        if (ClaimNames.AllCompanyAdminClaims.Contains(requirement.Permission))
            context.Succeed(requirement);
    }

    // 3. Check for specific permission claim
    if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission))
        context.Succeed(requirement);
}
```

### 5. AuthorizationPolicyExtension (`WebApi/Extension/AuthorizationPolicyExtension.cs`)

Registers all claim-based policies automatically:

```csharp
public static void AddPermissionPolicies(this AuthorizationOptions options)
{
    // Register policies for all claims
    foreach (var claim in ClaimNames.AllClaims)
    {
        options.AddPolicy(claim, policy =>
            policy.Requirements.Add(new PermissionRequirement(claim))
        );
    }
}
```

---

## How It Works

### Authorization Flow

1. **User makes a request** to a protected endpoint
2. **[Authorize(Policy = ClaimNames.ViewSubscriptionPlans)]** checks if user has the policy
3. **PermissionAuthorizationHandler** evaluates:
   - Is user Administrator? → Grant all admin permissions
   - Is user CompanyAdmin? → Grant all company permissions
   - Does user have the specific claim? → Grant that permission
4. **Request proceeds** if authorized, returns 403 Forbidden if not

### Example Scenarios

#### Scenario 1: Administrator User

```
User: admin@example.com
Role: Administrator
Claims: None needed

Request: GET /api/v1/subscriptions
Policy Required: Permissions.SubscriptionPlans.View

Authorization Check:
✅ User has Administrator role
✅ ViewSubscriptionPlans is in AllAdminClaims
✅ Access Granted
```

#### Scenario 2: Custom Admin Role

```
User: staff@example.com
Role: StaffManager (custom role created by admin)
Claims:
  - Permissions.SubscriptionPlans.View
  - Permissions.Companies.View

Request: GET /api/v1/subscriptions
Policy Required: Permissions.SubscriptionPlans.View

Authorization Check:
❌ User does NOT have Administrator role
✅ User HAS the specific claim "Permissions.SubscriptionPlans.View"
✅ Access Granted
```

#### Scenario 3: Company Admin User

```
User: company@example.com
Role: CompanyAdmin
Claims: None needed

Request: GET /api/v1/loans
Policy Required: Permissions.LoanRequests.View

Authorization Check:
✅ User has CompanyAdmin role
✅ ViewLoanRequests is in AllCompanyAdminClaims
✅ Access Granted
```

#### Scenario 4: Custom Company Role

```
User: supervisor@company.com
Role: BranchSupervisor (custom role created by company admin)
Claims:
  - Permissions.LoanRequests.View
  - Permissions.LoanRequests.Approve
  - Permissions.Payroll.View

Request: GET /api/v1/loans
Policy Required: Permissions.LoanRequests.View

Authorization Check:
❌ User does NOT have CompanyAdmin role
✅ User HAS the specific claim "Permissions.LoanRequests.View"
✅ Access Granted
```

#### Scenario 5: Employee with Custom Claims

```
User: employee@company.com
Role: Employee
Claims:
  - Permissions.Employee.ViewDashboard
  - Permissions.Employee.ApplyForStandardLoan

Request: POST /api/v1/employee/loan-application
Policy Required: Permissions.Employee.ApplyForStandardLoan

Authorization Check:
❌ User does NOT have Administrator or CompanyAdmin role
✅ User HAS the specific claim "Permissions.Employee.ApplyForStandardLoan"
✅ Access Granted
```

---

## Usage in Controllers

### Simple Permission Check

```csharp
[Authorize(Policy = ClaimNames.ViewSubscriptionPlans)]
[HttpGet]
public async Task<GetSubscriptionsResponseModel> GetSubscriptions()
{
    // Only accessible if:
    // - User is Administrator, OR
    // - User has "Permissions.SubscriptionPlans.View" claim
}
```

### Multiple Permissions (Separate Endpoints)

```csharp
[Authorize(Policy = ClaimNames.CreateLoanOffers)]
[HttpPost]
public async Task<CreateLoanOfferResponseModel> CreateLoanOffer()
{
    // Only Administrator or users with CreateLoanOffers claim
}

[Authorize(Policy = ClaimNames.ApproveLoanRequests)]
[HttpPost("approve")]
public async Task<ApproveLoanResponseModel> ApproveLoan()
{
    // Only CompanyAdmin or users with ApproveLoanRequests claim
}
```

### Role-Only Check (Optional)

```csharp
[Authorize(Policy = "AdministratorOnly")]
[HttpPost("system-settings")]
public async Task<UpdateSystemSettingsResponseModel> UpdateSystemSettings()
{
    // ONLY Administrator role, no custom claims allowed
}
```

---

## Assigning Claims to Custom Roles

### Admin Creates Custom Role

```csharp
// Example: Create "Operations Manager" role with limited permissions
var role = new Role { Name = "OperationsManager" };
await roleManager.CreateAsync(role);

// Assign specific claims
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ViewCompanies));
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ViewSubscriptionPlans));
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ViewAdminStaff));
```

### Company Admin Creates Custom Role

```csharp
// Example: Create "Loan Officer" role
var role = new Role { Name = "LoanOfficer" };
await roleManager.CreateAsync(role);

// Assign loan-related claims
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ViewLoanRequests));
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ApproveLoanRequests));
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.RejectLoanRequests));
await roleManager.AddClaimAsync(role, new Claim("Permission", ClaimNames.ViewLoanHistory));
```

---

## Available Policies

### Admin Module Policies

| Policy | Description | Full Access |
|--------|-------------|------------|
| `Permissions.SubscriptionPlans.View` | View subscription plans | Administrator |
| `Permissions.SubscriptionPlans.Create` | Create subscription plans | Administrator |
| `Permissions.SubscriptionPlans.Update` | Update subscription plans | Administrator |
| `Permissions.SubscriptionPlans.Delete` | Delete subscription plans | Administrator |
| `Permissions.Companies.View` | View companies | Administrator |
| `Permissions.Companies.Approve` | Approve company registrations | Administrator |
| `Permissions.AdminStaff.View` | View admin staff | Administrator |
| `Permissions.AdminRoles.Create` | Create admin roles | Administrator |
| `Permissions.GlobalSettings.Update` | Update global settings | Administrator |

### Company Module Policies

| Policy | Description | Full Access |
|--------|-------------|------------|
| `Permissions.Branches.View` | View branches | CompanyAdmin |
| `Permissions.Branches.Create` | Create branches | CompanyAdmin |
| `Permissions.CompanyStaff.View` | View company staff | CompanyAdmin |
| `Permissions.CompanyStaff.Create` | Create company staff | CompanyAdmin |
| `Permissions.LoanOffers.View` | View loan offers | CompanyAdmin |
| `Permissions.LoanRequests.View` | View loan requests | CompanyAdmin |
| `Permissions.LoanRequests.Approve` | Approve loan requests | CompanyAdmin |
| `Permissions.Payroll.Generate` | Generate payroll | CompanyAdmin |
| `Permissions.CompanySettings.Update` | Update company settings | CompanyAdmin |

### Employee Module Policies

| Policy | Description |
|--------|-------------|
| `Permissions.Employee.ViewDashboard` | View employee dashboard |
| `Permissions.Employee.ApplyForStandardLoan` | Apply for standard loan |
| `Permissions.Employee.CreateCustomLoanRequest` | Create custom loan request |
| `Permissions.Employee.RequestAdvanceSalary` | Request salary advance |
| `Permissions.Employee.MakeLoanPayment` | Make loan payments |

---

## Best Practices

### 1. Always Use Claim-Based Policies for Controllers

✅ **Good:**
```csharp
[Authorize(Policy = ClaimNames.ViewSubscriptionPlans)]
```

❌ **Avoid:**
```csharp
[Authorize(Roles = "Administrator")]
```

### 2. Document Required Permissions in Controller Comments

```csharp
/// <summary>
/// Create Subscription Plan
/// Requires: Administrator role OR CreateSubscriptionPlans permission
/// </summary>
[Authorize(Policy = ClaimNames.CreateSubscriptionPlans)]
[HttpPost]
public async Task<CreateSubscriptionResponseModel> CreateSubscription()
```

### 3. Use Appropriate Granularity

- Use specific permissions for CRUD operations
- Group related permissions when it makes sense
- Don't create too many granular permissions that become unmanageable

### 4. Test with Different Role Scenarios

Test your endpoints with:
- Administrator role (full access)
- CompanyAdmin role (full company access)
- Custom role with specific claims
- User without permission (should return 403)

---

## Testing Authorization

### Using Swagger/Postman

1. **Login as Administrator**
   ```
   POST /connect/token
   grant_type=password
   username=admin@example.com
   password=Admin@123
   ```

2. **Copy access token**

3. **Call protected endpoint**
   ```
   GET /api/v1/subscriptions
   Authorization: Bearer {access_token}
   ```

4. **Expected Results:**
   - Administrator: ✅ Success
   - User without permission: ❌ 403 Forbidden

---

## Troubleshooting

### User has Administrator role but still gets 403

**Check:**
1. Is the claim in `AllAdminClaims` array?
2. Is the authorization handler registered in Startup.cs?
3. Is the policy correctly registered?

### Custom role user can't access endpoint

**Check:**
1. Does the user's role have the specific claim assigned?
2. Is the claim value exactly matching (case-sensitive)?
3. Is the claim type "Permission"?

### Policy not found error

**Check:**
1. Is `options.AddPermissionPolicies()` called in Startup.cs?
2. Is the claim name spelled correctly in the controller?
3. Is the application restarted after adding new claims?

---

## Summary

This authorization system provides:

✅ **Flexibility** - Create custom roles with specific permissions
✅ **Scalability** - Easy to add new permissions via ClaimNames
✅ **Maintainability** - Centralized permission definitions
✅ **Security** - Fine-grained access control
✅ **Simplicity** - Administrators/CompanyAdmins get automatic full access
✅ **Extensibility** - Easy to extend with new modules and claims