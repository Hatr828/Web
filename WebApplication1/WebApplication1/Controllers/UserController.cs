using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var formModel = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                var (validationStatus, errors) = ValidateUserSignUpFormModel(formModel);
                ViewData["formModel"] = formModel;
                ViewData["validationStatus"] = validationStatus;
                ViewData["errors"] = errors;
                HttpContext.Session.Remove("formModel");
            }
            return View();
        }

        public IActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            // return View("Index");  // Украй не рекомендується переходити на 
            // представлення після прийняття даних форми

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


            /* Д.З. Завершити валідацію даних від форми реєстрації користувача
             * Пароль: повинен містити літеру, цифру, спец-символ (дозволяється доповнити)
             * Повтор паролю: має збігатись з паролем
             * !! при відображенні помилок паролі не прийнято відновлювати у полях
             */

            return (status, errors);
        }
    }
}