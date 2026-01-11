using System;
using Godot;
using Godot.Collections;

namespace InventorySystem;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public String Name { get; set; }
    [Export] public Array<Vector2> Cells { get; set; }
    [Export] public Color Color { get; set; }
    [Export] public Texture2D Texture { get; set; }

    public ItemData() : this(null, null, Colors.Fuchsia, null) {}

    public ItemData(String name, Array<Vector2> cells, Color color, Texture2D texture)
    {
        Name = name ?? String.Empty;
        Cells = cells;
        Color = color;
        Texture = texture;
    }
}
