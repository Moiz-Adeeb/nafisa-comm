namespace Common.Constants
{
    public class CustomClaimTypes
    {
        ///<summary>A claim that specifies the permission of an entity</summary>
        public const string Permission = "permission";
        public const string Hospital = "hospital";

        ///<summary>A claim that specifies the full name of an entity</summary>
        public const string FullName = "fullName";

        ///<summary>A claim that specifies the job title of an entity</summary>
        public const string JobTitle = "jobtitle";

        ///<summary>A claim that specifies the email of an entity</summary>
        public const string Email = "email";

        ///<summary>A claim that specifies the phone number of an entity</summary>
        public const string Phone = "phoneNumber";

        ///<summary>A claim that specifies the userId of an entity</summary>
        public const string UserId = "userId";
        public const string ChatId = "chatId";

        ///<summary>A claim that specifies the UserName of an entity</summary>
        public const string UserName = "userName";

        public const string CompanyId = "companyId";

        ///<summary>A claim that specifies the configuration/customizations of an entity</summary>
        public const string Configuration = "configuration";

        ///<summary>A claim that specifies the verification status of an entity</summary>
        public const string IsVerified = "isVerified";

        ///<summary>A claim that specifies the merchant of an entity</summary>
        public const string MerchantId = "merchantId";
        public const string Balance = "balance";

        ///<summary>A claim that specifies the image of an entity</summary>
        public const string Image = "image";
        public const string ShopId = "shopId";
        public const string EmployeeId = "employeeId";
        public const string Address = "address";
        public const string TeamLeaderId = "teamLeaderId";
        public const string DepartmentId = "departmentId";
        public const string GroupIds = "groupIds";
        public const string TeamId = "teamId";
        public const string MarketPlace = "marketPlace";
    }

    public class StripeConstant
    {
        public const string Succeeded = "succeeded";
        public const double StripeCharge = 0.97;
        public const double StripeUpCharge = 1.03;
        public const double StripeTax = 0.03;
        public const string Currency = "sgd";
    }
}
