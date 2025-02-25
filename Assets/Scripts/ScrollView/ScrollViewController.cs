using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ObjectPool))]
public class ScrollViewController : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] Vector2 cellSize;
    [SerializeField] Vector2 spacing;
    [SerializeField] int columns;
    
    private ScrollRect _scrollRect;
    private RectTransform _rectTransform;
    private ObjectPool _objectPool;
    
    private List<StageCellItem> _items;
    private LinkedList<Cell> _visibleCells;

    private float _lastYValue = 1f;
    int[] dx = { -1, 0, 1 };

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();
        _objectPool = GetComponent<ObjectPool>();
        _scrollRect.onValueChanged.AddListener(OnScroll);
    }
    
    public void SetData(List<StageCellItem> items)
    {
        _items = items;
        ReloadData();
    }

    private (int startIndex, int endIndex) GetVisibleRowIndexRange()
    {
        var visibleRect = new Rect(
            _scrollRect.content.anchoredPosition.x,
            _scrollRect.content.anchoredPosition.y,
            _rectTransform.rect.width,
            _rectTransform.rect.height);

        // 스크롤 위치에 따른 시작 인덱스 계산
        var startIndex = Mathf.FloorToInt(visibleRect.y / (cellSize.y + spacing.y));

        // 화면에 보이게 될 Cell 개수 계산
        int visibleCount = Mathf.CeilToInt(visibleRect.height / (cellSize.y + spacing.y));

        // 버퍼 추가
        startIndex = Mathf.Max(0, startIndex);
        visibleCount += 2;

        return (startIndex, startIndex + visibleCount - 1);
    }

    private bool IsVisibleIndex(int index)
    {
        var (startIndex, endIndex) = GetVisibleRowIndexRange();
        endIndex = Mathf.Min(endIndex, _items.Count - 1);
        return startIndex * columns <= index && index <= endIndex * columns;
    }
   
    private void ReloadData()
    {
        // _visibleCell 초기화
        _visibleCells = new LinkedList<Cell>();
        
        // Content의 높이를 _items의 데이터의 수만큼 계산해서 높이를 지정
        var contentSizeDelta = _scrollRect.content.sizeDelta;
        contentSizeDelta.y = (int)((_items.Count / columns) + 1) * (cellSize.y + spacing.y);
        _scrollRect.content.sizeDelta = contentSizeDelta;

        //TODO: UserInfo로 관리(임시값)
        //현재 Stage로 이동
        var lastStageIndex = 90;
        int row = lastStageIndex / columns;
        var contentPos = row * cellSize.y + row * spacing.y ;
        _scrollRect.content.anchoredPosition = new Vector2(0, contentPos);

        // 화면에 보이는 영역에 Cell 추가
        var (startIndex, endIndex) = GetVisibleRowIndexRange();
        var maxEndIndex = Mathf.Min(endIndex, _items.Count - 1);

        for (int i = startIndex; i < maxEndIndex; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                var idx = i * columns + j;
                if (idx >= _items.Count - 1) break;
                
                // 셀 만들기
                var cellObject = _objectPool.GetObject(cellPrefab);
                var cell = cellObject.GetComponent<Cell>();
                var cellButton = cellObject.GetComponent<StageCellButton>();
                var cellRect = cellObject.GetComponent<RectTransform>();
                
                cell.SetItem(_items[idx], idx);
                cellButton.SetStageCell(_items[idx]);
                
                cellRect.sizeDelta = cellSize;
                cell.transform.localPosition = new Vector3(dx[j] * (cellSize.x + spacing.x), -i * (cellSize.y+ spacing.y), 0);
                _visibleCells.AddLast(cell);
            }
        }
    }

    #region Scroll Rect Events

    private void OnScroll(Vector2 value)
    {
        if (_lastYValue < value.y)
        {
            ProcessScrollUp();
        }
        else
        {
            ProcessScrollDown();
        }

        _lastYValue = value.y;
    }

    void ProcessScrollUp()
    {
        // 1. 상단에 새로운 셀이 필요한지 확인 후 필요하면 추가
        var firstCell = _visibleCells.First.Value;
        var newFirstIndex = firstCell.Index - 1;

        if (IsVisibleIndex(newFirstIndex))
        {
            for (int j = 0; j < columns; j++)
            {
                int idx = newFirstIndex - j;
                var cellObject = _objectPool.GetObject(cellPrefab);
                var cell = cellObject.GetComponent<Cell>();
                var cellButton = cellObject.GetComponent<StageCellButton>();
                
                cell.SetItem(_items[idx], idx);
                cellButton.SetStageCell(_items[idx]);
                
                cell.transform.localPosition = new Vector3(dx[columns - j - 1] * (cellSize.x + spacing.x), -(idx/columns) * (cellSize.y + spacing.y), 0);
                _visibleCells.AddFirst(cell);
            }
        }

        // 2. 하단에 있는 셀이 화면에서 벗어나면 제거
        if (!IsVisibleIndex(_visibleCells.Last.Value.Index))
        {
            int lastRowStart = _visibleCells.Last.Value.Index / columns * columns;
            int lastRowEnd = _visibleCells.Last.Value.Index;
            
            for (int j = lastRowEnd; j >= lastRowStart; j--)
            {
                var lastCell = _visibleCells.Last.Value;
                
                _objectPool.ReturnObject(lastCell.gameObject);
                _visibleCells.RemoveLast();
            }
        }
    }

    void ProcessScrollDown()
    {
        // 1. 하단에 새로운 셀이 필요한지 확인 후 필요하면 추가
        var lastCell = _visibleCells.Last.Value;
        var newLastIndex = lastCell.Index + 1;

        if (IsVisibleIndex(newLastIndex))
        {
            for (int j = 0; j < columns; j++)
            {
                int idx = newLastIndex + j;
                if (idx >= _items.Count) break;

                var cellObject = _objectPool.GetObject(cellPrefab);
                var cell = cellObject.GetComponent<Cell>();
                var cellButton = cellObject.GetComponent<StageCellButton>();
                
                cell.SetItem(_items[idx], idx);
                cellButton.SetStageCell(_items[idx]);
                
                cell.transform.localPosition = new Vector3(dx[j] * (cellSize.x + spacing.x), -(idx/columns) * (cellSize.y + spacing.y), 0);
                _visibleCells.AddLast(cell);
            }
        }

        if (!IsVisibleIndex(_visibleCells.First.Value.Index))
        {
            for (int j = 0; j < columns; j++)
            {
                var firstCell = _visibleCells.First.Value;
            
                _objectPool.ReturnObject(firstCell.gameObject);
                _visibleCells.RemoveFirst();
            }
        }
    }

    #endregion
}