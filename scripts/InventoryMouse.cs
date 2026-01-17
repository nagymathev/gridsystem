using Godot;
using System;

namespace InventorySystem;

[GlobalClass]
public partial class InventoryMouse : Node
{
    [Export] private InventoryItem _draggedItem;
    [Export] private bool _isDragging;
    [Export] private Inventory _currentInventory;
    private Grid<TextureRect> _renderDraggedItem;
    
    [Signal] public delegate void OnDragStartEventHandler();
    [Signal] public delegate void OnDragEndEventHandler();
    
    // TODO: Identify which inventory the mouse is hovering over.
    public void SetInventory(Inventory inventory)
    {
        _currentInventory = inventory;
    }

    public override void _Ready()
    {
        OnDragStart += () =>
        {
            _isDragging = true;
            PickItem(InventoryPosition(_currentInventory.GetLocalMousePosition()));
        };
        
        OnDragEnd += () =>
        {
            _isDragging = false;
            PlaceItem(InventoryPosition(_currentInventory.GetLocalMousePosition()));
        };
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
        _draggedItem = _currentInventory.GetItem(pos);
        if (_draggedItem == null)
        {
            return;
        }

        _currentInventory.RemoveItem(_draggedItem);
    }

    private void PlaceItem(Vector2 pos)
    {
        if (_draggedItem == null)
        {
            return;
        }

        _currentInventory.AddItem(_draggedItem, pos);
        _draggedItem = null;
    }

    private Vector2 InventoryPosition(Vector2 globalPos)
    {
        var gridPos = new Vector2(
            Mathf.Floor(globalPos.X / (_currentInventory.inventoryParameters.cellSize + _currentInventory.inventoryParameters.cellGapSize)),
            Mathf.Floor(globalPos.Y / (_currentInventory.inventoryParameters.cellSize + _currentInventory.inventoryParameters.cellGapSize))
        );
        return gridPos;
    }
}
