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
    private Control _renderContainer;

    [Export] private Color _positiveActionColor;
    [Export] private Color _negativeActionColor;
    
    // For moving the held item back
    private Vector2 _originalLocation;
    private Inventory _originalInventory;
    
    [Signal] public delegate void OnDragStartEventHandler();
    [Signal] public delegate void OnDragEndEventHandler();
    [Signal] public delegate void OnItemRotateEventHandler();
    
    public void SetInventory(Inventory inventory)
    {
        _currentInventory = inventory;
    }

    public override void _Ready()
    {
        _renderContainer = new Control();
        _renderContainer.PivotOffset = new Vector2(32, 32);
        AddChild(_renderContainer);
        _renderDraggedItem = new TextureRect();
        _renderDraggedItem.PivotOffset = new Vector2(32, 32);
        _renderDraggedItem.ZIndex = 15;
        _renderDraggedItem.MouseFilter = Control.MouseFilterEnum.Ignore;
        _renderContainer.AddChild(_renderDraggedItem);
        
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

        OnItemRotate += () =>
        {
            if (_isDragging != null && _draggedItem != null)
            {
                _draggedItem.Rotate();
                _renderDraggedItem.SetRotation(_draggedItem.Rotation);
                
            }
        };
    }

    public override void _Process(double delta)
    {
        if (_isDragging && _draggedItem != null)
        {
            _renderDraggedItem.Position = _currentInventory.GetGlobalMousePosition() -
                                                (new Vector2(0.5f, 0.5f) + _draggedItem.Center()) *
                                                (_currentInventory.inventoryParameters.cellSize + _currentInventory.inventoryParameters.cellGapSize);
            
            // Visualizing whether we can place the item.
            var canPlace = PlaceItem(InventoryPosition(_currentInventory.GetLocalMousePosition()), _currentInventory, true);
            _renderDraggedItem.Modulate = canPlace ? _positiveActionColor :  _negativeActionColor;
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
            else if (mb.ButtonIndex == MouseButton.Left && !mb.Pressed)
            {
                EmitSignal(SignalName.OnDragEnd);
            }

            if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
            {
                EmitSignal(SignalName.OnItemRotate);
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
        _renderDraggedItem.SetRotation(_draggedItem.Rotation);
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
            _renderDraggedItem.SetRotation(0);
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
