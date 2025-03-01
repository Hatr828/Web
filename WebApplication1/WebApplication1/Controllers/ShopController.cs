using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models;
using WebApplication1.Models.Shop;
using WebApplication1.Services.Storage;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class ShopController(ApplicationDbContext dataContext,
     IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;
        private readonly ApplicationDbContext _dataContext = dataContext;

        public IActionResult Index()
        {
            ShopIndexPageModel model = new()
            {
                Categories = [.. _dataContext.Categories]
            };

            if (HttpContext.Session.Keys.Contains("productModelErrors"))
            {
                model.Errors = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    HttpContext.Session.GetString("productModelErrors")!
                );
                model.ValidationStatus = JsonSerializer.Deserialize<bool>(
                    HttpContext.Session.GetString("productModelStatus")!
                );
                HttpContext.Session.Remove("productModelErrors");
                HttpContext.Session.Remove("productModelStatus");
            }

            return View(model);
        }

        public ViewResult Category([FromRoute] String id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                    .Categories
                    .Include(c => c.Products)
                    .FirstOrDefault(c => c.Slug == id)
            };
            return View(model);
        }

        public ViewResult Product([FromRoute] String id)
        {
            ShopProductPageModel model = new()
            {
                Product = _dataContext
                    .Products
                    .Include(p => p.Category)
                    .ThenInclude(c => c.Products)
                    .FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id)
            };
            return View(model);
        }

        [HttpDelete]
        public JsonResult CloseCart([FromRoute] String id)
        {
            // Чи користувач авторизований?
            String? sid = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (sid == null)
            {
                return Json(new { status = 401, message = "UnAuthorized" });
            }

            // чи є такий кошик
            Guid cartId;
            try { cartId = Guid.Parse(id); }
            catch { return Json(new { status = 400, message = "id unrecognized" }); }

            var cart = _dataContext
                .Carts
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.Id == cartId);
            if (cart == null)
            {
                return Json(new { status = 404, message = "Requested ID Not Found" });
            }

            // Чи належить він авторизованому користувачеві?           
            if (cart.UserId.ToString() != sid)
            {
                return Json(new { status = 403, message = "Forbidden" });
            }
            String cartAction = Request.Headers["Cart-Action"].ToString();
            if (cartAction == "Buy")
            {
                cart.MomentBuy = DateTime.Now.ToUniversalTime();
                // Скорочуємо кількість товарів
                foreach (var cd in cart.CartDetails)
                {
                    cd.Product.Stock -= cd.Cnt;
                }
            }
            else
            {
                cart.MomentCancel = DateTime.Now.ToUniversalTime();
            }

            _dataContext.SaveChanges();
            return Json(new { status = 200, message = "OK" });
        }

        [HttpPatch]
        public JsonResult ModifyCart([FromRoute] String id, [FromQuery] int delta)
        {
            Guid cartDetailId;
            try
            {
                cartDetailId = Guid.Parse(id);
            }
            catch
            {
                return Json(new { status = 400, message = "id unrecognized" });
            }
            if (delta == 0)
            {
                return Json(new { status = 400, message = "dummy action" });
            }
            var cartDetail = _dataContext
                .CartDetails
                .Include(cd => cd.Product)
                .Include(cd => cd.Cart)
                .FirstOrDefault(cd => cd.Id == cartDetailId);

            if (cartDetail is null)
            {
                return Json(new { status = 404, message = "id respond no item" });
            }
            // Д.З. У методі ModifyCart додати перевірку на власність -
            // елемент кошику, що змінюється, належить авторизованому користувачу
            // За відсутності авторизації також надіслати відмову у змінах

            // Перевіряємо delta
            // 1) що її врахування не призведе до від"ємних чисел
            if (cartDetail.Cnt + delta < 0)
            {
                return Json(new { status = 422, message = "decrement too large" });
            }
            // 2) що кількість не перевищує товарні залишки
            if (cartDetail.Cnt + delta > cartDetail.Product.Stock)
            {
                return Json(new { status = 406, message = "increment too large" });
            }

            if (cartDetail.Cnt + delta == 0)  // видалення останнього
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
            return Json(new { status = 202, message = "Accepted" });
        }

        public RedirectToActionResult AddProduct([FromForm] ShopProductFormModel model)
        {
            (bool? status, Dictionary<string, string> errors) = ValidateShopProductModel(model);

            if (status ?? false)
            {
                String? imagesCsv = null;
                if (model!.Images != null)
                {
                    imagesCsv = "";
                    foreach (IFormFile file in model!.Images)
                    {
                        imagesCsv += _storageService.Save(file) + ',';
                    }
                }

                _dataContext.Products.Add(new Data.Entities.Product
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    Price = model.Price,
                    Stock = model.Stock,
                    Slug = model.Slug,
                    ImagesCsv = imagesCsv
                });
                _dataContext.SaveChanges();
            }

            HttpContext.Session.SetString("productModelErrors",
            JsonSerializer.Serialize(errors));
            HttpContext.Session.SetString("productModelStatus",
            JsonSerializer.Serialize(status));

            return RedirectToAction(nameof(Index));
        }

        [HttpPut]
        public JsonResult AddToCart([FromRoute] String id)
        {
            String? userId = HttpContext
                .User
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)
                ?.Value;

            if (userId == null)
            {
                return Json(new { status = 401, message = "UnAuthorized" });
            }
            Guid uid = Guid.Parse(userId);

            // Перевіряємо id на UUID 
            Guid productId;
            try { productId = Guid.Parse(id); }
            catch
            {
                return Json(new { status = 400, message = "UUID required" });
            }
            // Перевіряємо id на належність товару
            var product = _dataContext
                .Products
                .FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { status = 404, message = "Product not found" });
            }

            // Шукаємо відкритий кошик користувача
            var cart = _dataContext
                .Carts
                .FirstOrDefault(
                    c => c.UserId == uid &&
                    c.MomentBuy == null &&
                    c.MomentCancel == null);

            if (cart == null)  // немає відкритого - тоді відкриваємо новий
            {
                cart = new Data.Entities.Cart()
                {
                    Id = Guid.NewGuid(),
                    MomentOpen = DateTime.Now.ToUniversalTime(),
                    UserId = uid,
                    Price = 0
                };
                _dataContext.Carts.Add(cart);
            }

            // Перевіряємо чи є такий товар у кошику
            var cd = _dataContext
                .CartDetails
                .FirstOrDefault(d =>
                    d.CartId == cart.Id &&
                    d.ProductId == product.Id
                );
            if (cd != null)  // товар вже є у кошику
            {
                cd.Cnt += 1;
                cd.Price += product.Price;
            }
            else   // товару немає - створюємо новий запис
            {
                cd = new Data.Entities.CartDetail()
                {
                    Id = Guid.NewGuid(),
                    Moment = DateTime.Now.ToUniversalTime(),
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Cnt = 1,
                    Price = product.Price
                };
                _dataContext.CartDetails.Add(cd);
            }
            cart.Price += product.Price;
            _dataContext.SaveChanges();
            return Json(new { status = 201, message = "Created" });
        }

        private (bool, Dictionary<string, string>) ValidateShopProductModel(ShopProductFormModel? model)
        {
            bool status = true;
            Dictionary<string, string> errors = [];

            if (model == null)
            {
                status = false;
                errors["ModelState"] = "The model was not provided.";
            }
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    status = false;
                    errors["ProductName"] = "The product name cannot be empty.";
                }
                else if (model.Name.Length < 3)
                {
                    status = false;
                    errors["ProductName"] = "The product name must be at least 3 characters long.";
                }

                if (string.IsNullOrEmpty(model.Description))
                {
                    status = false;
                    errors["ProductDescription"] = "The product description cannot be empty.";
                }
                else if (model.Description.Length < 15)
                {
                    status = false;
                    errors["ProductDescription"] = "The product description must be at least 15 characters long.";
                }

                if (model.Price < 0)
                {
                    status = false;
                    errors["ProductPrice"] = "The product price cannot be negative.";
                }

                if (model.Stock < 0)
                {
                    status = false;
                    errors["ProductStock"] = "The product stock cannot be negative.";
                }

                if (string.IsNullOrEmpty(model.Slug))
                {
                    status = false;
                    errors["ProductSlug"] = "The product slug cannot be empty.";
                }
                else if (_dataContext.Products.Count(p => p.Slug == model.Slug) > 0)
                {
                    status = false;
                    errors["ProductSlug"] = "The product slug already exists.";
                }

                if (model.Images == null)
                {
                    status = false;
                    errors["ProductImages"] = "The product must have at least one image.";
                }
                else
                {
                    foreach (var image in model.Images)
                    {
                        string fileExtension = Path.GetExtension(image.FileName);
                        List<string> availableExtensions = [".jpg", ".png", ".webp", ".jpeg"];

                        if (!availableExtensions.Contains(fileExtension))
                        {
                            status = false;
                            errors["ProductImages"] = "The file must have an extension.";
                            break;
                        }
                    }
                }
            }
            return (status, errors);
        }
    }

}