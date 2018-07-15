using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ShootGameServer.SharedData;

public class MainMenu : MonoBehaviour {

    public Button hostButton;
    public Button quitButton;
    public InputField myNameInput;
    public InputField serverInputField;

    System.Random rnd = new System.Random();

	// Use this for initialization
	void Start () {
        myNameInput.text = (ShootGameManager.MyId == ConstSettings.DefaultName) ? ShootGameManager.MyId + rnd.Next ().ToString () : ShootGameManager.MyId;
        serverInputField.text = ShootGameManager.ServerEndpoint;
            
        hostButton.onClick.AddListener (NewGame);
        quitButton.onClick.AddListener (Application.Quit);
	}


    void NewGame(){
        ShootGameManager.MyId = myNameInput.text;
        ShootGameManager.ServerEndpoint = serverInputField.text;

        SceneManager.LoadScene ("GameStart");
    }
}
