using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]	
public class Conversation : MonoBehaviour {

	public enum Perspectives {
		Classic_Two_Shot, 
		Player_Profile, Player_Halfprofile, Near_Player_looking_at_NPC, Behind_Player_looking_at_NPC, 
		NPC_Profile, NPC_Halfprofile, Near_NPC_looking_at_Player, Behind_NPC_looking_at_Player 
	};
	public enum Angles {
		low, level, high
	}
	public enum Distances {
		closeup, medium, full
	}
	public enum Sides {
		left, right
	}
	public enum Waits {
		Audio, Animation, Subtitle
	}
	public enum Speakers {
		Player, NPC
	}

	[Serializable]	
	public class DialogItem {
		public bool show = true;
		public Speakers speaker = Speakers.NPC;
		public AudioClip spoken = null;
		public string subtitle = "(enter subtitle text)";
		public AnimationClip player_animation = null;
		public AnimationClip npc_animation = null;
		public Waits wait = Waits.Audio;
		public bool show_camera = true;
		public Perspectives perspective = Perspectives.Classic_Two_Shot;
		public Angles angle = Angles.level;
		public Distances distance = Distances.medium;
		public Sides side = Sides.left;
		
		public bool is_branch = false;
		public List<String> choice;
		public List<int> jump;
		
		public bool has_action = false;
		public String ActionMethod;
		public String ActionParameter;

		public DialogItem(bool branch=false) {
			if (branch) {
				is_branch = true;
				subtitle = "Branching Choice";
			}
		}
	}

	public List<DialogItem> dialog;


	// other variables
	public bool Repeat = false; // whether or not we repeat the conversation when the player comes a 2nd time
	public GUISkin MySkin;
	public float subtitle_width = 0.8f;
	public float subtitle_offset = 0.05f;
	public float subtitle_margin = 32f;
	
	private bool firsttime = true;
	private GameObject Player;
	private Transform CamOrigParent;
	private Vector3 CamOrigPos;
	private Quaternion CamOrigRot;

	private Transform Player_Face;
	private Transform NPC_Face;
	private float scale;

	private bool talking = false;
	private bool branching = false;
	private int index = 0;
	private bool CursorLocked = false;

	void OnTriggerEnter() {
		if (firsttime || Repeat) {
			firsttime = false;
			gameObject.BroadcastMessage("StartConversationActions", null, SendMessageOptions.DontRequireReceiver);
			StartCoroutine(StartConversation());
		}
	}

	IEnumerator StartConversation() {
		if (dialog!=null) {
			CursorLocked = Screen.lockCursor;
			Player = GameObject.FindWithTag("Player");
			Player.BroadcastMessage("SetControllable", false);
			scale = Player.transform.lossyScale.y/2;
			Player_Face = Player.transform.Find("Face");
			if (!Player_Face) { // fallback - our main position
				Player_Face = Player.transform;
			}
			NPC_Face = transform.Find("Face");
			if (!NPC_Face) { // fallback - we don't have a face, so we use our main position
				NPC_Face = transform;
			}

			CamOrigParent = Camera.main.transform.parent;
			CamOrigPos = Camera.main.transform.localPosition;
			CamOrigRot = Camera.main.transform.localRotation;
			Camera.main.transform.parent = null;

			// run the conversation tree		
			for (index = 0; index < dialog.Count; index++) {
				DialogItem d = dialog[index];

				PositionCamera(Camera.main.transform, d);

				if (d.is_branch) {
					if (CursorLocked) Screen.lockCursor = false; // unlock the cursor, because we need it
					branching = true; talking = false;
					yield return StartCoroutine(Branch(d, index));
					index--; // correct that the for loop will increment it
					if (CursorLocked) Screen.lockCursor = true; // lock the cursor, if it was locked
				} else {
					talking = true; branching = false;
					yield return StartCoroutine(Talk(d));
				}
			} // endof loop over all dialog elements

			ReturnToScene();
		}
	}

	IEnumerator Talk(DialogItem d) {
		float WaitTime = 0f;

		switch (d.wait) {
			case Waits.Audio:
				if (d.spoken!=null) WaitTime = d.spoken.length;
				break;
			case Waits.Animation:
				if (d.speaker==Speakers.Player) {
					if (d.player_animation) WaitTime = d.player_animation.length;
				} else {
					if (d.npc_animation) WaitTime = d.npc_animation.length;
				}
				break;
		}
		// fallback - same as Waits.Subtitle
		if (WaitTime==0f) WaitTime = (float)d.subtitle.Length/12f;
		
		// spoken text audio output
		if (d.spoken) {
			audio.enabled = true;
			audio.clip = d.spoken;
			audio.Play();
		}
		
		// animations
		if (d.player_animation && Player.animation!=null) Player.animation.CrossFade(d.player_animation.name);
		if (d.npc_animation && animation!=null) animation.CrossFade(d.npc_animation.name);

		// wait until we are done before doing next in loop
		yield return new WaitForSeconds(WaitTime);
		
	}

	IEnumerator Branch(DialogItem d, int i) {
		while (index == i) {
			yield return null;
		}
	}

	void ReturnToScene() {
		talking = false;
		gameObject.BroadcastMessage("StopConversationActions", null, SendMessageOptions.DontRequireReceiver);
		
		Camera.main.transform.parent = CamOrigParent;
		Camera.main.transform.localPosition = CamOrigPos;
		Camera.main.transform.localRotation = CamOrigRot;

		Player.BroadcastMessage("SetControllable", true);

		// back to the default animation - since we're making a camera cut, no need to fade
		if (animation!=null) {
			animation.Stop();
			animation.Play();
		}
	}


