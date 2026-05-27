using System;

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}

public class MinHeap<T> where T : IHeapItem<T>
{
    private T[] _items;
    private int _currentItemCount;

    public int Count => _currentItemCount;

    public MinHeap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        _currentItemCount++;
        SortUp(item);
    }

    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _currentItemCount--;

        _items[0] = _items[_currentItemCount];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);

        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public bool Contains(T item)
    {
        if (item.HeapIndex >= _currentItemCount) return false;
        return Equals(_items[item.HeapIndex], item);
    }

    private void SortDown(T item)
    {
        while (true)
        {
            int left = item.HeapIndex * 2 + 1;
            int right = item.HeapIndex * 2 + 2;
            int swapIndex = item.HeapIndex;

            if (left < _currentItemCount)
            {
                swapIndex = left;

                if (right < _currentItemCount &&
                    _items[right].CompareTo(_items[left]) > 0)
                {
                    swapIndex = right;
                }

                if (_items[swapIndex].CompareTo(item) > 0)
                    Swap(item, _items[swapIndex]);
                else
                    return;
            }
            else return;
        }
    }

    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parent = _items[parentIndex];

            if (item.CompareTo(parent) > 0)
                Swap(item, parent);
            else
                break;

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(T a, T b)
    {
        _items[a.HeapIndex] = b;
        _items[b.HeapIndex] = a;

        (a.HeapIndex, b.HeapIndex) = (b.HeapIndex, a.HeapIndex);
    }
}