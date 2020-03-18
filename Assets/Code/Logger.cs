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

    private static void init()
    {
        if (!initialised)
        {
            File.WriteAllText("log.txt", "");
        }
        initialised = true;
    }

    public static void Log(string message)
    {
        init();
        Debug.Log(message);
        File.AppendAllText("log.txt", $"({DateTime.Now.ToString()}) INFO:\n{message}\n");
    }

    public static void LogError(string message)
    {
        init();
        Debug.LogError(message);
        File.AppendAllText("log.txt", $"({DateTime.Now.ToString()}) ERROR:\n{message}\n");
    }

    public static void LogWarning(string message)
    {
        init();
        Debug.LogWarning(message);
        File.AppendAllText("log.txt", $"({DateTime.Now.ToString()}) WARNING:\n{message}\n");
    }
}
