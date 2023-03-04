extends OptionButton

@export var render_item: Control

func _ready():
	visible = false

func set_scenes():
	clear()
	for scene in render_item.scenes:
		add_item(scene)
	select(render_item.last_scene_index)
	visible = true
		
