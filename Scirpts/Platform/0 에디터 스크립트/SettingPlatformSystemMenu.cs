#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

//빌드할 때마다 일일이 지정해야되는게 너무 귀찮아서 만든 커스텀 스크립트임.
//나중에 시간되면 에디터 뷰를 하나 만들던가 해야겠음.
//일단 지금은 코드상에서 때려박는 방식으로 가고 나중에 안드로이드 설정파일, IOS 설정파일 등 
//OS관련 설정파일을 만들어서 관리하는 방식을 생각해야됨.(각 함수를 보듯이 값만 다를 뿐 처리방식이 완전히 동일하다.)
//현재 AOS일 때 구글 스토어와 원스토어에 관한 xml파일 처리는 아직 되어있지 않음. 이걸 해야 좀 편한데 시간이 좀 걸림.
//(아 씨발 시간 걸리는게 왜 이리 많아 ㅡㅡ... 시간 좀 달라고!!!! 빼애애애애애애애애애애액)
//어느정도 윤곽이 잡혀있긴한데 가장 중요한 것은 설정을 하지 않고 빌드를 할 경우 대응책이 없다는 것이다.
//이 부분에 대해서는 자세히 알아보진 않았지만 5.6이상버전부터는 빌드 버튼 클릭시 사전에 호출할 수 있다는 함수가 있다는 것을 알게 되었으나
//그 이전에 대해서는 지원하는게 아직 없는 것 같아보인다. 좀 더 찾아봐야겠음.
//Init함수에서 PlatformStoreManager.SetCurStore(store); 로 현재 프로젝트를 설정하는데 이거 빌드를 한뒤 앱으로 실행하면 동작이 제대로 안된다.
//왜냐하면 apk를 빌드한 뒤에는 빌드정보를 참고해야하는데 그게 없어서 그런 것이다. 실행할 때 앱에서 빌드 정보를 참고할 수 있도록 구현해야한다.
//지금은 시간이 없는 관계로 기능만 정의해두기로 함.
public class SettingPlatformSystemMenu
{
    [MenuItem("Edit/Store/BuildSetting")]
    private static void BuildSettingToGoogle()
    {
        EditorWindow.GetWindow<SettingPlatformSystemWindow>(
            false, 
            "BuildSetting", 
            true).Show();
    }
}
#endif