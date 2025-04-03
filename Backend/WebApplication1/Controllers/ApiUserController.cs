using ASP_P22.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ApiUserController(DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpGet]
        public RestResponseModel Authenticate()
        {
            RestResponseModel restResponseModel = new()
            {
                CacheLifetime = 86400,
                Description = "User API: Authenticate",
                Meta = new() {
                     { "locale", "uk" },
                     { "dataType", "object" }
                 },
            };

            try
            {
                restResponseModel.Data = _dataAccessor.BasicAuthenticate();
            }
            catch (Exception e)
            {
                restResponseModel.Status.Code = 500;
                restResponseModel.Status.Phrase = "Internal Server Error";
                restResponseModel.Status.IsSuccess = false;
                restResponseModel.Description = e.Message;
            }

            return restResponseModel;
        }
    }
}
