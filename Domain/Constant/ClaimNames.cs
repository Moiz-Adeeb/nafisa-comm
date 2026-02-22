namespace Domain.Constant;

public class ClaimNames
{
    #region Admin Module Claims

    // Subscription Plans Management
    public const string ViewSubscriptionPlans = "Permissions.SubscriptionPlans.View";
    public const string CreateSubscriptionPlans = "Permissions.SubscriptionPlans.Create";
    public const string UpdateSubscriptionPlans = "Permissions.SubscriptionPlans.Update";
    public const string DeleteSubscriptionPlans = "Permissions.SubscriptionPlans.Delete";
    public static string AssignSubscription { get; set; } =
        "Permissions.AdminRoles.AssignSubscription";
    public static string UpdateCompanySubscription { get; set; } =
        "Permission.AdminRoles.UpdateCompanySubscription";

    // Company Management
    public const string ViewCompanies = "Permissions.Companies.View";
    public const string CreateCompanies = "Permissions.Companies.Create";
    public const string UpdateCompanies = "Permissions.Companies.Update";
    public const string DeleteCompanies = "Permissions.Companies.Delete";
    public const string ApproveCompanies = "Permissions.Companies.Approve";
    public const string RejectCompanies = "Permissions.Companies.Reject";
    public const string AssignSubscriptionPlan = "Permissions.Companies.AssignSubscriptionPlan";
    public const string ViewCompanyDocuments = "Permissions.Companies.ViewDocuments";

    // Admin Staff Management
    public const string ViewAdminStaff = "Permissions.AdminStaff.View";
    public const string CreateAdminStaff = "Permissions.AdminStaff.Create";
    public const string UpdateAdminStaff = "Permissions.AdminStaff.Update";
    public const string DeleteAdminStaff = "Permissions.AdminStaff.Delete";
    public const string ActivateAdminStaff = "Permissions.AdminStaff.Activate";
    public const string DeactivateAdminStaff = "Permissions.AdminStaff.Deactivate";

    // Admin Roles & Permissions
    public const string ViewAdminRoles = "Permissions.AdminRoles.View";
    public const string CreateAdminRoles = "Permissions.AdminRoles.Create";
    public const string UpdateAdminRoles = "Permissions.AdminRoles.Update";
    public const string DeleteAdminRoles = "Permissions.AdminRoles.Delete";
    public const string AssignAdminRolePermissions = "Permissions.AdminRoles.AssignPermissions";

    // Global App Settings
    public const string ViewGlobalSettings = "Permissions.GlobalSettings.View";
    public const string UpdateGlobalSettings = "Permissions.GlobalSettings.Update";

    #endregion

    #region Company Module Claims

    // Company Roles & Permissions
    public const string ViewCompanyRoles = "Permissions.CompanyRoles.View";
    public const string CreateCompanyRoles = "Permissions.CompanyRoles.Create";
    public const string UpdateCompanyRoles = "Permissions.CompanyRoles.Update";
    public const string DeleteCompanyRoles = "Permissions.CompanyRoles.Delete";
    public const string AssignCompanyRolePermissions = "Permissions.CompanyRoles.AssignPermissions";

    // Branch Management
    public const string ViewBranches = "Permissions.Branches.View";
    public const string CreateBranches = "Permissions.Branches.Create";
    public const string UpdateBranches = "Permissions.Branches.Update";
    public const string DeleteBranches = "Permissions.Branches.Delete";
    public const string ActivateBranches = "Permissions.Branches.Activate";
    public const string DeactivateBranches = "Permissions.Branches.Deactivate";

