using LiteDB;
using System.Globalization;

namespace Blockchain
{
    public class Database
    {
        private static LiteDatabase database;
        
        private readonly string file = "blockchain.db";

        public Database() 
        {
            if (database == null)
            {
                #if DEBUG
                    database = new LiteDatabase(file, GetMapper());
                #else
                    database = new LiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file), GetMapper());
                #endif
            }
        }

        public LiteDatabase Instance()
        {
            return database;
        }

        private BsonMapper GetMapper()
        {
            BsonMapper mapper = new BsonMapper();
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
