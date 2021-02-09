namespace API.Entities
{
    public class Connection
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }

        public Connection() { } //ctor for enitity framework

        public Connection(string connectionId, string userName)
        {
            ConnectionId = connectionId;
            UserName = userName;
        }
    }
}