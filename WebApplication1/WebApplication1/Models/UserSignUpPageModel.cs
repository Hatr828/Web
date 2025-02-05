    
namespace WebApplication1.Models
{
    public class UserSignUpPageModel
    {
        public UserSignUpFormModel? FormModel { get; set; }
        public bool? ValidationStatus { get; set; }
        public Dictionary<string, string>? Errors { get; set; }
        public WebApplication1.Data.Entities.User? User { get; set; }
    }
}
