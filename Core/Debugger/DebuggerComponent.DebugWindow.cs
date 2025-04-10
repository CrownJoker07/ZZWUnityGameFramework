//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZWUnityGameFramework.Debugger
{
    public class CommandData
    {
        public string Name;
        public Action<string> Action;
        public Func<bool> State;
        public Func<string> Info;
        public int DebugType;
    }
    
    public sealed partial class DebuggerComponent
    {
        public List<CommandData> CommandDatas = new List<CommandData>();

        public static void SetCommandDatas(List<CommandData> commandDatas)
        {
            Instance.CommandDatas = commandDatas;
        }
        
        private sealed class DebugWindow : IDebuggerWindow
        {
            private const int columns = 4; // 每行列数
            private Vector2 m_ScrollPosition = Vector2.zero;
            private string _textContent = String.Empty;

            public void Initialize(params object[] args)
            {
            }

            public void Shutdown()
            {
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void OnDraw()
            {
                _textContent = GUILayout.TextField(_textContent, 30);
                
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    OnDrawScrollableWindow();
                }
                GUILayout.EndScrollView();
            }
            
            private void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>System Information</b>");
                GUILayout.BeginVertical("box");
                {
                    // 计算每个按钮宽度
                    float buttonWidth = Screen.width / DefaultWindowScale / columns - 15;

                    int index = 0;

                    int lastDebugType = -1;
                    bool showTitle = false;
                    do
                    {
                        GUILayout.BeginHorizontal();
                        {
                            for (int j = 0; j < columns; j++)
                            {
                                if (index >= Instance.CommandDatas.Count) break;
                                
                                CommandData commandData = Instance.CommandDatas[index];

                                if (lastDebugType != commandData.DebugType)
                                {
                                    if (lastDebugType != -1)
                                    {
                                        showTitle = true;
                                    }

                                    lastDebugType = commandData.DebugType;
                                    break;
                                }

                                index++;
                                if (GUILayout.Button(commandData.Name, GUILayout.Width(buttonWidth), GUILayout.Height(30f)))
                                {
                                    commandData.Action?.Invoke(_textContent);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (showTitle)
                        {
                            showTitle = false;
                            GUILayout.Space(10f);
                        }
                    } while (index < Instance.CommandDatas.Count);
                }
                GUILayout.EndVertical();
            }
        }
    }
}
