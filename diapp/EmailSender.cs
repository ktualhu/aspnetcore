namespace diapp;

public class EmailSender : IEmailSender, IMessageSender
{
    private readonly NetworkClient _client;
    private readonly MessageFactory _factory;

    public EmailSender(MessageFactory factory, NetworkClient client)
    {
        _factory = factory;
        _client = client;
    }
    
    public void SendEmail(string username)
    {
        var email = _factory.Create(username);
        _client.SendEmail(email);
        Console.WriteLine($"Email sent to {username}");
    }

    public string SendMessage(string message) => $"New email message: {message}";
}

public class MessageSender : IMessageSender
{
    public string SendMessage(string message) => $"New message: {message}";
}

public class FacebookSender : IMessageSender
{
    public string SendMessage(string message) => $"New Facebook message: {message}";
}