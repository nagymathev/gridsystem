using Godot;
using System;

namespace InventorySystem;

public partial class InventoryMouse : Node
{
    [Export] private InventoryItem _draggedItem;
    [Export] private bool _isDragging;
    private Grid<TextureRect> _renderDraggedItem;
    
    // TODO: Identify which inventory the mouse is hovering over.
}
