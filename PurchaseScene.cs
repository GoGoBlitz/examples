using Assets.Scripts;
using Assets._Project.Scripts.Misc;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets._Project.Scripts.Scenes
{
    public class PurchaseScene : MonoBehaviour
    {
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Text log;

        private UnityIAP _iapModule;

        void Start()
        {
            UserEventsTracker.TrackEvent("Purchase screen. Start");

            _iapModule = new UnityIAP();
            _iapModule.Init(OnInitializeFailed);

            EventsChannel.General.Subscribe(EventConstants.PurchaseSuccess, (o, o1) =>
            {
                UserEventsTracker.TrackEvent("Purchase screen. Success purchase");

                OnSuccessPurchase();
            });

            EventsChannel.General.Subscribe(EventConstants.PurchaseFailed, (o, o1) =>
            {
                UserEventsTracker.TrackEvent("Purchase screen. Purchase failed:" + o1.ToString());

                OnFailedPurchase((PurchaseFailureReason)o1);
            });
        }

        void Purchase()
        {
            UserEventsTracker.TrackEvent("Purchase screen. Click purchase");

            _iapModule.BuyProduct();
        }

        private void OnInitializeFailed(InitializationFailureReason reason)
        {
            if (log != null)
                log.text = reason.ToString();

            UserEventsTracker.TrackEvent("Purchase screen. OnInitializeFailed:" + reason.ToString());

            purchaseButton.interactable = false;
        }

        private void OnSuccessPurchase()
        {
            SceneManager.LoadScene(Constants.SuccessPurchaseSceneName);
        }

        private void OnFailedPurchase(PurchaseFailureReason reason)
        {
            purchaseButton.interactable = true;
            //do smth?
        }


        public void Back()
        {
            UserEventsTracker.TrackEvent("Purchase screen. Back button clicked");

            SceneManager.LoadScene(Constants.MenuSceneName);
        }
    }
}
