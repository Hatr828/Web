using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models;
using WebApplication1.Models.Shop;
using WebApplication1.Services.Storage;
using System.Security.Claims;
using WebApplication1.Data.Entities;
using WebApplication1.Data;
using System.ComponentModel;

namespace WebApplication1.Controllers
{
    public class ShopController(DataContext dataContext,
     IStorageService storageService, DataAccessor dataAccessor) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IStorageService _storageService = storageService;
        private readonly DataContext _dataContext = dataContext;
        public IActionResult Index()
        {
            /*ShopIndexPageModel model = new()
			{
				Categories = [.. _dataContext.Categories]
			};*/
            ShopIndexPageModel model = _dataAccessor.CategoriesList();
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
        public ViewResult Product([FromRoute] string id)
        {
            return View(_dataAccessor.ProductById(id));
        }
        public JsonResult Rate([FromBody] ShopRateFormModel rateModel)
        {
            if (!Guid.TryParse(rateModel.UserId, out Guid userId))
            {
                return Json(new { status = 400, errors = new { UserId = "Некоректний ідентифікатор користувача" } });
            }

            if (!Guid.TryParse(rateModel.ProductId, out Guid productId))
            {
                return Json(new { status = 400, errors = new { ProductId = "Некоректний ідентифікатор товару" } });
            }

            var user = _dataContext.Users.Include(u => u.Rates).FirstOrDefault(u => u.Id == userId);
            if (user is null)
            {
                return Json(new { status = 401, errors = new { User = "Користувач не авторизований" } });
            }

            var product = _dataContext.Products.FirstOrDefault(p => p.Id == productId);
            if (product is null)
            {
                return Json(new { status = 404, errors = new { Product = "Товар не знайдено" } });
            }

            bool hasRating = rateModel.Rating.HasValue;
            bool hasComment = !string.IsNullOrWhiteSpace(rateModel.Comment);

            if (!hasRating && !hasComment)
            {
                return Json(new { status = 400, errors = new { Rating = "Оцінка або коментар мають бути вказані." } });
            }

            if (hasComment && rateModel.Comment.Length < 5)
            {
                return Json(new { status = 400, errors = new { Comment = "Коментар має бути не коротше 5 символів." } });
            }

            var givenRate = user.Rates?.FirstOrDefault(r => r.ProductId == productId);
            if (givenRate is not null)
            {
                givenRate.Comment = hasComment ? rateModel.Comment : givenRate.Comment;
                givenRate.Rating = hasRating ? rateModel.Rating : givenRate.Rating;
                givenRate.Moment = DateTime.Now;
            }
            else
            {
                givenRate = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    Comment = hasComment ? rateModel.Comment : null,
                    Rating = hasRating ? rateModel.Rating : null,
                    Moment = DateTime.Now
                };
                _dataContext.Rates.Add(givenRate);
            }

            _dataContext.SaveChanges();

            return Json(new { status = 200, message = "Ok", data = givenRate });
        }
        public ViewResult Category([FromRoute] string id)
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
            return View(model);
        }
        [HttpPut]
        public JsonResult AddToCart([FromRoute] string id)
        {
            string? userId = HttpContext
                .User
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?
                .Value;
            if (userId == null)
            {
                return Json(new { status = 401, message = "Unauthorized" });
            }
            try
            {
                _dataAccessor.AddToCart(userId, id);
                return Json(new { status = 201, message = "Created" });
            }
            catch (Win32Exception ex)
            {
                return Json(new { status = ex.ErrorCode, message = ex.Message });
            }
        }
        [HttpPatch]
        public JsonResult ModifyCart([FromRoute] string id, [FromQuery] int delta)
        {
            try
            {
                _dataAccessor.ModifyCart(id, delta);
                return Json(new { status = 202, message = "Accepted" });
            }
            catch (Win32Exception ex)
            {
                return Json(new { status = ex.ErrorCode, message = ex.Message });
            }
        }
        [HttpDelete]
        public JsonResult CloseCart([FromRoute] string id)
        {
            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                return Json(new { status = 401, message = "Unauthorized" });
            }

            Guid cartId;
            try { cartId = Guid.Parse(id); }
            catch { return Json(new { status = 400, message = "id unrecognized" }); }

            var cart = _dataContext.Carts.Include(c => c.CartDetails).ThenInclude(cd => cd.Product).FirstOrDefault(c => c.Id == cartId);
            if (cart == null)
            {
                return Json(new { status = 404, message = "Requested ID Not Found" });
            }

            if (cart.UserId.ToString() != userId)
            {
                return Json(new { status = 403, message = "Forbidden" });
            }

            string cartAction = Request.Headers["Cart-Action"].ToString();
            if (cartAction == "Buy")
            {
                cart.MomentBuy = DateTime.Now;
                foreach (var cd in cart.CartDetails)
                {
                    cd.Product.Stock -= cd.Cnt;
                }
            }
            else
            {
                cart.MomentCancel = DateTime.Now;
            }
            _dataContext.SaveChanges();
            return Json(new { status = 200, message = "OK" });
        }
        [HttpPost]
        public JsonResult RepeatCart([FromRoute] string id)
        {
            string? userId = HttpContext
                .User
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?
                .Value;
            if (userId == null)
            {
                return Json(new { status = 401, message = "Unauthorized" });
            }

            Guid uid = Guid.Parse(userId);

            Guid cartId;
            try { cartId = Guid.Parse(id); }
            catch
            {
                return Json(new { status = 400, message = "UUID required" });
            }

            var oldCart = _dataContext.Carts
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.Id == cartId && c.UserId == uid);

            if (oldCart == null)
            {
                return Json(new { status = 404, message = "Cart not found" });
            }

            var activeCart = _dataContext.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefault(c => c.UserId == uid && c.MomentBuy == null && c.MomentCancel == null);

            if (activeCart == null)
            {
                activeCart = new Data.Entities.Cart()
                {
                    Id = Guid.NewGuid(),
                    MomentOpen = DateTime.Now,
                    UserId = uid,
                    Price = 0,
                };
                _dataContext.Carts.Add(activeCart);
            }

            List<string> warnings = new List<string>();

            foreach (var oldDetail in oldCart.CartDetails)
            {
                var product = oldDetail.Product;
                if (product == null)
                {
                    warnings.Add($"Товар '{oldDetail.ProductId}' більше не доступний.");
                    continue;
                }

                int availableQuantity = product.Stock;
                if (availableQuantity <= 0)
                {
                    warnings.Add($"Товар '{product.Name}' недоступний для замовлення.");
                    continue;
                }

                int addQuantity = Math.Min(oldDetail.Cnt, availableQuantity);
                var activeDetail = _dataContext.CartDetails.FirstOrDefault(d => d.CartId == activeCart.Id && d.ProductId == product.Id);

                if (activeDetail != null)
                {
                    activeDetail.Cnt += addQuantity;
                    activeDetail.Price += product.Price * addQuantity;
                }
                else
                {
                    var newDetail = new Data.Entities.CartDetail()
                    {
                        Id = Guid.NewGuid(),
                        Moment = DateTime.Now,
                        CartId = activeCart.Id,
                        ProductId = product.Id,
                        Price = product.Price * addQuantity,
                        Cnt = addQuantity
                    };
                    _dataContext.CartDetails.Add(newDetail);
                }

                activeCart.Price += product.Price * addQuantity;

                if (addQuantity < oldDetail.Cnt)
                {
                    warnings.Add($"Товар '{product.Name}' додано у меншій кількості ({addQuantity} шт.) через обмежений залишок.");
                }
            }

            _dataContext.SaveChanges();

            return Json(new
            {
                status = 200,
                message = "Cart repeated successfully",
                warnings
            });
        }
        public RedirectToActionResult AddProduct([FromForm] ShopProductFormModel model)
        {
            (bool? status, Dictionary<string, string> errors) = ValidateShopProductModel(model);

            if (status ?? false)
            {
                string? imagesCsv = null;
                if (model.Images != null)
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

        private (bool, Dictionary<string, string>) ValidateShopProductModel(ShopProductFormModel? model)
        {
            bool status = true;
            Dictionary<string, string> errors = [];
            if (model == null)
            {
                status = false;
                errors["ModelState"] = "Модель не передано.";
            }
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    status = false;
                    errors["ProductName"] = "Назва товару не може бути порожньою.";
                }
                else if (model.Name.Length < 3)
                {
                    status = false;
                    errors["ProductName"] = "Назва товару повинна мати більше 3 символів.";
                }

                if (string.IsNullOrEmpty(model.Description))
                {
                    status = false;
                    errors["ProductDescription"] = "Опис товару не може бути порожнім.";
                }
                else if (model.Description.Length < 15)
                {
                    status = false;
                    errors["ProductDescription"] = "Опис товару повинен мати більше 15 символів.";
                }

                if (model.Price < 0)
                {
                    status = false;
                    errors["ProductPrice"] = "Ціна товару не може бути від'ємною.";
                }

                if (model.Stock < 0)
                {
                    status = false;
                    errors["ProductStock"] = "Кількість товару не може бути від'ємною.";
                }

                if (!string.IsNullOrEmpty(model.Slug))
                {
                    if (_dataContext.Products.Count(p => p.Slug == model.Slug) > 0)
                    {
                        status = false;
                        errors["ProductSlug"] = "Slug товару вже існує.";
                    }
                }

                if (model.Images != null)
                {
                    foreach (var image in model.Images)
                    {
                        string fileExtension = Path.GetExtension(image.FileName);
                        List<string> availableExtensions = [".jpg", ".png", ".webp", ".jpeg"];
                        if (!availableExtensions.Contains(fileExtension))
                        {
                            status = false;
                            errors["ProductImages"] = "Файл повинен мати розширення .jpg, .png, .webp, .jpeg.";
                            break;
                        }
                    }
                }

            }
            return (status, errors);
        }
    }
}