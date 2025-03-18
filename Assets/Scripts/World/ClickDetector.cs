using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;


public class ClickDetector : MonoBehaviour
{
    private bool _isDragging;
    private bool hexShift;
    private Vector3 _offset;
    [SerializeField] private GameObject clickedTile;
    [SerializeField] private bool selected = false;
    [SerializeField] private BallController ball;

    [SerializeField] private int maxHitLength = 7;

    public Pathfinding pathfinder;
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
    [SerializeField] GameObject targetCrosshair;
    [SerializeField] GameObject activeCrosshair;
    public Vector3 tileheightOffset = new Vector3 (0,0,-.25f);

    private bool active;
    private ITile ballTileScript;


    public InputAction touchPressAction;
    public InputAction touchDragAction;
    private Camera _mainCamera;


    public LineRenderer lineRenderer;
    public int resolution = 20; // Number of points for smoothness
    public float arcHeight = 2f; // How high the arc should go

    public float hitTime = 1f; //How long the player has to hit the ball.
    public float hitTolerance = .5f; //How lenient the hit is.
    public float minPower = 0.3f;         // Minimum shot power (if mistimed)
    public float maxPower = 1.0f;

    public float holdTime = 0f;
    public float shotDistance = 0;

    public int shotCount = 0;
    public TMP_Text currentShots;

    public GameUIManager gameUIManager;

    private void Awake()
    {
       
  
    
    }




    private void Start()
    {
        gameUIManager = GameObject.FindGameObjectWithTag("InputController").GetComponent<GameUIManager>();
        _mainCamera = Camera.main;
        hexGrid = GetComponent<HexGrid>();
        pathfinder = GetComponent<Pathfinding>();
        hexShift = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.05f;

        activeCrosshair = Instantiate(targetCrosshair);
        activeCrosshair.SetActive(false);
       

    }


