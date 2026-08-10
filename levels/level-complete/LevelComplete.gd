class_name LevelComplete
extends Node

signal next_level_requested

const FADE_DURATION: float = 1.0
var _has_fired: bool = false
var score: Score

var _score_node: Label
var _extra_score_node: Label


func _ready() -> void:
	var tween: Tween = get_tree().create_tween()

	var text_node: Control = get_node("%Title")
	text_node.modulate = Color(Color.WHITE, 0)

	tween.tween_property(text_node, "modulate:a", 1.0, FADE_DURATION) \
		.set_trans(Tween.TRANS_CUBIC)

	_score_node = get_node("%ScoreText")
	_score_node.modulate = Color(Color.WHITE, 0)

	tween.tween_property(_score_node, "modulate:a", 1.0, FADE_DURATION) \
		.set_trans(Tween.TRANS_CUBIC)
	tween.parallel() \
		.tween_method(_set_visible_score, 0, score.num_lifted_components, 2.0)

	_extra_score_node = get_node("%ExtraScoreText")
	_extra_score_node.modulate = Color(Color.WHITE, 0)

	tween.tween_property(_extra_score_node, "modulate:a", 1.0, FADE_DURATION) \
		.set_trans(Tween.TRANS_CUBIC)
	tween.parallel() \
		.tween_method(_set_visible_extra_score, 0, score.num_extras, 2.0)

	var continue_button: Button = get_node("%ContinueButton")
	continue_button.modulate = Color(Color.WHITE, 0)

	tween.tween_property(continue_button, "modulate:a", 1.0, FADE_DURATION) \
		.set_trans(Tween.TRANS_CUBIC)
	tween.tween_callback(func(): continue_button.pressed.connect(_on_continue))


func _set_visible_score(count: int) -> void:
	_score_node.text = "Components lifted: %d / %d" % [count, score.total_components]


func _set_visible_extra_score(count: int) -> void:
	_extra_score_node.text = "Extra objects: %d" % count


func _on_continue() -> void:
	print("ContinueButton::OnMouseEvent")

	if _has_fired:
		return
	_has_fired = true

	next_level_requested.emit()
