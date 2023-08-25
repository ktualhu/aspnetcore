namespace diapp.Extensions;

public static class EmailServiceCollectionExtension
{
    public static IServiceCollection AddEmailSender(
        this IServiceCollection services)
    {
        services
            .AddScoped<IEmailSender, EmailSender>()
            .AddScoped<NetworkClient>()
            .AddSingleton<MessageFactory>()
            .AddScoped(
                provider =>
                    new EmailServerSettings(
                        Host: "smtp.server.com",
                        Port: 25
                    )
            )
            .AddSingleton(typeof(IRepository<>), typeof(DbRepository<>))
            .AddSingleton(typeof(IRepository<>), typeof(TestRepository<>));
        return services;
    }
}