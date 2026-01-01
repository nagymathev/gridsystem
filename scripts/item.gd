extends Node
class_name Item

## Unique ID created upon adding the item to the inventory.
## Only used for identifying which cells belong to which item as a whole.
@export var item_id: int
@export var index: int ## Index of the first cell of the item on the inventory grid.
@export var item_name: String
@export var item_image: Texture2D
@export var item_color: Color:
	set(value):
		item_color = value
		item_color_busy = item_color.darkened(0.4)

@export var item_color_busy: Color
@export var item_cells: Array[Vector2]

func _init(id: int = 0, name: String = "default", cells: Array[Vector2] = [Vector2(0,0)]) -> void:
	self.item_id = id
	self.item_name = name
	self.item_cells = cells