	/* ========== Camera Handling and GUI ========== */

	void OnGUI() {
		if (talking || branching) {
			GUI.skin = MySkin;

			// estimate the height we will need
			int fontsize = GUI.skin.label.fontSize;
			if (fontsize==0) {
				// default font size - how do we get this? TODO
				fontsize=24;
			}
			if (talking) {
				float chars_per_line = (float)Screen.width*0.8f / (float)fontsize;
				float lines = 1.2f*Mathf.Ceil((float)(dialog[index].subtitle.Length)/(chars_per_line*2f)); // 1.2 lines-with-spacing times lines required, estimating that width of characters is 1/2 height at average

				GUILayout.BeginArea(new Rect(Screen.width*(1f-subtitle_width)/2f, Screen.height-(fontsize*lines)-(Screen.height*subtitle_offset)-subtitle_margin, Screen.width*subtitle_width, (float)(fontsize*lines)+ subtitle_margin), new GUIStyle("box"));
				GUILayout.Label(dialog[index].subtitle);
				GUILayout.EndArea();
			} else {
				float lines = 1.6f*(float)dialog[index].choice.Count; // need more space, for the buttons

				GUILayout.BeginArea(new Rect(Screen.width*(1f-subtitle_width)/2f, Screen.height-(fontsize*lines)-(Screen.height*subtitle_offset)-subtitle_margin, Screen.width*subtitle_width, (float)(fontsize*lines)+ subtitle_margin), new GUIStyle("box"));
				for (int i=0; i<dialog[index].choice.Count; i++) {
					if (GUILayout.Button((i+1)+" - "+dialog[index].choice[i])) {
						index = dialog[index].jump[i];
						branching = false;
					}
				}
				GUILayout.EndArea();				
			}
		}
	}


	void PositionCamera(Transform DialogCamera, DialogItem d) {
		Vector3 Look;
		Vector3 Side;
		float distance = 2f;
			
		if (d.side==Sides.left) Side=Vector3.left; else Side=Vector3.right;
			
		if (d.distance==Distances.closeup) {
			distance = 1f;
		} else if (d.distance==Distances.full) {
			distance = 3f;
		} else { // medium = default
			distance = 2f;
		}


		if (d.perspective == Perspectives.Near_Player_looking_at_NPC) {
			DialogCamera.position = Player_Face.position;
			DialogCamera.LookAt(NPC_Face);
			DialogCamera.Translate(Side * distance);
			Look = NPC_Face.position;
		} else if (d.perspective == Perspectives.Behind_Player_looking_at_NPC) {
			DialogCamera.position = Player_Face.position;
			DialogCamera.LookAt(NPC_Face);
			DialogCamera.Translate((Side-Vector3.forward)*distance);
			Look = NPC_Face.position;
		} else if (d.perspective == Perspectives.Near_NPC_looking_at_Player) {
			DialogCamera.position = NPC_Face.position;
			DialogCamera.LookAt(Player_Face);
			DialogCamera.Translate(Side*distance);
			Look = Player_Face.position;
		} else if (d.perspective == Perspectives.Behind_NPC_looking_at_Player) {
			DialogCamera.position = NPC_Face.position;
			DialogCamera.LookAt(Player_Face);
			DialogCamera.Translate((Side-Vector3.forward)*distance);
			Look = Player_Face.position;
		} else if (d.perspective == Perspectives.Player_Profile) {
			DialogCamera.position = Player_Face.position;
			DialogCamera.LookAt(NPC_Face);
			DialogCamera.Translate(Side*distance);
			Look = Player_Face.position;
		} else if (d.perspective == Perspectives.Player_Halfprofile) {
			DialogCamera.position = Player_Face.position;
			DialogCamera.LookAt(NPC_Face);
			DialogCamera.Translate((Side+Vector3.forward)*distance);
			Look = Player_Face.position;
		} else if (d.perspective == Perspectives.NPC_Profile) {
			DialogCamera.position = NPC_Face.position;
			DialogCamera.LookAt(Player_Face);
			DialogCamera.Translate(Side*distance);
			Look = NPC_Face.position;
		} else if (d.perspective == Perspectives.NPC_Halfprofile) {
			DialogCamera.position = NPC_Face.position;
			DialogCamera.LookAt(Player_Face);
			DialogCamera.Translate((Side+Vector3.forward)*distance);
			Look = NPC_Face.position;
		} else { // classic two_shot = default
			var Target = Vector3.Min(NPC_Face.position, Player_Face.position) + (Vector3.Max(NPC_Face.position, Player_Face.position) - Vector3.Min(NPC_Face.position, Player_Face.position))/2;
			DialogCamera.position = Target;
			DialogCamera.LookAt(NPC_Face);
			DialogCamera.Translate(Side * distance * Vector3.Distance(NPC_Face.position, Player_Face.position)/2);
			Look = Target;
		}

		if (d.angle==Angles.low) {
			DialogCamera.Translate(-Vector3.up * distance * scale, Space.World);
		} else if (d.angle==Angles.high) {
			DialogCamera.Translate(Vector3.up * distance * scale, Space.World);
		} else { // level = default
		}

		DialogCamera.LookAt(Look);
		
	}
}
