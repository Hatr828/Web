using ASP_P22.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Middleware.Auth;

namespace WebApplication1.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class ApiCartController(DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        RestResponseModel restResponseModel => new()
        {
            CacheLifetime = 0,
            Description = "User's Cart API: Active Cart",
            Manipulations = new()
            {
                Read = "/api/cart",
            },
            Meta = new()
             {
                 { "locale", "uk" },
                 { "dataType", "object" }
             }
        };
        [HttpGet]
        public RestResponseModel DoGet(string? id)
        {
            var res = restResponseModel;

            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                res.Status.Code = 401;
                res.Status.Phrase = "Unauthorized";
                res.Status.IsSuccess = false;
                res.Description = "Authentication failed";
                res.Data = HttpContext.Items[nameof(AuthTokenMiddleware)];
                return res;
            }
            Cart? cart;
            try { cart = _dataAccessor.GetCart(userId, id); }
            catch (AccessViolationException ex)
            {
                res.Status.Code = 403;
                res.Status.Phrase = "Forbidden";
                res.Status.IsSuccess = false;
                res.Data = ex.Message;
                return res;
            }
            catch (Exception ex)
            {
                res.Status.Code = 400;
                res.Status.Phrase = "Bad Request";
                res.Status.IsSuccess = false;
                res.Data = ex.Message;
                return res;
            }
            res.Data = cart;

            return res;
        }

        [HttpPost]
        public RestResponseModel DoPost([FromQuery] string productId)
        {
            var res = restResponseModel;

            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                res.Status.Code = 401;
                res.Status.Phrase = "Unauthorized";
                res.Status.IsSuccess = false;
                res.Description = "Authentication failed";
                res.Data = HttpContext.Items[nameof(AuthTokenMiddleware)];
                return res;
            }
            try
            {
                _dataAccessor.AddToCart(userId, productId);
                res.Data = "Created";
            }
            catch (Exception ex)
            {
                res.Status.Code = 400;
                res.Status.Phrase = "Bad Request";
                res.Status.IsSuccess = false;
                res.Data = ex.Message;
            }

            return res;
        }
        [HttpPatch]
        public RestResponseModel DoPatch([FromRoute] string id, [FromQuery] int delta)
        {
            var res = restResponseModel;
            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                res.Status.Code = 401;
                res.Status.Phrase = "Unauthorized";
                res.Status.IsSuccess = false;
                res.Description = "Authentication failed";
                res.Data = HttpContext.Items[nameof(AuthTokenMiddleware)];
                return res;
            }
            Cart? cart;
            try { cart = _dataAccessor.GetCart(userId, null); }
            catch (AccessViolationException ex)
            {
                res.Status.Code = 403;
                res.Status.Phrase = "Forbidden";
                res.Status.IsSuccess = false;
                res.Data = ex.Message;
                return res;
            }
            try
            {
                _dataAccessor.ModifyCart(id, delta);
                res.Data = _dataAccessor.GetCart(userId, null);
            }
            catch (Win32Exception ex)
            {
                res.Status.Code = ex.ErrorCode;
                res.Status.Phrase = "Bad Request";
                res.Status.IsSuccess = false;
                res.Data = ex.Message;
            }
            return res;
        }
    }
}
