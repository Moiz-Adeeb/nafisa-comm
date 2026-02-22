namespace Common.Response
{
    public class ResponseExceptionViewModel
    {
        public string Exception { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public bool Success { get; set; }
    }
}
