# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


extends ScrollContainer

@export var output_container: VBoxContainer
@export var follow_bottom: bool = true

func _ready():
	get_v_scroll_bar().scrolling.connect(scroll)


func scroll():
	follow_bottom = output_container.size.y - scroll_vertical < size.y + 1


func _on_output_container_resized():
	if follow_bottom:
		@warning_ignore("narrowing_conversion")
		scroll_vertical = output_container.size.y
