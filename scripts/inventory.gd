extends Control
class_name Inventory

@export var background_color: Color = Color("#292831")
@export var item_color: Color = Color("#fbbbad")

@export var items: Array[Item] ## Stores pointers to the items themselves.
@export var grid: Array[int] ## Stores IDs of the items. Can be used to find the actual Item.
@export var grid_size: Vector2i = Vector2i(10, 10)
var total_grid_slots := grid_size.x * grid_size.y
@export var cell_size: int = 30
@export var cell_gap_size: int = 2

var grid_rects: Array[ColorRect] ## Used for rendering
var previous_cell: Node
var previous_color: Color

var dragged_item: Item
var dragged_item_render: Array[ColorRect]
var is_dragging: bool = false

signal drag_start
signal drag_end

func _on_drag_start() -> void:
	is_dragging = true
	pick_item_at(mouse_pos_index())

func _on_drag_end() -> void:
	is_dragging = false
	place_item_at(mouse_pos_index())

func _ready() -> void:
	drag_start.connect(_on_drag_start)
	drag_end.connect(_on_drag_end)

	grid.resize(total_grid_slots)
	grid.fill(0)
	
	grid_rects.resize(total_grid_slots)
	
	for i in total_grid_slots:
		var rect := create_rect(index_to_vector(i))
		self.add_child(rect)
		grid_rects[i] = rect
	
	var item1 := Item.new(1, "something", [Vector2(0, 0)])
	item1.index = 1
	item1.item_color = Color("#4a7a96")
	append_item(item1)
	var item2 := Item.new(2, "else", [Vector2(0, 0), Vector2(0, 1)])
	item2.index = 3
	item2.item_color = Color("#ee8695")
	append_item(item2)

	update_cells()

func _process(delta: float) -> void:
	if is_dragging and dragged_item:
		render_item(dragged_item)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		var mouse_pos = (event as InputEventMouseMotion).position
		var grid_mouse_pos := screen_space_pixel_to_grid_position(mouse_pos)
		var grid_index := vector_to_index(grid_mouse_pos)
		
		if grid_mouse_pos.x < grid_size.x and grid_mouse_pos.x >= 0 and grid_mouse_pos.y >= 0 and grid_mouse_pos.y < grid_size.y:
			if previous_cell != grid_rects[grid_index]:
				if previous_cell:
					previous_cell.color = previous_color
				previous_cell = grid_rects[grid_index]
				previous_color = grid_rects[grid_index].color
			grid_rects[grid_index].color = Color("#f32f")
	
	if event is InputEventMouseButton:
		if (event as InputEventMouseButton).button_index == MouseButton.MOUSE_BUTTON_LEFT and (event as InputEventMouseButton).pressed == true:
			drag_start.emit()
		if (event as InputEventMouseButton).button_index == MouseButton.MOUSE_BUTTON_LEFT and (event as InputEventMouseButton).pressed == false:
			drag_end.emit()

func pick_item_at(idx: int) -> void:
	var remove_at: int = -1
	for i in items.size():
		for cell in items[i].item_cells:
			if (vector_to_index_relative(items[i].index, cell) == idx):
				remove_at = i
				break
		
	if remove_at < 0:
		return
	
	# Not removing it anymore.
	# dragged_item = items.pop_at(remove_at)
	dragged_item = items[remove_at]

	# for cell in dragged_item.item_cells:
	# 	grid[dragged_item.index + vector_to_index(cell)] = 0
	
	for cell in dragged_item.item_cells:
		grid_rects[vector_to_index_relative(dragged_item.index, cell)].self_modulate = dragged_item.item_color.darkened(0.2)
	
	previous_color = dragged_item.item_color
	update_cells()

	# For rendering the picked up item.
	var mouse_pos := get_local_mouse_position()
	for cell in dragged_item.item_cells:
		var rect := create_rect(mouse_pos + (cell * (cell_size / 2)), dragged_item.item_color)
		dragged_item_render.append(rect)
		add_child(rect)

