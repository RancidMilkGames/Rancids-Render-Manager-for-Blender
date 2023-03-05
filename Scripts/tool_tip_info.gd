# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


extends Control


func _make_custom_tooltip(_for_text):
	var par = get_parent().get_parent()
	var label = load("res://Scenes/custom_tooltip.tscn").instantiate()
	if get_theme_color("font_color").is_equal_approx(par.good_sel):
		label.text = "Start Frame: " + str(par.frame_start) + "\n"
		label.text += "Current Frame: " + str(par.frame_current) + "\n"
		label.text += "End Frame: " + str(par.frame_end)
	elif get_theme_color("font_color").is_equal_approx(par.searching_sel):
		label.text = "Getting .blend file info."
	else:
		label.text = "Blender file is invalid or issue getting file info"
	
	return label
