#nullable enable
using System;
using System.Collections.Generic;

namespace HLab.Base;

public class SortedQueue<T>(Func<T, T, int> comparator)
{
    public SortedQueue() : this((a, b) => Comparer<T>.Default.Compare(a, b))
    {
    }

    class Node        
    {
        public Node? Next;
        public T? Value;
    }

    Node? _head = null;

    public Func<T, T, int> Comparator { get; } = comparator;

    public void Enqueue(T item)
    {
        ref var node = ref _head;
        while(node is not null && Comparator(item,node.Value) > 0)
        {
            node = ref node.Next;
        }
        node = new Node { Value = item, Next = node };
    }

    public bool TryDequeue(out T? item)
    {
        var node = _head;
        if (node == null) {
            item = default;
            return false;
        }
        _head = node.Next;
        item = node.Value;
        return true;
    }
    public bool TryDequeue(out T? item, Func<T,bool> condition)
    {
        var node = _head;
        if (node == null) {
            item = default;
            return false;
        }
        if(!condition(node.Value))
        {
            item = default;
            return false;
        }
        _head = node.Next;
        item = node.Value;
        return true;
    }

    public bool TryPeek(out T? item)
    {
        var node = _head;
        if (node == null) {
            item = default;
            return false;
        }
        item = node.Value;
        return true;
    }

}