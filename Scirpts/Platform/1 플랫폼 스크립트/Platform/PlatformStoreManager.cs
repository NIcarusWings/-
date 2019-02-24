using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class PlatformStoreManager : CustomInstance<PlatformStoreManager>
{
    private const string cExtraFileName = "PlatformExtraFile";

    private static PlatformProvider.eStore sStore = PlatformProvider.eStore.None;

    private PlatformProvider mPlatformProvider = null;
    
    public static bool IsCorrespondPlatform()
    {
        return 
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;
    }

    public static RuntimePlatform GetRuntimePlatform()
    {
        return Application.platform;
    }
    
    void Awake()
    {
        if (Instance == this)
			return;

		if (Instance != null)
		{
			enabled = false;
			Destroy(this);
			return;
		}

		DontDestroyOnLoad(this);

        Instance = this;        
    }
    
    public void CreateProvider()
    {
        //게임은 하나의 스토어에서만 실행됨.
        if (mPlatformProvider == null)
        {
            CreateProvider(sStore);
        }
    }

    public void Operation(
        PlatformProvider.eService service,
        PlatformProvider.eWork work,
        Action<bool> action,
        params string[] extraData)
    {
        if (mPlatformProvider != null)
        {
            mPlatformProvider.Operation(
                service,
                work,
                action,
                extraData);
        }
    }

    //게스트 일 경우 로그인시키는 것을 보류하기로 함.
    //경우의 수가 많음. 안드로이드라고 해도 구글뿐만 아니라 카카오가 될 수도 있고 페이스북이 될 수도 있음.
    //인앱 결재때문에 디폴트로 구글로 로그인을 하면 된다고 생각하지만 인앱 결재가 안들어가는 게임일 경우 문제가 발생함.
    //따라서 로그인이 되어있는지 안되어있는지 체크를 해보고 안되어있으면 로그인을 시킨 다음 로그인이 성공적으로 되면
    //Operation함수를 호출하는 것이 맞음.
    public void Operation(
        PlatformProvider.eSocial social,
        PlatformProvider.eService service,
        PlatformProvider.eWork work,
        Action<bool> action,
        params string[] extraData)
    {
        if (mPlatformProvider != null && IsCorrespondSocial(social))
        {
            if (GameManager.IsServiceMode())
            {
                mPlatformProvider.Operation(social, service, work, action, extraData);
            }
        }
    }

    public string GetStringData(
        //PlatformProvider.eSocial social,
        //PlatformProvider.eService service,
        PlatformProvider.eWork work,
        params string[] extraData)
    {
        //if (IsGuest(social)) return null;
        if (GameManager.IsServiceMode())
        {
            //return mProviderStore[social].GetStringData(service, work, extraData);
            return mPlatformProvider.GetStringData(
                mPlatformProvider.GetSocialByStore(),
                work,
                extraData);
        }
        else
        {
            return null;
        }
    }

    public void OutputAchievements(out SocialAchievement[] achievements)
    {
        OutputAchievements(
            mPlatformProvider.GetSocialByStore(),
            out achievements);
    }
    
    public void OutputAchievements(PlatformProvider.eSocial social, out SocialAchievement[] achievements)
    {
        mPlatformProvider.OutputAchievements(
            social,
            out achievements);
    }

    public void OutputAchievement(SocialAchievement.eAchievementKey key, out SocialAchievement achievement)
    {
        OutputAchievement(
            mPlatformProvider.GetSocialByStore(),
            key, 
            out achievement);
    }

    public void OutputAchievement(PlatformProvider.eSocial social, SocialAchievement.eAchievementKey key, out SocialAchievement achievement)
    {
        mPlatformProvider.OutputAchievement(
            social,
            key, 
            out achievement);
    }

    public void Release()
    {
        GC.Collect();
    }
    
    private bool IsCorrespondSocial(PlatformProvider.eSocial social)
    {
        return
            social != PlatformProvider.eSocial.None ||
            social != PlatformProvider.eSocial.Guest;
    }
    
    private void CreateProvider(PlatformProvider.eStore store)
    {
        TextAsset textAsset = Resources.Load("Extra/" + cExtraFileName) as TextAsset;

        if (textAsset != null)
        {
            SettingClassList.ExtraSetting extraSetting =
                (SettingClassList.ExtraSetting)JsonUtility.FromJson(
                    textAsset.text,
                    typeof(SettingClassList.ExtraSetting));

            if (extraSetting != null)
            {
                mPlatformProvider = new PlatformProvider(extraSetting.Store);

                sStore = extraSetting.Store;
            }
        }
    }

    public void OpenUpdatingView()
    {
        switch (GetCurStore())
        {
            case PlatformProvider.eStore.Google:
                Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.bundleIdentifier);
                break;

            case PlatformProvider.eStore.OneStore:
                Application.OpenURL("http://onesto.re/0000719329");
                break;

            case PlatformProvider.eStore.IOS:
                PopupManager.Instance.OpenNotificationMenu("NOTICE_UPDATE_APP", null, CallbackEndGame);
                break;
        }
    }

    private void CallbackEndGame()
    {
        Application.Quit();
    }

    public static PlatformProvider.eStore GetCurStore()
    {
        return sStore;
    }

    public PlatformProvider.eSocial GetCurSocial()
    {
        return mPlatformProvider.GetCurSocial();
    }

    //앱 스토어와 연관되서 소셜이 들어갈 경우, 앱 스토어와 소셜에 의해 ID를 구분해야될 경우가 생김
    //문장을 만들어보면 [구글 앱 스토어에 올라가고 구글 소셜을 사용하는 유저 ID의 식별자를 만든다]라는 형식을 토대로 만든 함수임.
    public string GetIdentifierID()
    {
        string value = null;

        switch (GetCurStore())
        {
            case PlatformProvider.eStore.Google://구글 앱 스토어에 올라가고
                switch (GetCurSocial())
                {
                    case PlatformProvider.eSocial.Google://구글 소셜을 사용하는
                        value = "G_";//유저 ID의 식별자
                        break;

                    case PlatformProvider.eSocial.Kakao:
                        value = "K_";
                        break;

                    case PlatformProvider.eSocial.Facebook:
                        value = "F_";
                        break;
                }
                break;

            case PlatformProvider.eStore.OneStore://원 스토어에 올라가고(원 
                value = "O_";
                break;

            case PlatformProvider.eStore.IOS://IOS 앱 스토어에 올라가고
                value = "I_";
                break;
        }

        return value;
    }

    public static string GetExtraFileName()
    {
        return cExtraFileName;
    }
}
