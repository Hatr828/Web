using WebApplication1.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebApplication1.Data.DBContexts;
using WebApplication1.Services.kdf;

namespace WebApplication1.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(User user, string login, string password, IKdfService kdf)
        {
            var salt = Guid.NewGuid().ToString();
            var dk = kdf.Dk(password, salt, 10, 32);

            var userAccess = new UserAccess
            {
                Login = login,
                Dk = dk,
                Salt = salt,
                User = user
            };

            _context.Users.Add(user);
            _context.UsersAccess.Add(userAccess);

            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            var user = await _context.Users
                                      .Include(u => u.Accesses)
                                      .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }
    }
}
