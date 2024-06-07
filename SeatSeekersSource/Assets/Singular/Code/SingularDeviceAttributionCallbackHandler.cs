using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public interface SingularDeviceAttributionCallbackHandler {
    void OnSingularDeviceAttributionCallback(Dictionary<string, object> attributionInfo);
}
