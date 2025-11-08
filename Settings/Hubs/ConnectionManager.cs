using System.Collections.Concurrent;

namespace ChatService.Settings.Hubs
{
    public static class ConnectionManager
    {
        // Maps connection IDs → usernames
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public static void Add(string connectionId, string userName)
        {
            _connections[connectionId] = userName;
        }

        public static void Remove(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public static string? GetUser(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var userName);
            return userName;
        }

        public static IReadOnlyDictionary<string, string> AllConnections => _connections;
    }
}