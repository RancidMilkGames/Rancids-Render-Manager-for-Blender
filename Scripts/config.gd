# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 

class_name Config extends Node

@export var base: BaseGUI
@export var mono: Node
@onready var config = ConfigFile.new()
@export var current_prog_bar: Control

@export var persist_close_box: CheckBox
@export var shutdown_finish_box: CheckBox
@export var thread_box: CheckBox
@export var current_progress_box: CheckBox
@export var max_log_lines_box: SpinBox

var persist_on_close = false
var shutdown_finish = false
var separate_thread = true
var current_progress = true:
	set(c):
		current_progress = c
		current_prog_bar.visible = current_progress
var max_log_lines: int = 100

func load_config():
	if FileAccess.file_exists("user://config.cfg"):
		config.load("user://config.cfg")
	else:
		save_config()
	
	base.install_loc_line.text = config.get_value("settings", "install_path")
	persist_on_close = config.get_value("settings", "persist_on_close")
	shutdown_finish = config.get_value("settings", "shutdown_finish")
	separate_thread = config.get_value("settings", "separate_thread")
	current_progress = config.get_value("settings", "current_progress")
	max_log_lines = config.get_value("settings", "max_log_lines")
	
	if config.has_section_key("settings", "override_dest"):
		base.override_dest.text = config.get_value("settings", "override_dest")
	else:
		save_config()
	
	persist_close_box.set_pressed_no_signal(persist_on_close)
	shutdown_finish_box.set_pressed_no_signal(shutdown_finish)
	thread_box.set_pressed_no_signal(separate_thread)
	current_progress_box.set_pressed_no_signal(current_progress)
	max_log_lines_box.value = max_log_lines
	
	
	mono.UpdateConfig()

func save_config():

	config.set_value("settings", "install_path", base.install_loc_line.text)
	config.set_value("settings", "persist_on_close", persist_on_close)
	config.set_value("settings", "shutdown_finish", shutdown_finish)
	config.set_value("settings", "separate_thread", separate_thread)
	config.set_value("settings", "current_progress", current_progress)
	config.set_value("settings", "max_log_lines", max_log_lines)
	config.set_value("settings", "override_dest", base.override_dest.text)

	mono.UpdateConfig()
	config.save("user://config.cfg")


func _on_kill_w_program_toggled(button_pressed):
	persist_on_close = button_pressed
	save_config()


func _on_shutdown_on_finish_toggled(button_pressed):
	shutdown_finish = button_pressed
	save_config()


func _on_use_thread_toggled(button_pressed):
	separate_thread = button_pressed
	save_config()


func _on_current_progress_toggled(button_pressed):
	current_progress = button_pressed
	save_config()


func _on_max_lines_spin_box_value_changed(value):
	max_log_lines = value
	save_config()


func _on_override_location_text_changed(new_text):
	if DirAccess.dir_exists_absolute(new_text):
		save_config()
