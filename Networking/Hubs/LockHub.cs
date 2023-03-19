using Blockchain.Contexts;
using Blockchain.Model;
using Microsoft.AspNetCore.SignalR;
using Networking.Data.Requests;
using Networking.Data.Responses;

namespace Networking.Hubs
{
    public class LockHub : Hub
    {
        private readonly Context context;

        public LockHub(Context context)
        {
            this.context = context;
        }

        public LockResponse Lock(LockRequest request)
        {
            Link link = context.Get(request.LockId);
            Link last = context.GetLastLink();

            if (link != null && last != null)
            {
                if (link.Id != last.Id)
                {
                    return new LockResponse()
                    {
                        Success = false
                    };
                }
            }

            if (link.Lock != null)
            {
                return new LockResponse()
                {
                    Success = false,
                    LockInsteadId = link.Lock.NextId
                };
            }

            link.Lock = new Lock()
            {
                NextId = request.NextId,
                Owner = request.Owner,
                Expires = DateTime.UtcNow.AddMinutes(1)
            };

            context.Update(link);

            return new LockResponse()
            {
                Success = true
            };
        }
    }
}
