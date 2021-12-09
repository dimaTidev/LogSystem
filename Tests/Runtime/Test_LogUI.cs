using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_LogUI : MonoBehaviour
{
    public float speedThrow = 1;
    public int maxLogCount = 20;

    int log_count;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(speedThrow);
        log_count++;
        Debug.Log("TestLog: " + log_count);
        if(log_count < maxLogCount)
            StartCoroutine(Start());
    }

    [ContextMenu("Restart throw debug")]
    public void StartCor()
    {
        log_count = 0;
        StartCoroutine(Start());
    }
}
