using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace Server.Services
{
    public class ContextCollection<T>
    {
        private readonly Dictionary<T, HashSet<HubCallerContext>> _users = new Dictionary<T, HashSet<HubCallerContext>>();

        public void Add(T key, HubCallerContext context)
        {
            lock (_users)
            {
                if (!_users.TryGetValue(key, out HashSet<HubCallerContext> contexts))
                {
                    contexts = new HashSet<HubCallerContext>();
                    _users.Add(key, contexts);
                }

                lock (contexts)
                {
                    contexts.Add(context);
                }
            }
        }

        public IEnumerable<HubCallerContext> GetContexts(T key)
        {
            if (_users.TryGetValue(key, out HashSet<HubCallerContext> contexts))
                return contexts;

            return Enumerable.Empty<HubCallerContext>();
        }

        public bool ContainsKey(T key) => _users.ContainsKey(key);

        public bool ContainsConnectionId(T key, string connectionId)
        {
            foreach (var context in GetContexts(key))
                if (context.ConnectionId == connectionId)
                    return true;

            return false;
        }

        public void Remove(T key, HubCallerContext context)
        {
            lock (_users)
            {
                if (!_users.TryGetValue(key, out HashSet<HubCallerContext> contexts))
                    return;

                lock (contexts)
                {
                    contexts.Remove(context);

                    if (contexts.Count == 0)
                        _users.Remove(key);
                }
            }
        }

        public void RemoveByConnectionId(T key, string connectionId)
        {
            lock (_users)
            {
                if (!_users.TryGetValue(key, out HashSet<HubCallerContext> contexts))
                    return;

                lock (contexts)
                {
                    contexts.Remove(contexts.First(x => x.ConnectionId == connectionId));

                    if (contexts.Count == 0)
                        _users.Remove(key);
                }
            }
        }

        public void RemoveAll(T key)
        {
            lock (_users)
            {
                _users.Remove(key);
            }
        }
    }
}
