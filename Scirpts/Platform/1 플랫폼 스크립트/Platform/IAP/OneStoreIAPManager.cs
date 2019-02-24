using UnityEngine;
using IapResponse;
using IapError;
using IapVerifyReceipt;
using System.Collections.Generic;

public class OneStoreIAPManager : IAPManager
{
    private const string cAppID = "OA00719329";

    private AndroidJavaObject iapRequestAdapter = null;

    private IapResponse.Product mCurrentProduct;
    
    public override void Initialize()
    {        
        base.Initialize();
        
        if (currentActivity != null)
        {
            // RequestAdapter를 초기화
            // ---------------------------------
            // 함수 parameter 정리
            // ---------------------------------
            // (1) 콜백을 받을 클래스 이름
            // (2) Activity Context
            // (3) debug 여부
            
            iapRequestAdapter = new AndroidJavaObject("com.onestore.iap.unity.RequestAdapter", this.name, currentActivity, false); //Release
            //iapRequestAdapter = new AndroidJavaObject("com.onestore.iap.unity.RequestAdapter", "IapSample", currentActivity, true); //Debug
        }
    }

    public void Exit()
    {
        if (iapRequestAdapter != null)
        {
            mCurrentProduct = null;
            iapRequestAdapter.Call("exit");
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (iapRequestAdapter != null)
            iapRequestAdapter.Dispose();
    }

    public override void Purchase(PlatformProvider.PlatformData data)
    {
        base.Purchase(data);
        
        RequestProductInfo();
    }

    //------------------------------------------------
    //
    // Command - Callback
    //
    //------------------------------------------------

    public void CommandResponse(string response)
    {
        //Debug.Log("--------------------------------------------------------");
        //Debug.Log("[UNITY] CommandResponse >>> " + response);
        //Debug.Log("--------------------------------------------------------");

        // Parsing Json string to "Reponse" class
        Response data = JsonUtility.FromJson<Response>(response);

        if (mPurchaseState == ePurchaseState.RequestPaymenetItemInfo)
        {
            if (data != null && data.result != null)
            {
                List<IapResponse.Product> productList = data.result.product;
                
                if (productList != null && productList.Count != 0)
                {
                    string purchaseID = mData.PopStringExtraData()[0];

                    mCurrentProduct = null;
                    
                    for (int i = 0; i < productList.Count; ++i)
                    {
                        if (productList[i].id.CompareTo(purchaseID) == 0)
                        {
                            mCurrentProduct = productList[i];
                            break;
                        }
                    }
                    
                    if (mCurrentProduct != null)
                    {
                        RequestPaymenet();
                    }
                }
            }
            else
            {
                Debug.Log("Error Product");
                mData.GetAction()(false);
            }
        }
        
        //Debug.Log(">>> " + data.ToString());
        //Debug.Log("--------------------------------------------------------");
    }

    public void CommandError(string message)
    {
        Debug.Log("--------------------------------------------------------");
        Debug.Log("[UNITY] CommandError >>> " + message);
        Debug.Log("--------------------------------------------------------");

        // Parsing Json string to "Error" class
        Error data = JsonUtility.FromJson<Error>(message);
        Debug.Log(">>> " + data.ToString());
        Debug.Log("--------------------------------------------------------");

        mData.GetAction()(false);
    }

    public void RequestProductInfo()
    {
        // ---------------------------------
        // 함수 parameter 정리
        // ---------------------------------
        // (0) 메소드이름 : 상품정보 조회
        // ---------------------------------
        // (1) 필요시에는 UI 노출
        // (2) appId
        // ----------------------------------
        iapRequestAdapter.Call("requestProductInfo", false, cAppID);
        //iapRequestAdapter.Call ("requestProductInfo", true, "OA00679020"); // UI노출 없이 Background로만 진행
    }

    //------------------------------------------------
    //
    // Payment - Request
    //
    //------------------------------------------------
    public void RequestPaymenet()
    {
        mPurchaseState = ePurchaseState.RequestPaymenet;

        // ---------------------------------
        // 함수 parameter 정리
        // ---------------------------------
        // (0) 메소드이름 : 구매요청
        // ---------------------------------
        // (1) appId
        // (2) productId
        // (3) proudtName
        // (4) tId
        // (5) bpInfo
        // ----------------------------------
        iapRequestAdapter.Call(
            "requestPayment", 
            cAppID, mCurrentProduct.id,
            mCurrentProduct.name, 
            "", "");
    }

    public void VerifyReceipt()
    {
        // ---------------------------------
        // 함수 parameter 정리
        // ---------------------------------
        // (0) 메소드이름 : 구매요청
        // ---------------------------------
        // (1) appId
        // (2) txId
        // (3) signData
        // ----------------------------------
        //iapRequestAdapter.Call ("verifyReceipt", appId, txId, signData);
    }

    //------------------------------------------------
    //
    // Payment - Callback
    //
    //------------------------------------------------

    public void PaymentResponse(string response)
    {
        //Debug.Log("--------------------------------------------------------");
        //Debug.Log("[UNITY] PaymentResponse >>> " + response);
        //Debug.Log("--------------------------------------------------------");

        // Parsing Json string to "Reponse" class
        Response data = JsonUtility.FromJson<Response>(response);
        bool isResult = false;
        //Debug.Log(">>> " + data.ToString());
        //Debug.Log("--------------------------------------------------------");

        mPurchaseState = ePurchaseState.Exit;
        
        if (data != null && data.result != null)
        {
            isResult = (data.result.code.CompareTo("0000") == 0);
        }

        mData.GetAction()(isResult);

        //영수증 체크 안한다고 함.
        // Try ReceiptVerification
        //iapRequestAdapter.Call("verifyReceipt", "OA00679020", data.result.txid, data.result.receipt);
    }

    public void PaymentError(string message)
    {
        //Debug.Log("--------------------------------------------------------");
        //Debug.Log("[UNITY] PaymentError >>> " + message);
        //Debug.Log("--------------------------------------------------------");

        //// Parsing Json string to "Error" class
        //Error data = JsonUtility.FromJson<Error>(message);
        //Debug.Log(">>> " + data.ToString());
        //Debug.Log("--------------------------------------------------------");
        PopupManager.Instance.OpenNotificationMenu("PAY_CANCEL");
        
        mData.GetAction()(false);
    }

    public void ReceiptVerificationResponse(string result)
    {
        Debug.Log("--------------------------------------------------------");
        Debug.Log("[UNITY] ReceiptVerificationResponse >>> " + result);
        Debug.Log("--------------------------------------------------------");

        // Parsing Json string to "VerifyReceipt" class
        VerifyReceipt data = JsonUtility.FromJson<VerifyReceipt>(result);
        Debug.Log(">>> " + data.ToString());
        Debug.Log("--------------------------------------------------------");
    }

    public void ReceiptVerificationError(string message)
    {
        Debug.Log("--------------------------------------------------------");
        Debug.Log("[UNITY] ReceiptVerificationError >>> " + message);
        Debug.Log("--------------------------------------------------------");

        // Parsing Json string to "Error" class
        Error data = JsonUtility.FromJson<Error>(message);
        Debug.Log(">>> " + data.ToString());
        Debug.Log("--------------------------------------------------------");
    }

    // ------------------------------------------------------
}
