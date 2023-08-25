namespace diapp.Extensions;

public static class MessageServiceExtension
{
    public static IServiceCollection AddSendMessage(
        this IServiceCollection services)
    {
        services
            .AddScoped<IMessageSender, MessageSender>()
            .AddScoped<IMessageSender, EmailSender>()
            .AddScoped<IMessageSender, FacebookSender>();
        return services;

    }
}