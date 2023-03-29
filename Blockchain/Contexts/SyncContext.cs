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

        public bool Sync(List<List<Link>> linkSets)
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
                    Transfer(GetLastId().Value);
                    SetSynced();
                }
            } else
            {
                SetSynced();
            }
            
            return true;
        }

        public new Guid? GetLastId() => base.GetLastId();
    }
}
