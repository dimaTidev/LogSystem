//https://forum.unity.com/threads/player-log-file-location-changed-is-there-an-option-in-settings-somewhere-to-change-it-back.500955/#post-6257543

using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DimaTi.Debugging
{
    public class LogAnywhere : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [SerializeField] GameObject[] recordIndicators = null;
        [SerializeField] bool isClearFileInStart = true;
        [SerializeField] RuntimePlatform[] platformWork = new RuntimePlatform[] 
        {
            RuntimePlatform.Android
        };
        [SerializeField] LogType[] filter = new LogType[] 
        {
            LogType.Error,
            LogType.Exception,
            LogType.Assert,
            LogType.Log,
            LogType.Warning
        };
        [SerializeField] string filename = "log_record";
        string path = "";

        void OnEnable()
        {
            if (!CheckPlatform()) return;
                Application.logMessageReceived += Log;
            if (recordIndicators != null && recordIndicators.Length > 0)
                for (int i = 0; i < recordIndicators.Length; i++)
                    recordIndicators[i].SetActive(true);
        }
        void OnDisable()
        {
            if (!CheckPlatform()) return;
            Application.logMessageReceived -= Log;
            if (recordIndicators != null && recordIndicators.Length > 0)
                for (int i = 0; i < recordIndicators.Length; i++)
                    recordIndicators[i].SetActive(false);
        }

        void Awake()
        {
            if (!CheckPlatform()) return;
            path = Application.persistentDataPath + "/" + filename + ".txt";
            if(isClearFileInStart)
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            if (path == "")
            {
                /*string d = System.Environment.GetFolderPath(
                  System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
                System.IO.Directory.CreateDirectory(d);*/
                //filename = d + "/my_happy_log.txt";
                path = Application.persistentDataPath + "/" + filename + ".txt";
            }

            for (int i = 0; i < filter.Length; i++)
            {
                if(type == filter[i])
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append((int)type);
                    sb.Append("#");
                    sb.Append(Time.realtimeSinceStartup);
                    sb.Append("#");
                    sb.Append(logString);
                    sb.Append("#");
                    sb.Append(stackTrace);
                    sb.Append("$");
                    try
                    {
                       // System.IO.File.AppendAllText(path, "NewError: " + logString + "\n" + stackTrace);
                        System.IO.File.AppendAllText(path, sb.ToString());
                    }
                    catch { }
                    break;
                }
            }
        }

        bool CheckPlatform()
        {
            RuntimePlatform currentPlatform = Application.platform;
            
            for (int i = 0; i < platformWork.Length; i++)
            {
               // Debug.Log("Current Platform = " + currentPlatform.ToString() + "/" + platformWork[i].ToString() + "/deb = " + Debug.isDebugBuild);
                if (currentPlatform == platformWork[i])
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }


#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(LogAnywhere))]
        public class LogAnywhere_editor : Editor
        {
            LogAnywhere scr;

            void OnEnable()
            {
                scr = target as LogAnywhere;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                GUILayout.BeginVertical("Helpbox");
                if (GUILayout.Button("Open persistentDataPath"))
                {
                    string itemPath = Application.persistentDataPath;
                    itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                    System.Diagnostics.Process.Start("explorer.exe", itemPath);
                }
                GUILayout.EndVertical();
            }
        }
#endif
}