    // Company Staff Management
    public const string ViewCompanyStaff = "Permissions.CompanyStaff.View";
    public const string CreateCompanyStaff = "Permissions.CompanyStaff.Create";
    public const string UpdateCompanyStaff = "Permissions.CompanyStaff.Update";
    public const string DeleteCompanyStaff = "Permissions.CompanyStaff.Delete";
    public const string ActivateCompanyStaff = "Permissions.CompanyStaff.Activate";
    public const string DeactivateCompanyStaff = "Permissions.CompanyStaff.Deactivate";
    public const string ViewCompanyStaffDocuments = "Permissions.CompanyStaff.ViewDocuments";

    // Loan Offers Management
    public const string ViewLoanOffers = "Permissions.LoanOffers.View";
    public const string CreateLoanOffers = "Permissions.LoanOffers.Create";
    public const string UpdateLoanOffers = "Permissions.LoanOffers.Update";
    public const string DeleteLoanOffers = "Permissions.LoanOffers.Delete";
    public const string ActivateLoanOffers = "Permissions.LoanOffers.Activate";
    public const string DeactivateLoanOffers = "Permissions.LoanOffers.Deactivate";

    // Payroll Generation
    public const string ViewPayroll = "Permissions.Payroll.View";
    public const string CreatePayroll = "Permissions.Payroll.Create";
    public const string DeletePayroll = "Permissions.Payroll.Delete";
    public const string GeneratePayroll = "Permissions.Payroll.Generate";
    public const string ApprovePayroll = "Permissions.Payroll.Approve";
    public const string RejectPayroll = "Permissions.Payroll.Reject";
    public const string UploadPayrollProof = "Permissions.Payroll.UploadProof";

    // Loan Requests Management
    public const string ViewLoanRequests = "Permissions.LoanRequests.View";
    public const string CreateLoanRequests = "Permissions.LoanRequests.Create";
    public const string UpdateLoanRequests = "Permissions.LoanRequests.Update";
    public const string DeleteLoanRequests = "Permissions.LoanRequests.Delete";
    public const string ApproveLoanRequests = "Permissions.LoanRequests.Approve";
    public const string RejectLoanRequests = "Permissions.LoanRequests.Reject";
    public const string ViewLoanRequestDocuments = "Permissions.LoanRequests.ViewDocuments";

    // Loan Dashboard & Reports
    public const string ViewLoanHistory = "Permissions.Loans.ViewHistory";
    public const string ViewLoanBalance = "Permissions.Loans.ViewBalance";
    public const string ViewPaymentHistory = "Permissions.Loans.ViewPaymentHistory";
    public const string ViewPaymentStatus = "Permissions.Loans.ViewPaymentStatus";
    public const string ViewActiveLoansByBranch = "Permissions.Loans.ViewActiveLoansByBranch";

    // Company App Settings
    public const string ViewCompanySettings = "Permissions.CompanySettings.View";
    public const string UpdateCompanySettings = "Permissions.CompanySettings.Update";

    public const string ViewCompanyReports = "Permissions.Employee.ViewCompanyReports";
    public const string ExportCompanyReports = "Permissions.Employee.ExportCompanyReports";

    #endregion

    #region Employee Module Claims

    // Employee Dashboard
    public const string ViewEmployeeDashboard = "Permissions.Employee.ViewDashboard";
    public const string ViewOwnPendingLoans = "Permissions.Employee.ViewOwnPendingLoans";
    public const string ViewOwnActiveLoans = "Permissions.Employee.ViewOwnActiveLoans";
    public const string ViewOwnLoanHistory = "Permissions.Employee.ViewOwnLoanHistory";

    // Loan Request - Standard Offers
    public const string ViewAvailableOffers = "Permissions.Employee.ViewAvailableOffers";
    public const string ApplyForStandardLoan = "Permissions.Employee.ApplyForStandardLoan";

    // Custom Loan Request
    public const string CreateCustomLoanRequest = "Permissions.Employee.CreateCustomLoanRequest";
    public const string UploadLoanDocuments = "Permissions.Employee.UploadLoanDocuments";

    // Advance Salary Request
    public const string RequestAdvanceSalary = "Permissions.Employee.RequestAdvanceSalary";

