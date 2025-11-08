namespace ChatService.Application.Hub.HubModels
{
    public class PersonInfo
    {
        public string userName { get; set; }
        public string password { get; set; }
    }
    public class User
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string connId { get; set; } //signalrId

        public User(Guid someId, string someName, string someConnId)
        {
            id = someId;
            name = someName;
            connId = someConnId;
        }
    }
}
