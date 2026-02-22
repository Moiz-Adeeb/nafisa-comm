namespace Common.Response
{
    public class ResponseViewModel
    {
        public Dictionary<string, object> Response { get; set; }
        public int Status { get; set; }

        public ResponseViewModel()
        {
            Status = 200;
        }

        public ResponseViewModel(object data)
            : this()
        {
            Response = new Dictionary<string, object>() { { "success", true }, { "data", data } };
        }

        public ResponseViewModel(object data, int count)
            : this()
        {
            Response = new Dictionary<string, object>()
            {
                { "success", true },
                { "data", data },
                { "count", count },
            };
        }

        public ResponseViewModel CreateOk(object data)
        {
            Status = 200;
            Response = new Dictionary<string, object>() { { "success", true }, { "data", data } };
            return this;
        }

        public ResponseViewModel CreateOk(object data, int count)
        {
            Status = 200;
            Response = new Dictionary<string, object>()
            {
                { "success", true },
                { "data", data },
                { "count", count },
            };
            return this;
        }

        public ResponseViewModel CreateOk(object data, long count)
        {
            Status = 200;
            Response = new Dictionary<string, object>()
            {
                { "success", true },
                { "data", data },
                { "count", count },
            };
            return this;
        }

        public ResponseViewModel CreateOkCustom(List<KeyValuePair<string, object>> values)
        {
            Status = 200;
            Response = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> data in values)
            {
                Response.Add(data.Key, data.Value);
            }
            return this;
        }

        public ResponseViewModel CreateCustom(int status, params object[] values)
        {
            Status = status;
            Response = new Dictionary<string, object>();
            foreach (var data in values)
            {
                Response.Add(nameof(data), data);
            }
            return this;
        }
    }
}