    void Update()
    {

        if(_isDragging)
        {
            AimCrosshair();
            //AimShotOriginal();
            holdTime += Time.deltaTime;
        }

        if(_isDragging)
        {




        }




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

                    List<ITile> path = pathfinder.FindPath(Mathf.RoundToInt(ballTile.GetCoordinates().x), Mathf.RoundToInt(ballTile.GetCoordinates().y), Mathf.RoundToInt(selectedTile.GetCoordinates().x), Mathf.RoundToInt(selectedTile.GetCoordinates().y));

                    Debug.Log("Length is: " + pathfinder.GetPathLength(path));

                }
            }
        }
    }

    //Get start position as current tile ball is on.
    //Get neighbours of it. Check crosshair position distance is closer or further away to neighbour hexes.
    //if closer, switch active hex, get neighbours and compare distances again.


    private void AimCrosshair()
    {

        Vector3 invertedCrossHair = startTouchPosition + (startTouchPosition - crosshairAiming);



        float distanceFromStart = Vector3.Distance(targetPosition, invertedCrossHair);

        Vector3 relativeTilePosition = Vector3.zero;

        

        relativeTilePosition = startingTile.transform.position + (invertedCrossHair - startTouchPosition);

        activeCrosshair.transform.position = relativeTilePosition + tileheightOffset;

       // Debug.DrawLine(startingTile.transform.position, relativeTilePosition, Color.blue, 0.2f);
    }







    private void AimShot()
    {
        if (hexShift)
        {
            targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
            if (targetNeighbours == null || targetNeighbours.Count == 0)
            {
                Debug.LogError("No valid neighbors found.");
                return;
            }
            hexShift = false;
        }

        Vector3 invertedCrossHair = startTouchPosition + (startTouchPosition - crosshairAiming);
        int neighborCount = targetNeighbours.Count;

        float3 invertedCrossFloat = invertedCrossHair;

        NativeArray<float2> nativeNeighbors = new NativeArray<float2>(neighborCount, Allocator.TempJob);
        NativeArray<float3> neighborPositions = new NativeArray<float3>(neighborCount, Allocator.TempJob);
        NativeArray<float> distances = new NativeArray<float>(neighborCount, Allocator.TempJob);

        for (int i = 0; i < neighborCount; i++)
        {
            nativeNeighbors[i] = targetNeighbours[i];
            GameObject neighborTile = hexGrid.GetHex((int)targetNeighbours[i].x, (int)targetNeighbours[i].y);
            if (neighborTile != null)
            {
                neighborPositions[i] = neighborTile.transform.position;
            }
            else
            {
                neighborPositions[i] = math.INFINITY; // Prevent selecting null tiles
            }
        }

        FindClosestTileJob job = new FindClosestTileJob
        {
            invertedCrossHair = invertedCrossFloat,
            targetPosition = targetPosition,
            neighbors = nativeNeighbors,
            neighborPositions = neighborPositions,
            distances = distances
        };

        JobHandle jobHandle = job.Schedule(neighborCount, 1);
        jobHandle.Complete();

        // Find the best tile with the minimum distance
        int bestIndex = -1;
        float minDistance = float.MaxValue;
        for (int i = 0; i < neighborCount; i++)
        {
            if (distances[i] < minDistance)
            {
                minDistance = distances[i];
                bestIndex = i;
            }
        }

        if (bestIndex != -1)
        {
            GameObject bestTile = hexGrid.GetHex((int)nativeNeighbors[bestIndex].x, (int)nativeNeighbors[bestIndex].y);
            if (bestTile != null)
            {
                targetTile = bestTile;
                previousTile = bestTile;
                targetPosition = targetPosition + (bestTile.transform.position - previousTile.transform.position);
                hexShift = true;
            }
        }

        // Dispose of NativeArrays
        nativeNeighbors.Dispose();
        neighborPositions.Dispose();
        distances.Dispose();
    }


    private void AimShotCustom()
    {

        if (hexShift)
        {
            if (hexGrid.GetNeighbourListCoordinates(previousTile) == null)
            {
                Debug.LogError("previousTile is null! Aborting AimShot.");
                return;
            }
            // DrawTrajectory(startingTile.transform.position, targetTile.transform.position);
            targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
            hexShift = false;

        }

        //Inverting the  crosshair for pullback aiming.
        Vector3 invertedCrossHair = startTouchPosition + (startTouchPosition - crosshairAiming);



        float distanceFromStart = Vector3.Distance(targetPosition, invertedCrossHair);

        Vector3 relativeTilePosition = Vector3.zero;

        // Debug.DrawLine(previousTile.transform.position, startTouchPosition, Color.blue, 0.2f); // Previous Tile -> Start Position
        // Debug.DrawLine(startTouchPosition, invertedCrossHair, Color.red, 0.2f); //perfect.



        //Converting vectors to gameObjs, then checking whether the distance between the crosshair and the tile is smaller than the distance from the cross hair and previous tile.
        for (int i = 0; i < targetNeighbours.Count; i++)
        {

            GameObject currentTile = hexGrid.GetHex((int)targetNeighbours[i].x, (int)targetNeighbours[i].y);

            if (currentTile == null)
            {
                Debug.LogWarning($"Hex at {targetNeighbours[i].x}, {targetNeighbours[i].y} is null. Skipping.");
                continue; // Skip this iteration if the tile doesn't exist
            }

            //Translating the tile position to the clicked target, so user can click anywhere and pull back and it maps directly.
            relativeTilePosition = targetPosition + (currentTile.transform.position - previousTile.transform.position);


            //    Debug.DrawLine(previousTile.transform.position, currentTile.transform.position, Color.green, 0.2f); // Previous Tile -> Current Tile
            //  Debug.DrawLine(startTouchPosition, relativeTilePosition, Color.yellow, 0.2f); // Start Position -> Relative Tile Position


            if (Vector3.Distance(relativeTilePosition, invertedCrossHair) < distanceFromStart)
            {

                // Debug.Log("Relative Position: " + relativeTilePosition);
                targetTile = currentTile;
                previousTile = currentTile;
                targetPosition = relativeTilePosition;



                ITile currentTileScript = currentTile.GetComponentInChildren<ITile>();

                //   shotDistance = pathfinder.GetPathLength(path);


                hexShift = true;
            }

        }


    }



    //To make shot faster, could set it so it works off position if there is certain speed?
    private void AimShotOriginal()
    {

            if(hexShift)
            {
                if (hexGrid.GetNeighbourListCoordinates(previousTile) == null)
                {
                    Debug.LogError("previousTile is null! Aborting AimShot.");
                    return;
                }
           // DrawTrajectory(startingTile.transform.position, targetTile.transform.position);
            targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
                hexShift = false;

            }

        //Inverting the  crosshair for pullback aiming.
        Vector3 invertedCrossHair = startTouchPosition + (startTouchPosition - crosshairAiming);

        

        float distanceFromStart = Vector3.Distance(targetPosition, invertedCrossHair);

        Vector3 relativeTilePosition = Vector3.zero;

        // Debug.DrawLine(previousTile.transform.position, startTouchPosition, Color.blue, 0.2f); // Previous Tile -> Start Position
        // Debug.DrawLine(startTouchPosition, invertedCrossHair, Color.red, 0.2f); //perfect.



        //Converting vectors to gameObjs, then checking whether the distance between the crosshair and the tile is smaller than the distance from the cross hair and previous tile.
        for (int i = 0; i < targetNeighbours.Count; i++)
            {

                GameObject currentTile = hexGrid.GetHex((int)targetNeighbours[i].x, (int)targetNeighbours[i].y);

                if (currentTile == null)
                {
                    Debug.LogWarning($"Hex at {targetNeighbours[i].x}, {targetNeighbours[i].y} is null. Skipping.");
                    continue; // Skip this iteration if the tile doesn't exist
                }

            //Translating the tile position to the clicked target, so user can click anywhere and pull back and it maps directly.
            relativeTilePosition = targetPosition + (currentTile.transform.position - previousTile.transform.position);


        //    Debug.DrawLine(previousTile.transform.position, currentTile.transform.position, Color.green, 0.2f); // Previous Tile -> Current Tile
          //  Debug.DrawLine(startTouchPosition, relativeTilePosition, Color.yellow, 0.2f); // Start Position -> Relative Tile Position


            if (Vector3.Distance(relativeTilePosition, invertedCrossHair) < distanceFromStart)
                {

               // Debug.Log("Relative Position: " + relativeTilePosition);
                    targetTile = currentTile;
                    previousTile = currentTile;
                    targetPosition = relativeTilePosition;

                    

                ITile currentTileScript = currentTile.GetComponentInChildren<ITile>();

             //   shotDistance = pathfinder.GetPathLength(path);


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

     //   AimShot();


    }

    private void OnPressDownPosition(Vector3 position)
    {
        //Debug.Log("Received Touch Down: " + position);

        //Might have to check whether the gamestate is playing or not (or that the press is in the valid play area)
        //This would allow for UI to determine whether it has been struck.

        //Setting startingTile as the current Ball Tile.

        activeCrosshair.SetActive(true);
        activeCrosshair.transform.position = position;

        shotDistance = 0f;
        holdTime = 0f;
        crosshairAiming = position;
        startTouchPosition = position;
        targetPosition = position;
        startingTile = hexGrid.RetreiveCurrentBallTile();

        ballTileScript = startingTile.GetComponentInChildren<ITile>();

        previousTile = startingTile;
        targetTile = null;
        targetNeighbours = hexGrid.GetNeighbourListCoordinates(previousTile);
        _isDragging = true;
  

    }

    private void OnPressUpPosition(Vector3 position)
    {
       // Debug.Log("Received Touch Up: " + position);
        // Use the position here

        //Where we would confirm the shot.

        ReleaseShot();

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

    private void Hit()
    {


        //Calculate hit works, just need to ensure the hole tile remains active.
        // CalculateHit(hexGrid.RetreiveCurrentBallTile(), clickedTile);
    


        ball.Jump(hexGrid.RetreiveCurrentBallTile(),targetTile);

        hexGrid.SwapActiveHex(targetTile);

        gameUIManager.addToCurrentShots(1);

        selected = false;







    }

    private void ReleaseShot()
    {

        targetTile = hexGrid.GetHexFromWorldPosition(activeCrosshair.transform.position);


       // float distanceQuality = CalculatePower(holdTime);
        Hit();



    }

    private float CalculatePower(float holdDuration)
    {
        float difference = Mathf.Abs(holdDuration - hitTime);

        if (difference <= hitTolerance)
        {
            return maxPower; // Perfect shot
        }
        else
        {
            return Mathf.Lerp(maxPower, minPower, difference / (hitTime + hitTolerance));
        }
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 endPos)
    {
        List<Vector3> trajectoryPoints = new List<Vector3>();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution; // Normalized time (0 to 1)
            Vector3 point = Vector3.Lerp(startPos, endPos, t); // Linear interpolation
            point.z += Mathf.Sin(t * Mathf.PI) * -arcHeight; // Add height to create an arc
            trajectoryPoints.Add(point);
        }

        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());
    }




}




[BurstCompile]
public struct FindClosestTileJob : IJobParallelFor
{
    [ReadOnly] public float3 invertedCrossHair;
    [ReadOnly] public float3 targetPosition;
    [ReadOnly] public NativeArray<float2> neighbors;
    [ReadOnly] public NativeArray<float3> neighborPositions;

    [WriteOnly] public NativeArray<float> distances;

    public void Execute(int index)
    {
        Vector3 relativeTilePosition = targetPosition + (neighborPositions[index] - targetPosition);
        distances[index] = Vector3.Distance(relativeTilePosition, invertedCrossHair);

        

    }
}