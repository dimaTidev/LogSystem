//#define UI_LOG_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.Debugging
{
    public static class LogUI_enabler
    {
#if UI_LOG_SYSTEM || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (LogUIManager.instance)
                    return;

                GameObject prefab = Resources.Load<GameObject>("LogSystem/Canvas_LogSystem");
                if (prefab)
                    MonoBehaviour.Instantiate(prefab);
            }
        }
#endif
    }
}
