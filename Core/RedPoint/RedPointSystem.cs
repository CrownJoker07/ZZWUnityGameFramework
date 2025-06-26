using System;
using System.Collections.Generic;
using UnityEngine;

public class RedPointSystem : Singleton<RedPointSystem>
{
    private const string RootTreeNodeName = "Root";

    public class RedPointTreeNode
    {
        public string Name;

        private RedPointTreeNode _parentRedPointTreeNode;

        private readonly Dictionary<string, RedPointTreeNode> _childRedPointTreeNodeDictionary =
            new Dictionary<string, RedPointTreeNode>();

        private Func<int> _redPointFunc;
        private readonly List<Action<int>> _redPointActions = new List<Action<int>>();
        private int _redPointNum;
        private int RedPointNum => _redPointNum;

        private bool _noSpecificNum;

        public RedPointTreeNode GetOrAddRedPointTreeNode(string path)
        {
            if (!_childRedPointTreeNodeDictionary.TryGetValue(path, out RedPointTreeNode redPointTreeNode))
            {
                redPointTreeNode = new RedPointTreeNode
                {
                    Name = path,
                    _parentRedPointTreeNode = this,
                    _redPointNum = 0,
                };

                _childRedPointTreeNodeDictionary[path] = redPointTreeNode;
            }

            return redPointTreeNode;
        }

        public void AddRedPointAction(Action<int> redPointAction, Func<int> redPointFunc, bool noSpecificNum)
        {
            _redPointFunc = redPointFunc;
            _noSpecificNum = noSpecificNum;

            if (_redPointActions.Contains(redPointAction))
            {
                Debug.LogError($"重复添加红点行为, 路径为:{GetPath()}");
            }

            _redPointActions.Add(redPointAction);

            _redPointNum = redPointFunc.Invoke();
            // 添加默认先刷新一遍
            redPointAction?.Invoke(_redPointNum);
        }

        public void RemoveRedPointAction(Action<int> redPointAction)
        {
            if (!_redPointActions.Contains(redPointAction))
            {
                // Debug.LogError($"重复删除红点行为, 路径为:{GetPath()}");
            }

            _redPointActions.Remove(redPointAction);

            if (_redPointActions.Count == 0)
            {
                _redPointFunc = null;
            }
        }

        public void NotifyAllRedPointActions()
        {
            // 根节点不刷新
            if(Name == RootTreeNodeName) return;
            
            int tempRedPointNum = 0;

            bool haveRedPointChild = false;
            if (_childRedPointTreeNodeDictionary.Count > 0)
            {
                foreach (var child in _childRedPointTreeNodeDictionary)
                {
                    RedPointTreeNode tempRedPointTreeNode = child.Value;

                    if (tempRedPointTreeNode._redPointFunc == null ||
                        tempRedPointTreeNode._redPointActions.Count == 0) continue;

                    haveRedPointChild = true;
                        
                    tempRedPointNum += tempRedPointTreeNode.RedPointNum;

                    if (tempRedPointNum > 0 && _noSpecificNum)
                    {
                        break;
                    }
                }
            }

            // 没有子节点或者红点数量为零时，父级自己重新计算一遍
            if ((!haveRedPointChild || tempRedPointNum == 0) && _redPointFunc != null)
            {
                tempRedPointNum = _redPointFunc.Invoke();
            }

            if (tempRedPointNum != _redPointNum)
            {
                _redPointNum = tempRedPointNum;

                foreach (var redPointAction in _redPointActions)
                {
                    redPointAction.Invoke(_redPointNum);
                }
            }
            
            _parentRedPointTreeNode.NotifyAllRedPointActions();
        }

        private string GetPath()
        {
            List<string> paths = new List<string>();
            paths.Add(Name);

            RedPointTreeNode tempRedPointTreeNode = _parentRedPointTreeNode;
            while (tempRedPointTreeNode != null)
            {
                paths.Add(tempRedPointTreeNode.Name);
                tempRedPointTreeNode = tempRedPointTreeNode._parentRedPointTreeNode;
            }

            string path = String.Empty;
            for (int i = paths.Count - 1; i >= 0; i--)
            {
                path += paths[i];
            }

            return path;
        }
    }

    private readonly RedPointTreeNode _rootRedPointTreeNode = new RedPointTreeNode()
    {
        Name = RootTreeNodeName,
    };

    public static void AddListener(string path, Action<int> redPointAction, Func<int> redPointFunc,
        bool noSpecificNum = false)
    {
        RedPointSystem redPointSystem = Instance;

        RedPointTreeNode redPointTreeNode = redPointSystem.GetOrAddRedPointTreeNode(path);
        redPointTreeNode.AddRedPointAction(redPointAction, redPointFunc, noSpecificNum);
    }
    
    public static void AddListener(string path, Action<int> redPointAction, Func<bool> redPointFunc,
        bool noSpecificNum = false)
    {
        RedPointSystem redPointSystem = Instance;

        RedPointTreeNode redPointTreeNode = redPointSystem.GetOrAddRedPointTreeNode(path);
        redPointTreeNode.AddRedPointAction(redPointAction, () => redPointFunc.Invoke() ? 1 : 0, noSpecificNum);
    }

    public static void RemoveListener(string path, Action<int> redPointAction)
    {
        RedPointSystem redPointSystem = Instance;

        RedPointTreeNode redPointTreeNode = redPointSystem.GetOrAddRedPointTreeNode(path);
        redPointTreeNode.RemoveRedPointAction(redPointAction);
    }

    public static void Notify(string path)
    {
        RedPointSystem redPointSystem = Instance;

        RedPointTreeNode redPointTreeNode = redPointSystem.GetOrAddRedPointTreeNode(path);

        redPointTreeNode.NotifyAllRedPointActions();
    }

    private RedPointTreeNode GetOrAddRedPointTreeNode(string path)
    {
        RedPointTreeNode tempRedPointTreeNode = _rootRedPointTreeNode;
        string[] paths = path.Split('/');
        foreach (var tempPath in paths)
        {
            tempRedPointTreeNode = tempRedPointTreeNode.GetOrAddRedPointTreeNode(tempPath);
        }

        return tempRedPointTreeNode;
    }
}