using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models.Shop;
using WebApplication1.Services.kdf;

namespace WebApplication1.Data
{
    public class DataAccessor(
          DataContext dataContext,
          IHttpContextAccessor httpContextAccessor,
          IKdfService kdfService,
          IConfiguration configuration)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IConfiguration _configuration = configuration;

        public Data.Entities.UserAccess? BasicAuthenticate()
        {
            String? authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();  // "Basic dGVzdDoxMjM="
            if (String.IsNullOrEmpty(authHeader))
            {
                throw new Exception("Authorization header required");
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                throw new Exception($"Authorization scheme error: '{authScheme}' required");
            }
            String credentials = authHeader[authScheme.Length..];  // "dGVzdDoxMjM="

            String authData = System.Text.Encoding.UTF8.GetString(
                Base64UrlTextEncoder.Decode(credentials));         // "test:123"

            String[] parts = authData.Split(':', 2);               // ["test", "123"]
            if (parts.Length != 2)
            {
                throw new Exception("Authorization credentials malformed");
            }
            // login - parts[0], password - parts[1]
            var ua = _dataContext
                .UsersAccess
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.Login == parts[0]);

            if (ua == null)
            {
                throw new Exception("Authorization rejected");
            }
            var (iter, len) = KdfSettings();
            String dk1 = _kdfService.Dk(parts[1], ua.Salt, iter, len);
            if (dk1 != ua.Dk)
            {
                throw new Exception("Authorization rejected.");
            }
            return ua;
        }
        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }

        public ShopIndexPageModel CategoriesList()
        {
            ShopIndexPageModel model = new()
            {
                Categories = [.. _dataContext.Categories],
            };
            foreach (var c in model.Categories)
            {
                c.ImagesCsv = c.ImagesCsv == null ? null : String.Join(',',
                    c.ImagesCsv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => StoragePrefix + i)
                );
            }
            return model;
            /* Д.З. Реалізувати клонування сутностей EF при модифікації адрес
             * зображень, що передаються у відповідь від бекенда
             * (метод DataAccessor::CategoriesList)
             */
        }
        public ShopCategoryPageModel CategoryById(String id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                    .Categories
                    .Include(c => c.Products)
                        .ThenInclude(p => p.Rates)
                    .FirstOrDefault(c => c.Slug == id)
            };
            if (model.Category != null)
            {
                model.Category = model.Category with
                {
                    Products = model.Category
                    .Products
                    .Select(p => p with
                    {
                        ImagesCsv = p.ImagesCsv == null
                        ? StoragePrefix + "no-image.jpg"
                        : String.Join(',',
                            p.ImagesCsv
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => StoragePrefix + i)
                        )
                    }).ToList()
                };
            }
            //foreach(var product in model.Category?.Products ?? [])
            //{
            //    product.ImagesCsv = product.ImagesCsv == null 
            //        ? StoragePrefix + "no-image.jpg"
            //        : String.Join(',',
            //            product.ImagesCsv
            //            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            //            .Select(i => StoragePrefix + i)
            //        );
            //}
            _dataContext.SaveChanges();
            return model;
        }

        public ShopProductPageModel ProductById(String id)
        {
            String? authUserId = _httpContextAccessor.HttpContext?.User.Claims
               .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

            var Product = _dataContext
                    .Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Products)
                    .Include(p => p.Rates)
                        .ThenInclude(r => r.User)
                    .FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id);

            ShopProductPageModel model = new()
            {
                Product = Product
            };

            if (Product != null && authUserId != null)
            {
                var cds = _dataContext
                    .CartDetails.Where(cd =>
                        cd.ProductId == Product.Id &&
                        cd.Cart.UserId.ToString() == authUserId);

                model.CanUserRate = cds.Any();

                model.UserRate =
                    _dataContext.Rates.FirstOrDefault(r =>
                        r.ProductId == Product.Id &&
                        r.UserId.ToString() == authUserId);

                model.AuthUserId = authUserId;
            }
            return model;
        }

        private String StoragePrefix => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/";
    }
}
