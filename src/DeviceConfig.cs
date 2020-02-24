namespace GrandStream
{
    public class DeviceConfig
    {
        public string Host { get; }
        public string Password { get; }
        public string Note { get; }

        public DeviceConfig(string host, string password, string note = null)
        {
            Host = host;
            Password = password;
            Note = note;
        }
    }
}