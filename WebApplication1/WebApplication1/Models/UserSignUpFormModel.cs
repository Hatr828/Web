using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class UserSignUpFormModel
    {
        public String UserName { get; set; } = null!;
        public String UserEmail { get; set; } = null!;
        public String UserLogin { get; set; } = null!;
        public String Password1 { get; set; } = null!;
        public String Password2 { get; set; } = null!;

        public String UserPhone { get; set; } = null!;
        public String UserPosition { get; set; } = null!;

        public String Slug { get; set; } = "";
        [JsonIgnore]
        public IFormFile UserPhoto { get; set; } = null!;
        public String UserPhotoSavedName { get; set; } = null!;
    }
}
