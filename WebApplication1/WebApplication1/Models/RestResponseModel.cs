namespace ASP_P22.Models
{
    public class RestResponseModel
    {
        public RestResponseStatus Status { get; set; } = new();
        public long CacheLifetime { get; set; } = 0L;
        public String Description { get; set; } = "Self descriptive message";
        public RestResponseManipulations Manipulations { get; set; } = new();
        public Dictionary<String, Object> Meta { get; set; } = [];
        public Object? Data { get; set; }
    }

    public class RestResponseStatus
    {
        public int Code { get; set; } = 200;
        public String Phrase { get; set; } = "OK";
        public bool IsSuccess { get; set; } = true;
    }

    public class RestResponseManipulations
    {
        public String? Create { get; set; }
        public String? Read { get; set; }
        public String? Update { get; set; }
        public String? Delete { get; set; }
    }
}