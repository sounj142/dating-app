namespace API.SignalR
{
    public interface IPresenceTracker
    {
        void AddConnection(string userName, string connectionId);
        bool IsOnline(string userName);
        void RemoveConnection(string userName, string connectionId);
        int TotalConnections(string userName);
    }
}