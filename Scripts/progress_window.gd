extends Window

@export var progress_control: Control
@export var tab_cont: TabContainer
var is_child

# Called when the node enters the scene tree for the first time.
func _ready():
	pass



func _on_close_requested():
#	mode = Window.MODE_MINIMIZED
	remove_child(progress_control)
	tab_cont.add_child(progress_control)
	hide()
	



func _on_about_to_popup():
	if not is_ancestor_of(progress_control):
		tab_cont.remove_child(progress_control)
		add_child(progress_control)
		progress_control.visible = true
