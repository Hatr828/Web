namespace ASP_P22.Models
{
    public class RestResponseModel
    {
        public RestResponseStatus Status { get; set; } = new();
        public long CacheLifetime { get; set; } = 0L;
        public string Description { get; set; } = "Self-descriptive message";
        public RestResponseManipulations Manipulations { get; set; } = new();
        public Dictionary<string, object> Meta { get; set; } = [];
        public object? Data { get; set; }
    }

    public class RestResponseStatus
    {
        public int Code { get; set; } = 200;
        public string Phrase { get; set; } = "OK";
        public bool isSuccess { get; set; } = true;
    }

    public class RestResponseManipulations
    {
        public string? Create { get; set; }
        public string? Read { get; set; }
        public string? Update { get; set; }
        public string? Delete { get; set; }
    }
}