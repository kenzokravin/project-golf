using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    private bool _isDragging;
    private Vector3 _offset;
    [SerializeField] private GameObject clickedTile;
    [SerializeField] private bool selected = false;
    [SerializeField] private BallController ball;

    [SerializeField] private int maxHitLength = 7;

    private Pathfinding pathfinder;
    private HexGrid hexGrid;

    private Touch touch;
    private Vector3 dragStartPos;
    public PlayerInput playerInput;

    public InputAction touchPressAction;
    public InputAction touchDragAction;
    private Camera _mainCamera;

    private InputSystem_Actions _actions;

    private void Start()
    {
        _mainCamera = Camera.main;
        hexGrid = GetComponent<HexGrid>();
        pathfinder = GetComponent<Pathfinding>();


        //   ball = hexGrid.RetreiveBallObj().GetComponent<BallController>();
      //  _actions.Touch.TouchPress.started += ctx => TouchPress(ctx);
       // _actions.Touch.TouchPress.canceled += ctx => EndPress(ctx);

       


    }


    private void Awake()
    {
        _actions = new InputSystem_Actions();

        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing!");
            return;
        }

        touchDragAction = playerInput.actions.FindAction("TouchAim");
        touchPressAction = playerInput.actions.FindAction("TouchPress");

        if (touchDragAction == null)
        {
            Debug.LogError("TouchAim action not found in Input Actions!");
        }
        if (touchPressAction == null)
        {
            Debug.LogError("TouchPress action not found in Input Actions!");
        }
    }


    void Update()
    {



        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
               // Debug.Log("Clicked on: " + hit.collider.gameObject.name);

                // If the object has a specific script
                ITile clickable = hit.collider.gameObject.GetComponentInChildren<ITile>();
                if (clickable != null)
                {
                    if(selected)
                    {

                        if(hit.collider.gameObject.transform.parent.gameObject == clickedTile)
                        {

                           
                            
                            ITile confirmTile = clickedTile.GetComponentInChildren<ITile>();

                            confirmTile.OnConfirm();

                            //Calculate hit works, just need to ensure the hole tile remains active.
                           // CalculateHit(hexGrid.RetreiveCurrentBallTile(), clickedTile);

                            ball.Jump(hexGrid.RetreiveCurrentBallTile(), clickedTile);

                            hexGrid.SwapActiveHex(hit.collider.gameObject.transform.parent.gameObject);

                            selected = false;

                        }
                        else
                        {

                            ITile previousTile = clickedTile.GetComponentInChildren<ITile>();
                            previousTile.OnClick();
                            selected = false;

                        }



                    }

                   
                    selected = true;
                    clickable.OnClick();

                    

                    clickedTile = hit.collider.gameObject.transform.parent.gameObject;

                    ITile ballTile = hexGrid.RetreiveCurrentBallTile().GetComponentInChildren<ITile>();
                    ITile selectedTile = clickedTile.GetComponentInChildren<ITile>();

                    Debug.Log(pathfinder);

                 //   Debug.Log("ballTile is: " + hexGrid.RetreiveCurrentBallTile() + " at: "  + Mathf.RoundToInt(ballTile.GetCoordinates().x) + "," + Mathf.RoundToInt(ballTile.GetCoordinates().y) + ". Selected is: " + 
                   //     clickedTile +" at: " + Mathf.RoundToInt(selectedTile.GetCoordinates().x) + ", " + Mathf.RoundToInt(selectedTile.GetCoordinates().y));

                    List<ITile> path = pathfinder.FindPath(Mathf.RoundToInt(ballTile.GetCoordinates().x), Mathf.RoundToInt(ballTile.GetCoordinates().y), Mathf.RoundToInt(selectedTile.GetCoordinates().x), Mathf.RoundToInt(selectedTile.GetCoordinates().y));

                    Debug.Log("Length is: " + pathfinder.GetPathLength(path));

                }
            }
        }
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
            Vector2 touchPos = GetTouchPosition();
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

    private Vector2 GetTouchPosition()
    {
        return Touchscreen.current.primaryTouch.position.ReadValue();
    }






    private void CalculateHit(GameObject tileWithBall, GameObject targetTile)
    {
        ITile tileBallScript = tileWithBall.GetComponent<ITile>();
        ITile targetTileScript = targetTile.GetComponent<ITile>();


        List<ITile> path = pathfinder.FindPath(Mathf.RoundToInt(tileBallScript.GetCoordinates().x), Mathf.RoundToInt(tileBallScript.GetCoordinates().y), Mathf.RoundToInt(targetTileScript.GetCoordinates().x), Mathf.RoundToInt(targetTileScript.GetCoordinates().y));

        if(pathfinder.GetPathLength(path) < maxHitLength)
        {
            Debug.Log("Hit is valid.");

            ball.Jump(tileWithBall, targetTile);



        }


    }


    public void SetBall(GameObject ballObj)
    {

        ball=ballObj.GetComponent<BallController>();

    }


}
