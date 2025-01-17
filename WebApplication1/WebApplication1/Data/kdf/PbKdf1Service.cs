using ASP_P22.Data.Hash;

namespace ASP_P22.Data.Kdf
{
    // sec. 5.1 PBKDF1 by RFC2898 (https://datatracker.ietf.org/doc/html/rfc2898)
    public class PbKdf1Service(IHashService hashService) : IKdfService
    {
        private readonly IHashService _hashService = hashService;

        public string Dk(string password, string salt, uint iterationCount, uint dkLength)
        {
            ArgumentOutOfRangeException.ThrowIfZero(iterationCount);

            String t = _hashService.Digest(password + salt);
            for (uint i = 0; i < iterationCount - 1; i++)
            {
                t = _hashService.Digest(t);
            }
            if (dkLength > t.Length)
            {
                throw new ArgumentException(
                    $"dkLength {dkLength} must be <= Hash.length {t.Length}");
            }
            return t[..(int)dkLength];
        }
    }
}
/* 1. Інжектувати сервіс DK до контролера, одержати його результат
 * для тестових даних, вивести на сторінці сайту.
 * 2. Імплементувати другу функцію обчислення DK (5.2 PBKDF2) зі 
 * стандарту RFC2898 (https://datatracker.ietf.org/doc/html/rfc2898)
 */