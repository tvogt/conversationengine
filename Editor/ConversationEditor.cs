using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(Conversation))]

public class ConversationEditor : Editor {
	
	
	public override void OnInspectorGUI() {
		Conversation Convo = (Conversation)target;
		
		EditorGUIUtility.LookLikeControls();
		if (Application.isPlaying) {
			//return;
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("GUI Skin");
		Convo.MySkin = (GUISkin)EditorGUILayout.ObjectField(Convo.MySkin, typeof(GUISkin));
		EditorGUILayout.EndHorizontal();

		Convo.Repeat = EditorGUILayout.Toggle("Repeat Conversation", Convo.Repeat);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Subtitle Width");
		Convo.subtitle_width = EditorGUILayout.Slider(Convo.subtitle_width, 0.2f, 1.0f);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Top/Bottom Margin");
		Convo.subtitle_margin = EditorGUILayout.Slider(Convo.subtitle_margin, 0f, 128f);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Bottom Offset");
		Convo.subtitle_offset = EditorGUILayout.Slider(Convo.subtitle_offset, 0f, 1.0f);
		EditorGUILayout.EndHorizontal();

		if (Convo.dialog!=null) for (int i = 0; i < Convo.dialog.Count; i++) {
			Conversation.DialogItem dialog = Convo.dialog[i];
			
			EditorGUILayout.BeginVertical(new GUIStyle("box"));
			EditorGUILayout.BeginHorizontal();
			dialog.show = EditorGUILayout.Foldout(dialog.show, dialog.subtitle.Substring(0, Mathf.Min(40,dialog.subtitle.Length))+"...");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("up")) {
				if (i>0) {
					Convo.dialog.Insert(i-1, dialog);
					Convo.dialog.RemoveAt(i+1);
				}
			}
			if (GUILayout.Button("down")) {
				if (i+1<Convo.dialog.Count) {
					Convo.dialog.Insert(i+2, dialog);
					Convo.dialog.RemoveAt(i);
				}
			}
			if (GUILayout.Button("delete")) {
				Convo.dialog.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
			if (dialog.show) {
				EditorGUILayout.BeginHorizontal(); EditorGUILayout.Space(); EditorGUILayout.BeginVertical();

				if (dialog.is_branch) {
					if (dialog.choice!=null) for (int j = 0; j < dialog.choice.Count; j++) {
						EditorGUILayout.BeginHorizontal();
							dialog.choice[j] = EditorGUILayout.TextField(dialog.choice[j]);
							String[] option_names = new String[Convo.dialog.Count+1];
							int[] option_numbers = new int[Convo.dialog.Count+1];
							for (int k = 0; k < Convo.dialog.Count; k++) {
								if (k<i) {
									option_names[k] = Convo.dialog[k].subtitle.Substring(0, Mathf.Min(40,Convo.dialog[k].subtitle.Length))+"...";
									option_numbers[k] = k;
								} else if (k>i) {
									option_names[k-1] = Convo.dialog[k].subtitle.Substring(0, Mathf.Min(40,Convo.dialog[k].subtitle.Length))+"...";
									option_numbers[k-1] = k;								
								}
							}
							option_names[Convo.dialog.Count] = "(end conversation)"; option_numbers[Convo.dialog.Count] = -1;
							dialog.jump[j] = EditorGUILayout.IntPopup(dialog.jump[j], option_names, option_numbers);
						EditorGUILayout.EndHorizontal();
					}
					if (GUILayout.Button("Add choice")) {
						if (dialog.choice==null) {
							dialog.choice = new List<String>();
							dialog.jump = new List<int>();
						}
						dialog.choice.Add("(enter text to display)");
						dialog.jump.Add(-1);
						EditorUtility.SetDirty(Convo);
					}
					
				} else {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Speaker");
					dialog.speaker = (Conversation.Speakers)EditorGUILayout.EnumPopup(dialog.speaker);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Audio Clip");
					dialog.spoken = (AudioClip)EditorGUILayout.ObjectField(dialog.spoken, typeof(AudioClip));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Subtitle");
					dialog.subtitle = EditorGUILayout.TextArea(dialog.subtitle);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Animation Player");
					dialog.player_animation = (AnimationClip)EditorGUILayout.ObjectField(dialog.player_animation, typeof(AnimationClip));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Animation NPC");
					dialog.npc_animation = (AnimationClip)EditorGUILayout.ObjectField(dialog.npc_animation, typeof(AnimationClip));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("wait until end of");
					dialog.wait = (Conversation.Waits)EditorGUILayout.EnumPopup(dialog.wait);
					EditorGUILayout.EndHorizontal();

					dialog.has_action = EditorGUILayout.Toggle("Action", dialog.has_action);
					if (dialog.has_action) {
						EditorGUILayout.BeginHorizontal(); EditorGUILayout.Space(); EditorGUILayout.BeginVertical();
							dialog.ActionMethod = EditorGUILayout.TextField("Method", dialog.ActionMethod);
							dialog.ActionParameter = EditorGUILayout.TextField("Parameter", dialog.ActionParameter);
						EditorGUILayout.EndVertical(); EditorGUILayout.EndHorizontal();				
					}
				} // endif is_branch


				EditorGUILayout.BeginHorizontal();
				dialog.show_camera = EditorGUILayout.Foldout(dialog.show_camera, "Camera");
				if (dialog.show_camera) {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("make pan shot")) {
						// TODO
					}
				}
				EditorGUILayout.EndHorizontal();
				if (dialog.show_camera) {
					EditorGUILayout.BeginHorizontal(); EditorGUILayout.Space(); EditorGUILayout.BeginVertical();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Perspective");
					dialog.perspective = (Conversation.Perspectives)EditorGUILayout.EnumPopup(dialog.perspective);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Angle");
					dialog.angle = (Conversation.Angles)EditorGUILayout.EnumPopup(dialog.angle);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Distance");
					dialog.distance = (Conversation.Distances)EditorGUILayout.EnumPopup(dialog.distance);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Side");
					dialog.side = (Conversation.Sides)EditorGUILayout.EnumPopup(dialog.side);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical(); EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.EndVertical(); EditorGUILayout.EndHorizontal();				
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
		} // endof loop over dialog elements

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Add dialog element")) {
			if (Convo.dialog==null) {
				Convo.dialog = new List<Conversation.DialogItem>();
			}
			Convo.dialog.Add(new Conversation.DialogItem());
			EditorUtility.SetDirty(Convo);
		}
		if (GUILayout.Button("Add branching choices")) {
			if (Convo.dialog==null) {
				Convo.dialog = new List<Conversation.DialogItem>();
			}
			Convo.dialog.Add(new Conversation.DialogItem(true));
			EditorUtility.SetDirty(Convo);
		}
		EditorGUILayout.EndHorizontal();

	}


	void Resort() {
		
	}
}