using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using DimaTi.UI.Reusable;

namespace DimaTi.Debugging
{
    public class LogUI : MonoBehaviour, IPointerClickHandler, HorizontalAndVerticalLayoutGroup_DataBlock<LogUIManager.LogData>
    {
        [SerializeField] Image image_icon = null;
        [SerializeField] Text text_log = null;
        [SerializeField] Text text_trace = null;
        [SerializeField] Image image_fon = null;
     //   [SerializeField] bool isUseSiblineColorize = false;
        [SerializeField] Sprite[] sprites = null;

        LogUIManager.LogData logData;
        public LogUIManager.LogData LogData => logData;

        Color[] colorArray =
           {
                Color.red,
                Color.magenta,
                Color.yellow,
                Color.white,
                Color.red
            };
        Color GetColor(LogType type)
        {
            if ((int)type < colorArray.Length)
                return colorArray[(int)type];
            return Color.white;
        }

     //   Action<LogUI> onClick;
        [SerializeField] UnityEvent onClickEvent = null;

        public void Log(LogUIManager.LogData data)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(data.time);
            Color col = GetColor(data.type);
            if (text_log)
            {
                text_log.text = "[" + timeSpan.ToString("hh':'mm':'ss") + "] " + data.logString;
                text_log.color = col;
            }
            if (text_trace)
            {
                text_trace.text = data.stackTrace;
                text_trace.color = new Color(col.r, col.g, col.b, 0.58f);
            }

            if (image_icon)
            {
                image_icon.color = new Color(1,1,1, col.a);
                if(sprites != null && (int)data.type < sprites.Length)
                    image_icon.sprite = sprites[(int)data.type];
            }

            if (image_fon) image_fon.enabled = true;
        }
        public void Clear()
        {
            if (image_icon) image_icon.color = Color.clear;
            if (text_log) text_log.color = Color.clear;
            if (text_trace) text_trace.color = Color.clear;
            if (image_fon) image_fon.enabled = false;
            logData = default;
        }
        public void SetID(int id)
        {
            if (image_fon)
                image_fon.color = id % 2 == 0 ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.28f, 0.28f, 0.28f, 1f);
        }

      //  public void Subscribe_OnClick(Action<LogUI> callback) => onClick = callback;
        public void OnPointerClick(PointerEventData eventData)
        {
            //onClick?.Invoke(this);
           // if (LogUIManager.Instance)
           //     LogUIManager.Instance.Callback_OnClick(this);
            onClickEvent?.Invoke();
        }


        public void Set_Data(LogUIManager.LogData data)
        {
            logData = data;
            Log(data);
        }
        public void Copy_Data(MonoBehaviour mono)
        {
            if(mono is LogUI log)
            {
                Set_Data(log.LogData);
            }
        }
    }
}
