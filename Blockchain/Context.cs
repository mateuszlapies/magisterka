using LiteDB;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Blockchain
{
    public class Context
    {
        private readonly LiteDatabase database;
        private readonly ILiteCollection<Link> chain;

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

        public Context()
        {
            database = new LiteDatabase("./blockchain.db", GetMapper());
            chain = database.GetCollection<Link>("chain");
        }

        public Context(string path)
        {
            database = new LiteDatabase(Path.Combine(path, "blockchain.db"), GetMapper());
            chain = database.GetCollection<Link>("chain");
        }

        public void Clear()
        {
            chain.DeleteAll();
        }

        public void Close()
        {
            database.Dispose();
        }

        public Guid Add<T>(T obj, RSAParameters key)
        {
            Link last = GetLastLink();
            Guid id = Guid.NewGuid();
            Link link = new Link()
            {
                Id = id,
                Object = obj,
                ObjectType = obj.GetType().ToString(),
                LastId = last?.Id,
                LastLink = last,
                Signature = null
            };
            link.Signature = Sign(link, key);
            chain.Insert(link);
            return id;
        }

        public T Get<T>(Guid id)
        {
            return (T)chain.FindOne(q => q.Id == id).Object;
        }

        public bool Verify(Guid id)
        {
            Link link = Get(id);
            if (link.LastId != null)
            {
                link.LastLink = Get(link.LastId.Value);
                return Verify(link) && Verify(link.LastId.Value);
            } else
            {
                return Verify(link);
            }
        }

        private Link Get(Guid id)
        {
            return chain.FindOne(q => q.Id == id);
        }

        private string Serialize(Link link)
        {
            string json = JsonSerializer.Serialize(link);
            Console.WriteLine(json);
            return json;
        }

        private Signature Sign(Link link, RSAParameters key)
        {
            string json = Serialize(link);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(key);
                byte[] unsigned = Encoding.ASCII.GetBytes(json);
                byte[] signed = rsa.SignData(unsigned, SHA256.Create());
                Signature signature = new Signature()
                {
                    Hash = Convert.ToBase64String(signed),
                    Owner = Convert.ToBase64String(rsa.ExportRSAPublicKey())
                };
                return signature;
            }
        }

        private bool Verify(Link link)
        {
            Signature hash = link.Signature;
            link.Signature = null;
            string json = Serialize(link);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(hash.Owner), out int bytesRead);
                byte[] unsigned = Encoding.ASCII.GetBytes(json);
                byte[] signed = Convert.FromBase64String(hash.Hash);
                return rsa.VerifyData(unsigned, SHA256.Create(), signed);
            }
        }
            
        private Link GetLastLink()
        {
            Link link = chain.Query().Where(q => q.LastId == null).SingleOrDefault();
            while (link?.LastId != null)
            {
                link = chain.Query().Where(q => q.LastId == link.Id).SingleOrDefault();
            }
            return link;
        }
    }
}