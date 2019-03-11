using System.Collections.Generic;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public interface IMogglesFeatureToggleProvider
    {
        List<FeatureToggle> GetFeatureToggles();
    }
}
