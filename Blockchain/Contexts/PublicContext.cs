﻿using System.Security.Cryptography;

namespace Blockchain.Contexts
{
    public class PublicContext : Context
    {
        public T Get<T>(Guid id)
        {
            return (T)Get(id).Object;
        }

        public List<T> Get<T>()
        {
            return Chain.Query().Where(q => q.ObjectType == typeof(T).ToString()).Select(x => (T)x.Object).ToList();
        }

        public new Guid Add<T>(T obj, RSAParameters key) => Add<T>(obj, key);
    }
}
