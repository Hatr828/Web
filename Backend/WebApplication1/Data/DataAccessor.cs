using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;
using WebApplication1.Data.DBContexts;
using WebApplication1.Data.Entities;
using WebApplication1.Models.Shop;
using WebApplication1.Services.kdf;

namespace WebApplication1.Data
{
    public class DataAccessor(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
         IKdfService kdfService, IConfiguration configuration)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IConfiguration _configuration = configuration;
        public void AddToCart(string userId, string productId)
        {
            Guid userGuid;
            try { userGuid = Guid.Parse(userId); }
            catch { throw new Win32Exception(400, "User ID is invalid UUID"); }

            Guid productGuid;
            try { productGuid = Guid.Parse(productId); }
            catch { throw new Win32Exception(400, "Product ID is invalid UUID"); }

            var product = _dataContext.Products.FirstOrDefault(p => p.Id == productGuid);
            if (product == null)
            {
                throw new Win32Exception(404, "Product ID is not found");
            }

            var cart = _dataContext.Carts.FirstOrDefault(c => c.UserId == userGuid && c.MomentBuy == null && c.MomentCancel == null);
            if (cart == null)
            {
                cart = new Data.Entities.Cart()
                {
                    Id = Guid.NewGuid(),
                    MomentOpen = DateTime.Now,
                    UserId = userGuid,
                    Price = 0,
                };
                _dataContext.Carts.Add(cart);
            }
            var cd = _dataContext.CartDetails.FirstOrDefault(d => d.CartId == cart.Id && d.ProductId == product.Id);
            if (cd != null)
            {
                cd.Cnt += 1;
                cd.Price += product.Price;
                cart.Price += product.Price;
            }
            else
            {
                cd = new Data.Entities.CartDetail()
                {
                    Id = Guid.NewGuid(),
                    Moment = DateTime.Now.ToUniversalTime(),
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Price = product.Price,
                    Cnt = 1
                };
                cart.Price += product.Price;
                _dataContext.CartDetails.Add(cd);
            }
            _dataContext.SaveChanges();
        }
        public Cart? GetCart(string userId, string? cartId)
        {
            Guid userGuid;
            try { userGuid = Guid.Parse(userId); }
            catch { throw new Exception("User ID is invalid UUID"); }

            Cart? cart;

            if (cartId == null)
            {
                cart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefault(c => c.UserId == userGuid && c.MomentCancel == null && c.MomentBuy == null);
            }
            else
            {
                Guid cartGuid;
                try { cartGuid = Guid.Parse(cartId); }
                catch { throw new Exception("Cart ID is invalid UUID"); }

                cart = _dataContext
                        .Carts
                        .Include(c => c.CartDetails)
                        .ThenInclude(cd => cd.Product)
                        .FirstOrDefault(c => c.Id == cartGuid);
            }

            if (cart == null)
            {
                return null;
            }
            if (cart.UserId != userGuid)
            {
                throw new AccessViolationException("Forbidden");
            }
            cart = cart with
            {
                CartDetails = cart.CartDetails.Select(cd => cd with
                {
                    Product = cd.Product with
                    {
                        ImagesCsv = cd.Product.ImagesCsv == null
                        ? StoragePrefix + "no-image.jpg"
                        : string.Join(',', cd.Product.ImagesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => StoragePrefix + i))
                    }
                }).ToList()
            };
            _dataContext.SaveChanges();
            return cart;
        }
        public void ModifyCart(string id, int delta)
        {
            Guid cartDetailId;
            try
            {
                cartDetailId = Guid.Parse(id);
            }
            catch
            {
                throw new Win32Exception(400, "Id unrecognized");
            }
            if (delta == 0)
            {
                throw new Win32Exception(400, "Dummy action");
            }
            var cartDetail = _dataContext
                .CartDetails
                .Include(cd => cd.Product)
                .Include(cd => cd.Cart)
                .FirstOrDefault(cd => cd.Id == cartDetailId);
            if (cartDetail == null)
            {
                throw new Win32Exception(404, "Item not found");
            }

            if (cartDetail.Cnt + delta < 0)
            {
                throw new Win32Exception(422, "Decrement too large");
            }

            if (cartDetail.Cnt + delta > cartDetail.Product.Stock)
            {
                throw new Win32Exception(406, "Increment too large");
            }

            if (cartDetail.Cnt + delta == 0)
            {
                cartDetail.Cart.Price += delta * cartDetail.Product.Price;
                _dataContext.CartDetails.Remove(cartDetail);
            }
            else
            {
                cartDetail.Cnt += delta;
                cartDetail.Price += delta * cartDetail.Product.Price;
                cartDetail.Cart.Price += delta * cartDetail.Product.Price;
            }
            _dataContext.SaveChanges();
        }
        public Data.Entities.UserAccess? BasicAuthenticate()
        {
            string? authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                throw new Exception("Потрібен заголовок авторизації.");
            }
            string authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                throw new Exception($"Помилка схеми авторизації: потрібна '{authScheme}'.");
            }
            string credentials = authHeader[authScheme.Length..];
            string authData = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(credentials));
            string[] parts = authData.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new Exception($"Облікові дані авторизації неправильні.");
            }
            var access = _dataContext.Accesses.Include(a => a.User).FirstOrDefault(a => a.Login == parts[0]);
            if (access == null)
            {
                throw new Exception($"Авторизацію відхилено.");
            }
            var (iterationCount, dkLength) = KdfSettings();
            string dk1 = _kdfService.Dk(parts[1], access.Salt, iterationCount, dkLength);
            if (dk1 != access.DK)
            {
                throw new Exception($"Авторизацію відхилено.");
            }
            return access;
        }
        public Entities.AuthToken CreateTokenForUserAccess(Entities.UserAccess ua)
        {
            int lifetime = _configuration.GetSection("AuthToken").GetSection("Lifetime").Get<int>();
            var token = _dataContext.AuthTokens.FirstOrDefault(t => t.Sub == ua.Id && t.Exp > DateTime.Now);
            if (token != null)
            {
                token.Exp = token.Exp.AddSeconds(lifetime);
            }
            else
            {
                token = new Entities.AuthToken()
                {
                    Jti = Guid.NewGuid(),
                    Iss = "ASP_P22",
                    Sub = ua.Id,
                    Aud = null,
                    Iat = DateTime.Now,
                    Exp = DateTime.Now.AddSeconds(lifetime),
                    Nbf = null
                };
                _dataContext.AuthTokens.Add(token);
            }
            _dataContext.SaveChanges();

            return token;
        }
        public ShopIndexPageModel CategoriesList()
        {
            ShopIndexPageModel model = new ShopIndexPageModel
            {
                Categories = _dataContext.Categories.ToArray()
            };

            if (model.Categories != null)
            {
                model.Categories = model.Categories
                    .Select(c => c with
                    {
                        ImagesCsv = c.ImagesCsv == null
                            ? StoragePrefix + "no-image.jpg"
                            : string.Join(',', c.ImagesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => StoragePrefix + i))
                    })
                    .ToArray();
            }

            return model;
        }
        public ShopCategoryPageModel CategoryById(string id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                .Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Rates)
                .FirstOrDefault(c => c.Slug == id),
                Categories = [.. _dataContext.Categories]
            };
            if (model.Category != null)
            {
                model.Category = model.Category with
                {
                    Products = model.Category.Products.Select(p => p with
                    {
                        ImagesCsv = p.ImagesCsv == null
                            ? StoragePrefix + "no-image.jpg"
                            : string.Join(',', p.ImagesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => StoragePrefix + i)
                    )
                    }).ToList()
                };
            }

            return model;
        }
        public ShopProductPageModel ProductById(string id)
        {
            string? authUserId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

            ShopProductPageModel model = new()
            {
                Product = _dataContext
                .Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Products)
                .Include(p => p.Rates)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id),
                IsUserCanRate = authUserId != null && _dataContext
                    .CartDetails
                    .Any(cd => (cd.ProductId.ToString() == id || cd.Product.Slug == id) && cd.Cart.UserId.ToString() == authUserId),
                UserRate = authUserId == null ? null : _dataContext.Rates.FirstOrDefault(r => (r.ProductId.ToString() == id || r.Product.Slug == id) && r.UserId.ToString() == authUserId),
                AuthUserId = authUserId,
                Categories = [.. _dataContext.Categories]
            };

            return model;
        }
        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }
        private string StoragePrefix => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/";
    }
}
