namespace Infrastructure.Options;

public class GoogleMapOption
{
    public string ApiKey { get; set; }
}

public class S3Option
{
    public string Region { get; set; }
    public string Bucket { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}

public class SmsOption
{
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
    public string PhoneNumber { get; set; }
}

public class SmtpOption
{
    public string Email { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public string Smtp { get; set; }
}
