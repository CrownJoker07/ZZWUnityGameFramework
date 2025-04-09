//------------------------------------------------------------
// File : CircleScrollRect.cs
// Email: mailto:zewei.zhuang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// MVC框架中的数据层(Model)
public abstract class CirculateNodeBase
{
    protected float AnchorPositionX;
    public float AnchorPositionX_Public => AnchorPositionX;
    protected float AnchorPositionY;
    public float AnchorPositionY_Public => AnchorPositionY;
    private Transform _parentTransform;
    protected IObjectPrefab ObjectPrefab;
    private AttachParentPrefabPool _attachParentPrefabPool;
    private int _indexID;
    protected RectTransform NodeRect;

    public void Init(AttachParentPrefabPool attachParentPrefabPool, int indexID, float anchorPositionX,
        float anchorPositionY,
        Transform parentTransform)
    {
        _attachParentPrefabPool = attachParentPrefabPool;
        _indexID = indexID;
        AnchorPositionX = anchorPositionX;
        AnchorPositionY = anchorPositionY;
        _parentTransform = parentTransform;
    }

    private void SpawnNode()
    {
        if (ObjectPrefab != null) return;

        ObjectPrefab =
            _attachParentPrefabPool.Spawn(_indexID, _parentTransform, typeof(IObjectPrefab)) as IObjectPrefab;

        if (ObjectPrefab == null) return;

        if (ObjectPrefab is Component component)
        {
            NodeRect = component.GetComponent<RectTransform>();
            NodeRect.anchoredPosition = new Vector2(AnchorPositionX, AnchorPositionY);
        }

        CustomSpawnNode();
    }

    protected virtual void CustomSpawnNode()
    {
    }

    public void DeSpawnNode()
    {
        if (ObjectPrefab == null) return;

        _attachParentPrefabPool.DeSpawn(_indexID, (MonoBehaviour)ObjectPrefab);
        ObjectPrefab = null;
    }

    public virtual void CheckBorder(float topBorder, float bottomBorder, float leftBorder, float rightBorder)
    {
        if (Mathf.Abs(AnchorPositionY) < topBorder || Mathf.Abs(AnchorPositionY) > bottomBorder ||
            Mathf.Abs(AnchorPositionX) < leftBorder || Mathf.Abs(AnchorPositionX) > rightBorder)
        {
            DeSpawnNode();
        }
        else
        {
            SpawnNode();
        }
    }
}

public class CircleScrollRect : ScrollRect
{
    private float _offset;
    private RectTransform _selfRectTransform;
    public RectTransform SelfRectTransform => _selfRectTransform;

    private List<CirculateNodeBase> _circulateNodeBases =
        new List<CirculateNodeBase>();

    public bool IsDraging;

    public event Action BeginDragEvent;
    public event Action EndDragEvent;

    protected override void Awake()
    {
        base.Awake();

        _selfRectTransform = GetComponent<RectTransform>();
    }

    public void InitData(List<CirculateNodeBase> circulateNodeBases, Vector2 contentSizeDelta,
        float offset = 100f)
    {
        _circulateNodeBases = circulateNodeBases;
        _offset = offset;
        content.sizeDelta = contentSizeDelta;

        onValueChanged.AddListener(ValueChange);
        RefreshNodes();

        IsDraging = false;
    }

    public void DeSpawn()
    {
        onValueChanged.RemoveListener(ValueChange);

        foreach (var nodeInfo in _circulateNodeBases)
        {
            nodeInfo.DeSpawnNode();
        }

        _circulateNodeBases.Clear();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        IsDraging = true;

        BeginDragEvent?.Invoke();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        IsDraging = false;

        EndDragEvent?.Invoke();
    }

    private float TopBorder => content.anchoredPosition.y - _offset;
    private float BottomBorder => content.anchoredPosition.y + _selfRectTransform.rect.height + _offset;
    private float LeftBorder => -content.anchoredPosition.x - _offset;
    private float RightBorder => -content.anchoredPosition.x + _selfRectTransform.rect.width + _offset;

    private void ValueChange(Vector2 value)
    {
        foreach (var nodeInfo in _circulateNodeBases)
        {
            nodeInfo.CheckBorder(TopBorder, BottomBorder, LeftBorder, RightBorder);
        }
    }

    public void RefreshNodes()
    {
        ValueChange(Vector2.zero);
    }

