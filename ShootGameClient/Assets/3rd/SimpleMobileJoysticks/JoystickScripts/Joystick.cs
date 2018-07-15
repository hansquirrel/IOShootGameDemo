using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Joystick : MonoBehaviour , IPointerUpHandler , IDragHandler {
	[Tooltip("The Touch Aria.")]
	public Vector2 TouchAria = new Vector2 (300, 300);
    private Vector3 startPos;
	[Tooltip("Call to get output on other code.")]
	public Vector2 JoystickInput;
	[HideInInspector] public Vector3 last; // vector to set the Gizmo
	[Tooltip("Hide Joystick if not in use.")]
	public bool hideJoystick = false;
	private Image JoystickImage; //If you want the object to hide it will try to find the image on the object
	private bool startAction; // is called when you want the object to start on drag
	[Tooltip("Start joystick at 0 in any aria.")]
	public bool startActionAtDrag;
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

        startPos = transform.position;
		if (gameObject.GetComponent<Image> ()) {
			JoystickImage= 	gameObject.GetComponent<Image> ();	
		}
		if (JoystickImage && hideJoystick == true) {
			JoystickImage.CrossFadeAlpha (0, 0, true);
		}
    }

	void LateUpdate(){
		/*This assures that each object is assigned it's own Touch ID so that you do not activate other buttons on drag*/
#if !UNITY_EDITOR
		if (hideJoystick) {
			if(Input.touchCount <= 0){
				ID = -1;
			}
			else if(ID == -1 && Input.touchCount>0){
				for (var i = 0; i < Input.touchCount; ++i) {
					if (Mathf.Abs(Input.touches [i].position.x - transform.position.x) < TouchAria.x/2 &&
					    Mathf.Abs(Input.touches [i].position.y- transform.position.y) < TouchAria.y/2 ){
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
		}
#else
		if(!UsingUnityRemote){
		if(Input.GetMouseButtonUp(0)){
			FakeID = false;
			startAction = false;
			startPos = transform.position;
			transform.position = last;
			UpdateVirtualAxes (startPos);
			if (JoystickImage && hideJoystick == true) {
				JoystickImage.CrossFadeAlpha (0, .2f, true);
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
			if (hideJoystick) {
				if(Input.touchCount <= 0){
					ID = -1;
				}
				else if(ID == -1 && Input.touchCount>0){
					for (var i = 0; i < Input.touchCount; ++i) {
						if (Mathf.Abs(Input.touches [i].position.x - transform.position.x) < TouchAria.x/2 &&
						    Mathf.Abs(Input.touches [i].position.y- transform.position.y) < TouchAria.y/2 ){
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
			}
		}
#endif

	}

    private void UpdateVirtualAxes (Vector3 value) {

        var delta = startPos - value;
        delta.y = -delta.y;
		delta.x /= TouchAria.x/2;
		delta.y /= TouchAria.y/2;
		JoystickInput = new Vector2 (-delta.x, delta.y);


    }
	public void FollowFinger(Touch _Touch){
		Vector3 data;
#if !UNITY_EDITOR

		TouchOnJoystick =  _Touch;
		if (Mathf.Abs(TouchOnJoystick.position.x - transform.position.x) < TouchAria.x/2 &&
		    Mathf.Abs(TouchOnJoystick.position.y- transform.position.y) < TouchAria.y/2 ){

			gameObject.transform.position = new Vector3( _Touch.position.x, _Touch.position.y,transform.position.z);
				if (JoystickImage) {
					JoystickImage.CrossFadeAlpha (1, .2f, true);
				}
				startAction = true;
			}
			if(startActionAtDrag && TouchOnJoystick.phase == TouchPhase.Began){
				startPos = transform.position;
			}
			if (startAction) {

				data = new Vector3(TouchOnJoystick.position.x,TouchOnJoystick.position.y,transform.position.z);
				Vector3 newPos = Vector3.zero;
				
				
				int deltax = (int)(data.x - startPos.x);
			deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
			newPos.x = deltax;
			
			
			
			int deltay = (int)(data.y - startPos.y);
			deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
			newPos.y = deltay;
				
				transform.position = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
				UpdateVirtualAxes (transform.position);
			}
			



		
		if (TouchOnJoystick.phase == TouchPhase.Ended) {

			startAction = false;
			if (JoystickImage) {
				JoystickImage.CrossFadeAlpha (0, .1f, true);
			}
			transform.position = last;
			UpdateVirtualAxes (startPos);
		}

#else
if(!UsingUnityRemote){
if (Mathf.Abs(Input.mousePosition.x - transform.position.x) < TouchAria.x/2 &&
		    Mathf.Abs(Input.mousePosition.y- transform.position.y) < TouchAria.y/2  && Input.GetMouseButton(0)){
			gameObject.transform.position = new Vector3(Input.mousePosition.x,Input.mousePosition.y,transform.position.z);
			if (JoystickImage) {
				JoystickImage.CrossFadeAlpha (1, .2f, true);
			}
			startAction = true;
		}

		if(startActionAtDrag && Input.GetMouseButtonDown(0)){
			startPos = transform.position;
		}

	
	if (startAction) {
			data = new Vector3(Input.mousePosition.x,Input.mousePosition.y,transform.position.z);
						Vector3 newPos = Vector3.zero;
		
		
						int deltax = (int)(data.x - startPos.x);
			deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
			newPos.x = deltax;
			
			
			
			int deltay = (int)(data.y - startPos.y);
			deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
			newPos.y = deltay;
		
						transform.position = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
						UpdateVirtualAxes (transform.position);
				}

		if (Input.GetMouseButtonUp (0)) {
			startAction = false;

			if (JoystickImage) {
				JoystickImage.CrossFadeAlpha (0, .2f, true);
			}
			transform.position = startPos;
			UpdateVirtualAxes (startPos);
		}
		}
		else{
			TouchOnJoystick =  _Touch;
			if (Mathf.Abs(TouchOnJoystick.position.x - transform.position.x) < TouchAria.x/2 &&
			    Mathf.Abs(TouchOnJoystick.position.y- transform.position.y) < TouchAria.y/2 ){
				
				gameObject.transform.position = new Vector3( _Touch.position.x, _Touch.position.y,transform.position.z);
				if (JoystickImage) {
					JoystickImage.CrossFadeAlpha (1, .2f, true);
				}
				startAction = true;
			}
			if(startActionAtDrag && TouchOnJoystick.phase == TouchPhase.Began){
				startPos = transform.position;
			}
			if (startAction) {
				
				data = new Vector3(TouchOnJoystick.position.x,TouchOnJoystick.position.y,transform.position.z);
				Vector3 newPos = Vector3.zero;
				
				
				int deltax = (int)(data.x - startPos.x);
				deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
				newPos.x = deltax;
				
				
				
				int deltay = (int)(data.y - startPos.y);
				deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
				newPos.y = deltay;
				
				transform.position = new Vector3 (startPos.x + newPos.x, startPos.y + newPos.y, startPos.z + newPos.z);
				UpdateVirtualAxes (transform.position);
			}
			
			
			
			
			
			if (TouchOnJoystick.phase == TouchPhase.Ended) {
				
				startAction = false;
				if (JoystickImage) {
					JoystickImage.CrossFadeAlpha (0, .1f, true);
				}
				transform.position = last;
				UpdateVirtualAxes (startPos);
			}
		}

#endif


	}


    public  void OnDrag(PointerEventData data) {
		if(!hideJoystick){
        Vector3 newPos = Vector3.zero;

      
            int deltax = (int) (data.position.x - startPos.x);
			deltax = Mathf.Clamp (deltax, - Mathf.FloorToInt(TouchAria.x), Mathf.FloorToInt(TouchAria.x));
			newPos.x = deltax;
			
			
			
			int deltay = (int)(data.position.y - startPos.y);
			deltay = Mathf.Clamp (deltay, Mathf.FloorToInt(-TouchAria.x), Mathf.FloorToInt(TouchAria.y));
			newPos.y = deltay;
        
        transform.position = new Vector3(startPos.x + newPos.x , startPos.y + newPos.y , startPos.z + newPos.z);
        UpdateVirtualAxes (transform.position);
	}
    }


    public  void OnPointerUp(PointerEventData data)
    {
		if(!hideJoystick){
				if (JoystickImage && hideJoystick == true) {
						JoystickImage.CrossFadeAlpha (0, .2f, true);
				}
				transform.position = startPos;
				UpdateVirtualAxes (startPos);
		}
    }

	void OnDrawGizmos() {
		/* This is all the visual stuff to help visualize the bounds for the joystick */
		Vector3 lastpos;
		if (!Application.isPlaying) {
						lastpos = transform.position;
			last= lastpos;
				}
		Gizmos.DrawWireCube (last, new Vector3 (TouchAria.x, TouchAria.y, .1f));
				
	}

}
