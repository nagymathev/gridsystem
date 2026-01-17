using Godot;
using Godot.Collections;

namespace InventorySystem;

[GlobalClass]
public partial class InventoryItem : Node, IRenderable
{
	[Export] public int itemId; // Not unique, inventory dependent.
	[Export] public Vector2 inventoryPosition;
	[Export] public ItemData itemData; // Static data.

	public InventoryItem Clone()
	{
		var newItem = new InventoryItem();
		newItem.inventoryPosition = inventoryPosition;
		newItem.itemData = itemData;
		return newItem;
	}

	public void RenderSelf()
	{
		throw new System.NotImplementedException();
	}
}
