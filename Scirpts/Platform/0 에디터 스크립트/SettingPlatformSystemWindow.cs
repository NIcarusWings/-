#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SettingPlatformSystemWindow : EditorWindow
{
    private SettingClassList.CCommonOtherSetting mCCommonOtherSetting;
    private SettingClassList.ExtraSetting mExtraSetting;

    private Dictionary<PlatformProvider.eStore, int> mBuildCount;
    private Dictionary<PlatformProvider.eStore, string[]> mSourceFilePath;

    private string mDestFilePath = null;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (mBuildCount == null)
        {
            mBuildCount = new Dictionary<PlatformProvider.eStore, int>();
        }

        if (mSourceFilePath == null)
        {
            mSourceFilePath = new Dictionary<PlatformProvider.eStore, string[]>();
        }

        InitField();
    }

    private void InitField()
    {
        string data = null;

        if (mCCommonOtherSetting == null)
        {
            mCCommonOtherSetting = new SettingClassList.CCommonOtherSetting();
        }

        if (EditorPrefs.HasKey(SettingClassList.CCommonOtherSetting.GetUniqueKey()))
        {
            data = EditorPrefs.GetString(SettingClassList.CCommonOtherSetting.GetUniqueKey());
        }

        if (data == null)
        {
            mCCommonOtherSetting.Init(
                PlayerSettings.bundleIdentifier, 
                PlayerSettings.bundleVersion,
                #if UNITY_ANDROID
                PlayerSettings.Android.bundleVersionCode.ToString()
                #elif UNITY_IOS
                PlayerSettings.iOS.buildNumber
                #else
                null
                #endif
                );
        }
        else
        {
            mCCommonOtherSetting.Init(data);
        }
        
    }
    
    private void OnGUI()
    {
        if (mCCommonOtherSetting == null)
        {
            Init();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Store", GUILayout.Width(100f));
        mCCommonOtherSetting.Store = (PlatformProvider.eStore)EditorGUILayout.EnumPopup(mCCommonOtherSetting.Store);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("BundleIdentifier", GUILayout.Width(100f));
        mCCommonOtherSetting.BundleIdentifier = EditorGUILayout.TextField(mCCommonOtherSetting.BundleIdentifier);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Version", GUILayout.Width(100f));
        mCCommonOtherSetting.Version = EditorGUILayout.TextField(mCCommonOtherSetting.Version);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("VersionCode", GUILayout.Width(100f));
        mCCommonOtherSetting.VersionCode = EditorGUILayout.TextField(mCCommonOtherSetting.VersionCode);
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Setting", GUILayout.Width(100f)))
        {
            SettingPlatformStore(mCCommonOtherSetting);
            Save();

            AssetDatabase.Refresh();
        }
    }

    private void SettingPlatformStore(SettingClassList.CCommonOtherSetting cCommonOtherSetting)
    {
        SettingField(cCommonOtherSetting.Store);
        SettingFile(cCommonOtherSetting.Store);
        SettingToOtherSettings(cCommonOtherSetting);
    }
    
    private void SettingField(PlatformProvider.eStore store)
    {
        if (mBuildCount.ContainsKey(store))
        {
            //++sBuildCount[store];
        }
        else
        {
            mBuildCount.Add(store, 0);
        }

        if (!mSourceFilePath.ContainsKey(store))
        {
            SetFileListBy(store);
        }

        //https://forum.unity3d.com/threads/how-to-get-the-current-target-platform.459559/
        mDestFilePath = Application.dataPath + "/Plugins/" + EditorUserBuildSettings.activeBuildTarget + "/AndroidManifest.xml";
    }
    
    private void SettingFile(PlatformProvider.eStore store)
    {
        if (mSourceFilePath[store] != null)
        {
            if (File.Exists(mDestFilePath))
            {
                File.Delete(mDestFilePath);
            }

            File.Copy(mSourceFilePath[store][0], mDestFilePath);
        }
    }

    //File -> Builld Setting -> Player Setting -> OtherSetting 에서 설정하는 부분.
    private void SettingToOtherSettings(SettingClassList.CCommonOtherSetting cCommonOtherSetting)
    {
        //http://answers.unity3d.com/questions/455324/how-to-change-bundle-identifier.html
        PlayerSettings.bundleIdentifier = cCommonOtherSetting.BundleIdentifier;
        PlayerSettings.bundleVersion = cCommonOtherSetting.Version;

        //스토어마다 번들의 버전코드를 설정하는 방법이 다르다.!!!!
        //현재 파악된 것은 AOS 와 IOS 뿐이고 나머지는 아직 해본 적이 없어서 모르겠음.
        //플레이어 셋팅에도 없어서 클래스 내부를 살펴보다가 우연히 안드로이드 라는 걸 발견한 뒤 bundleVersionCode를 쳐보니 있어서 알게 된거임.
        switch (cCommonOtherSetting.Store)
        {
            case PlatformProvider.eStore.Google:
            case PlatformProvider.eStore.OneStore:
                //IOS와 달리 리턴 값이 정수형이기 때문에 문자열로 교체하는 방식을 사용해야함.
                PlayerSettings.Android.bundleVersionCode = int.Parse(cCommonOtherSetting.VersionCode);
                break;

            case PlatformProvider.eStore.IOS:
                //AOS와 달리 리턴 값이 문자열이기 때문에 문자열로 교체하는 방식을 사용하되 자리수를 맞추는 작업이 추가적으로 들어감.
                PlayerSettings.iOS.buildNumber = cCommonOtherSetting.VersionCode;
                break;
        }
    }

    private void Save()
    {
        EditorPrefs.SetString(SettingClassList.CCommonOtherSetting.GetUniqueKey(), mCCommonOtherSetting.ToString());

        SaveExtraFile();
    }

    private void SaveExtraFile()
    {
        if (mExtraSetting == null)
        {
            mExtraSetting = new SettingClassList.ExtraSetting();
        }

        mExtraSetting.Store = mCCommonOtherSetting.Store;
        
        if (!Directory.Exists(GetExtraDirectoryPath()))
        {
            Directory.CreateDirectory(GetExtraDirectoryPath());
            File.Create(GetExtraFileFullPath());
        }
        
        File.WriteAllText(GetExtraFileFullPath(), mExtraSetting.ToString());        
    }

    private void SetFileListBy(PlatformProvider.eStore store)
    {
        string[] fileList = null;

        switch (store)
        {
            case PlatformProvider.eStore.Google:
                fileList = new string[]
                {
                    Application.dataPath + "/Editor/Plugins/" + store.ToString() + "/AndroidManifest.xml"
                };
                break;

            case PlatformProvider.eStore.OneStore:
                fileList = new string[]
                {
                    Application.dataPath + "/Editor/Plugins/" + store.ToString() + "/AndroidManifest.xml"
                };
                break;
        }

        mSourceFilePath.Add(store, fileList);
    }


    public static string GetExtraDirectoryPath()
    {
        return Application.dataPath + "/Resources/Extra";
    }

    public static string GetExtraFileFullPath()
    {
        return Application.dataPath + "/Resources/Extra/" + PlatformStoreManager.GetExtraFileName() + ".txt";
    }
}
#endif