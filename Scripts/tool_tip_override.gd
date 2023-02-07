# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


extends Control


func _make_custom_tooltip(for_text):
	var label = load("res://Scenes/custom_tooltip.tscn").instantiate()
	label.text = for_text
	return label