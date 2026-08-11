class_name TitleScreen
extends Control

signal level_selected(level_index: int)

var _main_menu: Control
var _level_selection_menu: Control
var _credits_menu: Control
var _tips_menu: Control


func _ready() -> void:
    _main_menu = get_node("MainMenu")
    _level_selection_menu = get_node("LevelSelection")
    _credits_menu = get_node("Credits")
    _tips_menu = get_node("Tips")

    # NOTE: Start actually shows tips
    _main_menu.get_node("%ButtonStart").pressed.connect(_set_tips)
    # tips.start starts the game
    var start_button: Button = _tips_menu.get_node("%ButtonStart")
    start_button.pressed.connect(func(): level_selected.emit(0))

    _main_menu.get_node("%ButtonLevelSelection").pressed.connect(_set_level_selection)
    _main_menu.get_node("%ButtonCredits").pressed.connect(_set_credits)

    var sound_slider: Slider = _main_menu.get_node("%VolumeSlider")
    _set_sound(sound_slider.value);
    sound_slider.value_changed.connect(_set_sound);

    _level_selection_menu.get_node("%ButtonBack").pressed.connect(_set_main_menu)
    _credits_menu.get_node("%ButtonBack").pressed.connect(_set_main_menu)
    _tips_menu.get_node("%ButtonBack").pressed.connect(_set_main_menu)

    var container: Container = _level_selection_menu.get_node("%LevelButtons")

    var level_idx: int = 0
    for node in container.get_children():
        if node is Button:
            var level_index_for_lambda: int = level_idx
            level_idx += 1
            print("Assigning level index %d to button %s" % [level_idx, node.name])
            node.pressed.connect(func(): level_selected.emit(level_index_for_lambda))

    _set_main_menu()


func _set_level_selection() -> void:
    _main_menu.visible = false
    _level_selection_menu.visible = true
    _credits_menu.visible = false
    _tips_menu.visible = false


func _set_credits() -> void:
    _main_menu.visible = false
    _level_selection_menu.visible = false
    _credits_menu.visible = true
    _tips_menu.visible = false


func _set_tips() -> void:
    _main_menu.visible = false
    _level_selection_menu.visible = false
    _credits_menu.visible = false
    _tips_menu.visible = true


func _set_main_menu() -> void:
    _main_menu.visible = true
    _level_selection_menu.visible = false
    _credits_menu.visible = false
    _tips_menu.visible = false

func _set_sound(percent: float) -> void:
    AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("Master"), percent / 100);
