/*
Dialog System

* attach one DialogControl to your NPC
* attach one or more Dialogs to your NPC
* both NPC and Player must have a gameobject named "Face", which is used to target the dialog camera
if they don't have one in the mesh, simply create an empty gameobject, name it "Face" and position it roughly where the face is

*/

var current:int=0;
var Elements:MonoBehaviour[];

private var Player:GameObject;
private var CamOrigParent:Transform;
private var CamOrigPos:Vector3;
private var CamOrigRot:Quaternion;
private var firsttime = true;

function OnTriggerEnter() {
	InitDialog();
}

function OnTriggerExit() {
	StopDialog();
}

function InitDialog() {
	Player = GameObject.FindWithTag("Player");
	if (firsttime) {
		Player.SendMessage("SetControllable", false);
		CamOrigParent = camera.main.transform.parent;
		CamOrigPos = camera.main.transform.localPosition;
		CamOrigRot = camera.main.transform.localRotation;
		camera.main.transform.parent = null;

		// look at player - not up or down, to prevent odd arrangement on floor
		transform.LookAt(Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z));
		transform.Rotate(0, 180, 0); // fucked up model axis

		Elements[current].enabled=true;
		SendMessage("RunFullDialog", camera.main.transform.transform);
	} else {
		Elements[current].enabled=true;
		SendMessage("RunSimpleDialog");
	}
}

function StopDialog() {
	Elements[current].enabled=false;
	ReturnToScene();
}

function DialogDone() {
	Elements[current].enabled=false;
	if (current<Elements.length-1) {
		current++;
		Elements[current].enabled=true;
		yield;
		if (firsttime) SendMessage("RunFullDialog", camera.main.transform.transform);
			else SendMessage("RunSimpleDialog", camera.main.transform.transform);
	} else {
		ReturnToScene();
	}
}

function ReturnToScene() {
	firsttime = false;
	camera.main.transform.parent = CamOrigParent;
	camera.main.transform.localPosition = CamOrigPos;
	camera.main.transform.localRotation = CamOrigRot;

	Player.SendMessage("SetControllable", true);

	// back to the default animation - since we're making a camera cut, no need to fade
	animation.Stop();
	animation.Play();
}

