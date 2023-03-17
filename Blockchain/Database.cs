using LiteDB;
using System.Globalization;

namespace Blockchain
{
    public class Database
    {
        private static LiteDatabase database;
        
        private static readonly string file = "blockchain.db";

        public static LiteDatabase Instance() 
        {
            if (database == null)
            {
                #if DEBUG
                    database = new LiteDatabase(file, GetMapper());
                #else
                    Instance = new LiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file), GetMapper());
                #endif
            }
            return database;
        }

        private static BsonMapper GetMapper()
        {
            BsonMapper mapper = new ();
            mapper.RegisterType(
                value => value.ToString("o", CultureInfo.InvariantCulture),
                bson => DateTime.ParseExact(bson, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
            mapper.RegisterType(
                value => value.ToString("o", CultureInfo.InvariantCulture),
                bson => DateTimeOffset.ParseExact(bson, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
            return mapper;
        }
    }
}
