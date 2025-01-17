namespace ASP_P22.Data.Kdf
{
    // key derivation function by RFC2898 (https://datatracker.ietf.org/doc/html/rfc2898)
    public interface IKdfService
    {
        String Dk(String password, String salt, uint iterationCount, uint dkLength);
    }
}