    // Pay Off / Loan Repayment
    public const string ViewPaymentSchedule = "Permissions.Employee.ViewPaymentSchedule";
    public const string MakeLoanPayment = "Permissions.Employee.MakeLoanPayment";
    public const string ViewRemainingBalance = "Permissions.Employee.ViewRemainingBalance";

    // Employee Profile
    public const string ViewOwnProfile = "Permissions.Employee.ViewOwnProfile";
    public const string UpdateOwnProfile = "Permissions.Employee.UpdateOwnProfile";

    #endregion

    #region System Claims

    // Audit & Logging
    public const string ViewAuditLogs = "Permissions.System.ViewAuditLogs";
    public const string ExportReports = "Permissions.System.ExportReports";

    // Notifications
    public const string SendNotifications = "Permissions.System.SendNotifications";
    public const string ViewNotifications = "Permissions.System.ViewNotifications";

    #endregion

    #region Claim Arrays for Easy Assignment

    // All Admin Claims
    public static readonly string[] AllAdminClaims =
    {
        // Subscription Plans
        ViewSubscriptionPlans,
        CreateSubscriptionPlans,
        UpdateSubscriptionPlans,
        DeleteSubscriptionPlans,
        AssignSubscriptionPlan,
        UpdateCompanySubscription,
        // Companies
        ViewCompanies,
        CreateCompanies,
        UpdateCompanies,
        DeleteCompanies,
        ApproveCompanies,
        RejectCompanies,
        AssignSubscriptionPlan,
        ViewCompanyDocuments,
        // Admin Staff
        ViewAdminStaff,
        CreateAdminStaff,
        UpdateAdminStaff,
        DeleteAdminStaff,
        ActivateAdminStaff,
        DeactivateAdminStaff,
        // Admin Roles
        ViewAdminRoles,
        CreateAdminRoles,
        UpdateAdminRoles,
        DeleteAdminRoles,
        AssignAdminRolePermissions,
        // Global Settings
        ViewGlobalSettings,
        UpdateGlobalSettings,
        // System
        ViewAuditLogs,
        ExportReports,
        SendNotifications,
        ViewNotifications,
    };

    // All Company Admin Claims
    public static readonly string[] AllCompanyAdminClaims =
    {
        // Company Roles
        ViewCompanyRoles,
        CreateCompanyRoles,
        UpdateCompanyRoles,
        DeleteCompanyRoles,
        AssignCompanyRolePermissions,
        // Branches
        ViewBranches,
        CreateBranches,
        UpdateBranches,
        DeleteBranches,
        ActivateBranches,
        DeactivateBranches,
        // Company Staff
        ViewCompanyStaff,
        CreateCompanyStaff,
        UpdateCompanyStaff,
        DeleteCompanyStaff,
        ActivateCompanyStaff,
        DeactivateCompanyStaff,
        ViewCompanyStaffDocuments,
        // Loan Offers
        ViewLoanOffers,
        CreateLoanOffers,
        UpdateLoanOffers,
        DeleteLoanOffers,
        ActivateLoanOffers,
        DeactivateLoanOffers,
        // Payroll
        ViewPayroll,
        CreatePayroll,
        DeletePayroll,
        GeneratePayroll,
        ApprovePayroll,
        RejectPayroll,
        UploadPayrollProof,
        // Loan Requests
        ViewLoanRequests,
        CreateLoanRequests,
        UpdateLoanRequests,
        DeleteLoanRequests,
        ApproveLoanRequests,
        RejectLoanRequests,
        ViewLoanRequestDocuments,
        // Loan Dashboard
        ViewLoanHistory,
        ViewLoanBalance,
        ViewPaymentHistory,
        ViewPaymentStatus,
        ViewActiveLoansByBranch,
        // Company Settings
        ViewCompanySettings,
        UpdateCompanySettings,
        ViewCompanyReports,
        ExportCompanyReports,
        // Notifications
        ViewNotifications,
        SendNotifications,
    };

