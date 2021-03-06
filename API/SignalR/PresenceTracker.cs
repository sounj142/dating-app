using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace API.SignalR
{
    public class PresenceTracker : IPresenceTracker
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _onlineUsers
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

        public void UserConnected(string userName, string connectionId)
        {
            _onlineUsers.AddOrUpdate(userName,
                new ConcurrentDictionary<string, bool>(new[] { new KeyValuePair<string, bool>(connectionId, true) }),
                (key, currentValue) =>
            {
                currentValue.AddOrUpdate(connectionId, true, (key, value) => true);
                return currentValue;
            });
        }

        public void UserDisconnected(string userName, string connectionId)
        {
            if (_onlineUsers.TryGetValue(userName, out var bag))
            {
                bag.TryRemove(connectionId, out bool _);
            }
        }

        public bool IsOnline(string userName)
        {
            return TotalConnections(userName) > 0;
        }

        public int TotalConnections(string userName)
        {
            if (_onlineUsers.TryGetValue(userName, out var bag))
            {
                return bag.Count;
            }

            return 0;
        }

        public IList<string> GetConnections(string userName)
        {
            if (_onlineUsers.TryGetValue(userName, out var bag))
            {
                return bag.Keys.ToList();
            }
            return new List<string>();
        }
    }
}
