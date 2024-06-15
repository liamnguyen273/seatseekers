using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RealTime
{
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    public int seconds;
    public int milliSeconds;
    public string dateTime;
    public string date;
    public string time;
    public string timeZone;
    public string dayOfWeek;
    public bool dstActive;
}

public class InternetTime : MonoBehaviour
{
    public static event Action<DateTime> OnUpdateDoubleEarningFinishTime = delegate { };
    public static event Action<DateTime> OnLoadDoubleEarningFinishTime = delegate { };
    public static event Action<DateTime> OnCheckOfflineEarning = delegate { };
    public static event Action<DateTime> OnGetPassedTime = delegate { };
    public static event Action<DateTime> OnSaveDateTime = delegate { };

    DateTime now;
    public static InternetTime Instance { get; set; }
    public string _requestURI = "https://www.timeapi.io/api/Time/current/zone?timeZone=Greenwich";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [ContextMenu("TEST")]
    public void Test()
    {
        StartCoroutine(APIRequest(_requestURI, "GET", ""));
    }

    public void GetInternetTime(string caller)
    {
        StartCoroutine(APIRequest(_requestURI, "GET", caller));
    }

    //Method can be "POST" or "GET"
    IEnumerator APIRequest(string URI, string method, string caller)
    {
        var req = new UnityWebRequest(URI, method);
        req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error While Sending: " + req.error);
        }
        else
        {
            string returnString = req.downloadHandler.text;
            RealTime rt = JsonUtility.FromJson<RealTime>(returnString);

            now = DateTime.Parse(rt.dateTime);

            if (caller.Equals("UpdateDoubleEarningFinishTime"))
            {
                OnUpdateDoubleEarningFinishTime(now);
            }
            else if (caller.Equals("LoadDoubleEarningFinishTime"))
            {
                OnLoadDoubleEarningFinishTime(now);
            }
            else if (caller.Equals("CheckOfflineEarning"))
            {
                OnCheckOfflineEarning(now);
            }
            else if (caller.Equals("GetPassedTime"))
            {
                OnGetPassedTime(now);
                //Debug.Log("GetPassedTime: " + now.ToString() );
            }
            else if (caller.Equals("SaveDateTime"))
            {
                OnSaveDateTime(now);
            }
        }
    }
}
