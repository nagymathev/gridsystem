using Godot;
using Godot.Collections;

namespace InventorySystem;

[GlobalClass]
public partial class InventoryItem : Node
{
	[Export] public int itemId; // Not unique, inventory dependent.
	[Export] public Vector2 inventoryPosition;
	[Export] public ItemData itemData; // Static data. (Maybe not so static...)

	[Export] public float Rotation; // Rotation in radians.

	public InventoryItem Clone()
	{
		var newItem = new InventoryItem();
		newItem.inventoryPosition = inventoryPosition;
		newItem.itemData = itemData;
		return newItem;
	}

	// Rotates item 90 degrees clockwise.
	public void Rotate()
	{
		var m = Transform2D.Identity;
		m = m.Rotated(Mathf.Pi / 2);

		var cells = new Array<Vector2>();
		var center = Center();
		foreach (var cell in itemData.Cells)
		{
			cells.Add(center + m * (cell - center));
		}

		itemData.Cells = cells;
		GD.Print(itemData.Cells);

		Rotation += Mathf.Pi * 0.5f;
	}

	public Vector2 Center()
	{
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;
		
		foreach (var cell in itemData.Cells)
		{
			minX = Mathf.Min(cell.X, minX);
			maxX = Mathf.Max(cell.X, maxX);
			minY = Mathf.Min(cell.Y, minY);
			maxY = Mathf.Max(cell.Y, maxY);
		}

		Vector2 centre = new Vector2(
			(minX + maxX) / 2,
			(minY + maxY) / 2
		);

		return centre;
	}
}
