using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Demo : MonoBehaviour {

	public TouchPad pad;
	public Text TouchPadOutput;
	public Joystick stick;
	public Text JoystickOutput;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (JoystickOutput && stick) {
			JoystickOutput.text = "Joystick output:"+ stick.JoystickInput;
		}

		if (TouchPadOutput && pad) {
			TouchPadOutput.text = "Touch Pad output:"+ pad.TouchPadInput;
		}
	
	}
}
