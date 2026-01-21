using Godot;
using System;

namespace InventorySystem;

[GlobalClass]
public partial class InventoryMouse : Node
{
    [Export] private InventoryItem _draggedItem;
    [Export] private bool _isDragging;
    [Export] private Inventory _currentInventory;
    private TextureRect _renderDraggedItem;
    
    // For moving the held item back
    private Vector2 _originalLocation;
    private Inventory _originalInventory;
    
    [Signal] public delegate void OnDragStartEventHandler();
    [Signal] public delegate void OnDragEndEventHandler();
    
    // TODO: Identify which inventory the mouse is hovering over.
    public void SetInventory(Inventory inventory)
    {
        _currentInventory = inventory;
    }

    public override void _Ready()
    {
        _renderDraggedItem = new TextureRect();
        _renderDraggedItem.ZIndex = 15;
        _renderDraggedItem.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_renderDraggedItem);
        
        OnDragStart += () =>
        {
            _isDragging = true;
            PickItem(InventoryPosition(_currentInventory.GetLocalMousePosition()), _currentInventory);
        };
        
        OnDragEnd += () =>
        {
            _isDragging = false;
            PlaceItem(InventoryPosition(_currentInventory.GetLocalMousePosition()), _currentInventory);
        };
    }

    public override void _Process(double delta)
    {
        if (_isDragging && _draggedItem != null)
        {
            _renderDraggedItem.GlobalPosition = _currentInventory.GetGlobalMousePosition() -
                                                Vector2.One * (_currentInventory.inventoryParameters.cellSize + _currentInventory.inventoryParameters.cellGapSize) / 2;
            
            // Visualizing whether we can place the item.
            var canPlace = PlaceItem(InventoryPosition(_currentInventory.GetLocalMousePosition()), _currentInventory, true);
            _renderDraggedItem.Modulate = canPlace ? new Color("#0f0") :  new Color("#f00");
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
    
    private void PickItem(Vector2 pos, Inventory inventory)
    {
        _draggedItem = inventory.GetItem(pos);
        if (_draggedItem == null)
        {
            return;
        }

        _originalLocation = _draggedItem.inventoryPosition;
        _originalInventory = inventory;
        inventory.RemoveItem(_draggedItem);

        _renderDraggedItem.Texture = _draggedItem.itemData.Texture;
    }

    private bool PlaceItem(Vector2 pos, Inventory inventory, bool preview = false)
    {
        if (_draggedItem == null)
        {
            return false;
        }

        if (preview)
        {
            return inventory.AddItem(_draggedItem, pos, true);
        }

        var success = inventory.AddItem(_draggedItem, pos);
        if (success)
        {
            _draggedItem = null;
            _renderDraggedItem.Texture = null;
        }
        else
        {
            PlaceItem(_originalLocation, _originalInventory);
        }

        return true;
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
