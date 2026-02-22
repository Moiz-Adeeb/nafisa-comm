namespace Application.Shared
{
    public class DataCountModel
    {
        public string Data { get; set; }
        public double Count { get; set; }
    }

    public class DataCountModel<T>
    {
        public T Data { get; set; }
        public decimal Count { get; set; }
    }
}
