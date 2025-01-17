using ASP_P22.Data.Hash;
using System.Security.Cryptography;

namespace ASP_P22.Data.Kdf
{
    // sec. 5.2 PBKDF1 by RFC2898 (https://datatracker.ietf.org/doc/html/rfc2898)
    public class PbKdf2Service(IHashService hashService) : IKdfService
    {
        private readonly IHashService _hashService = hashService;

        public string Dk(string password, string salt, uint iterationCount, uint dkLength)
        {
            ArgumentOutOfRangeException.ThrowIfZero(iterationCount);

            int l = (int)dkLength / password.Length;
            int r = (int)dkLength - (l - 1) * password.Length;

            return "";

        }

        private string F(string password, string salt, uint iterationCount, int index)
        {
            return "";
        }

    }
    //5.1 я бы смог и сам сделать но 5.2 не мой уровень пока...
    //я потратил около минут 30 и ничего не смог добиться(либо сильно сложно, либо ...)
}