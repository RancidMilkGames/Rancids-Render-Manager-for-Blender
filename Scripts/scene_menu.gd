extends MenuButton

@export var render_item: Control

func _ready():
	get_popup().id_pressed.connect(func(id): get_popup().set_item_checked(id, not get_popup().is_item_checked(id)))
	get_popup().hide_on_checkable_item_selection = false
	get_popup().hide_on_item_selection = false


	

func set_scenes():
	get_popup().clear()
	var check_first_one = true
	for scene in render_item.scenes:
		get_popup().add_check_item(scene)
		if check_first_one:
			check_first_one = false
			get_popup().set_item_checked(0, true)
	#select(render_item.last_scene_index)
	visible = true

#func set_scenes():
#	var already_checked = []
#	var check_first_one = false
#	for i in get_popup().item_count:
#		if get_popup().is_item_checked(i):
#			already_checked.append(get_popup().get_item_text(i))
#	get_popup().clear()
#	if already_checked.size() <= 0:
#		check_first_one = true
#	var count = 0
#	for scene in render_item.scenes:
#		get_popup().add_check_item(scene)
#		if check_first_one:
#			check_first_one = false
#			get_popup().set_item_checked(count, true)
#		if scene in already_checked:
#			get_popup().set_item_checked(count, true)
#		count += 1
#	#select(render_item.last_scene_index)
#	visible = true
