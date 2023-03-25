using Blockchain;
using LiteDB;

namespace Application.Utils
{
    public class Sequal
    {
        private static readonly string file = "sql.db";

        public static string ConnectionString()
        {
            #if DEBUG
                if (!Directory.GetCurrentDirectory().Contains("bin"))
                {
                    return Path.Combine("bin/Debug/net7.0", file);
                }
                else
                {
                return file;
                }
            #else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file);
            #endif
        }
    }
}
