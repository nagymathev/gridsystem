using Godot;
using Godot.Collections;

namespace InventorySystem;

public partial class InventoryRenderer : Control
{
    private Inventory _inventory;
    public Inventory Inventory
    {
        set => _inventory = value;
    }

    public void RenderItems(Array<InventoryItem> items)
    {
        foreach (var item in items)
        {
            var rect = CreateRect();
            rect.Texture = item.itemData.Texture;
            rect.GlobalPosition = (item.inventoryPosition * (_inventory.inventoryParameters.cellSize +
                                                             _inventory.inventoryParameters.cellGapSize));
            rect.PivotOffset = rect.Size / 2;
            rect.Rotation = item.Rotation;
            AddChild(rect);
        }
    }

    public void Flush()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
        }
    }

    private TextureRect CreateRect()
    {
        var rect = new TextureRect();
        rect.MouseFilter = MouseFilterEnum.Ignore;
        return rect;
    }
}