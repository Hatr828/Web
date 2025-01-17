namespace ASP_P22.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        public List<UserAccess> Accesses { get; set; } = [];
    }
}