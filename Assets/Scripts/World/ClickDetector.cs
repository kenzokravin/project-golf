using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    private bool _isDragging;
    private bool hexShift;
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

    public GameObject startingTile;
    public GameObject previousTile;
    public GameObject targetTile;
    public Vector3 crosshairAiming;
    public List<Vector2> targetNeighbours;
    public Vector3 startTouchPosition;
    public Vector3 targetPosition;

    private bool active;



    public InputAction touchPressAction;
    public InputAction touchDragAction;
    private Camera _mainCamera;

    private void Awake()
    {
       
  
    
    }




    private void Start()
    {
        _mainCamera = Camera.main;
        hexGrid = GetComponent<HexGrid>();
        pathfinder = GetComponent<Pathfinding>();
        hexShift = false;

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

    //Get start position as current tile ball is on.
    //Get neighbours of it. Check crosshair position distance is closer or further away to neighbour hexes.
    //if closer, switch active hex, get neighbours and compare distances again.



    private void AimShot()
    {
       // bool hexShift = false;

        

            if(hexShift)
            {
                if (hexGrid.GetNeighbourListCoordinates(previousTile) == null)
                {
                    Debug.LogError("previousTile is null! Aborting AimShot.");
                    return;
                }

                targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
                hexShift = false;

            }

        Vector3 invertedCrossHair = startTouchPosition + (startTouchPosition - crosshairAiming);

        

        float distanceFromStart = Vector3.Distance(targetPosition, invertedCrossHair);

        Debug.Log("Previous Position: " + previousTile.transform.position + ", edited cross hair position: " + invertedCrossHair + ", crosshair possie: "+ crosshairAiming);


        Debug.DrawLine(previousTile.transform.position, startTouchPosition, Color.blue, 0.2f); // Previous Tile -> Start Position
        Debug.DrawLine(startTouchPosition, invertedCrossHair, Color.red, 0.2f); //perfect.

        //Converting vectors to gameObjs, then checking whether the distance between the crosshair and the tile is smaller than the distance from the cross hair and previous tile.
        for (int i = 0; i < targetNeighbours.Count; i++)
            {

                GameObject currentTile = hexGrid.GetHex((int)targetNeighbours[i].x, (int)targetNeighbours[i].y);

                if (currentTile == null)
                {
                    Debug.LogWarning($"Hex at {targetNeighbours[i].x}, {targetNeighbours[i].y} is null. Skipping.");
                    continue; // Skip this iteration if the tile doesn't exist
                }

            Vector3 relativeTilePosition = targetPosition + (currentTile.transform.position - previousTile.transform.position);
            Debug.Log("Relative Position: " + relativeTilePosition);


            Debug.DrawLine(previousTile.transform.position, currentTile.transform.position, Color.green, 0.2f); // Previous Tile -> Current Tile
            Debug.DrawLine(startTouchPosition, relativeTilePosition, Color.yellow, 0.2f); // Start Position -> Relative Tile Position

            // **DEBUG: Draw spheres at key points**
            Debug.DrawRay(previousTile.transform.position, Vector3.up * 0.2f, Color.cyan); // Marker for previous tile
            Debug.DrawRay(currentTile.transform.position, Vector3.up * 0.2f, Color.magenta); // Marker for current tile
            Debug.DrawRay(relativeTilePosition, Vector3.up * 0.2f, Color.white); // Marker for relative position
            Debug.DrawRay(invertedCrossHair, Vector3.up * 0.2f, Color.red);


            if (Vector3.Distance(relativeTilePosition, invertedCrossHair) < distanceFromStart)
                {

               // Debug.Log("Relative Position: " + relativeTilePosition);
                    targetTile = currentTile;
                    previousTile = currentTile;
                    targetPosition = relativeTilePosition;
                    
                    
                    hexShift = true;
                }

            }




        

    }




    private void OnEnable()
    { 
        InputController.OnTouchPositionUpdated += HandleTouchPosition;
        InputController.OnTouchPressDownPosition += OnPressDownPosition;
        InputController.OnTouchPressUpPosition += OnPressUpPosition;

    }

    private void OnDisable()
    {
  
        InputController.OnTouchPositionUpdated -= HandleTouchPosition;
        InputController.OnTouchPressDownPosition -= OnPressDownPosition;
        InputController.OnTouchPressUpPosition -= OnPressUpPosition;

    }

    private void HandleTouchPosition(Vector3 position)
    {
       // Debug.Log("Received Touch Position in TouchListener: " + position);
        // Use the position here


        //Still have to set inverse drag aim mechanic.
        crosshairAiming = position;

        AimShot();


    }

    private void OnPressDownPosition(Vector3 position)
    {
        Debug.Log("Received Touch Down: " + position);

        //Might have to check whether the gamestate is playing or not (or that the press is in the valid play area)
        //This would allow for UI to determine whether it has been struck.

        //Setting startingTile as the current Ball Tile.
        crosshairAiming = position;
        startTouchPosition = position;
        targetPosition = position;
        startingTile = hexGrid.RetreiveCurrentBallTile();
        previousTile = startingTile;
        targetTile = null;
        targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
        _isDragging = true;
        //AimShot();

    }

    private void OnPressUpPosition(Vector3 position)
    {
        Debug.Log("Received Touch Up: " + position);
        // Use the position here

        _isDragging = false;
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
