using UnityEngine;

namespace com.brg.UnityCommon.UI
{
    public class WaitScreenHelper : MonoBehaviour
    {
        public void StartWait()
        {
            gameObject.SetActive(true);
        }

        public void EndWait()
        {
            gameObject.SetActive(false);
        }
    }
}