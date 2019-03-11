using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MogglesClient.PublicInterface;

namespace MogglesClient.Logging
{
    public class TelemetryClientService: IMogglesLoggingService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;

        public TelemetryClientService(IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
            var instrumentationKey = _mogglesConfigurationManager.GetInstrumentationKey();
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                _telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));
            }
        }

        public void TrackException(Exception ex)
        {
            _telemetryClient?.TrackException(ex, GetProperties());
        }

        public void TrackException(Exception ex, string customMessage)
        {
            var props = GetProperties();
            props.Add("CustomMessage", customMessage);
            _telemetryClient?.TrackException(ex, props);
        }

        public void TrackEvent(string eventName)
        {
            _telemetryClient?.TrackEvent(eventName, GetProperties());
        }

        private Dictionary<string, string> GetProperties()
        {
            return new Dictionary<string, string> { { "Application", _mogglesConfigurationManager.GetApplicationName() }, { "Environment", _mogglesConfigurationManager.GetEnvironment() } };
        }
    }
}
