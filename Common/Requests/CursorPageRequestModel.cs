namespace Common.Requests
{
    public class CursorRequestModel
    {
        // Reference point from the last item of the previous page
        public DateTimeOffset? CursorTime { get; set; }
        public string? CursorId { get; set; }

        private int _limit = 20;
        public int Limit
        {
            get => _limit;
            set => _limit = value > 100 ? 100 : value; // Cap limit for safety
        }

        public bool IsDescending { get; set; } = true;
    }
}
