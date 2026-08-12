class_name Game
extends Node

const COLLISION_LAYER_PRIMARY: int = 0b0001
const COLLISION_LAYER_GRABBABLE: int = 0b0001
const COLLISION_LAYER_MAGNET: int = 0b0001
const CENTRAL_X_COORDINATE: int = 0

const FADE_OUT_DURATION: float = 0.25
const FADE_IN_DURATION: float = 0.25

var _fader: CanvasItem
var _active_scene: Node

var _title_screen_scene: PackedScene
var _level_scenes: Array = []
var _scores: Array = []

var _current_level_idx: int = 0
var _current_level: Level


func _ready() -> void:
    _fader = get_node("%Fader")
    _fader.modulate = Color(1, 1, 1, 0)

    _title_screen_scene = load("res://levels/title-screen/scene.tscn")
    _level_scenes = [
        load("res://levels/level-1/scene.tscn"),
        load("res://levels/level-2/scene.tscn"),
        load("res://levels/level-3/scene.tscn"),
        load("res://levels/level-4/scene.tscn"),
        load("res://levels/level-5/scene.tscn"),
        load("res://levels/level-6/scene.tscn"),
    ]
    _scores.resize(_level_scenes.size())

    var title_screen: TitleScreen = _title_screen_scene.instantiate()
    title_screen.level_selected.connect(_start_level)
    add_child(title_screen)
    _active_scene = title_screen


func _transition_to(next_scene: Node) -> void:
    var tween: Tween = get_tree().create_tween()
    tween.tween_property(_fader, "modulate", Color(1, 1, 1, 1), FADE_OUT_DURATION)
    tween.tween_callback(func():
        _active_scene.queue_free()
        remove_child(_active_scene)
        add_child(next_scene)
        _active_scene = next_scene
    )
    tween.tween_property(_fader, "modulate", Color(1, 1, 1, 0), FADE_OUT_DURATION)


func _show_title_screen() -> void:
    _end_level()
    var title_screen: TitleScreen = _title_screen_scene.instantiate()
    title_screen.level_selected.connect(_start_level)
    title_screen.scores = _scores;
    _transition_to(title_screen)

func _start_level(level_index: int) -> void:
    _current_level_idx = level_index
    _instantiate_level(level_index)

# tallies score of current level, and sets _current_level to null
func _end_level() -> void:
    if _current_level != null:
        _scores[_current_level_idx] = _current_level.get_score()
        _current_level = null

# starts next level or returns to menu if none
func _next_level() -> void:
    _end_level()
    _current_level_idx += 1

    if _current_level_idx == _level_scenes.size():
        _show_title_screen()
        return

    _instantiate_level(_current_level_idx)


func _instantiate_level(level_index: int) -> void:
    print("Instantiating level %d" % (level_index + 1))
    var packed_scene: PackedScene = _level_scenes[level_index]
    _current_level = packed_scene.instantiate()
    _current_level.next_level_requested.connect(_next_level)
    _current_level.reset_requested.connect(func(): _start_level(level_index))
    _current_level.return_requested.connect(_show_title_screen)
    _transition_to(_current_level)
