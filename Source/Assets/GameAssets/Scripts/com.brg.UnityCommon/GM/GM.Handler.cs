using System;
using System.Linq;
using com.brg.UnityCommon.UI;
using JSAM;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public partial class GM
    {
        // public void HandleOnAdFreeButton()
        // {
        //     if (Player.GetAdFree())
        //     {
        //         Log.Warn("Already has ad free, this method should not be invoked in the first place.");
        //     }
        //     else
        //     {
        //         var popup = Popups.GetPopup<PopupBehaviourNoAds>(out var behaviour);
        //         popup.Show();
        //     }
        // }
        
        public Transform ResolveFlyerTarget(string item)
        {
            return item switch
            {
                _ => null
            };
        }
        
        public void ResolveAnimateAddItems(string[] items, int[] counts, bool usePopupCongrats, float delay = 0f)
        {
            var popup = Popups.GetPopup<PopupBehaviourNotify>(out var behaviour);
            
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                int i = -1;
                var actions = items.Select<string, Action>(x =>
                {
                    ++i;
                    return () => Player.AddResource(x, counts[i], true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                
                Effects.PlayFlyThings(popup.transform.position, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, false);
                }, 160, delay: delay);

                AudioManager.PlaySound(Sounds.PositiveFeedback);
            };

            if (usePopupCongrats)
            {
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }

        public void ResolveAnimateAddItems(Vector3 from, string[] items, bool usePopupCongrats)
        {
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                var actions = items.Select<string, Action>(x =>
                {
                    return () => Player.AddResource(x, 1, true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                var counts = Enumerable.Repeat(1, items.Length).ToArray();
                Effects.PlayFlyThings(from, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, false);
                }, 160);
            };

            if (usePopupCongrats)
            {
                var popup = Popups.GetPopup<PopupBehaviourNotify>(out var behaviour);
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }
        
        public void ResolveAnimateAddItems(Vector3 from, string[] items, int[] counts, bool usePopupCongrats, float delay = 0f)
        {
            Action resolveAction = () =>
            {
                var sprites = items.Select(x => Data.GetResourceIcon(x)).ToArray();
                int i = -1;
                var actions = items.Select<string, Action>(x =>
                {
                    ++i;
                    return () => Player.AddResource(x, counts[i], true, true);
                }).ToArray();
                var targets = items.Select(ResolveFlyerTarget).ToArray();
                
                Effects.PlayFlyThings(from, sprites, counts, targets, actions, () =>
                {
                    Player.RequestSaveData(true, false);
                }, 160, delay: delay);

                AudioManager.PlaySound(Sounds.PositiveFeedback);
            };

            if (usePopupCongrats)
            {
                var popup = Popups.GetPopup<PopupBehaviourNotify>(out var behaviour);
                behaviour.OnHideStart(resolveAction);
                popup.Show();
            }
            else
            {
                resolveAction.Invoke();
            }
        }
    }
}