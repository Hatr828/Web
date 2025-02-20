using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models;
using WebApplication1.Services.Storage;

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

        public RedirectToActionResult AddProduct([FromForm] ShopProductFormModel model)
        {
            (bool? status, Dictionary<string, string> errors) = ValidateShopProductModel(model);

            if (status ?? false)
            {
                string imagesCsv = "";
                foreach (IFormFile file in model!.Images)
                {
                    imagesCsv += _storageService.Save(file) + ',';
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