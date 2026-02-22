using Newtonsoft.Json;

namespace Persistence.Model
{
    public partial class ReservationSetting
    {
        [JsonProperty("general")]
        public General General { get; set; }
    }

    public partial class General
    {
        [JsonProperty("minimumTimeRequirementPriorToBooking")]
        public int? MinimumTimeRequirementPriorToBooking { get; set; }
    }

    public partial class CategoryResponseModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("categories")]
        public Category[] Categories { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("serviceList")]
        public ServiceList[] ServiceList { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("translations")]
        public object Translations { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("pictureFullPath")]
        public string PictureFullPath { get; set; }

        [JsonProperty("pictureThumbPath")]
        public string PictureThumbPath { get; set; }
    }

    public partial class ServiceList
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("deposit")]
        public int Deposit { get; set; }

        [JsonProperty("depositPayment")]
        public string DepositPayment { get; set; }

        [JsonProperty("depositPerPerson")]
        public bool DepositPerPerson { get; set; }

        [JsonProperty("pictureFullPath")]
        public string PictureFullPath { get; set; }

        [JsonProperty("pictureThumbPath")]
        public string PictureThumbPath { get; set; }

        [JsonProperty("extras")]
        public Extra[] Extras { get; set; }

        [JsonProperty("coupons")]
        public object[] Coupons { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("settings")]
        public string Settings { get; set; }

        [JsonProperty("fullPayment")]
        public bool FullPayment { get; set; }

        [JsonProperty("minCapacity")]
        public int MinCapacity { get; set; }

        [JsonProperty("maxCapacity")]
        public int MaxCapacity { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("timeBefore")]
        public int? TimeBefore { get; set; }

        [JsonProperty("timeAfter")]
        public int? TimeAfter { get; set; }

        [JsonProperty("bringingAnyone")]
        public bool BringingAnyone { get; set; }

        [JsonProperty("show")]
        public bool Show { get; set; }

        [JsonProperty("aggregatedPrice")]
        public bool AggregatedPrice { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("categoryId")]
        public int CategoryId { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("priority")]
        public object[] Priority { get; set; }

        [JsonProperty("gallery")]
        public Gallery[] Gallery { get; set; }

        [JsonProperty("recurringCycle")]
        public string RecurringCycle { get; set; }

        [JsonProperty("recurringSub")]
        public string RecurringSub { get; set; }

        [JsonProperty("recurringPayment")]
        public int RecurringPayment { get; set; }

        [JsonProperty("translations")]
        public object Translations { get; set; }

        [JsonProperty("minSelectedExtras")]
        public object MinSelectedExtras { get; set; }

        [JsonProperty("mandatoryExtra")]
        public bool MandatoryExtra { get; set; }

        [JsonProperty("customPricing")]
        public string CustomPricing { get; set; }

        [JsonProperty("maxExtraPeople")]
        public int? MaxExtraPeople { get; set; }

        [JsonProperty("limitPerCustomer")]
        public string LimitPerCustomer { get; set; }
    }

    public partial class Extra
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("maxQuantity")]
        public int MaxQuantity { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("serviceId")]
        public object ServiceId { get; set; }

        [JsonProperty("aggregatedPrice")]
        public bool AggregatedPrice { get; set; }

        [JsonProperty("translations")]
        public object Translations { get; set; }
    }

    public partial class Gallery
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("entityId")]
        public int EntityId { get; set; }

        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        [JsonProperty("pictureFullPath")]
        public string PictureFullPath { get; set; }

        [JsonProperty("pictureThumbPath")]
        public string PictureThumbPath { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }
    }
}
