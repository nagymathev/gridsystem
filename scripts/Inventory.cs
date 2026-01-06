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
			var rect = CreateRect(IndexToVector(i), _backgroundColor);
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
			var gridIndex = VectorToIndex(gridMousePos);

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
			_gridRects[VectorToIndexRelative(_draggedItem.index, cell)].SelfModulate = _draggedItem.itemColor.Darkened(0.2f);
		}

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

		foreach (var cell in _draggedItem.itemCells)
		{
			int cellPos = VectorToIndexRelative(idx, cell);
			if (cellPos > _grid.Count || _grid[cellPos] > 0)
			{
				// Something is in the way or out of bounds
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
		}

		OriginalColor(_draggedItem);

		foreach (var cell in _draggedItem.itemCells)
		{
			_grid[VectorToIndexRelative(_draggedItem.index, cell)] = 0;
			_grid[VectorToIndexRelative(idx, cell)] = _draggedItem.itemId;
		}

		_draggedItem.index = idx;

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
			_gridRects[VectorToIndexRelative(item.index, cell)].SelfModulate = new Color(1, 1, 1, 1);
		}
	}

	private int MousePosIndex()
	{
		return VectorToIndex(ScreenSpacePixelToGridPosition(GetLocalMousePosition()));
	}

	private Vector2 ScreenSpacePixelToGridPosition(Vector2 pos)
	{
		var gridPos = new Vector2(
			Mathf.Clamp(Mathf.Floor(pos.X / (_cellSize + _cellGapSize)), 0.0f, _gridSize.X - 1.0f),
			Mathf.Clamp(Mathf.Floor(pos.Y / (_cellSize + _cellGapSize)), 0.0f, _gridSize.Y - 1.0f)
		);
		return gridPos;
	}

	private int VectorToIndex(Vector2 pos)
	{
		int idx = (int)(pos.X + (_gridSize.X * pos.Y));
		return idx;
	}

	private int VectorToIndexRelative(int idx, Vector2 offset)
	{
		return idx + VectorToIndex(offset);
	}

	private Vector2 IndexToVector(int idx)
	{
		return new Vector2(idx % _gridSize.X, Mathf.Floor(idx / _gridSize.X)) * (_cellSize + _cellGapSize);
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
				if (VectorToIndexRelative(item.index, cell) == idx)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool AddItem(int idx, Item item)
	{
		item.index = idx;
		foreach (var cell in item.itemCells)
		{
			_grid[VectorToIndexRelative(item.index, cell)] = item.itemId;
		}
		UpdateCells();
		_items.Add(item);
		
		return false;
	}
}
