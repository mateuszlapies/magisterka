using Blockchain.Model;

namespace Blockchain.Contexts
{
    public class SyncContext : TempContext
    {
        public (bool, List<Link>) Get(Guid? id)
        {
            List<Link> links = new();
            Link lastLink = GetLastLink();
            if (lastLink == null)
            {
                return (true, links);
            }

            if (id.HasValue)
            {
                Link link = base.Get(id.Value);
                if (link == null)
                {
                    return (false, null);
                }

                if (lastLink.Id == link.Id)
                {
                    return (true, links);
                }

                while (lastLink.Id != link.Id)
                {
                    links.Add(lastLink);
                    lastLink = base.Get(lastLink.LastId.Value);
                }

                return (true, links);
            }
            else
            {
                return (true, Get());
            }
        }

        public void Sync(List<List<Link>> linkSets)
        {
            if (linkSets.Count > 0)
            {
                int success = 0;
                int failure = 0;

                List<Link> successList = null;

                foreach (var links in linkSets)
                {
                    Temp.DeleteAll();
                    Temp.InsertBulk(links);
                    CalculateLastLink();
                    if (Verify())
                    {
                        success++;
                        if (successList == null || links.Count > successList.Count)
                        {
                            successList = links;
                        }
                    } else
                    {
                        failure++;
                    }
                }

                if (success > failure)
                {
                    Temp.DeleteAll();
                    Temp.InsertBulk(successList);
                    CalculateLastLink();
                    Transfer();
                    SetSynced();
                }
            } else
            {
                SetSynced();
            }
        }

        private void Transfer()
        {
            List<Link> links = new();
            var lastChainId = GetLastChainId();
            var link = Temp.Query().Where(q => q.LastId == lastChainId).Single();
            while (link != null && ((link.LastId == null && lastChainId == null) || (link.Lock != null && link.Lock.Confirmed)))
            {
                links.Add(link);
                if (link.Lock != null)
                {
                    link = Temp.Query().Where(q => q.Id == link.Lock.NextId).SingleOrDefault();
                } else
                {
                    link = null;
                }
            }

            if (links.Count > 0)
            {
                Chain.Insert(links);
                Temp.DeleteMany(q => links.Contains(q));
            }
        }

        public new static Guid? GetLastId() => TempContext.GetLastId();
        public new void Clear() => base.Clear();
    }
}
