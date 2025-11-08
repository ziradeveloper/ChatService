using ChatService.Application.Connection.Model;
using ChatService.Application.Hub.HubModels;
using ChatService.Settings.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Settings.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _ctx;

        public ChatHub(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public static Dictionary<string, string> UserConnections = new(); // userKey → connId
        public static Dictionary<string, (string userKey, string userName)> ConnectionUsers = new(); // connId → (userKey, userName)

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userKey = httpContext?.Request.Query["userKey"].ToString();
            var userName = httpContext?.Request.Query["userName"].ToString();

            if (!string.IsNullOrEmpty(userKey))
            {
                UserConnections[userKey] = Context.ConnectionId;
                Console.WriteLine($"✅ User {userKey} connected with ID {Context.ConnectionId}");

                // 🧠 Fetch person name from database
                var person = await _ctx.Person.FirstOrDefaultAsync(p => p.ConnectionKey == userKey);
                var finalName = person?.Name ?? userName ?? "Unknown";

                // Save both mappings
                ConnectionUsers[Context.ConnectionId] = (userKey, finalName);

                if (person != null)
                {
                    await Clients.Caller.SendAsync("connectionInfo", new
                    {
                        connectionId = Context.ConnectionId,
                        userKey = userKey,
                        userName = person.Name
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        //public override async Task OnDisconnectedAsyncOLD(Exception exception)
        //{
        //    var connection = await _ctx.Connections
        //        .FirstOrDefaultAsync(c => c.SignalrId == Context.ConnectionId);

        //    if (connection != null)
        //    {
        //        Guid personId = connection.PersonId;

        //        _ctx.Connections.RemoveRange(_ctx.Connections.Where(c => c.PersonId == personId));
        //        await _ctx.SaveChangesAsync();

        //        await Clients.Others.SendAsync("userOff", personId);
        //    }

        //    await base.OnDisconnectedAsync(exception);
        //}

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userKey = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userKey != null)
            {
                UserConnections.Remove(userKey);
                ConnectionManager.Remove(Context.ConnectionId);
                Console.WriteLine($"❌ User {userKey} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task authMe(PersonInfo personInfo)
        {
            string currSignalrId = Context.ConnectionId;

            var person = await _ctx.Person
                .FirstOrDefaultAsync(p => p.Username == personInfo.userName && p.Password == personInfo.password);

            if (person != null)
            {
                //Console.WriteLine($"✅ {person.Name} logged in (SignalR ID: {currSignalrId})");

                var connection = new Connections
                {
                    Id = Guid.NewGuid(),
                    PersonId = person.Id,
                    SignalrId = currSignalrId,
                    TimeStamp = DateTime.UtcNow
                };
                await _ctx.Connections.AddAsync(connection);
                await _ctx.SaveChangesAsync();

                var newUser = new User(person.Id, person.Name, currSignalrId)
                {
                    StableKey = person.ConnectionKey
                };

                //var newUser = new User(person.Id, person.Name, currSignalrId);
                await Clients.Caller.SendAsync("authMeResponseSuccess", newUser);
                await Clients.Others.SendAsync("userOn", newUser);
            }
            else
            {
                await Clients.Caller.SendAsync("authMeResponseFail");
            }
        }

        public async Task reauthMe(Guid personId)
        {
            string currSignalrId = Context.ConnectionId;

            var person = await _ctx.Person.FirstOrDefaultAsync(p => p.Id == personId);
            if (person != null)
            {
                var connection = new Connections
                {
                    Id = Guid.NewGuid(),
                    PersonId = person.Id,
                    SignalrId = currSignalrId,
                    TimeStamp = DateTime.UtcNow
                };
                await _ctx.Connections.AddAsync(connection);
                await _ctx.SaveChangesAsync();

                var user = new User(person.Id, person.Name, currSignalrId);
                await Clients.Caller.SendAsync("reauthMeResponse", user);
                await Clients.Others.SendAsync("userOn", user);
            }
        }

        public async Task reauthUsingStableKey(string stableKey)
        {
            string currSignalrId = Context.ConnectionId;

            var person = await _ctx.Person.FirstOrDefaultAsync(p => p.ConnectionKey == stableKey);
            if (person != null)
            {
                var connection = new Connections
                {
                    Id = Guid.NewGuid(),
                    PersonId = person.Id,
                    SignalrId = currSignalrId,
                    TimeStamp = DateTime.UtcNow
                };
                await _ctx.Connections.AddAsync(connection);
                await _ctx.SaveChangesAsync();

                var user = new User(person.Id, person.Name, currSignalrId)
                {
                    StableKey = person.ConnectionKey
                };

                await Clients.Caller.SendAsync("reauthMeResponse", user);
                await Clients.Others.SendAsync("userOn", user);
            }
            else
            {
                await Clients.Caller.SendAsync("reauthFail");
            }
        }

        public async Task logOut(Guid personId)
        {
            var connections = _ctx.Connections.Where(c => c.PersonId == personId);
            _ctx.Connections.RemoveRange(connections);
            await _ctx.SaveChangesAsync();

            await Clients.Caller.SendAsync("logoutResponse");
            await Clients.Others.SendAsync("userOff", personId);
        }

        public async Task getOnlineUsers()
        {
            var currPersonId = _ctx.Connections
                .Where(c => c.SignalrId == Context.ConnectionId)
                .Select(c => c.PersonId)
                .FirstOrDefault();

            var onlineUsers = await _ctx.Connections
                .Where(c => c.PersonId != currPersonId)
                .Select(c => new User(
                    c.PersonId,
                    _ctx.Person.Where(p => p.Id == c.PersonId).Select(p => p.Name).FirstOrDefault(),
                    c.SignalrId))
                .ToListAsync();

            await Clients.Caller.SendAsync("getOnlineUsersResponse", onlineUsers);
        }

        //public async Task sendMsg(string connId, string msg)
        //{
        //    await Clients.Client(connId).SendAsync("sendMsgResponse", Context.ConnectionId, msg);
        //}

        //public async Task sendMsg(string connId, string msg)
        //{
        //    // send to the target
        //    await Clients.Client(connId)
        //                 .SendAsync("sendMsgResponse", Context.ConnectionId, msg);
        //    // echo from server so sender doesn't miss events (instead of manual push)
        //    await Clients.Caller
        //                 .SendAsync("sendMsgResponse", Context.ConnectionId, msg);
        //}

        public async Task SendMsg(string targetUserKey, string message)
        {
            // Ensure both users exist
            if (!UserConnections.TryGetValue(targetUserKey, out var targetConnId))
                return;

            // Get sender info
            var senderConnId = Context.ConnectionId;
            string fromName = "Unknown";

            if (ConnectionUsers.TryGetValue(senderConnId, out var senderInfo))
            {
                fromName = senderInfo.userName;
            }

            // Send message to target
            await Clients.Client(targetConnId)
                .SendAsync("sendMsgResponse", senderConnId, message, fromName);

            // Echo message to sender
            await Clients.Caller
                .SendAsync("sendMsgResponse", senderConnId, message, fromName);

            Console.WriteLine($"💬 {fromName} → {targetUserKey}: {message}");
        }

    }
}
