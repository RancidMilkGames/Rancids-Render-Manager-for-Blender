# This file is part of Rancid's Render Manager.

# Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

# Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

# You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 


extends Label

var lerp_to
var dot_count = 0

func _ready():
	lerp_to = Color("5378a7")
	set_physics_process(false)
	get_node("Timer").stop()

func _physics_process(delta):
	lerp(get_theme_color("font_color"), lerp_to, delta * 50)
	if get_theme_color("font_color").is_equal_approx(lerp_to):
		if lerp_to == Color("5378a7"):
			lerp_to = Color("383552")
		if lerp_to == Color("383552"):
			lerp_to = Color("5378a7")


func _on_panel_container_visibility_changed():
	if visible:
		set_physics_process(true)
		get_node("Timer").start()
	else:
		set_physics_process(false)
		get_node("Timer").stop()


func _on_timer_timeout():
	var t = "Reading"
	dot_count += 1
	if dot_count >= 4:
		dot_count = 0
	text = t + ".".repeat(dot_count)
