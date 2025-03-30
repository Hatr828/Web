using ASP_P22.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Services.Storage;

namespace WebApplication1.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ApiProductController(DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IStorageService _storageService = storageService;

        [HttpGet("{id}")]
        public RestResponseModel ProductById(string id)
        {
            return new()
            {
                CacheLifetime = 86400,
                Description = "Product API: Product By Id",
                Meta = new() {
                     { "locale", "uk" },
                     { "dataType", "object" }
                 },
                Data = _dataAccessor.ProductById(id)
            };
        }
    }
}
