﻿namespace Likkle.BusinessServices
{
    public interface IConfigurationWrapper
    {
        bool AutomaticallyCleanupGroupsAndAreas { get; }
        string GoogleApiKeyForReverseGeoCoding { get; }
        string GoogleMapsApiRoot { get; }
        string NumverifyApiRoot { get; }
        string NumverifyApiKey { get; }
        int PersonWalkingSpeedInKmh { get; }
    }
}
