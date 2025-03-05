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




    private void Start()
    {
        _mainCamera = Camera.main;
        hexGrid = GetComponent<HexGrid>();
        pathfinder = GetComponent<Pathfinding>();


        //   ball = hexGrid.RetreiveBallObj().GetComponent<BallController>();
      //  _actions.Touch.TouchPress.started += ctx => TouchPress(ctx);
       // _actions.Touch.TouchPress.canceled += ctx => EndPress(ctx);

       


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
        InputController.OnTouchPositionUpdated += HandleTouchPosition;

    }

    private void OnDisable()
    {

        _actions.Disable();
        InputController.OnTouchPositionUpdated -= HandleTouchPosition;

    }

    private void HandleTouchPosition(Vector3 position)
    {
        Debug.Log("Received Touch Position in TouchListener: " + position);
        // Use the position here
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
