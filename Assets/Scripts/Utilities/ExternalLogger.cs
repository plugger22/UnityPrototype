using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// adds specific system log calls to seed.txt
/// </summary>
public class ExternalLogger : MonoBehaviour
{
        /// <summary>
        /// Register event
        /// </summary>
        public void OnEnable()
        { Application.logMessageReceivedThreaded += HandleLog; }

        public void OnDisable()
        { Application.logMessageReceivedThreaded -= HandleLog; }

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logInfo = "";
        if (String.IsNullOrEmpty(logString) == false)
        {
            StringBuilder builder = new StringBuilder();
            switch (type)
            {
                case LogType.Error:
                    builder.AppendFormat("ERROR {0}", logString);
                    builder.AppendLine();
                    builder.AppendFormat("{0}", stackTrace.Replace("\n", "\n\t"));
                    logInfo = builder.ToString();
                    break;
                case LogType.Warning:
                    builder.AppendFormat("WARNING {0}", logString);
                    builder.AppendLine();
                    builder.AppendFormat("{0}", stackTrace.Replace("\n", "\n\t"));
                    logInfo = builder.ToString();
                    break;
                case LogType.Exception:
                    builder.AppendFormat("ASSERT {0}", logString);
                    builder.AppendLine();
                    builder.AppendFormat("{0}", stackTrace.Replace("\n", "\n\t"));
                    logInfo = builder.ToString();
                    break;
            }
            //write to file
            File.AppendAllText("Seed.txt", logInfo);
        }
    }
}
