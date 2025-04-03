﻿using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    public class LogModule : ConsoleModule
    {
        private struct ConsoleMessage
        {
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;

            public ConsoleMessage(string message, string stackTrace, LogType type)
            {
                this.message = message;
                this.stackTrace = stackTrace;
                this.type = type;
            }
        }

        public override string Title { get; set; } = "Log";
        private GUIContent clearLabel = new("Clear", "Clear the contents of the console.");
        private GUIContent collapseLabel = new("Collapse", "Hide repeated messages.");
        private GUIContent scrollToBottomLabel = new("ScrollToBottom", "Scroll bar always at bottom");

        private Vector2 scrollPos;
        private bool scrollToBottom = true;
        private List<ConsoleMessage> entries = new();
        private bool collapse;

        public override void OnInit()
        {
            Application.logMessageReceived += HandleLog;
        }

        private FluentGUIStyle mLabelStyle = new(() => new GUIStyle
        {
            fontSize = 10,
            normal =
            {
                textColor = Color.white
            }
        });

        public override void DrawGUI()
        {
            if (scrollToBottom)
                GUILayout.BeginScrollView(Vector2.up * entries.Count * 100.0f);
            else
                scrollPos = GUILayout.BeginScrollView(scrollPos);

            // Go through each logged entry
            for (var i = 0; i < entries.Count; i++)
            {
                var entry =
                    entries
                        [i]; // If this message is the same as the last one and the collapse feature is chosen, skip it 
                if (collapse && i > 0 && entry.message == entries[i - 1].message) continue;

                // Change the text colour according to the log type
                switch (entry.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        GUI.contentColor = Color.red;
                        break;
                    case LogType.Warning:
                        GUI.contentColor = Color.yellow;
                        break;
                    default:
                        GUI.contentColor = Color.white;
                        break;
                }


                if (entry.type == LogType.Exception)
                    GUILayout.Label(entry.message + " || " + entry.stackTrace, mLabelStyle);
                else
                    GUILayout.Label(entry.message, mLabelStyle);
            }

            GUI.contentColor = Color.white;
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            // Clear button
            if (GUILayout.Button(clearLabel)) entries.Clear();

            // Collapse toggle
            collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
            scrollToBottom =
                GUILayout.Toggle(scrollToBottom, scrollToBottomLabel, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            var entry = new ConsoleMessage(message, stackTrace, type);
            entries.Add(entry);
        }

        public override void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}