using Godot;
using Godot.Collections;
using System;

namespace InventorySystem;

[GlobalClass]
public partial class InventoryItem : Node
{
	[Export] public int itemId; // Not unique, inventory dependent.
	[Export] public int index; // This stores the location in the inventory. No clue why it's here.
	
	[Export] public ItemData itemData;
	public InventoryItem Clone()
	{
		var newItem = new InventoryItem();
		newItem.index = index;
		newItem.itemData = itemData;
		return newItem;
	}
}
