using Godot;
using Godot.Collections;

namespace InventorySystem;

[GlobalClass]
public partial class Inventory : Control
{
	[Export] private Color _backgroundColor = new Color("#292831");
	[Export] private Texture2D _backgroundTexture;
	
	// Data
	[Export] private Array<InventoryItem> _items = new Array<InventoryItem>();
	[Export] private Array<int> _grid = new Array<int>();
	[Export] private Vector2I _gridSize = new Vector2I(10, 10);
	[Export] private int _cellSize = 30;
	[Export] private int _cellGapSize = 2;
	private Grid<InventoryItem> _itemGrid; // Instantiate it later using gridsize variable.

	// Background Rendering
	private Array<TextureRect> _gridRects = new Array<TextureRect>();

	// Dragging (Mouse) Data
	private InventoryItem _draggedInventoryItem;
	private Array<TextureRect> _draggedItemRender = new Array<TextureRect>();
	private bool _isDragging = false;

	[Export] public Array<InventoryItem> _startingItems = new Array<InventoryItem>();

	[Signal] public delegate void OnDragStartEventHandler();
	[Signal] public delegate void OnDragEndEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_itemGrid = new Grid<InventoryItem>(_gridSize.X, _gridSize.Y);
		
		if (Engine.IsEditorHint())
		{
			if (_items.Count > 0)
			{
				_items.Clear();
			}

			if (_grid.Count > 0)
			{
				_grid.Clear();
			}
		}
		
		OnDragStart += () => {
			_isDragging = true;
			PickItem(ScreenSpacePixelToGridPosition(GetLocalMousePosition()));
		};
		OnDragEnd += () => {
			_isDragging = false;
			PlaceItem(ScreenSpacePixelToGridPosition(GetLocalMousePosition()));
		};

		var totalGridSize = _gridSize.X * _gridSize.Y;

		_grid.Resize(totalGridSize);
		_grid.Fill(0);

		_gridRects.Resize(totalGridSize);

		// Background Rects
		for (int i = 0; i < totalGridSize; i++)
		{
			var rect = CreateCell(GridIndexToVector(i) * (_cellSize + _cellGapSize), _backgroundTexture);
			AddChild(rect);
			_gridRects[i] = rect;
		}

		foreach (var item in _startingItems)
		{
			AddItem(item, item.inventoryPosition);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_isDragging && _draggedInventoryItem != null)
		{
			RenderItem(_draggedInventoryItem);
		}
	}

    public override void _Input(InputEvent @event)
    {
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

	private void PickItem(Vector2 pos)
	{
		_draggedInventoryItem = GetItem(pos);
		if (_draggedInventoryItem == null)
		{
			return;
		}

		RemoveItem(_draggedInventoryItem);

		// This creates the hovering look.
		// TODO: Remove this and implement IRenderable on items.
		var mousePos = GetLocalMousePosition();
		foreach (var cell in _draggedInventoryItem.itemData.Cells)
		{
			var rect = CreateCell(mousePos + (cell * (_cellSize / 2)), _draggedInventoryItem.itemData.Texture);
			_draggedItemRender.Add(rect);
			AddChild(rect);
		}
	}

	private void PlaceItem(Vector2 pos)
	{
		if (_draggedInventoryItem == null)
		{
			return;
		}

		AddItem(_draggedInventoryItem, pos);
		_draggedInventoryItem = null;

		// TODO: Remove this after implementing IRenderable on the InventoryItems.
		foreach (var rect in _draggedItemRender)
		{
			RemoveChild(rect);
			rect.QueueFree();
		}
		_draggedItemRender.Clear();
	}

	private void RenderItem(InventoryItem inventoryItem)
	{
		var mousePos = GetLocalMousePosition();
		for (int i = 0; i < inventoryItem.itemData.Cells.Count; i++)
		{
			var rect = _draggedItemRender[i];
			rect.Position = new Vector2(
				(mousePos.X - _cellSize / 2) + inventoryItem.itemData.Cells[i].X * (_cellSize + _cellGapSize),
				(mousePos.Y - _cellSize / 2) + inventoryItem.itemData.Cells[i].Y * (_cellSize + _cellGapSize)
			);
		}
	}

	private int MousePosIndex()
	{
		return GridVectorToIndex(ScreenSpacePixelToGridPosition(GetLocalMousePosition()));
	}

	// Mouse pos on grid.
	private Vector2 ScreenSpacePixelToGridPosition(Vector2 pos)
	{
		var gridPos = new Vector2(
			Mathf.Floor(pos.X / (_cellSize + _cellGapSize)),
			Mathf.Floor(pos.Y / (_cellSize + _cellGapSize))
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

	private bool IsOutOfBounds(Vector2 cell)
	{
		// This is simple rectangular checking, will not work if I later have crazy shapes.
		if ((int)cell.X < 0 || (int)cell.X > _gridSize.X - 1 || (int)cell.Y < 0 || (int)cell.Y > _gridSize.Y - 1)
		{
			return true;
		}
		return false;
	}

	private TextureRect CreateCell(Vector2 pos, Texture2D tex)
	{
		var cell = new TextureRect();
		cell.CustomMinimumSize = new Vector2(_cellSize, _cellSize);
		cell.Texture = tex;
		cell.Position = pos;

		return cell;
	}

	private ColorRect CreateRect(Vector2 pos, Color color)
	{
		var rect = new ColorRect();
		rect.CustomMinimumSize = new Vector2(_cellSize, _cellSize);
		rect.Color = color;
		rect.Position = pos;
		
		return rect;
	}

	public InventoryItem GetItem(Vector2 pos)
	{
		return _itemGrid.Get(pos);
	}

	public void AddItem(InventoryItem item, Vector2 pos)
	{
		foreach (var cell in item.itemData.Cells)
		{
			_itemGrid.Add(pos + cell, item);
		}
	}

	public void RemoveItem(InventoryItem item)
	{
		_itemGrid.Delete(item);
	}
}
