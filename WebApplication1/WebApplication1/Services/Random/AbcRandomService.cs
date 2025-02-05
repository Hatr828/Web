namespace WebApplication1.Services.Random
{
    public class AbcRandomService : IRandomService
    {
        private readonly String _fileChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly System.Random _random = new();
        public string FileName()
        {
            char[] chars = new char[20];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = _fileChars[_random.Next(_fileChars.Length)];
            }
            return new String(chars);
        }
    }
}
