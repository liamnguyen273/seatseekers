using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstallSourceValidator : MonoBehaviour
{
    /// <summary>
    /// This is just a simple installer check. It might not work accurately in some devices.
    /// For more accurate and comprehensive solution please check https://github.com/Unity-Technologies/GooglePlayLicenseVerification
    /// </summary>
    private void Awake()
    {
        if (Application.installerName == "com.android.vending" ||
            Application.installerName == "com.google.android.vending" ||
            Application.installerName == "com.android.packageinstaller" ||
            Application.installerName == "com.google.android.packageinstaller" ||
            Application.installMode.ToString() == "Store")
        {
            //LEGIT
        }
        else
        {
            //POSSIBLE FRAUD INSTALL
        }
    }
}
