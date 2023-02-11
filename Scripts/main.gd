# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


class_name BaseGUI extends ColorRect


@onready var dir_access = DirAccess.open("res://")
@onready var file_dialog: FileDialog = FileDialog.new()
@export var config: Config
@export var render_item_scene: PackedScene
@export var render_item_container: Control
@export var add_item_button: Button
@export var install_loc_line: LineEdit
@export var running_panel: Panel
@export var settings: PopupPanel
@export var about: PopupPanel
@export var override_dest: LineEdit
var dir_override_search = false


func _ready():
	add_child(file_dialog)
	file_dialog.access = FileDialog.ACCESS_FILESYSTEM
	file_dialog.set_filters(PackedStringArray(["*.blend ; Blender files"]))
	file_dialog.files_selected.connect(on_files_dropped)
	file_dialog.dir_selected.connect(on_dir_selected)
	file_dialog.file_selected.connect(on_install_file_selected)
	file_dialog.visibility_changed.connect(turn_off_override_search)
	get_viewport().files_dropped.connect(on_files_dropped)
	config.load_config()
	file_dialog.current_dir = OS.get_executable_path()
	settings.hide()
	Input.set_custom_mouse_cursor(load("res://Images/Cursors/cursor-pointer-18-dark.png"), Input.CURSOR_POINTING_HAND, Vector2i(0, 0))


func on_files_dropped(files):
	for f in files:
		if f.to_lower().ends_with(".blend"):
			var item: RenderItem = render_item_scene.instantiate()
			item.path.text = f
			render_item_container.add_child(item)
			item._on_path_line_text_changed(f)


func on_dir_selected(dir):
	if dir_override_search:
		override_dest.text = dir
		return
	
	var files = []
	for f in DirAccess.get_files_at(dir):
		files.append(dir.path_join(f))

	on_files_dropped(files)


func on_install_file_selected(file):
	install_loc_line.text = file
	config.save_config()


func _on_add_folder_pressed():
	set_dir_root()
	file_dialog.file_mode = FileDialog.FILE_MODE_OPEN_DIR
	file_dialog.popup_centered(Vector2i(700, 500))


func _on_add_file_pressed():
	set_dir_root()
	file_dialog.file_mode = FileDialog.FILE_MODE_OPEN_FILES
	file_dialog.popup_centered(Vector2i(700, 500))


func set_dir_root():
	if file_dialog.current_dir.to_lower().contains("Program Files/Blender Foundation".to_lower()):
		file_dialog.current_dir = OS.get_executable_path().replace(OS.get_executable_path().split("/")[-1], "")


func _on_add_pressed():
	var item: RenderItem = render_item_scene.instantiate()
	render_item_container.add_child(item)


func _on_render_item_container_pre_sort_children():
	render_item_container.move_child(add_item_button, -1)


func _on_search_blend_install_pressed():
	if DirAccess.dir_exists_absolute("C:/Program Files/Blender Foundation/"):
		file_dialog.current_dir = "C:/Program Files/Blender Foundation/"
	file_dialog.file_mode = FileDialog.FILE_MODE_OPEN_FILE
	file_dialog.set_filters(PackedStringArray(["*.exe ; Blender Install"]))
	file_dialog.popup_centered(Vector2i(700, 500))
	await file_dialog.visibility_changed
	file_dialog.set_filters(PackedStringArray(["*.blend ; Blender files"]))


func _on_install_loc_text_submitted(_new_text):
	config.save_config()


func _on_settings_pressed():
	settings.popup_centered()


func _on_clear_pressed():
	for c in render_item_container.get_children():
		if c != add_item_button:
			c.queue_free()


func _on_about_pressed():
	about.popup_centered()


func _on_override_search_pressed():
	dir_override_search = true
	set_dir_root()
	file_dialog.file_mode = FileDialog.FILE_MODE_OPEN_DIR
	file_dialog.popup_centered(Vector2i(700, 500))


func turn_off_override_search():
	if not file_dialog.visible:
		await get_tree().create_timer(.1).timeout
		dir_override_search = false


