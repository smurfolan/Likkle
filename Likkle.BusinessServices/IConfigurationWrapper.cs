namespace Likkle.BusinessServices
{
    public interface IConfigurationWrapper
    {
        bool AutomaticallyCleanupGroupsAndAreas { get; }
        string GoogleApiKeyForReverseGeoCoding { get; }
        string GoogleMapsApiRoot { get; }
        string NumverifyApiRoot { get; }
        string NumverifyApiKey { get; }
        int PersonWalkingSpeedInKmh { get; }
        bool MailSupportOnException { get; }
        string SupportEmail { get; }
        string SmtpClientHost { get; }
        string SupportEmailPassword { get; }
        string HostingEnvironment { get; }
    }
}
