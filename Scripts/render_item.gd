# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


class_name RenderItem extends Control

@export var path: LineEdit
@export var option_button: OptionButton
@export var image_num: SpinBox
@export var resolution_percent: SpinBox
@export var searching_panel: Control
@export var img_override: OptionButton
@export var anim_override: OptionButton
@export var search_label: Label
@export var scene_selection: OptionButton
@export var selected_style_box: StyleBoxFlat
#@export var scene_popup: PopupMenu
@export var scene_popup_menu: MenuButton
@onready var base: BaseGUI = get_tree().get_first_node_in_group("Base")
@onready var mono = get_tree().get_first_node_in_group("Mono")
var last_scene_index: int = 0

var bad_sel = "bd516d"
var searching_sel = "fffbde"
var good_sel = "b0d18b"

var frame_start = -111
var frame_end = -111
var frame_current = -111:
	set(fc):
		frame_current = fc
		if fc != -111:
			path.add_theme_color_override("font_color", Color.from_string(good_sel, Color.WHITE))
			searching_panel.visible = false
			
var locked = false:
	set(l):
		locked = l
		if locked:
			search_label.locked = "L"
		else:
			search_label.locked = ""
			get_tree().get_first_node_in_group("Mono").FileAdded()
			
var scenes = []:
	set(s):
		scenes = s
		scene_popup_menu.set_scenes()
var cameras = []
			
func _ready():
	set_physics_process(false)
	img_override.visible = false
	anim_override.visible = true
	
	
	


func _on_option_button_item_selected(index):
	if index == 1:
		image_num.visible = true
		img_override.visible = true
		anim_override.visible = false
	else:
		image_num.visible = false
		img_override.visible = false
		anim_override.visible = true


func _on_close_pressed():
	if locked:
		base.notify("File is being read. 
		Please wait for it to finish loading before removing it from the queue.")
	else:
		queue_free()


func _on_path_line_text_changed(new_text):
	if new_text.ends_with(".blend") and FileAccess.file_exists(new_text):
		path.add_theme_color_override("font_color", Color.from_string(searching_sel, Color.WHITE))
		searching_panel.visible = true
		mono.FileAdded() # (path.text, name)
	else:
		frame_current = -111
		frame_end = -111
		frame_start = -111
		path.add_theme_color_override("font_color", Color.from_string(bad_sel, Color.WHITE))
		searching_panel.visible = false


#func _on_scene_selection_item_selected(index):
#	if index != last_scene_index:
#		last_scene_index = index
#		searching_panel.visible = true
#		mono.FileAdded()


func _on_grab_button_down():
	set_physics_process(true)
	add_theme_stylebox_override("panel", selected_style_box)


func _on_grab_button_up():
	set_physics_process(false)
	remove_theme_stylebox_override("panel")
	
func _physics_process(_delta):
#	prints(position, get_global_mouse_position(), global_position)
	if get_global_mouse_position().y > global_position.y + size.y:
		var my_index
		for i in base.render_item_container.get_child_count():
			if base.render_item_container.get_child(i) == self:
				my_index = i
		base.render_item_container.move_child(self, my_index + 1)
	elif get_global_mouse_position().y < global_position.y - size.y:
		var my_index
		for i in base.render_item_container.get_child_count():
			if base.render_item_container.get_child(i) == self:
				my_index = i
		base.render_item_container.move_child(self, my_index - 1)





#func _on_popup_menu_id_pressed(id):
#	scene_popup_menu.get_popup().set_item_checked(id, not scene_popup_menu.get_popup().is_item_checked(id))
