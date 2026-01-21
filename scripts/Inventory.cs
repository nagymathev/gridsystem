using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace InventorySystem;

public struct InventoryParameters
{
	public int sizeX;
	public int sizeY;
	public int cellSize;
	public int cellGapSize;
}

[GlobalClass]
public partial class Inventory : Control
{
	[Export] private Color _backgroundColor = new Color("#292831");
	[Export] private Texture2D _backgroundTexture;
	private TextureRect _backgroundRect;
	private InventoryRenderer _renderer;
	
	// Data
	public InventoryParameters inventoryParameters { get; }= new()
	{
		sizeX = 10,
		sizeY = 10,
		cellSize = 30,
		cellGapSize = 2
	};

	private Array<InventoryItem> _items;
	private Grid<InventoryItem> _itemGrid; // Instantiate it later using gridsize variable.

	[Export] public Array<InventoryItem> _startingItems = new Array<InventoryItem>();
	
	public override void _Ready()
	{
		_items = new Array<InventoryItem>();
		
		_itemGrid = new Grid<InventoryItem>(inventoryParameters.sizeX, inventoryParameters.sizeY);
		_renderer = new InventoryRenderer();
		_renderer.Inventory = this;
		_renderer.SetZAsRelative(true);
		_renderer.SetZIndex(10);
		AddChild(_renderer);

		// Background
		_backgroundRect = new TextureRect();
		_backgroundRect.Texture = _backgroundTexture;
		_backgroundRect.SetStretchMode(TextureRect.StretchModeEnum.Tile);
		Vector2 backgroundSize = new Vector2(
			inventoryParameters.sizeX * (inventoryParameters.cellSize + inventoryParameters.cellGapSize),
			inventoryParameters.sizeY * (inventoryParameters.cellSize + inventoryParameters.cellGapSize)
		);
		_backgroundRect.SetSize(backgroundSize);
		AddChild(_backgroundRect);

		// Dummy Data
		foreach (var item in _startingItems)
		{
			AddItem(item, item.inventoryPosition);
		}

		_renderer.RenderItems(_items);
	}

	public override void _Process(double delta)
	{
		_renderer.Flush();
		_renderer.RenderItems(_items);
	}

	private Vector2 GridIndexToVector(int idx)
	{
		return new Vector2(idx % inventoryParameters.sizeX, Mathf.Floor(idx / inventoryParameters.sizeX));
	}

	private TextureRect CreateCell(Vector2 pos, Texture2D tex)
	{
		var cell = new TextureRect();
		cell.CustomMinimumSize = new Vector2(inventoryParameters.cellSize, inventoryParameters.cellSize);
		cell.Texture = tex;
		cell.Position = pos;

		return cell;
	}

	public InventoryItem GetItem(Vector2 pos)
	{
		return _itemGrid.Get(pos);
	}

	public bool AddItem(InventoryItem item, Vector2 pos, bool preview = false)
	{
		if (!_itemGrid.IsFree(item.itemData.Cells.Select(p => p + pos))) // Need to offset
		{
			return false;
		}

		// After this point we know that we can place the item.
		if (preview)
		{
			return true;
		}
		
		foreach (var cell in item.itemData.Cells)
		{
			_itemGrid.Add(pos + cell, item);
		}

		item.inventoryPosition = pos;
		_items.Add(item);
		return true;
	}

	public void RemoveItem(InventoryItem item)
	{
		_itemGrid.Delete(item);
		item.inventoryPosition = Vector2.Zero;
		_items.Remove(item);
	}
}