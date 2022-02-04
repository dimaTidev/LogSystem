using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DimaTi.UI.Reusable;

namespace DimaTi.Debugging
{

   // public class Container_LogUILayoutDada
   // {
   //     public LogUIManager.LogData data;
   //     public Action<LogUI> onClick;
   // }

    public class LogUILayout_Manager : AHorizontalAndVerticalLayoutGroup_Reusable_Manager<LogUI, LogUIManager.LogData>
    {
        [SerializeField] LogUIManager logManager;
        protected override int ElementCount => logManager != null ? logManager.LogsCount : 0;

        void OnEnable()
        {
            logManager.OnReceivingLog += RefreshLayoutCount; //иначе возможна рекурсия при логе со скриптов задействованных в этой лог системе!
            RefreshLayoutCount();
        }
        void OnDisable() => logManager.OnReceivingLog -= RefreshLayoutCount;

        protected override LogUIManager.LogData GetData(int id) => logManager.GetFilteredLogByID(id);
    }
}