    public List<CirculateNodeBase> GetCirculateNodes()
    {
        return _circulateNodeBases;
    }
}

public class CircleScrollData
{
    public Vector2 ContentLimitSizeDelta = Vector2.zero;
    public float OffsetX = 0f;
}

public static class CircleScrollRectUtility
{
    // border的值分别为左上右下
    public static CircleScrollData InitData<TCell, TNodeBase, TData>(this CircleScrollRect circleScrollRect,
        AttachParentPrefabPool attachParentPrefabPool, List<TData> dataList,
        Action<TNodeBase, TData> initDataAction = null,
        float spaceX = 10f, float spaceY = 10f, int maxColumn = 0, int maxRow = 0, Vector4 border = new Vector4(),
        float safeOffset = 0f, int indexID = 0, RectTransform.Axis axis = RectTransform.Axis.Vertical,
        List<Vector2> customAnchorPosition = null, Vector2 customSizeDelta = new Vector2())
        where TCell : Component
        where TNodeBase : CirculateNodeBase, new()
    {
        CircleScrollData circleScrollData = new CircleScrollData();
        
        float height = 0;
        float width = 0;
        
        Rect cellRect = attachParentPrefabPool.GetObjectPrefabComponent(indexID, typeof(TCell))
            .GetComponent<RectTransform>()
            .rect;
        Rect rect = circleScrollRect.viewport.rect;
        
        float leftBorder = border.x;
        float rightBorder = border.z;
        float topBorder = border.y;
        float bottomBorder = border.w;
        
        float offsetX = 0;
        switch (axis)
        {
            case RectTransform.Axis.Horizontal:
            {
                maxColumn = int.MaxValue;
                break;
            }
            case RectTransform.Axis.Vertical:
            {
                if (maxColumn == 0)
                {
                    maxColumn = (int)((rect.width + spaceX + leftBorder + rightBorder) /
                                      (cellRect.width + spaceX + leftBorder + rightBorder));

                    offsetX = (rect.width - ((cellRect.width + spaceX) * maxColumn - spaceX)) / 2f;
                    circleScrollData.OffsetX = offsetX;
                }

                break;
            }
        }

        int totalCount = dataList.Count;

        if (maxColumn <= 0) maxColumn = 1;
        if (maxRow <= 0) maxRow = 1;
        
        if (customAnchorPosition == null)
        {
            customAnchorPosition = new List<Vector2>(dataList.Count);

            int index = -1;

            for (int i = 0; i < dataList.Count; i++)
            {
                index++;

                int currentColumn = index % maxColumn;
                int currentRow = index / maxColumn;

                float x = offsetX + currentColumn * (cellRect.width + spaceX);
                float y = -currentRow * (cellRect.height + spaceY);

                customAnchorPosition.Add(new Vector2(x + leftBorder, y - topBorder));
            }

            width = (Mathf.CeilToInt(totalCount / (float)maxRow)) * (cellRect.width + spaceX) - spaceX;
            height = (Mathf.CeilToInt(totalCount / (float)maxColumn)) * (cellRect.height + spaceY) - spaceY;

            circleScrollData.ContentLimitSizeDelta.x = width;
            circleScrollData.ContentLimitSizeDelta.y = height;
            
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                {
                    height = circleScrollRect.content.sizeDelta.y;

                    width += leftBorder + rightBorder;
                    break;
                }
                case RectTransform.Axis.Vertical:
                {
                    width = circleScrollRect.content.sizeDelta.x;
                    
                    height += topBorder + bottomBorder;
                    break;
                }
            }
        }
        else
        {
            width = customSizeDelta.x;
            height = customSizeDelta.y;
        }
        
        List<CirculateNodeBase> circulateNodes = new List<CirculateNodeBase>();

        for (var index = 0; index < dataList.Count; index++)
        {
            var data = dataList[index];
            TNodeBase newNodeBase = new TNodeBase();

            initDataAction?.Invoke(newNodeBase, data);

            Vector2 anchorPosition = customAnchorPosition[index];
            newNodeBase.Init(attachParentPrefabPool, indexID, anchorPosition.x, anchorPosition.y,
                circleScrollRect.content);

            circulateNodes.Add(newNodeBase);
        }

        circleScrollRect.InitData(circulateNodes,
            new Vector2(Mathf.Abs(width), Mathf.Abs(height)), cellRect.height + safeOffset);

        return circleScrollData;
    }
}