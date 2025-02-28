using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private Button restorePurchaseButton;
    
    protected void Start()
    {
        GetComponent<PopupPanelController>().SetTitle("Shop");
    }

    public void OnClickShopItemButton(int index)
    {
        switch (index)
        {
            case 0:
            {
                break;
            }
            case 1:
            {
                AdmobAdsManager.Instance.ShowRewardedAd((Reward reward) =>
                {
                    GameManager.Instance.HeartCount += (int)reward.Amount;
                });
                break;
            }
            case 2:
            {
                break;
            }
            case 3:
            {
                break;
            }
            case 4:
            {
                break;
            }
            case 5:
            {
                break;
            }
            case 6:
            {
                break;
            }
            case 7:
            {
                break;
            }
            case 8:
            {
                break;
            }
        }
    }
}
