using System.Collections.Generic;

namespace API.SignalR
{
    public interface IPresenceTracker
    {
        void UserConnected(string userName, string connectionId);
        bool IsOnline(string userName);
        void UserDisconnected(string userName, string connectionId);
        int TotalConnections(string userName);
        IList<string> GetConnections(string userName);
    }
}