using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DimaTi.UI;

namespace DimaTi.Debugging
{
    [DefaultExecutionOrder(-200)]
    public class LogUIManager : MonoBehaviour
    {
        [Tooltip("if true && (currentPlatform == WindowsEditor || OSXEditor) - then in Awake will Destroy this gameObject")]
        [SerializeField] bool removeInEditor = false;
        [Tooltip("if true && if not developer build && (currentPlatform != WindowsEditor & OSXEditor) - then in Awake will disable logger")]
        [SerializeField] bool isDisableLogs = false;
        [SerializeField] bool isUseRealtimeRefresh; //realtime refresh list or refresh only when open viewer
        [SerializeField] LogUI prefab = null;
       // List<LogUI> logs = new List<LogUI>();
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

            if (scrollReusable)
                scrollReusable.Set_Count(1);
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
                    LogImmediately(logArray.Length > 2 ? logArray[2] : "", logArray.Length > 3 ? logArray[3] : "", (LogType)idLogType, tempTime);
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
            LogImmediately(logString, stackTrace, type, Time.realtimeSinceStartup);
        }

        List<LogData> logsData_toShow = new List<LogData>();
        List<LogData> logsData = new List<LogData>();

        public struct LogData
        {
            public string 
                logString, 
                stackTrace;
            public LogType type;
            public float time;

            public LogData(string logString, string stackTrace, LogType type, float time)
            {
                this.logString = logString;
                this.stackTrace = stackTrace;
                this.type = type;
                this.time = time;
            }
        }

        /// Do not subscribe this to 'Application.logMessageReceived += LogImmediatly' it is not safety.
        /// Becouse log can be invoke in 'UI rebuild loop', that means if you change any UI component you get Error.
        /// For avoid this Error call 'IELog()', this IEnumerator wait for next frame and invoke LogImmediatly in outside 'UI rebuild loop'
        void LogImmediately(string logString, string stackTrace, LogType type, float time)
        {
            logsData.Add(new LogData(logString, stackTrace, type, time));
           // logsData_toShow.Add(new LogData(logString, stackTrace, type, time));

            RefreshReusableAll();

          // if (!prefab || !content) return;
          // if (logs == null) logs = new List<LogUI>();
          // GameObject go = Instantiate(prefab.gameObject, content);
          // go.transform.SetAsFirstSibling();
          // go.transform.localScale = Vector3.one;
          // LogUI logUI = go.GetComponent<LogUI>();
          // logs.Add(logUI);
          //
          // TimeSpan timeSpan = TimeSpan.FromSeconds(time);
          //
          // logUI.Log(logString, stackTrace, type, timeSpan.ToString("hh':'mm':'ss"));
          // logUI.Subscribe_OnClick(Callback_OnClick);
          //
          //
           int idCount = type == LogType.Log ? 0 : type == LogType.Warning ? 1 : 2;
         //  if (filters != null && idCount >= 0 && idCount < filters.Length && filters[idCount])
         //  {
         //      go.SetActive(true);
         //      if (logSimple)
         //          logSimple.Log(logString, stackTrace, type, "");
         //  }
         //  else
         //      go.SetActive(false);
          
           if (counts != null && idCount >= 0 && idCount < counts.Length)
               counts[idCount]++;

            if (isUseRealtimeRefresh)
            {
                if(panel_allLogs.activeSelf)
                    RefreshFilters();
            }

            RefreshCounts();
        }

        public void Callback_OnClick(LogUI log)
        {
            if (!log)
                return;

            if (logDescription)
            {
                logDescription.Log(log);
                logDescription.gameObject.SetActive(true);
            }
        }

        public void Clear()
        {
            if (logsData == null || logsData_toShow == null) return;

            logsData.Clear();
            logsData_toShow.Clear();

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

            logsData_toShow.Clear();

            for (int i = 0; i < logsData.Count; i++)
            {
                LogData fLog = logsData[i];
                LogType type = fLog.type;

                if (type == LogType.Log)
                {
                    if (filters[0])
                        logsData_toShow.Add(fLog);
                     counts[0]++;
                }
                else if (type == LogType.Warning)
                {
                    if (filters[1])
                        logsData_toShow.Add(fLog);
                    counts[1]++;
                }
                else
                {
                    if (filters[2])
                        logsData_toShow.Add(fLog);
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
            RefreshReusableAll();
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


        #region ReusableUI
        //-----------------------------------------------------------------------------------------------------------------
        [SerializeField] ScrollRect_Reusable scrollReusable = null;

        void RefreshReusableAll() 
        {
            scrollReusable.Set_Count(logsData_toShow.Count);
            scrollReusable.RefhreshAllChilds();
        }

        public void RefreshReusableLot(GameObject target, int id)
        {
            if (!target )  return;
            LogUI logUI = target.GetComponent<LogUI>();
            if (!logUI) return;

            if (logsData_toShow == null || id < 0 || id >= logsData_toShow.Count)
                return;
            
            TimeSpan timeSpan = TimeSpan.FromSeconds(logsData_toShow[id].time);
            logUI.Log(logsData_toShow[id].logString, logsData_toShow[id].stackTrace, logsData_toShow[id].type, timeSpan.ToString("hh':'mm':'ss"));
            logUI.Subscribe_OnClick(Callback_OnClick);
            logUI.SetID(id);
        }
        //-----------------------------------------------------------------------------------------------------------------
        #endregion
    }
}
