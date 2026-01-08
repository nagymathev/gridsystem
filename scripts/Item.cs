using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Item : Node
{
	[Export] public int itemId;
	[Export] public int index; // This stores the location in the inventory. No clue why it's here.
	[Export] public String itemName;
	[Export] public Texture2D itemImage;
	[Export] public Color itemColor;
	[Export] public Array<Vector2> itemCells;

	public Item(Item item)
	{
		itemId = item.itemId;
		index = item.index;
		itemName = item.itemName;
		itemImage = item.itemImage;
		itemColor = item.itemColor;
		itemCells = item.itemCells.Duplicate(true);
	}
	public Item(int id, String name, Array<Vector2> cells)
	{
		this.itemId = id;
		this.index = -1;
		this.itemName = name;
		this.itemCells = cells;
	}
}
