namespace Domain.Constant;

public class Policies
{
    // Role-based policies
    public const string AdministratorOnly = "AdministratorOnly";
    public const string CompanyAdminOnly = "CompanyAdminOnly";
    public const string EmployeeOnly = "EmployeeOnly";

    // Combined policies
    public const string AdminOrCompanyAdmin = "AdminOrCompanyAdmin";
    public const string AnyAuthenticatedUser = "AnyAuthenticatedUser";
}
