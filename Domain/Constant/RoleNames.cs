namespace Domain.Constant;

public class RoleNames
{
    // System Admin Roles
    public const string Administrator = "Administrator";

    // Company Admin Roles
    public const string CompanyAdmin = "CompanyAdmin";

    // Employee Roles
    public const string Employee = "Employee";

    // Role Arrays
    public static string[] AllAdminRoles = { Administrator };
    public static string[] AllCompanyRoles = { CompanyAdmin };
    public static string[] AllEmployeeRoles = { Employee };
    public static string[] AllRoles = { Administrator, CompanyAdmin, Employee };
}
