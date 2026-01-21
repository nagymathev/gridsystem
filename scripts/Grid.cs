using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace InventorySystem;

/// <summary>
/// Generic <c>Grid</c> containing pointers to its elements.
/// </summary>
///
/// <example>
/// Example creating a new grid with a specified size.
/// <code>
/// public Grid&lt;InventoryItem&gt; grid = new(10,10);
/// </code>
/// </example>
public class Grid<T> where T : class
{
    private int _sizeX;
    private int _sizeY;
    private List<T> _grid = [];

    public Grid(int sizeX, int sizeY)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _grid.Capacity = sizeX * sizeY;
        _grid.AddRange(Enumerable.Repeat<T>(null, sizeX * sizeY));
    }

    public T Get(Vector2 pos)
    {
        return !IsInRange(pos) ? null : _grid[VectorToIndex(pos)];
    }

    public bool Add(Vector2 pos, T element)
    {
        if (!IsInRange(pos))
        {
            return false;
        }
        _grid[VectorToIndex(pos)] = element;
        return true;
    }
    
    public void Add(IEnumerable<Vector2> pos, T element)
    {
        foreach (var p in pos)
        {
            Add(p, element);
        }
    }

    public void Delete(T element)
    {
        for (int i = 0; i < _grid.Capacity; i++)
        {
            if (_grid[i] == element)
            {
                _grid[i] = null;
            }
        }
    }

    public void Delete(Vector2 pos)
    {
        IsInRange(pos);
        _grid[VectorToIndex(pos)] = null;
    }
    
    public void Delete(IEnumerable<Vector2> pos)
    {
        foreach (var p in pos)
        {
            Delete(p);
        }
    }

    public bool IsFree(Vector2 pos)
    {
        if (!IsInRange(pos))
        {
            return false;
        }
        
        if (_grid[VectorToIndex(pos)] == null)
        {
            return true;
        }

        return false;
    }
    
    public bool IsFree(IEnumerable<Vector2> pos)
    {
        bool isFree = true;
        foreach (var p in pos)
        {
            if (IsFree(p)) continue;
            isFree = false;
            break;
        }

        return isFree;
    }

    private bool IsInRange(Vector2 pos)
    {
        return !(pos.X < 0) && !(pos.X >= _sizeX) && !(pos.Y < 0) && !(pos.Y >= _sizeY);
    }
    
    private int VectorToIndex(Vector2 pos)
    {
        var idx = (int)(pos.X + (_sizeX * pos.Y));
        return idx;
    }
}