func place_item_at(idx: int) -> void:
	if !dragged_item:
		return
	
	for cell in dragged_item.item_cells:
		var cell_pos := vector_to_index_relative(idx, cell)
		if  cell_pos > grid.size() or grid[cell_pos] > 0:
			# This is when we need to return the item to its original location.
			original_color(dragged_item)
			for rect in dragged_item_render:
				remove_child(rect)
				rect.queue_free()
			dragged_item_render.clear()
			dragged_item = null
			return # There's something where we're trying to place the item
	
	original_color(dragged_item)

	# Beyond this point we're certain that we can place the item.
	for cell in dragged_item.item_cells:
		grid[dragged_item.index + vector_to_index(cell)] = 0
		grid[vector_to_index_relative(idx, cell)] = dragged_item.item_id
	
	dragged_item.index = idx # I have no idea at this point why I'm have this variable
	# items.append(dragged_item)

	

	previous_color = dragged_item.item_color
	
	dragged_item = null
	update_cells()

	# Removing the dragged item rendering.
	for rect in dragged_item_render:
		remove_child(rect)
		rect.queue_free()
	dragged_item_render.clear()

func original_color(_item: Item) -> void:
	for cell in _item.item_cells:
		grid_rects[vector_to_index_relative(_item.index, cell)].self_modulate = Color(1, 1, 1, 1)

func create_rect(pos: Vector2 = Vector2.ZERO, color: Color = background_color) -> ColorRect:
	var crect := ColorRect.new()
	crect.custom_minimum_size = Vector2(cell_size, cell_size)
	crect.color = color
	crect.position = pos

	return crect

func render_item(item: Item) -> void:
	var mouse_pos := get_local_mouse_position()

	for i in item.item_cells.size():
		var rect := dragged_item_render[i]
		rect.position = Vector2(
			(mouse_pos.x - cell_size / 2) + item.item_cells[i].x * (cell_size + cell_gap_size),
			(mouse_pos.y - cell_size / 2) + item.item_cells[i].y * (cell_size + cell_gap_size)
		)
		rect.color = item.item_color

func mouse_pos_index() -> int:
	return vector_to_index(screen_space_pixel_to_grid_position(get_local_mouse_position()))

## Converts pixels to grid positions.
func screen_space_pixel_to_grid_position(pos: Vector2) -> Vector2:
	var grid_pos := Vector2(
		clamp(floor(pos.x / (cell_size + cell_gap_size)), 0, grid_size.x - 1),
		clamp(floor(pos.y / (cell_size + cell_gap_size)), 0, grid_size.y - 1)
	)
	# print("[DEBUG]: (SS: ", pos, ") -> (GP: ", grid_pos, ")")
	return grid_pos

## Transforms index + relative vector into index
func vector_to_index_relative(cell_index: int, current_cell: Vector2i) -> int:
	return cell_index + vector_to_index(current_cell)

## Converts cell indexes to grid space vector positions.
func index_to_vector(index: int) -> Vector2:
	return Vector2(index % grid_size.x, floor(index / grid_size.x)) * (cell_size + cell_gap_size)

## Converts grid space vector positions to cell indexes.
func vector_to_index(pos: Vector2) -> int:
	var idx := pos.x + (grid_size.x * pos.y)
	# print("[DEBUG]: (VEC: ", pos, ") -> (IDX: ", idx, ")")
	return idx

func update_cells() -> void:
	for i in grid.size():
		if grid[i] > 0:
			for item in items:
				if item.item_id == grid[i]:
					grid_rects[i].color = item.item_color
		else:
			grid_rects[i].color = background_color

func append_item(item: Item) -> void:
	for cell in item.item_cells:
		grid[vector_to_index_relative(item.index, cell)] = item.item_id
		update_cells()
	items.append(item)
