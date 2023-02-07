# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


extends Label

var dot_count = 1
@export var main_screen: Control
@export var timer: Timer

func _on_timer_timeout():
	dot_count += 1
	text = ".".repeat(wrapi(dot_count, 1, 4))


func _on_main_screen_visibility_changed():
	if main_screen.visible:
		timer.stop()
	else:
		timer.start()
