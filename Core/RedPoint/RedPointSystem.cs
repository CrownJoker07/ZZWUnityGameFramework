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
                    _redPointNum = -1,
                };

                _childRedPointTreeNodeDictionary[path] = redPointTreeNode;
            }

            return redPointTreeNode;
        }

        public void AddRedPointAction(Action<int> redPointAction, Func<int> redPointFunc, bool noSpecificNum)
        {
            // 只需设置一遍即可，其他相同节点调用的是同一种获取红点的方法
            if (_redPointFunc == null)
            {
                _redPointFunc = redPointFunc;
                _noSpecificNum = noSpecificNum;
            }

            if (_redPointActions.Contains(redPointAction))
            {
                Debug.LogError($"重复添加红点行为, 路径为:{GetPath()}");
            }

            _redPointActions.Add(redPointAction);
            
            // 添加默认先刷新一遍
            redPointAction?.Invoke(redPointFunc.Invoke());
        }

        public void RemoveRedPointAction(Action<int> redPointAction)
        {
            if (!_redPointActions.Contains(redPointAction))
            {
                Debug.LogError($"重复删除红点行为, 路径为:{GetPath()}");
            }

            _redPointActions.Remove(redPointAction);
        }

        public void NotifyAllRedPointActions()
        {
            // 根节点不刷新
            if(Name == RootTreeNodeName) return;
            
            int tempRedPointNum = 0;

            if (_childRedPointTreeNodeDictionary.Count > 0)
            {
                foreach (var child in _childRedPointTreeNodeDictionary)
                {
                    RedPointTreeNode tempRedPointTreeNode = child.Value;
                    tempRedPointNum += tempRedPointTreeNode.RedPointNum;

                    if (tempRedPointNum > 0 && _noSpecificNum)
                    {
                        break;
                    }
                } 
            }
            else
            {
                if(_redPointFunc == null) return;
                
                tempRedPointNum = _redPointFunc.Invoke();
            }
            
            if (_redPointNum == tempRedPointNum)
            {
                return;
            }

            _redPointNum = tempRedPointNum;

            foreach (var redPointAction in _redPointActions)
            {
                redPointAction.Invoke(_redPointNum);
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

    public static void AddListener(Action<int> redPointAction, Func<int> redPointFunc, bool noSpecificNum, string path)
    {
        RedPointSystem redPointSystem = Instance;

        RedPointTreeNode redPointTreeNode = redPointSystem.GetOrAddRedPointTreeNode(path);
        redPointTreeNode.AddRedPointAction(redPointAction, redPointFunc, noSpecificNum);
    }

    public static void RemoveListener(Action<int> redPointAction, string path)
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