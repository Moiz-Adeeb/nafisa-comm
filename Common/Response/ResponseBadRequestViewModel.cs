namespace Common.Response
{
    public class ResponseBadRequestViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }

    public class UnAuthorizedBadRequestViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsExpired { get; set; }
    }

    public class OperationCancelledRequestViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
