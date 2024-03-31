using UnityEngine;

namespace com.brg.UnityComponents
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