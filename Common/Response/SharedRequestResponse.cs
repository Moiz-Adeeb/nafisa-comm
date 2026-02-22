namespace Common.Response
{
    public class SharedRequestResponse
    {
        public double Amount { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public string TransactionId { get; set; }
        public int LoadType { get; set; }
    }
}
