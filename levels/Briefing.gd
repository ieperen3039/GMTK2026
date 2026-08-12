class_name Briefing
extends Control

var start_button: Button:
	get: return get_node("%ButtonStart")

var main_menu_button: Button:
	get: return get_node("%ButtonMainMenu")
