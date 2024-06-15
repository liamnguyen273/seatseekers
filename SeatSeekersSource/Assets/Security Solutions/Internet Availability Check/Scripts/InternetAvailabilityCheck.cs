using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetAvailabilityCheck : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _noInternetConnectionPanel;
    [SerializeField] private GameObject _searchInternetConnectionPanel;

    private void Awake()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _canvas.SetActive(true);
            StopGame();
        }
        else
            CheckInternetReachability();
    }

    private void CheckInternetReachability()
    {
        StartCoroutine(CheckInternetReachabilityCoroutine());
    }

    private IEnumerator CheckInternetReachabilityCoroutine()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _canvas.SetActive(true);
                StopGame();
                yield break;
            }

            yield return new WaitForSecondsRealtime(5f);
        }
    }

    private void SearchingInternetReachability()
    {
        StartCoroutine(SearchingInternetReachabilityCoroutine());
    }

    private IEnumerator SearchingInternetReachabilityCoroutine()
    {
        _searchInternetConnectionPanel.SetActive(true);
        _noInternetConnectionPanel.SetActive(false);

        yield return new WaitForSecondsRealtime(2f);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _noInternetConnectionPanel.SetActive(true);
            _searchInternetConnectionPanel.SetActive(false);
        }
        else
        {
            _canvas.SetActive(false);
            _searchInternetConnectionPanel.SetActive(false);
            _noInternetConnectionPanel.SetActive(true);
            StartGame();
            CheckInternetReachability();
        }
    }

    private void StartGame()
    {
        Time.timeScale = 1;
    }

    private void StopGame()
    {
        Time.timeScale = 0;
    }

    // Button Action
    public void OnClickCheckButton()
    {
        SearchingInternetReachability();
    }
}
