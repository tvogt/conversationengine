
var Name : String; // this is not actually used anywhere, it's for the inspector display to spot segments easier
var Text : String;
var Talk : AudioClip;
var Animations : String[];

enum Perspectives { Classic_Two_Shot, Player_Profile, Player_Halfprofile, Near_Player_looking_at_NPC, Behind_Player_looking_at_NPC, NPC_Profile, NPC_Halfprofile, Near_NPC_looking_at_Player, Behind_NPC_looking_at_Player };
var Camera_Perspective : Perspectives = Perspectives.Classic_Two_Shot;
enum Angles { low, level, high };
var Camera_Angle : Angles = Angles.level;
enum Distances { closeup, medium, full };
var Camera_Distance : Distances = Distances.medium;

private var NPC_Face:Transform;
private var Player_Face:Transform;
private var DialogCamera:Transform = null;

private var scale;
private var stopped = false;

function RunSimpleDialog() {
	if (!enabled) return;
	// some defaults
	if (Talk) {
		Wait = Talk.length;
	} else {
		Wait = Text.length/12;
	}

	// run animations
	if (Animations.length>0) {
		animation.CrossFade(Animations[0]);
		if (Animations.length>1) {
			for (var i=1; i<Animations.length; i++) {
				animation.CrossFadeQueued(Animations[i]);
			}
		}
	}
	
	if (Talk) {
		yield;
		audio.enabled = true;
		audio.clip = Talk;
		audio.Play();
	}

	// next or return
	yield WaitForSeconds(Wait);
	if (stopped) return;
	if (Talk) {
		audio.Stop();
		audio.enabled = false;
	}
	DialogCamera = null; // disable updates
	SendMessage("DialogDone");
}


function RunFullDialog(Cam:Transform) {
	if (!enabled) return;
	// setup data
	DialogCamera = Cam;
	Player = GameObject.FindWithTag("Player");
	scale = Player.transform.lossyScale.y/2;
	Player_Face = Player.transform.Find("Face");
	if (!Player_Face) {
		// fallback - our main position
		Player_Face = Player.transform;
	}
	NPC_Face = transform.Find("Face");
	if (!NPC_Face) {
		// fallback - we don't have a face, so we use our main position
		NPC_Face = transform;
	}

	RunSimpleDialog();
}


function Update() {
	if (!DialogCamera) return;
	
	if (Input.GetKey(KeyCode.Escape)) {
		stopped=true;
		SendMessage("StopDialog");
	}
	// position Camera
	var d = 2.0;
	if (Camera_Distance==Distances.closeup) {
		d = 1.0;
	} else if (Camera_Distance==Distances.full) {
		d = 3.0;
	} else { // medium = default
		d = 2.0;
	}
	
	
	if (Camera_Perspective == Perspectives.Near_Player_looking_at_NPC) {
		DialogCamera.position = Player_Face.position;
		DialogCamera.LookAt(NPC_Face);
		DialogCamera.Translate(Vector3.right * d);
		Look = NPC_Face;
	} else if (Camera_Perspective == Perspectives.Behind_Player_looking_at_NPC) {
		DialogCamera.position = Player_Face.position;
		DialogCamera.LookAt(NPC_Face);
		DialogCamera.Translate((Vector3.right-Vector3.forward)*d);
		Look = NPC_Face;
	} else if (Camera_Perspective == Perspectives.Near_NPC_looking_at_Player) {
		DialogCamera.position = NPC_Face.position;
		DialogCamera.LookAt(Player_Face);
		DialogCamera.Translate(Vector3.right*d);
		Look = Player_Face;
	} else if (Camera_Perspective == Perspectives.Behind_NPC_looking_at_Player) {
		DialogCamera.position = NPC_Face.position;
		DialogCamera.LookAt(Player_Face);
		DialogCamera.Translate((Vector3.right-Vector3.forward)*d);
		Look = Player_Face;
	} else if (Camera_Perspective == Perspectives.Player_Profile) {
		DialogCamera.position = Player_Face.position;
		DialogCamera.LookAt(NPC_Face);
		DialogCamera.Translate(Vector3.right*d);
		Look = Player_Face;
	} else if (Camera_Perspective == Perspectives.Player_Halfprofile) {
		DialogCamera.position = Player_Face.position;
		DialogCamera.LookAt(NPC_Face);
		DialogCamera.Translate((Vector3.right+Vector3.forward)*d);
		Look = Player_Face;
	} else if (Camera_Perspective == Perspectives.NPC_Profile) {
		DialogCamera.position = NPC_Face.position;
		DialogCamera.LookAt(Player_Face);
		DialogCamera.Translate(Vector3.right*d);
		Look = NPC_Face;
	} else if (Camera_Perspective == Perspectives.NPC_Halfprofile) {
		DialogCamera.position = NPC_Face.position;
		DialogCamera.LookAt(Player_Face);
		DialogCamera.Translate((Vector3.right+Vector3.forward)*d);
		Look = NPC_Face;
	} else { // classic two_shot = default
		var Target = Vector3.Min(NPC_Face.position, Player_Face.position) + (Vector3.Max(NPC_Face.position, Player_Face.position) - Vector3.Min(NPC_Face.position, Player_Face.position))/2;
		DialogCamera.position = Target;
		DialogCamera.LookAt(NPC_Face);
		DialogCamera.Translate(Vector3.right*d*Vector3.Distance(NPC_Face.position, Player_Face.position)/2);
	}

	if (Camera_Angle==Angles.low) {
		DialogCamera.Translate(-Vector3.up*d*scale, Space.World);
	} else if (Camera_Angle==Angles.high) {
		DialogCamera.Translate(Vector3.up*d*scale, Space.World);
	} else { // level = default
	}
	
	if (Look) DialogCamera.LookAt(Look);
}


function OnGUI() {
	// we assume that the GameController has a static variable MySkin for the GUI skin to use
	GUI.skin = GameController.MySkin;
	
	// estimate the height we will need
	var fontsize = GUI.skin.label.fontSize;
	if (fontsize==0) {
		// default font size - how do we get this? TODO
		fontsize=24;
	}
	var chars_per_line = Screen.width*0.8 / fontsize;
	var lines = 1.2*Mathf.Ceil(Text.length/(chars_per_line*2)); // 1.2 lines-with-spacing times lines required, estimating that width of characters is 1/2 height at average
	
	GUILayout.BeginArea(Rect(Screen.width*0.1, Screen.height-fontsize*lines, Screen.width*0.8, fontsize*lines), GUIStyle("box"));
	GUILayout.Label(Text);
	GUILayout.EndArea();
}
