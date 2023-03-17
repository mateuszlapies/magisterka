namespace Application.Utils
{
    public class Sequal
    {
        private static readonly string file = "sql.db";

        public static string ConnectionString()
        {
            #if DEBUG
                return file;
            #else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file);
            #endif
        }
    }
}
