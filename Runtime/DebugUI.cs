using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] bool isDisableLog = false;
    [SerializeField] Text text = null;
    static Text textLog;

    static List<string> logs = new List<string>();


    void Start()
    {
        if (!isDisableLog)
            textLog = text;
        text.text = "";
    }

    public static void Log(string log)
    {
        if (logs == null) logs = new List<string>();
        logs.Add(log);

        if (textLog)
        {
            /*string text = textLog.text;// + "\n" + log;

            string[] array = text.Split('\n');
            text = "";
            int k = 0;
            for (int i = 0; i < 50; i++)
            {
                k = array.Length - i - 1;
                if (k < 0)
                    break;

                if(array[k] != "")
                    text += array[k] + "\n";
            }
            textLog.text = text + "\n" + log;*/

            string text = "";
            for (int i = 0; i < logs.Count; i++)
            {
                text += logs[i] + "\n";
            }

            textLog.text = text;
        }
    }
}