    // All Employee Claims
    public static readonly string[] AllEmployeeClaims =
    {
        // Dashboard
        ViewEmployeeDashboard,
        ViewOwnPendingLoans,
        ViewOwnActiveLoans,
        ViewOwnLoanHistory,
        // Loan Applications
        ViewAvailableOffers,
        ApplyForStandardLoan,
        CreateCustomLoanRequest,
        UploadLoanDocuments,
        // Advance Salary
        RequestAdvanceSalary,
        // Payments
        ViewPaymentSchedule,
        MakeLoanPayment,
        ViewRemainingBalance,
        // Profile
        ViewOwnProfile,
        UpdateOwnProfile,
        // Notifications
        ViewNotifications,
    };

    // All Claims Combined
    public static readonly string[] AllClaims =
    {
        // Admin Module
        ViewSubscriptionPlans,
        CreateSubscriptionPlans,
        UpdateSubscriptionPlans,
        DeleteSubscriptionPlans,
        ViewCompanies,
        CreateCompanies,
        UpdateCompanies,
        DeleteCompanies,
        ApproveCompanies,
        RejectCompanies,
        AssignSubscriptionPlan,
        ViewCompanyDocuments,
        ViewAdminStaff,
        CreateAdminStaff,
        UpdateAdminStaff,
        DeleteAdminStaff,
        ActivateAdminStaff,
        DeactivateAdminStaff,
        ViewAdminRoles,
        CreateAdminRoles,
        UpdateAdminRoles,
        DeleteAdminRoles,
        AssignAdminRolePermissions,
        ViewGlobalSettings,
        UpdateGlobalSettings,
        AssignSubscription,
        // Company Module
        ViewCompanyRoles,
        CreateCompanyRoles,
        UpdateCompanyRoles,
        DeleteCompanyRoles,
        AssignCompanyRolePermissions,
        ViewBranches,
        CreateBranches,
        UpdateBranches,
        DeleteBranches,
        ActivateBranches,
        DeactivateBranches,
        ViewCompanyStaff,
        CreateCompanyStaff,
        UpdateCompanyStaff,
        DeleteCompanyStaff,
        ActivateCompanyStaff,
        DeactivateCompanyStaff,
        ViewCompanyStaffDocuments,
        ViewLoanOffers,
        CreateLoanOffers,
        UpdateLoanOffers,
        DeleteLoanOffers,
        ActivateLoanOffers,
        DeactivateLoanOffers,
        ViewPayroll,
        CreatePayroll,
        DeletePayroll,
        GeneratePayroll,
        ApprovePayroll,
        RejectPayroll,
        UploadPayrollProof,
        ViewLoanRequests,
        CreateLoanRequests,
        UpdateLoanRequests,
        DeleteLoanRequests,
        ApproveLoanRequests,
        RejectLoanRequests,
        ViewLoanRequestDocuments,
        ViewLoanHistory,
        ViewLoanBalance,
        ViewPaymentHistory,
        ViewPaymentStatus,
        ViewActiveLoansByBranch,
        ViewCompanySettings,
        UpdateCompanySettings,
        // Employee Module
        ViewEmployeeDashboard,
        ViewOwnPendingLoans,
        ViewOwnActiveLoans,
        ViewOwnLoanHistory,
        ViewAvailableOffers,
        ApplyForStandardLoan,
        CreateCustomLoanRequest,
        UploadLoanDocuments,
        RequestAdvanceSalary,
        ViewPaymentSchedule,
        MakeLoanPayment,
        ViewRemainingBalance,
        ViewOwnProfile,
        UpdateOwnProfile,
        ViewCompanyReports,
        ExportCompanyReports,
        // System
        ViewAuditLogs,
        ExportReports,
        SendNotifications,
        ViewNotifications,
    };

    #endregion
}
