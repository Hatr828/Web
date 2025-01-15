using System.Globalization;

namespace WebApplication1.Services
{
    public class TimeDate : ITimeDate
    {
        public string GetCurrentTime_Sql()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public long GetCurrentTime_Stamp()
        {
           return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        }

        public long ParseSql_Stamp(string sqlDate)
        {
            DateTime parsedDate = DateTime.ParseExact(sqlDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return new DateTimeOffset(parsedDate).ToUnixTimeMilliseconds();
        }
    }
}
