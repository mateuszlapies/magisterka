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
                    if (!Directory.GetCurrentDirectory().Contains("bin"))
                    {
                        database = new LiteDatabase(Path.Combine("bin/Debug/net7.0", file), GetMapper());
                    } else
                    {
                        database = new LiteDatabase(file, GetMapper());
                    }
                #else
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Magisterka");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    database = new LiteDatabase(Path.Combine(path, file), GetMapper());
                #endif

                if (database.CollectionExists("temp"))
                {
                    database.DropCollection("temp");
                }
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
