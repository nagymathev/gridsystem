using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Inventory : Control
{
	[Export] private Color _backgroundColor = new Color("#292831");
	[Export] private Color _itemColor = new Color("#fbbbad");
	
	[Export] private Array<Item> _items = new Array<Item>();
	[Export] private Array<int> _grid = new Array<int>();
	[Export] private Vector2I _gridSize = new Vector2I(10, 10);
	[Export] private int _cellSize = 30;
	[Export] private int _cellGapSize = 2;

	private Array<ColorRect> _gridRects = new Array<ColorRect>();
	private ColorRect _previousCell = new ColorRect();
	private Color _previousColor = new Color();

	private Item _draggedItem = null;
	private Array<ColorRect> _draggedItemRender = new Array<ColorRect>();
	private bool _isDragging = false;

	[Signal] public delegate void OnDragStartEventHandler();
	[Signal] public delegate void OnDragEndEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OnDragStart += () => {
			_isDragging = true;
			PickItemAt(MousePosIndex());
		};
		OnDragEnd += () => {
			_isDragging = false;
			PlaceItemAt(MousePosIndex());
		};

		var totalGridSize = _gridSize.X * _gridSize.Y;

		_grid.Resize(totalGridSize);
		_grid.Fill(0);

		_gridRects.Resize(totalGridSize);

		for (int i = 0; i < totalGridSize; i++)
		{
			var rect = CreateRect(GridIndexToVector(i) * (_cellSize + _cellGapSize), _backgroundColor);
			this.AddChild(rect);
			_gridRects[i] = rect;
		}

		var item1 = new Item(1, "The Item", [new Vector2(0,0)]);
		item1.itemColor = new Color("#4a7a96");
		AddItem(2, item1);

		var item2 = new Item(2, "Other Item", [new Vector2(0,0), new Vector2(0,1), new Vector2(0,2)]);
		item2.itemColor = new Color("#ee8695");
		AddItem(5, item2);

		var item3 = new Item(3, "The Weird One", [
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(2,1)
		]);
		item3.itemColor = new Color("#ff7777");
		AddItem(6, item3);

		UpdateCells();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_isDragging && _draggedItem != null)
		{
			RenderItem(_draggedItem);
		}
	}

    public override void _Input(InputEvent @event)
    {
		// Just mouse visibility
		var mm = @event as InputEventMouseMotion;
		if (mm != null)
		{
			var mousePos = mm.Position;
			var gridMousePos = ScreenSpacePixelToGridPosition(mousePos);
			var gridIndex = GridVectorToIndex(gridMousePos);

			if (gridMousePos.X >= 0 && gridMousePos.X < _gridSize.X &&
				gridMousePos.Y >= 0 && gridMousePos.Y < _gridSize.Y)
			{
				if (_previousCell != _gridRects[gridIndex])
				{
					if (_previousCell != null)
					{
						_previousCell.Color = _previousColor;
					}
					_previousCell = _gridRects[gridIndex];
					_previousColor = _gridRects[gridIndex].Color;
				}
				_gridRects[gridIndex].Color = new Color("#f32f");
			}
			else
			{
				_gridRects[gridIndex].Color = _backgroundColor;
			}
		}
		

        var mb = @event as InputEventMouseButton;
		if (mb != null)
		{
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
			{
				EmitSignal(SignalName.OnDragStart);
			}
			if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
			{
				EmitSignal(SignalName.OnDragEnd);
			}
		}
    }

	private void PickItemAt(int idx)
	{
		_draggedItem = GetItem(idx);
		if (_draggedItem == null)
		{
			return;
		}

		foreach (var cell in _draggedItem.itemCells)
		{
			_gridRects[GridVectorToIndexRelative(_draggedItem.index, cell)].SelfModulate = _draggedItem.itemColor.Darkened(0.2f);
		}

		var isRemoved = RemoveItem(_draggedItem);
		GD.Print(isRemoved);
		
		_previousColor = _draggedItem.itemColor;
		UpdateCells();

		var mousePos = GetLocalMousePosition();
		foreach (var cell in _draggedItem.itemCells)
		{
			var rect = CreateRect(mousePos + (cell * (_cellSize / 2)), _draggedItem.itemColor);
			_draggedItemRender.Add(rect);
			AddChild(rect);
		}
	}

	private void PlaceItemAt(int idx)
	{
		if (_draggedItem == null)
		{
			return;
		}

		var newItem = new Item(_draggedItem);
		newItem.index = idx;
		if (IsItemOutOfBounds(newItem) || !IsItemCellsEmpty(newItem))
		{
			AddItem(_draggedItem.index, _draggedItem);
			OriginalColor(_draggedItem);

			foreach (var rect in _draggedItemRender)
			{
				RemoveChild(rect);
				rect.QueueFree();
			}

			_draggedItemRender.Clear();
			_draggedItem = null;

			return;
		}

		OriginalColor(_draggedItem);
		
		AddItem(idx, _draggedItem);

		_previousColor = _draggedItem.itemColor;
		
		_draggedItem = null;
		UpdateCells();

		foreach (var rect in _draggedItemRender)
		{
			RemoveChild(rect);
			rect.QueueFree();
		}
		_draggedItemRender.Clear();
	}

	private void RenderItem(Item item)
	{
		var mousePos = GetLocalMousePosition();
		for (int i = 0; i < item.itemCells.Count; i++)
		{
			var rect = _draggedItemRender[i];
			rect.Position = new Vector2(
				(mousePos.X - _cellSize / 2) + item.itemCells[i].X * (_cellSize + _cellGapSize),
				(mousePos.Y - _cellSize / 2) + item.itemCells[i].Y * (_cellSize + _cellGapSize)
			);
			rect.Color = item.itemColor;
		}
	}

	private void OriginalColor(Item item)
	{
		foreach (var cell in item.itemCells)
		{
			_gridRects[GridVectorToIndexRelative(item.index, cell)].SelfModulate = new Color(1, 1, 1, 1);
		}
	}

	private int MousePosIndex()
	{
		return GridVectorToIndex(ScreenSpacePixelToGridPosition(GetLocalMousePosition()));
	}

	private Vector2 ScreenSpacePixelToGridPosition(Vector2 pos)
	{
		var gridPos = new Vector2(
			Mathf.Clamp(Mathf.Floor(pos.X / (_cellSize + _cellGapSize)), 0.0f, _gridSize.X - 1.0f),
			Mathf.Clamp(Mathf.Floor(pos.Y / (_cellSize + _cellGapSize)), 0.0f, _gridSize.Y - 1.0f)
		);
		return gridPos;
	}

	private int GridVectorToIndex(Vector2 pos)
	{
		int idx = (int)(pos.X + (_gridSize.X * pos.Y));
		return idx;
	}

	private int GridVectorToIndexRelative(int idx, Vector2 offset)
	{
		return idx + GridVectorToIndex(offset);
	}

	private Vector2 GridIndexToVector(int idx)
	{
		return new Vector2(idx % _gridSize.X, Mathf.Floor(idx / _gridSize.X));
	}

	private Vector2 GridLocalToGlobal(int idx, Vector2 cell)
	{
		var pos = GridIndexToVector(idx);
		return pos + cell;
	}
	
	private bool IsCellEmpty(int idx)
	{
		// Any value greater than 0 indicates there's something.
		return _grid[idx] <= 0;
	}

	// Are there enough free cells for an item?
	private bool IsItemCellsEmpty(Item item)
	{
		foreach (var cell in item.itemCells)
		{
			if (!IsCellEmpty(GridVectorToIndexRelative(item.index, cell)))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsOutOfBounds(Vector2 cell)
	{
		// This is simple rectangular checking, will not work if I later have crazy shapes.
		if ((int)cell.X < 0 || (int)cell.X > _gridSize.X - 1 || (int)cell.Y < 0 || (int)cell.Y > _gridSize.Y - 1)
		{
			return true;
		}
		return false;
	}

	private bool IsItemOutOfBounds(Item item)
	{
		foreach (var cell in item.itemCells)
		{
			if (IsOutOfBounds(GridLocalToGlobal(item.index, cell)))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateCells()
	{
		for (int i = 0; i < _grid.Count; i++)
		{
			if (_grid[i] > 0)
			{
				foreach (var item in _items)
				{
					if (item.itemId == _grid[i])
					{
						_gridRects[i].Color = item.itemColor;
					}
				}
			}
			else
			{
				_gridRects[i].Color = _backgroundColor;
			}
		}
	}

	private ColorRect CreateRect(Vector2 pos, Color color)
	{
		var rect = new ColorRect();
		rect.CustomMinimumSize = new Vector2(_cellSize, _cellSize);
		rect.Color = color;
		rect.Position = pos;
		
		return rect;
	}

	public Item GetItem(int idx)
	{
		foreach (Item item in _items)
		{
			foreach (Vector2 cell in item.itemCells)
			{
				if (GridVectorToIndexRelative(item.index, cell) == idx)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool AddItem(int idx, Item item)
	{
		foreach (var cell in item.itemCells)
		{
			var i = GridVectorToIndexRelative(idx, cell);
			if (i < _grid.Count)
			{
				_grid[i] = item.itemId;
			}
			else
			{
				return false;
			}
		}
		item.index = idx;
		_items.Add(item);
		UpdateCells();
		
		return true;
	}

	public bool RemoveItem(Item item)
	{
		for (int i = 0; i < _grid.Count; i++)
		{
			if (_grid[i] == item.itemId)
			{
				_grid[i] = 0;
				_gridRects[i].Color = _backgroundColor;
			}
		}

		var success = _items.Remove(item);
		UpdateCells();
		return success;
	}
}
