extends Node3D

@export var combination = 1235
var keypadtext: Label
var max_Characters = false
var special_state = false
var locked = false

var node_interact: Node

var camera_pos: Node
var camera_look: Node

var isNowInteract = false

var passed_object

# Called when the node enters the scene tree for the first time.
func _ready():
	node_interact = $interactive_object
	camera_pos = $CameraPos
	camera_look = $CameraLook
	#$interactive_object/StaticBody3D/CollisionShape3D.disabled = true
	pass # Replace with function body.


func _play_sound():
	$KeyPress.play()
	
	
func _used():
	if isNowInteract == false:
		isNowInteract = true
		passed_object.DisableInputsAndCameraMoveLookTarget(camera_pos.get_global_position(), camera_look.get_global_position())
		$interactive_object/StaticBody3D/CollisionShape3D.disabled = true
		if CustomSettings.debug_oalar == true:
				Logging.info(self, "Keypad used")
		
func _input(event):
	if event.is_action_pressed("Jump") and isNowInteract == true:
		_used_quit()
		
		
func _used_quit():
		passed_object.EnableInputsAndCameraToNormal()
		$interactive_object/StaticBody3D/CollisionShape3D.disabled = false
		passed_object = null
		isNowInteract = false
		if CustomSettings.debug_oalar == true:
				Logging.info(self, "Keypad used")

func input(key):
	if key == "clear":
		_clear()
	elif key == "enter":
		_enter()
	else:
		if special_state == true:
			_clear()
			special_state = false
		_update_text(key)
	pass

func _update_text(text):
	if max_Characters == false:
		var x = $KeypadText.text
		x = x + str(text)
		$KeypadText.text = str(x)
		if len(x) == 4:
			max_Characters = true
	else:
		pass

func _clear():
	$KeypadText.text = ""
	max_Characters = false


func _enter():
	if str(combination) == $KeypadText.text:
		$KeypadText.text = "OK"
		locked = true
		$OK.play()
		_open()
		if CustomSettings.debug_oalar == true:
				Logging.info(self, "Correct combination entered")
	else:
		$KeypadText.text = "ERROR"
		special_state = true
		$Error.play()
		if CustomSettings.debug_oalar == true:
				Logging.info(self, "Incorrect combination entered")


func _open():
	var door = self.get_node("/root/worldlevel/bunker_door")
	door.open()

		
func message_update():
	var msg:String = node_interact.msgObject.GetMessage()
	if msg == "msg_get_use_action_text":
		node_interact.msgObject.SetStringData("Use keypad")
	if msg == "msg_get_interactive_object_name":
		node_interact.msgObject.SetStringData("placeholder")
	if msg == "msg_use_action":
		passed_object = node_interact.msgObject.GetNodeData()
		self._used()
