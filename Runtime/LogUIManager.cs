using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DimaTi.Debugging
{
    [DefaultExecutionOrder(-200)]
    public class LogUIManager : MonoBehaviour
    {
        [Tooltip("if true && (currentPlatform == WindowsEditor || OSXEditor) - then in Awake will Destroy this gameObject")]
        [SerializeField] bool removeInEditor = false;
        [Tooltip("if true && if not developer build && (currentPlatform != WindowsEditor & OSXEditor) - then in Awake will disable logger")]
        [SerializeField] bool isDisableLogs = false;
        [SerializeField] LogUI prefab = null;
        List<LogUI> logs = new List<LogUI>();
        [SerializeField] GameObject panel_allLogs = null;
        [SerializeField] LogUI logDescription = null;
        [SerializeField] LogUI logSimple = null;
        [SerializeField] Transform content = null;

        [SerializeField] Text[] text_logTypeCounts = null;
        [SerializeField] Text[] text_logTypeCountsSimple = null;
        [SerializeField] Button[] button_filters = null;

        [SerializeField] bool[] filters = null;

#if UNITY_EDITOR
        [SerializeField] TextAsset textAsset = null;
#endif

        void OnEnable()
        {
#if UNITY_EDITOR
            if (textAsset) return;
#endif
            Application.logMessageReceived += Log;
        }
        void OnDisable()
        {
#if UNITY_EDITOR
            if (textAsset) return;
#endif
            Application.logMessageReceived -= Log;
        }

        void Awake()
        {
            if(isDisableLogs)
                if(Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
                {
                    ILogger logger = Debug.unityLogger;
                    logger.logEnabled = Debug.isDebugBuild;
                }

         //   if (!Debug.isDebugBuild) //in Editor this variable all time return true
         //       Destroy(gameObject);

            if(removeInEditor)
                if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                    Destroy(gameObject);

            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
#if UNITY_EDITOR
            if (LoadFromTextAsset(textAsset)) return;
#endif
            //filters = new bool[] { true, true, true };
            RefreshFilters();
        }

        #if UNITY_EDITOR
        bool LoadFromTextAsset(TextAsset textAsset)
        {
            if (!textAsset) return false;

            string allText = textAsset.text;
            string[] blocks = allText.Split("$".ToCharArray());

           

            for (int i = 0; i < blocks.Length; i++)
            {
                string[] logArray = blocks[i].Split("#".ToCharArray());
                if (logArray == null || logArray.Length == 0) continue;
                //Debug.Log("logArray " + i + " = " + logArray.Length);
                //Debug.Log(logArray[0] + "/" + logArray[1] + "/" + logArray[2]);
                // 0 - LogType
                // 1 - Time.realtimeSinceStartup
                // 2 - logString  // can be empty
                // 3 - stackTrace // can be empty
                if (int.TryParse(logArray[0], out int idLogType))
                {
                    float tempTime = 0;
                    if(logArray.Length > 1)
                        float.TryParse(logArray[1].Replace(".", ","), out tempTime);
                    LogImmediatly(logArray.Length > 2 ? logArray[2] : "", logArray.Length > 3 ? logArray[3] : "", (LogType)idLogType, tempTime);
                }
            }
            return true;
        }
#endif

        /// For subscribe  Application.logMessageReceived += Log
        public void Log(string logString, string stackTrace, LogType type) => StartCoroutine(IELog(logString, stackTrace, type));

        IEnumerator IELog(string logString, string stackTrace, LogType type)
        {
            if (!prefab || !content) yield break;
            yield return new WaitForEndOfFrame();
            LogImmediatly(logString, stackTrace, type, Time.realtimeSinceStartup);
        }

        /// Do not subscribe this to 'Application.logMessageReceived += LogImmediatly' it is not safety.
        /// Becouse log can be invoke in 'UI rebuild loop', that means if you change any UI component you get Error.
        /// For avoid this Error call 'IELog()', this IEnumerator wait for next frame and invoke LogImmediatly in outside 'UI rebuild loop'
        void LogImmediatly(string logString, string stackTrace, LogType type, float time)
        {
            if (!prefab || !content) return;
            if (logs == null) logs = new List<LogUI>();
            GameObject go = Instantiate(prefab.gameObject, content);
            go.transform.SetAsFirstSibling();
            go.transform.localScale = Vector3.one;
            LogUI logUI = go.GetComponent<LogUI>();
            logs.Add(logUI);

            TimeSpan timeSpan = TimeSpan.FromSeconds(time);

            logUI.Log(logString, stackTrace, type, timeSpan.ToString("hh':'mm':'ss"));
            logUI.Subscribe_OnClick(Callback_OnClick);


            int idCount = type == LogType.Log ? 0 : type == LogType.Warning ? 1 : 2;
            if (filters != null && idCount >= 0 && idCount < filters.Length && filters[idCount])
            {
                go.SetActive(true);
                if (logSimple)
                    logSimple.Log(logString, stackTrace, type, "");
            }
            else
                go.SetActive(false);

            if (counts != null && idCount >= 0 && idCount < counts.Length)
                counts[idCount]++;

            RefreshCounts();
        }

        public void Callback_OnClick(LogUI log)
        {
            if (logDescription)
            {
                logDescription.Log(log);
                logDescription.gameObject.SetActive(true);
            }
        }

        public void Clear()
        {
            if (logs == null || logs.Count == 0) return;
            for (int i = 0; i < logs.Count; i++)
                if (logs[i])
                    Destroy(logs[i].gameObject);
            logs.Clear();
            if (logSimple) logSimple.Clear();

            RefreshFilters();
        }


        public void Filter(int idFilter)
        {
            if (idFilter < 0 || idFilter >= filters.Length) return;

            filters[idFilter] = !filters[idFilter];
            RefreshFilters();
        }

        int[] counts = new int[3];

        void RefreshFilters()
        {
            int[] counts = new int[3];

            for (int i = 0; i < logs.Count; i++)
            {
                LogUI fLog = logs[i];
                LogType type = fLog.TypeLog;
                if (type == LogType.Log)
                {
                    if (!filters[0])
                        fLog.gameObject.SetActive(false);
                    else
                        fLog.gameObject.SetActive(true);
                    counts[0]++;
                }
                else if (type == LogType.Warning)
                {
                    if (!filters[1])
                        fLog.gameObject.SetActive(false);
                    else
                        fLog.gameObject.SetActive(true);
                    counts[1]++;
                }
                else
                {
                    if (!filters[2])
                        fLog.gameObject.SetActive(false);
                    else
                        fLog.gameObject.SetActive(true);
                    counts[2]++;
                }
            }


            if (filters.Length == button_filters.Length)
                for (int i = 0; i < filters.Length; i++)
                {
                    Color col = button_filters[i].targetGraphic.color;
                    col.a = filters[i] ? 1 : 0.2f;
                    button_filters[i].targetGraphic.color = col;
                }
            this.counts = counts;
            RefreshCounts();
        }
        void RefreshCounts()
        {
            if (counts == null) return;
            if (text_logTypeCounts != null && text_logTypeCounts.Length == counts.Length)
                for (int i = 0; i < text_logTypeCounts.Length; i++)
                    text_logTypeCounts[i].text = counts[i].ToString();

            if (text_logTypeCountsSimple != null && text_logTypeCountsSimple.Length == counts.Length)
                for (int i = 0; i < text_logTypeCountsSimple.Length; i++)
                    text_logTypeCountsSimple[i].text = counts[i].ToString();
        }


        float tempTimeScale;

        public void Button_ShowAllLog()
        {
            if (panel_allLogs) panel_allLogs.gameObject.SetActive(!panel_allLogs.activeSelf);
            if (logSimple) logSimple.Clear();
            RefreshFilters();
            tempTimeScale = Time.timeScale;
            //Time.timeScale = 0;
        }

        public void Hide_AllLog()
        {
            if (panel_allLogs) panel_allLogs.gameObject.SetActive(false);
            Time.timeScale = tempTimeScale;
        }
    }
}
