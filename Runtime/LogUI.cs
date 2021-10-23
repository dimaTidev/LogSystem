using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace DimaTi.Debugging
{
    public class LogUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image image_icon = null;
        [SerializeField] Text text_log = null;
        [SerializeField] Text text_trace = null;
        [SerializeField] Image image_fon = null;
        [SerializeField] bool isUseSiblineColorize = false;
        [SerializeField] Sprite[] sprites = null;
        string logString;
        string stackTrace;
        LogType type;
        string time;

        public LogType TypeLog => type;

        [SerializeField] UnityEvent onClickEvent = null;

        public void Log(LogUI log)
        {
            log.Get_Log(out logString, out stackTrace, out type, out time);
            Log(logString, stackTrace, type, time);
        }

        public void Log(string logString, string stackTrace, LogType type, string time)
        {
            this.logString = logString;
            this.stackTrace = stackTrace;
            this.type = type;
            this.time = time;

            Color col = GetColor(type);
            if (text_log)
            {
                text_log.text = "[" + time + "] " + logString;
                text_log.color = col;
            }
            if (text_trace)
            {
                text_trace.text = stackTrace;
                text_trace.color = new Color(col.r, col.g, col.b, 0.58f);
            }

            if (image_icon)
            {
                image_icon.color = new Color(1,1,1, col.a);
                if(sprites != null && (int)type < sprites.Length)
                    image_icon.sprite = sprites[(int)type];
            }

            if (image_fon) image_fon.enabled = true;

            if (image_fon && isUseSiblineColorize)
                image_fon.color = transform.parent.childCount % 2 == 0 ? new Color(0.26f, 0.26f, 0.26f, 1f) : new Color(0.28f, 0.28f, 0.28f, 1f);
        }

        public void Get_Log(out string logString, out string stackTrace, out LogType type, out string time)
        {
            logString = this.logString;
            stackTrace = this.stackTrace;
            type = this.type;
            time = this.time;
        }

        public void Clear()
        {
            if (image_icon) image_icon.color = Color.clear;
            if (text_log) text_log.color = Color.clear;
            if (text_trace) text_trace.color = Color.clear;
            if (image_fon) image_fon.enabled = false;

            logString = "";
            stackTrace = "";
        }

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

        Action<LogUI> onClick;
        public void Subscribe_OnClick(Action<LogUI> callback) => onClick += callback;
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this);
            onClickEvent?.Invoke();
        }
    }
}
