using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private static bool initialised = false;
    private static string logBuf = "";
    private const int logChLimit = 1024; // approximate character limit for the log buffer. At this point, we would write it to disk (as opposed to writing on every log)
    private static string logFn = "";

    private static void init()
    {
        if (!initialised)
        {
            logFn = $"log-{DateTime.Now.ToString().Replace(":", ".").Replace("/", "-")}.txt";
            File.WriteAllText(logFn, "");
        }
        initialised = true;
    }

    private static void saveAndFlush()
    {
        File.AppendAllText(logFn, logBuf);
        logBuf = "";
    }

    public static void Log(string message)
    {
        init();
        Debug.Log(message);
        logBuf += $"({DateTime.Now.ToString()}) INFO:\n{message}\n";

        // Flush if log character limit reached or every 30s
        if (logChLimit <= logBuf.Length || (Mathf.RoundToInt(Time.time) % 30) < 2)
        {
            saveAndFlush();
        }
    }

    public static void LogError(string message)
    {
        init();
        Debug.LogError(message);
        logBuf += $"({DateTime.Now.ToString()}) ERROR:\n{message}\n";

        // Flush if log character limit reached or every 30s
        if (logChLimit <= logBuf.Length || (Mathf.RoundToInt(Time.time) % 30) < 5)
        {
            saveAndFlush();
        }
    }

    public static void LogWarning(string message)
    {
        init();
        Debug.LogWarning(message);
        logBuf += $"({DateTime.Now.ToString()}) WARNING:\n{message}\n";

        // Flush if log character limit reached or every 30s
        if(logChLimit <= logBuf.Length || (Mathf.RoundToInt(Time.time) % 30) < 5)
        {
            saveAndFlush();
        }
    }
}
