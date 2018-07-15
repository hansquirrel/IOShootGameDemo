using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TouchPad : MonoBehaviour , IPointerUpHandler {
	[Tooltip("The Touch Aria.")]
	public Vector2 TouchAria = new Vector2 (100, 150);
	private Vector3 startPos;
	[Tooltip("Call to get output on other code.")]
	public Vector2 TouchPadInput;
	[Tooltip("How big the thumb controler is.")]
	public Vector2 FingerSize = new Vector2(90,90);
	[HideInInspector] public Vector3 Finger;
	[HideInInspector] public Vector3 FingerS;
	[HideInInspector] public Vector3 last; // needed for Gizmo
	[Tooltip("Start joystick at 0 in any aria.")]
	private bool inTouchAria;
	private bool startAction;
	[Tooltip("Hide and show image.")]
	public bool UseImage;
	[Tooltip("Use Image bounds as 'Touch Aria'.")]
	public bool useImageSizeAsTouchAria;
	[Tooltip("Turn On To Use Touch From Remote.")]
	public bool UsingUnityRemote;
	private Touch TouchOnJoystick;
	private int ID;

	/// <summary>
	/// If using mouse input.
	/// </summary>
	private Touch FakeTouch = new Touch();
	private bool FakeID;


	void OnEnable () {
		startPos = Finger;
		if (!gameObject.GetComponent<Image> () && UseImage) {
						Debug.LogWarning ("No Image Found");
				} else {
			gameObject.GetComponent<Image>().CrossFadeAlpha(0,0,true);		
		}

		if (!gameObject.GetComponent<Image> () && useImageSizeAsTouchAria) {
			Debug.LogWarning("No Image Found");
		}


	}
	
	void LateUpdate(){
	/*This assures that each object is assigned it's own Touch ID so that you do not activate other buttons on drag*/
#if !UNITY_EDITOR
		if(Input.touchCount <= 0){
			ID = -1;
		}
		else if(ID == -1 && Input.touchCount>0){
			for (var i = 0; i < Input.touchCount; ++i) {
				if (Mathf.Abs(Input.touches [i].position.x - Finger.x) < TouchAria.x/2 &&
				    Mathf.Abs(Input.touches [i].position.y- Finger.y) < TouchAria.y/2 ){
				if(Input.touches [i].phase == TouchPhase.Began){
					ID = Input.touches [i].fingerId;
				}
			}
			}
		}

			for (var i = 0; i < Input.touchCount; ++i) {
				if(ID == Input.touches [i].fingerId){
				FollowFinger (Input.touches [i]);
				}
			}
#else
		if(!UsingUnityRemote){
		if(Input.GetMouseButtonUp(0)){
			FakeID = false;
			startAction = false;
			startPos = transform.position;
			Finger = startPos;
			UpdateVirtualAxes (startPos);

			if(gameObject.GetComponent<Image>() && UseImage){
				gameObject.GetComponent<Image>().CrossFadeAlpha(0,.1f,true);
			}

		}
		else if(FakeID == false && Input.GetMouseButtonDown(0)){
			if (Mathf.Abs(Input.mousePosition.x - transform.position.x) < TouchAria.x/2 &&
			    Mathf.Abs(Input.mousePosition.y- transform.position.y) < TouchAria.y/2){
				FakeID = true;
			}
		}
		
		if(FakeID == true){
			FollowFinger (FakeTouch);
		}
		}
		else{
			if(Input.touchCount <= 0){
				ID = -1;
			}
			else if(ID == -1 && Input.touchCount>0){
				for (var i = 0; i < Input.touchCount; ++i) {
					if (Mathf.Abs(Input.touches [i].position.x - Finger.x) < TouchAria.x/2 &&
					    Mathf.Abs(Input.touches [i].position.y- Finger.y) < TouchAria.y/2 ){
						if(Input.touches [i].phase == TouchPhase.Began){
							ID = Input.touches [i].fingerId;
						}
					}
				}
			}
			
			for (var i = 0; i < Input.touchCount; ++i) {
				if(ID == Input.touches [i].fingerId){
					FollowFinger (Input.touches [i]);
				}
			}}
		#endif
		}
	
	
	private void UpdateVirtualAxes (Vector3 value) {
		
		var delta = startPos - value;
		delta.y = -delta.y;
		delta.x /= TouchAria.x/2;
		delta.y /= TouchAria.y/2;
		if (delta.x > 1)
						delta.x = 1;

		if (delta.y > 1)
						delta.y = 1;

		if (delta.x < -1)
			delta.x = -1;
		
		if (delta.y < -1)
			delta.y = -1;

		TouchPadInput = new Vector2 (-delta.x, delta.y);
		
		
	}

	public void FollowFinger(Touch _Touch){
		Vector3 data;
#if !UNITY_EDITOR
		TouchOnJoystick = _Touch;

			if (Mathf.Abs(TouchOnJoystick.position.x - Finger.x) < TouchAria.x/2 &&
			    Mathf.Abs(TouchOnJoystick.position.y- Finger.y) < TouchAria.y/2 ){
				inTouchAria = true;
				if(TouchOnJoystick.phase != TouchPhase.Ended){
					if(gameObject.GetComponent<Image>() && UseImage){
						gameObject.GetComponent<Image>().CrossFadeAlpha(1,.1f,true);
					}
				}
			}
			else{

				inTouchAria = false;
				
			}
			

			if (inTouchAria) {
			Finger = new Vector3(_Touch.position.x,_Touch.position.y,Finger.z);
				startAction = true;
			}
			if(TouchOnJoystick.phase == TouchPhase.Began){
				startPos = Finger;
			}
			if (startAction) {
				data = new Vector3(TouchOnJoystick.position.x,TouchOnJoystick.position.y,Finger.z);
				Vector3 newPos = Vector3.zero;
				
				int deltax = (int)(data.x - startPos.x);
				deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
				newPos.x = deltax;
				
				
				
				int deltay = (int)(data.y - startPos.y);
				deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
				newPos.y = deltay;
				
				Finger = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
				UpdateVirtualAxes (Finger);
			}
			
			
			
			
		
		if (TouchOnJoystick.phase == TouchPhase.Ended) {
			if(gameObject.GetComponent<Image>()){
				gameObject.GetComponent<Image>().CrossFadeAlpha(0,.1f,true);
			}
			startAction = false;
			startPos = transform.position;
			Finger = startPos;
			UpdateVirtualAxes (startPos);

		}
	#else
		if(!UsingUnityRemote){
		if (Mathf.Abs(Input.mousePosition.x - Finger.x) < TouchAria.x/2 &&
		    Mathf.Abs(Input.mousePosition.y- Finger.y) < TouchAria.y/2 ){
			inTouchAria = true;
			if(Input.GetMouseButton(0)){
			if(gameObject.GetComponent<Image>() && UseImage){
				gameObject.GetComponent<Image>().CrossFadeAlpha(1,.1f,true);
			}
			}
		}
		else{
			inTouchAria = false;
		}
		if(gameObject.GetComponent<Image>() && UseImage && !Input.GetMouseButton(0)){
			gameObject.GetComponent<Image>().CrossFadeAlpha(0,.1f,true);
		}

			
		if (inTouchAria && Input.GetMouseButton(0)) {
			Finger = new Vector3(Input.mousePosition.x,Input.mousePosition.y,transform.position.z);

			startAction = true;
		}
		
		if(Input.GetMouseButtonDown(0)){
			startPos = Finger;
		}
		
		
		if (startAction) {
			data = new Vector3(Input.mousePosition.x,Input.mousePosition.y,Finger.z);
			Vector3 newPos = Vector3.zero;
			
			
			int deltax = (int)(data.x - startPos.x);
			deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
			newPos.x = deltax;
			
			
			
			int deltay = (int)(data.y - startPos.y);
			deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
			newPos.y = deltay;
			
			Finger = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
			UpdateVirtualAxes (Finger);
		}
		
		if (Input.GetMouseButtonUp (0)) {
			startAction = false;
			startPos = transform.position;
			Finger = startPos;
			UpdateVirtualAxes (startPos);
		}
		}
		else{
			TouchOnJoystick = _Touch;
			
			if (Mathf.Abs(TouchOnJoystick.position.x - Finger.x) < TouchAria.x/2 &&
			    Mathf.Abs(TouchOnJoystick.position.y- Finger.y) < TouchAria.y/2 ){
				inTouchAria = true;
				if(TouchOnJoystick.phase != TouchPhase.Ended){
					if(gameObject.GetComponent<Image>() && UseImage){
						gameObject.GetComponent<Image>().CrossFadeAlpha(1,.1f,true);
					}
				}
			}
			else{
				
				inTouchAria = false;
				
			}
			
			
			if (inTouchAria) {
				Finger = new Vector3(_Touch.position.x,_Touch.position.y,Finger.z);
				startAction = true;
			}
			if(TouchOnJoystick.phase == TouchPhase.Began){
				startPos = Finger;
			}
			if (startAction) {
				data = new Vector3(TouchOnJoystick.position.x,TouchOnJoystick.position.y,Finger.z);
				Vector3 newPos = Vector3.zero;
				
				int deltax = (int)(data.x - startPos.x);
				deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
				newPos.x = deltax;
				
				
				
				int deltay = (int)(data.y - startPos.y);
				deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
				newPos.y = deltay;
				
				Finger = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
				UpdateVirtualAxes (Finger);
			}
			
			
			
			
			
			if (TouchOnJoystick.phase == TouchPhase.Ended) {
				if(gameObject.GetComponent<Image>()){
					gameObject.GetComponent<Image>().CrossFadeAlpha(0,.1f,true);
				}
				startAction = false;
				startPos = transform.position;
				Finger = startPos;
				UpdateVirtualAxes (startPos);
				
			}}
#endif
	   
	}
	

	public  void OnPointerUp(PointerEventData data)
	{

		Finger = startPos;
		UpdateVirtualAxes (startPos);
	}
	
	void OnDrawGizmos() {
		/* This is all the visual stuff to help visualize the Touch Pad*/
		Vector3 lastpos;
		FingerS = new Vector3 (FingerSize.x, FingerSize.y, .4f);
		if (useImageSizeAsTouchAria && gameObject.GetComponent<Image>()) {
			Image ImageToUse = gameObject.GetComponent<Image>();

			TouchAria = new Vector2(ImageToUse.GetComponent<RectTransform>().rect.width,ImageToUse.GetComponent<RectTransform>().rect.height);
		}

		if (!Application.isPlaying) {
			lastpos = transform.position;
			last= lastpos;
			Finger = transform.position;
		}
		Gizmos.DrawCube (Finger, FingerS);
		Gizmos.DrawWireCube (last, new Vector3 (TouchAria.x, TouchAria.y, .1f));
		
	}
	
}
