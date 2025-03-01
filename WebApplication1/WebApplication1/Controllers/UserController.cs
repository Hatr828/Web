using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models;
using WebApplication1.Models.User;
using WebApplication1.Services.kdf;
using WebApplication1.Services.Random;
using WebApplication1.Services.Storage;

namespace WebApplication1.Controllers
{
    public class UserController(
        ApplicationDbContext dataContext,
        IKdfService kdfService,
        IRandomService randomService,
        IStorageService storageService,
        ILogger<UserController> logger,
        IConfiguration configuration) : Controller
    {

        private readonly ApplicationDbContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IRandomService _randomService = randomService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<UserController> _logger = logger;
        private readonly IStorageService _storageService = storageService;

        public IActionResult Index()
        {
            UserSignUpPageModel pageModel = new();

            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var formModel = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                pageModel.FormModel = formModel;
                (pageModel.ValidationStatus, pageModel.Errors) =
                    ValidateUserSignUpFormModel(formModel);


                // ViewData["formModel"] = formModel;
                // ViewData["validationStatus"] = validationStatus;
                // ViewData["errors"] = errors;
                if (pageModel.ValidationStatus ?? false)
                {
                    String slug = string.IsNullOrEmpty(formModel.Slug) ? formModel.UserLogin : formModel.Slug;

                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel!.UserName,
                        Email = formModel.UserEmail,
                        Phone = formModel.UserPhone,
                        WorkPosition = formModel.UserPosition,
                        PhotoUrl = formModel.UserPhotoSavedName,

                        Slug = slug,  
                    };
                    String salt = _randomService.FileName();
                    var (iter, len) = KdfSettings();
                    Data.Entities.UserAccess ua = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Login = formModel.UserLogin,
                        Salt = salt,
                        Dk = _kdfService.Dk(formModel.Password1, salt, iter, len)
                    };
                    _dataContext.Users.Add(user);
                    _dataContext.UsersAccess.Add(ua);
                    _dataContext.SaveChanges();
                    pageModel.User = user;
                }
                HttpContext.Session.Remove("formModel");
            }

            return View(pageModel);
        }

        public ViewResult Cart()
        {
            UserCartPageModel model = new();
            String? userId =
                HttpContext
                    .User
                    .Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Sid)
                    ?.Value;
            if (userId is not null)
            {
                Guid uid = Guid.Parse(userId);
                model.ActiveCart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(d => d.Product)
                    .FirstOrDefault(c =>
                        c.UserId == uid &&
                        c.MomentBuy == null &&
                        c.MomentCancel == null);

            }
            return View(model);
        }
        public ViewResult Profile([FromRoute] String id)
        {
            // Чи користувач авторизований?
            String? sid = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            var authUser = sid == null ? null :
                _dataContext
                    .Users
                    .Include(u => u.Carts)
                    .ThenInclude(c => c.CartDetails)
                    .FirstOrDefault(u => u.Id.ToString() == sid);

            UserProfilePageModel pageModel;
            var profileUser = _dataContext.Users.FirstOrDefault(u => u.Slug == id);

            bool isOwner = authUser?.Slug == profileUser?.Slug;
            if (profileUser == null)
            {
                pageModel = new() { IsFound = false };
            }
            else
            {
                pageModel = new()
                {
                    IsFound = true,
                    Name = profileUser.Name,
                    Email = profileUser.Email,
                    PhotoUrl = "/Storage/Item/" + profileUser.PhotoUrl,
                    Phone = profileUser.Phone ?? "--",
                    MostViewed = id,
                    Recent = "Razor",
                    Role = profileUser.WorkPosition ?? "--",

                    IsOwner = isOwner,
                    Carts = isOwner ? authUser!.Carts : [],
                };
                /* Name = HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Name)
                        ?.Value ?? String.Empty,*/
            }
            return View(pageModel);
        }

        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }

        public RedirectToActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            // return View("Index");  // Украй не рекомендується переходити на 
            // представлення після прийняття даних форми

            // Перевіряємо чи є у формі файл і зберігаємо його
            // Оскільки сам файл не серіалізується, у моделі зберігаємо
            //  ім'я (URL) з яким він збережений

            if (formModel.UserPhoto != null && formModel.UserPhoto.Length != 0)
            {
                _logger.LogInformation("File uploaded {name}", formModel.UserPhoto.FileName);

                formModel.UserPhotoSavedName = _storageService.Save(formModel.UserPhoto);
            }

            HttpContext.Session.SetString(
                "formModel",
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction("Index");
        }

        private (bool, Dictionary<String, String>) ValidateUserSignUpFormModel(UserSignUpFormModel? formModel)
        {
            bool status = true;
            Dictionary<String, String> errors = [];

            if (formModel == null)
            {
                status = false;
                errors["ModelState"] = "Модель не передано";
                return (status, errors);
            }

            if (String.IsNullOrEmpty(formModel.UserName))
            {
                status = false;
                errors["UserName"] = "Ім'я не може бути порожнім";
            }
            else if (!Regex.IsMatch(formModel.UserName, "^[A-ZА-Я].*"))
            {
                status = false;
                errors["UserName"] = "Ім'я має починатись з великої літери";
            }

            if (String.IsNullOrEmpty(formModel.UserEmail))
            {
                status = false;
                errors["UserEmail"] = "Email не може бути порожнім";
            }
            else if (!Regex.IsMatch(formModel.UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                status = false;
                errors["UserEmail"] = "Email не відповідає шаблону";
            }

            if (String.IsNullOrEmpty(formModel.UserLogin))
            {
                status = false;
                errors["UserLogin"] = "Логін не може бути порожнім";
            }
            else if (formModel.UserLogin.Contains(':'))
            {
                status = false;
                errors["UserLogin"] = "Логін не може містити символ ':'";
            }
            else if (_dataContext
                         .UsersAccess
                         .Count(ua => ua.Login == formModel.UserLogin) > 0)
            {
                status = false;
                errors["UserLogin"] = "Логін вже використовується";
            }

            if (String.IsNullOrEmpty(formModel.Password1))
            {
                status = false;
                errors["Password1"] = "Пароль Логін не може бути порожнім";
            }
            else if (!Regex.IsMatch(formModel.Password1, "[A-Za-z]"))
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити хоча б одну літеру.";
            }
            else if(!Regex.IsMatch(formModel.Password1, @"\d"))
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити хоча б одну цифру.";
            }
            else if(!Regex.IsMatch(formModel.Password1, @"[\W_]"))
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити хоча б один спеціальний символ.";
            }
            else if(formModel.Password1.Length < 8)
            {
                status = false;
                errors["Password1"] = "Пароль повинен бути не менше 8 символів.";
            }

            if (String.IsNullOrEmpty(formModel.Password2))
            {
                status = false;
                errors["Password2"] = "Пароль Логін не може бути порожнім";
            }
            else if(formModel.Password1 != formModel.Password2)
            {
                status = false;
                errors["Password2"] = "Паролі не збігаються.";
            }

            if (!string.IsNullOrEmpty(formModel.UserPhone))
            {
                if (!Regex.IsMatch(formModel.UserPhone, @"^\+?\d{10,13}$"))
                {
                    status = false;
                    errors["UserPhone"] = "Номер телефону не відповідає стандартному шаблону.";
                }
            }
            if (!string.IsNullOrEmpty(formModel.UserPosition))
            {
                if (formModel.UserPosition.Length < 3)
                {
                    status = false;
                    errors["UserPosition"] = "Посада не може бути коротшою за 3 символи.";
                }
                else if (char.IsDigit(formModel.UserPosition[0]))
                {
                    status = false;
                    errors["UserPosition"] = "Посада не повинна починатися з цифри.";
                }
                else if (Regex.IsMatch(formModel.UserPosition, @"[^A-Za-zА-Яа-я0-9\s-]"))
                {
                    status = false;
                    errors["UserPosition"] = "Посада не повинна містити спеціальні символи (окрім '-').";
                }
            }
            if (!string.IsNullOrEmpty(formModel.UserPhotoSavedName))
            {
                string fileExtension = Path.GetExtension(formModel.UserPhotoSavedName);
                List<string> availableExtensions = [".jpg", ".png", ".webp", ".jpeg"];
                if (!availableExtensions.Contains(fileExtension))
                {
                    status = false;
                    errors["UserPhoto"] = "Файл повинен мати розширення .jpg, .png, .webp, .jpeg.";
                }
            }

            if (_dataContext.Users.Any(u => u.Slug == formModel.Slug))
            {
                status = false;
                errors["Slug"] = "Слаг має бути унікальним";
            }



            /* Д.З. Завершити валідацію даних від форми реєстрації користувача
             * Пароль: повинен містити літеру, цифру, спец-символ (дозволяється доповнити)
             * Повтор паролю: має збігатись з паролем
             * !! при відображенні помилок паролі не прийнято відновлювати у полях
             */

            return (status, errors);
        }

        [HttpGet]
        public JsonResult Authenticate()
        {
            String authHeader = Request.Headers.Authorization.ToString();  // "Basic dGVzdDoxMjM="
            if (String.IsNullOrEmpty(authHeader))
            {
                return AuthError("Authorization header required");
            }
            String authScheme = "Basic ";
            if (!authHeader.StartsWith(authScheme))
            {
                return AuthError($"Authorization scheme error: '{authScheme}' required");
            }
            String credentials = authHeader[authScheme.Length..];  // "dGVzdDoxMjM="
            String authData = System.Text.Encoding.UTF8.GetString(
                Base64UrlTextEncoder.Decode(credentials));         // "test:123"
            String[] parts = authData.Split(':', 2);               // ["test", "123"]
            if (parts.Length != 2)
            {
                return AuthError("Authorization credentials malformed");
            }
            // login - parts[0], password - parts[1]
            var ua = _dataContext
                .UsersAccess
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.Login == parts[0]);
            if (ua == null)
            {
                return AuthError("Authorization rejected");
            }
            var (iter, len) = KdfSettings();
            String dk1 = _kdfService.Dk(parts[1], ua.Salt, iter, len);
            if (dk1 != ua.Dk)
            {
                return AuthError("Authorization rejected.");
            }
            // return Json(ua.User);
            HttpContext.Session.SetString(
                "authUser",
                JsonSerializer.Serialize(ua.User)
            );
            return Json("Ok");
        }
        private JsonResult AuthError(String message)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Json(message);
        }

    }
}