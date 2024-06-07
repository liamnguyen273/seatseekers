using System;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupOutOfCoin : PopupBehaviour
    {
        public void OnGetMore()
        {
            var popup = GM.Instance.Get<PopupManager>().GetPopup("popup_shop", out PopupBehaviour behaviour);
            Popup.Hide();
            popup.Show();
        }
    }
}