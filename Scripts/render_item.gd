# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


class_name RenderItem extends HBoxContainer

@export var path: LineEdit
@export var option_button: OptionButton
@export var image_num: SpinBox
@export var resolution_percent: SpinBox
@export var searching_panel: Control
@export var img_override: OptionButton
@export var anim_override: OptionButton
@export var search_label: Label
@export var scene_selection: OptionButton
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
		scene_selection.set_scenes()
var cameras = []
			
func _ready():
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
		get_tree().get_first_node_in_group("Base").notify("File is being read. 
		Please wait for it to finish loading before removing it from the queue.")
	else:
		queue_free()


func _on_path_line_text_changed(new_text):
	if new_text.ends_with(".blend") and FileAccess.file_exists(new_text):
		path.add_theme_color_override("font_color", Color.from_string(searching_sel, Color.WHITE))
		searching_panel.visible = true
		get_tree().get_first_node_in_group("Mono").FileAdded() # (path.text, name)
	else:
		frame_current = -111
		frame_end = -111
		frame_start = -111
		path.add_theme_color_override("font_color", Color.from_string(bad_sel, Color.WHITE))
		searching_panel.visible = false


func _on_scene_selection_item_selected(index):
	if index != last_scene_index:
		last_scene_index = index
		searching_panel.visible = true
		get_tree().get_first_node_in_group("Mono").FileAdded()
