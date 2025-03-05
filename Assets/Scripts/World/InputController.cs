using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class InputController : MonoBehaviour
{

    private bool _isDragging;
    public InputAction touchPressAction;
    public InputAction touchDragAction;
    private Camera _mainCamera;

    private InputSystem_Actions _actions;

    public static event Action<Vector3> OnTouchPositionUpdated;

    private void Awake()
    {
        _actions = new InputSystem_Actions();

    }


    private void Start()
    {
        _mainCamera = Camera.main;

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

    private void Drag(InputAction.CallbackContext context)
    {

        Debug.Log("Dragging");


    }

    public void TouchPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isDragging = true;
      
            //  _offset = transform.position - ScreenToWorld(touchPos);
            Debug.Log("Dragging!");
            _actions.Touch.TouchPosition.performed += ctx => TouchPosition(ctx);


        }
        else if (context.canceled)
        {
            _isDragging = false;
            Debug.Log("No Drag!");
        }
    }

    public void EndPress(InputAction.CallbackContext context)
    {
        Debug.Log("EndPress.");
        _actions.Touch.TouchPosition.performed -= ctx => TouchPosition(ctx);
    }



    public void TouchPosition(InputAction.CallbackContext context)
    {
        if (_isDragging)
        {
            Vector2 touchPos = context.ReadValue<Vector2>();
            Vector3 worldPos = ScreenToWorld(touchPos);
            Debug.Log("TouchPosition: " + worldPos);

            //Invokes the event which communicates that the world position of the drag is active.
            OnTouchPositionUpdated?.Invoke(worldPos);

        }
    }


    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        // Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _mainCamera.nearClipPlane + 5));
        // return new Vector3(worldPos.x, worldPos.y, transform.position.z);


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
