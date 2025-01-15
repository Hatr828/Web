namespace WebApplication1.Services
{
    public interface ITimeDate
    {
        public long GetCurrentTime_Stamp();

        public String GetCurrentTime_Sql();

        public long ParseSql_Stamp(String sqlFormat);
    }
}
