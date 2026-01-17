using Godot;
using Godot.Collections;

namespace InventorySystem;

public partial class InventoryManager : Node
{
    [Signal]
    public delegate void MouseEnterInventoryEventHandler(Inventory inventory);
    [Signal]
    public delegate void MouseExitInventoryEventHandler(Inventory inventory);

    [Export] private InventoryMouse _mouse;
    [Export] private Array<Inventory> _inventories;

    public override void _Ready()
    {
        _mouse = GetNode<InventoryMouse>("InventoryMouse");
        _inventories = new Array<Inventory>();
        for (int i = 0; i < GetChildCount(); i++)
        {
            if (GetChildOrNull<Inventory>(i) != null)
            {
                _inventories.Add(GetChild<Inventory>(i));
            }
        }

        foreach (var inventory in _inventories)
        {
            inventory.MouseEntered += () =>
            {
                EmitSignal(SignalName.MouseEnterInventory, inventory);
            };
        }

        MouseEnterInventory += inventory =>
        {
            _mouse.SetInventory(inventory);
            GD.Print("Mouse inventory changed");
        };
    }
}