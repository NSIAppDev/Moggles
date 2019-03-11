using System;

namespace MogglesClient.Logging
{
    public interface IMogglesLoggingService
    {
        void TrackException(Exception ex);
        void TrackException(Exception ex, string customMessage);
        void TrackEvent(string eventName);
    }
}
