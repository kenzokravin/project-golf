using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;


public class InputController : MonoBehaviour
{

    private bool _isDragging;
    private bool firstPress;
    public InputAction touchPressAction;
    public InputAction touchDragAction;
    private Camera _mainCamera;

    private InputSystem_Actions _actions;

    public static event Action<Vector3> OnTouchPositionUpdated;
    public static event Action<Vector3> OnTouchPressDownPosition;
    public static event Action<Vector3> OnTouchPressUpPosition;

    private void Awake()
    {
        _actions = new InputSystem_Actions();
        
    }


    private void Start()
    {
        _mainCamera = Camera.main;
        firstPress = false;
        _actions.Touch.TouchPress.started += ctx => TouchPress(ctx);
        _actions.Touch.TouchPress.canceled += ctx => EndPress(ctx);




    }

    void Update()
    {

    }

    private void OnEnable()
    {
        _actions.Enable();

    }

    private void OnDisable()
    {

        _actions.Disable();


    }

    public void TouchPress(InputAction.CallbackContext context)
    {



        if (context.started)
        {
            _isDragging = true;
           
            //  _offset = transform.position - ScreenToWorld(touchPos);
            //Debug.Log("Dragging!");
           // OnTouchPressDownPosition?.Invoke(worldPos);
            _actions.Touch.TouchPosition.performed += ctx => TouchPosition(ctx);


   

        }
        else if (context.canceled)
        {
            _isDragging = false;
          //  Debug.Log("No Drag!");
        }
    }

    public void EndPress(InputAction.CallbackContext context)
    {
      //  Debug.Log("EndPress.");
        _actions.Touch.TouchPosition.performed -= ctx => TouchPosition(ctx);


        Vector2 touchPos = _actions.Touch.TouchPosition.ReadValue<Vector2>();
        Vector3 worldPos = ScreenToWorld(touchPos);
        OnTouchPressUpPosition?.Invoke(worldPos);

        firstPress = false;
    }



    public void TouchPosition(InputAction.CallbackContext context)
    {
        if (_isDragging)
        {
            Vector2 touchPos = context.ReadValue<Vector2>();
            Vector3 worldPos = ScreenToWorld(touchPos);

            if (!firstPress)
            {

                OnTouchPressDownPosition?.Invoke(worldPos);
                firstPress = true;
            }
            else
            {

                OnTouchPositionUpdated?.Invoke(worldPos);
            }

            
          //  Debug.Log("TouchPosition: " + worldPos);

            //Invokes the event which communicates that the world position of the drag is active.
           

        }
    }


    private Vector3 ScreenToWorld(Vector2 screenPos)
    {

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        // Define the plane (normal, and a point on the plane)
        Plane worldPlane = new Plane(Vector3.forward, Vector3.zero); // Adjust the normal and position to match your world plane

        float enter;
        if (worldPlane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter); // Returns the intersection point
        }

        return Vector3.zero;



    }
}
