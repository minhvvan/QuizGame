using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public enum Type
    {
        NOADS,
        NOADS_HEART_60,
        HEART_20,
        HEART_60,
        HEART_150,
        HEART_320,
        HEART_450
    }

    public const string kPID_Heart20 = "heart_20";
    public const string kPID_Heart60 = "heart_60";
    public const string kPID_Heart150 = "heart_150";
    public const string kPID_Heart320 = "heart_320";
    public const string kPID_Heart450 = "heart_450";
    public const string kPID_NoAds = "noads";
    public const string kPID_NoAds_Heart60 = "noads_heart_60";
    
    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;

    private void Start()
    {
        if (_storeController == null)
        {
            InitializePurchasing();
        }
    }

    private void InitializePurchasing()
    {
        if (IsInitialize()) return;
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(kPID_Heart20, ProductType.Consumable);
        builder.AddProduct(kPID_Heart60, ProductType.Consumable);
        builder.AddProduct(kPID_Heart150, ProductType.Consumable);
        builder.AddProduct(kPID_Heart320, ProductType.Consumable);
        builder.AddProduct(kPID_Heart450, ProductType.Consumable);
        builder.AddProduct(kPID_NoAds, ProductType.NonConsumable);
        builder.AddProduct(kPID_NoAds_Heart60, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialize()
    {
        return _storeController != null && _storeController.products != null;
    }

    public void BuyProduct(string productId)
    {
        if (IsInitialize())
        {
            Product product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product '{0}'", product.definition.id));
                _storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log(string.Format("Failed to buy product '{0}'", productId));
            }
        }
        else
        {
            Debug.Log(string.Format("Failed to buy product '{0}'", productId));
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _extensionProvider = extensions;
    }
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_Heart20, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 20;
        }
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_Heart60, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 60;
        }
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_Heart150, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 150;
        }
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_Heart320, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 320;
        }        
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_Heart450, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 450;
        }
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_NoAds, StringComparison.Ordinal))
        {
            UserInformations.IsNoAds = true;
        }
        else if (String.Equals(purchaseEvent.purchasedProduct.definition.id, kPID_NoAds_Heart60, StringComparison.Ordinal))
        {
            UserInformations.HeartCount += 60;
            UserInformations.IsNoAds = true;
        }
        else
        {
            Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: '{purchaseEvent.purchasedProduct.definition.id}'");
        }
        
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Failed to process purchase '{product.definition.id}' failureReason: {failureReason}");
    }
}
