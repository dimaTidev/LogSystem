#define UI_LOG_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogUI_enabler
{
#if UI_LOG_SYSTEM || DEVELOPMENT_BUILD
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        if (Application.platform != RuntimePlatform.Android || Application.platform != RuntimePlatform.IPhonePlayer)
            return;

        GameObject prefab = Resources.Load<GameObject>("LogSystem/Canvas_LogSystem");
        if (prefab)
            MonoBehaviour.Instantiate(prefab);
    }
#endif
}
