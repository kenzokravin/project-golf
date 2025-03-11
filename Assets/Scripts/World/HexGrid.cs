using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class HexGrid : MonoBehaviour
{

    [SerializeField] public int mapWidth = 10;
    [SerializeField] public int mapHeight = 10;
    public float tileSize = 1;
    private float tileHorizWidth;

    public GameObject defaultHexPrefab;
    public GameObject fairwayPrefab;
    public GameObject outTilePrefab;
    public GameObject sandPrefab;
    public GameObject roughPrefab;
    public GameObject greenPrefab;
    public GameObject holePrefab;
    public GameObject ballPrefab;
    [SerializeField] private GameObject cloud;

    [SerializeField] GameObject currentBallTile;
    [SerializeField] GameObject ball;
    BallController ballController;
    public ClickDetector clickDetector;

    [SerializeField] int holeNum;
    [SerializeField] private bool isInitializing;
    [SerializeField] private int poolCounter;

    [SerializeField] GameObject holeTile;
    [SerializeField] private int courseWidth;
    [SerializeField] private Vector3 startTilePosition;
    [SerializeField] private float landHeightValue = 0.05f;
    private Vector3 startGenPosition;


    public Camera mainCam;

    //Noise Settings
    [SerializeField] private float noiseFrequency = 100f;
    [SerializeField] private float noiseThreshold = .5f;
    [SerializeField] private float noiseSeed = 1234567;
    [SerializeField] private float landNoiseSeed = 1234567;

    [SerializeField] private List<GameObject> activeTiles = new List<GameObject>();
    [SerializeField] private List<GameObject> pooledTiles = new List<GameObject>();
    [SerializeField] private List<GameObject> cloudPool = new List<GameObject>();

    [SerializeField] private GameObject movingCont;
    public Vector3 lastSpawnPosition;
    public GameObject movingContainer;
    public float maxHexHeight;
    public float highestCoordPooled;
    public bool isSpawning = false;

    private bool isMoving;
    public Vector2 nextHole;
    public int parOffset;
    public int holeSpawnRow;

    [SerializeField] GameObject[] celebrationSprites = new GameObject[6];
    


    void Start()
    {
        parOffset = 0;
        nextHole = new Vector2(0, 0);
        isMoving = false;
        pooledTiles = new List<GameObject>();
        holeNum = 0;
       
        clickDetector = GetComponent<ClickDetector>();
      
        isInitializing = false;
        poolCounter = 0;
        highestCoordPooled = 0f;

        InitializeMapGrid();
    }

    void Update()
    {
        

    }

    public void PoolHex(GameObject hexToPool, ITile tile)
    {

        //This will be called in tile script when they reach the bounds of the camera.

        if (!activeTiles.Contains(hexToPool))
        {
            Debug.LogWarning($" Hex {hexToPool.name} not found in activeTiles.");
            return;
        }


        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i] == hexToPool)
            {

                float yCoord = tile.GetCoordinates().y;

                if (yCoord > 0)
                {
                    if(holeNum != 0)
                    {
                        //This drops the y value of the current tiles, so y=0 is always the bottom-most tile.
                        //It also calls to spawn the next row.
                        ReassignCoords();

                        highestCoordPooled++;

                    }
                }

                activeTiles.RemoveAt(i);
         
                hexToPool.transform.parent = gameObject.transform;             

                hexToPool.transform.position = new Vector3(0, 0, 0);

                pooledTiles.Add(hexToPool);
                hexToPool.SetActive(false);
                break;

            }
        }

    }

 
    private void ReassignCoords()
    {
        //This function will be used to reassign the y coords of the moving hex values. (should be done before they are moved?)
       

        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
           

                ITile activeTile = activeTiles[i].GetComponentInChildren<ITile>();
                activeTile.AssignCoordinate(activeTile.GetCoordinates().x, activeTile.GetCoordinates().y - 1);

        }


      
        
        Debug.Log("Reassigning");
        SpawnNextRow();
        

}


    private void SpawnNextRow()
    {

        //This might be able to be fixed by spawning based on the previous row position and not on the start generation position.
        //This allows for us to consider the dynamic nature of moving the tiles (so there will not be any gaps between the rows.)

        for (int x = 0; x < mapWidth; x++)
        {

            //Should spawn at map height everytime.

            Vector3 hexCoords = GetNextHexCoords(x, mapHeight);

            if(nextHole.x == x && nextHole.y == highestCoordPooled)
            {

            }


            CheckWaterValue(hexCoords,x, mapHeight);

        }



    }

    private void MoveHexes()
    {
        //This moves the hexes down, signalled when hole is completed.

        highestCoordPooled = 0;

        movingContainer = Instantiate(movingCont,gameObject.transform);
        lastSpawnPosition = movingContainer.transform.position;


        for (int i = activeTiles.Count -1; i >= 0; i--)
        {

            activeTiles[i].transform.SetParent(movingContainer.transform);

            ITile activeTile = activeTiles[i].GetComponentInChildren<ITile>();

        }

        //To adjust for holes, the movement would have to be adjusted (based on full tile size and number of tiles to be shifted)

        isMoving = true;

        movingContainer.transform.DOMove(new Vector3(0, -(tileSize * parOffset), 0), 1.5f)
          .SetEase(Ease.InOutCubic)
          .OnComplete(() =>
          {
                // Unparent objects
                foreach (GameObject obj in activeTiles)
              {
                  obj.transform.SetParent(gameObject.transform);
              }

                // Destroy the container
                Destroy(movingContainer);
               movingContainer = null;

              isMoving = false;
              ReassignCoords();

          });
    }

    private void CheckWaterValue(Vector3 hexCoords, int xCoord, int yCoord)
    {
        Vector2 camBounds = GetCameraBounds();

        //Determining the noise offsetValue
        float noiseOffset = holeNum * mapHeight;

        if (noiseSeed == -1)
        {
            noiseSeed = Random.Range(0, 10000);

        }


        //Generating hole.
        if (nextHole.x == xCoord && holeSpawnRow == (highestCoordPooled))
        {
           // Debug.Log("next hole genning!");

            if (CheckHexPoolWater(xCoord, mapHeight, hexCoords, "Hole", 0f))
            {
              //  Debug.Log("Hole Pool Reached");
                return;
            }

            GameObject holeTile = Instantiate(holePrefab, hexCoords, Quaternion.Euler(0, 0, 90));
            ITile tileScript = holeTile.GetComponentInChildren<ITile>();
            tileScript.AssignCoordinate(xCoord, (yCoord));
            if (holeNum != 0 && isMoving)
            {
                //Setting parent of moving pool to moving container.
                holeTile.transform.parent = movingContainer.transform;
                //  pooledObj.transform.position = Vector3.zero;
            }

            return;

        }

        //Generating waterValue to determine if water hex or not.
        float waterValue = Mathf.PerlinNoise(((hexCoords.x) * noiseSeed) / noiseFrequency, ((hexCoords.y + noiseOffset) * noiseSeed) / noiseFrequency);

        if (hexCoords.x - (tileHorizWidth * 0.75f) <= -camBounds.x * .5f || hexCoords.x + (tileHorizWidth * 0.75f) >= camBounds.x * .5f)
        {
            //This is setting the width tiles to the water tiles.

            Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));


            if (CheckHexPoolWater(xCoord, yCoord, position, "Water", 0))
            {

                // continue;
                return;
            }


            GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0, 0, 90));
            ITile tileScript = tile.GetComponentInChildren<ITile>();
            tileScript.AssignCoordinate(xCoord, (yCoord));
            tile.transform.parent = transform;

            activeTiles.Add(tile);



        }
        else
        {

            bool isWater = waterValue < noiseThreshold;

            if (isWater)
            {

                Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0.02f, 0.07f, waterValue / noiseThreshold)));

                //May be issue with array size causing memory issues.

                //Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

                if (CheckHexPoolWater(xCoord, yCoord, positionWater, "Water", 0))
                {

                    return;
                }



                GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.Euler(0, 0, 90));
                tileWater.transform.parent = transform;

                ITile tileWaterScript = tileWater.GetComponentInChildren<ITile>();
                tileWaterScript.AssignCoordinate(xCoord, yCoord );

                activeTiles.Add(tileWater);

                return;



            }
            else
            {

                //    Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");
                InitLandTile(hexCoords.x, hexCoords.y, waterValue, xCoord, yCoord);

            }

        }
    



    }

    void InitializeMapGrid()
    {

        if(holeNum != 0)
        {
           return;
        }

        isInitializing = true;

        if (fairwayPrefab == null)
        {
            return;
        }

        MeshRenderer renderer = defaultHexPrefab.GetComponentInChildren<MeshRenderer>();

        Vector3 size = renderer.bounds.size;
            float tileWidth = size.x;
            float tileHeight = size.y;
            tileSize = tileHeight;

        tileHorizWidth = tileWidth;

            Vector2 camBounds = GetCameraBounds();

        

            mapWidth = Mathf.FloorToInt((camBounds.x) / (tileWidth * 0.75f)) + 2;
           // mapHeight = Mathf.FloorToInt(camBounds.y / (tileHeight * 0.75f));

        

        int tileHorizontalNum = 0;


        float xStartPositionCoord = 0 * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        float xEndPositionCoord = 0;


        for (int x = 0; x < mapWidth; x++)
        {
    
            tileHorizontalNum++;
            xEndPositionCoord = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        }


        courseWidth = tileHorizontalNum;

        float xTotalWidth = xEndPositionCoord - xStartPositionCoord;

        xTotalWidth = xTotalWidth / 2;

        // Calculate the starting X position (shift grid right to center)
        float startX = -xTotalWidth;

        // Get the camera's position and rotation
        Vector3 cameraPosition = mainCam.transform.position;  // Camera position at (0, -25, -25)
        Quaternion cameraRotation = mainCam.transform.rotation;  // Camera rotation of (-45, 0, 0)

        // Get the camera's orthographic size (half the height of the view)
        float orthographicSize = mainCam.orthographicSize;

        // Calculate the direction the camera is facing based on its rotation
        Vector3 cameraForward = cameraRotation * Vector3.forward;  // Camera's forward direction considering rotation

        // Calculate the vertical offset based on the camera's tilt (along the Y axis)
        float verticalVisibility = orthographicSize * Mathf.Abs(cameraForward.y);

        // Calculate the lowest point that the camera can see at (x = 0, z = 0)
        float lowestY = cameraPosition.y + verticalVisibility;

        maxHexHeight = (-camBounds.y * .5f) + (tileSize * (mapHeight)) + (0.5f * tileSize);

        maxHexHeight = 100f;


        Vector3 startPosition = new Vector3(startX, -camBounds.y * .5f, 0);

        startTilePosition = startPosition;
        startGenPosition = startPosition;   


        var result = GenerateHoleCoords(tileHorizontalNum);

        Vector2 holeCoords = new Vector2(result.Item1,result.Item2);
        Debug.Log("Hole Coords: " + holeCoords);

        float noiseOffset = holeNum * mapHeight;



        for (int z = 0; z < mapHeight * 2; z++)
        {
            if(z == mapHeight)
            {
                //If one hole has been genned, gen next hole coords.
                var nextHoleResult = GenerateHoleCoords(tileHorizontalNum);
                 holeCoords = new Vector2(nextHoleResult.Item1, (nextHoleResult.Item2 + mapHeight));
                Debug.Log("2nd Hole Coords: " + holeCoords);

            }

            for (int x = 0; x < mapWidth; x++) 
            {

                //Checking the hole coords to ensure that it isn't overwritten.

                Vector3 hexCoords = GetHexCoords(x, (z + (holeNum != 0 ? mapHeight : 0))) + startPosition;


                if (noiseSeed == -1)
               {
                    noiseSeed = Random.Range(0, 10000);

               }

                float waterValue = Mathf.PerlinNoise(((hexCoords.x) * noiseSeed) / noiseFrequency, ((hexCoords.y + noiseOffset) * noiseSeed) / noiseFrequency);



                //This is checking if coords are the same as the hole gen coords.
                if (Mathf.Approximately(holeCoords.x, x) && Mathf.Approximately(holeCoords.y, (z + (holeNum != 0 ? mapHeight : 0))))
                {

                    Debug.Log("Hole Generated: " + x + ", " + z);

                  //  Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

                    //-------------------------------
                    //Need to add hole type script to hole.
                    if (CheckHexPoolWater(x, z, hexCoords, "Hole",0))
                    {
                        Debug.Log("Hole Pool Reached");
                          continue;
                    }

                    InitHole(startPosition, (int)holeCoords.x, (int)holeCoords.y, 0);


                 //   Debug.Log("Matched: " + new Vector2(x, (z + (holeNum != 0 ? mapHeight : 0))));

                    continue;
                }



                if (hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));

                   // if (CheckHexPoolWater(x, z, position,"Water",0))
                  //  {

                   //    continue;
                  //  }


                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0,0,90));
                    ITile tileScript = tile.GetComponentInChildren<ITile>();

                    //Debug.Log((z + (holeNum != 0 ? mapHeight : 0)));
                    tileScript.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));
                   // tileScript.SetUpperBounds(maxHexHeight);
                    tile.transform.parent = transform;

                    ITile tileChildScript = tile.GetComponentInChildren<ITile>();
                    tileChildScript.SetUpperBounds(maxHexHeight);

                    activeTiles.Add(tile);



                } else {

                    bool isWater = waterValue < noiseThreshold;

                    if (isWater)
                    {

                        Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0.02f,0.07f,waterValue/noiseThreshold)));

                        //May be issue with array size causing memory issues.

                       // if (CheckHexPoolWater(x,z,positionWater,"Water",0))
                       // {

                        //    continue;
                      //  }
                        


                            GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.Euler(0, 0, 90));
                            tileWater.transform.parent = transform;

                            ITile tileWaterScript = tileWater.GetComponentInChildren<ITile>();
                            tileWaterScript.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));
                         tileWaterScript.SetUpperBounds(maxHexHeight);

                        ITile tileChildScript = tileWater.GetComponentInChildren<ITile>();
                        tileChildScript.SetUpperBounds(maxHexHeight);


                        activeTiles.Add(tileWater);

                            continue;

                        

                    } else
                    {

                   
                        InitLandTile(hexCoords.x, hexCoords.y, waterValue, x, z);

                    }
                   
                }
            }

        }


        //If the hole is not the start hole then do not spawn the ball
        if(holeNum == 0 ? true : false) BallSpawn();


        //Pools starter hexes.
        PoolStarterHexes();


        isInitializing = false;

       // poolCounter++;



    }

    private void PoolStarterHexes()
    {
        if (pooledTiles == null)
        {
            Debug.LogError("pooledTiles is null!");
            return;
        }

        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = activeTiles[i];
            ITile tileScript = tile.GetComponentInChildren<ITile>();

            if (tileScript == null)
            {
                Debug.LogWarning(tile + " has no ITile component!");
                continue;
            }

            Vector3 coordinates = tileScript.GetCoordinates();
  
            if (coordinates.y > 30)
            {
                if (pooledTiles.Contains(tile))
                {
                    Debug.LogWarning(tile + " is already in pooledTiles!");
                    continue;
                }

                tile.SetActive(false); // Deactivate first
                tile.transform.parent = gameObject.transform;
                tile.transform.position = Vector3.zero;

                pooledTiles.Add(tile);
                activeTiles.RemoveAt(i);
            }
        }
    }


    private void InitHole(Vector3 startPosition, int x, int y, float height)
    {


        //Generate a Y Value between 23-28 (The green zone) and randomly select an x coord.
        //Generate Tile based on this coord and the start position.
        //Store coord values and skip generate when the generation loop encounters the hole value.

        //Will have to find a range that is x tiles
        float tileHeight = (Mathf.Lerp(0f, 0.06f, height / (1 - noiseThreshold)));

        Vector3 hexCoords = GetHexCoords(x, y) + startPosition;

        //    Debug.Log("Hole GENNN");

        GameObject hole = Instantiate(holePrefab, new Vector3(hexCoords.x, hexCoords.y, -tileHeight), Quaternion.Euler(-90, 0, 0));

        if (holeTile == null) { 
              holeTile = hole;
        }
       ITile tileScript = hole.GetComponentInChildren<ITile>();
        tileScript.SetUpperBounds(maxHexHeight);
        tileScript.AssignCoordinate(x, y);

        activeTiles.Add(hole);

      

    }

    private (int,int) GenerateHoleCoords(int tileNumberHorizontal)
    {
        //Generates random hole coords per hole.
        int x = Random.Range(3, tileNumberHorizontal - 3);
        int y = Random.Range(21, 25);
        
        return (x, y);

    }



    private Vector3 GetHexCoords(int x, int z)
    {
        float xPos = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        float zPos = z * tileSize + ((x % 2 == 1) ? tileSize * .5f : 0);

        Vector3 position = new Vector3(xPos, zPos, 0);



        return position;

    }

    private Vector3 GetNextHexCoords(int x, int y)
    {
        //This might be able to be fixed by spawning based on the previous row position and not on the start generation position.
        //This allows for us to consider the dynamic nature of moving the tiles (so there will not be any gaps between the rows.)


        float xPos = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);

        GameObject previousRowHex = GetHex(x, y - 1);

        Vector3 prevPosition = previousRowHex.transform.position;

        float yOffset = tileSize;

        Vector3 newPosition;

        if (previousRowHex.transform.position == null)
        {

            newPosition = new Vector3(xPos,tileSize,0);


        } else
        {
            newPosition = new Vector3(prevPosition.x, prevPosition.y + yOffset, 0);
        }
       
        return newPosition;


    }


    private Vector2 GetCameraBounds()
    {
        float camHeight = 2f * mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        return new Vector2(camWidth, camHeight);


    }

    private void InitLandTile(float xCoord, float yCoord, float waterValue, int x, int z)
    {

       if (landNoiseSeed == -1)
        {
            landNoiseSeed = Random.Range(0, 10000);
         //   Debug.Log("Land Seed: " + landNoiseSeed);
        }


        string landTile = null;

        float noiseOffset = holeNum * mapHeight;

        float landValue = Mathf.PerlinNoise((xCoord * landNoiseSeed) / noiseFrequency, ((yCoord + noiseOffset) * landNoiseSeed) / noiseFrequency);
        float height = 0;

        if(landValue < 0.25)
        {

            landTile = "Sand";
           height = (Mathf.Lerp(0f,landHeightValue, waterValue / (1-noiseThreshold)));

        } else if (landValue >= 0.25 && landValue < 0.6)
        {
            landTile = "Fairway";
            height = (Mathf.Lerp(0f, landHeightValue, waterValue / (1 - noiseThreshold)));

        } else
        {

            landTile = "Rough";
            height = (Mathf.Lerp(0f, landHeightValue, waterValue / (1 - noiseThreshold)));
          //  Debug.Log("height: " + height + " .WaterVal: " + waterValue + " . Land Val: " + landValue);

            

        }

        Vector3 position = new Vector3(xCoord, yCoord, -height);

        //Checking if land tile is available in object pool by checking for the tile type.
        //This could be done better by cycling through before and passing the tile into this method. (reducing number of for loop).
    

        if(CheckHexPoolWater(x, z, position, landTile, 0))
        {
           // Debug.Log("Pooled Land ObJ spawned, with a noise value: " + landValue);
            return;
        };



        //The below only creates an object if the object pool is void of any matching types.
        GameObject landHex = null;

        switch (landTile)
        {

            case "Sand":
                landHex = sandPrefab;
                break;
            case "Fairway":
                landHex = fairwayPrefab;
                break;
            case "Rough":
                landHex = roughPrefab;
                break;

        }

       // Debug.Log("Instantiated type: " + landTile + ", at: " + x + ", " + (z + (holeNum != 0 ? mapHeight : 0)) + " with a Noise Value of: " + landValue);

        GameObject tile = Instantiate(landHex, position, Quaternion.Euler(-90, 0, 0));
        ITile tileScript = tile.GetComponentInChildren<ITile>();
         tileScript.SetUpperBounds(maxHexHeight);

       // Debug.Log("yVal of spawned hex: " + z);
        tileScript.AssignCoordinate(x, (z ));



        foreach (Transform child in tile.transform.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Hex"))
            {
             //   Debug.Log(child);
                ITile tileChildScript = child.GetComponent<ITile>();
                tileChildScript.SetUpperBounds(maxHexHeight);

                break;
            }
        }


      


        activeTiles.Add(tile);
        tile.transform.parent = transform;



    }


    public void BallSpawn()
    {
        bool ballSpawnLoop = true;

        //The following loop spawns the inital state of the ball.
        //If the tile type is not a Fairway type, then the ball does not spawn and the loop continues until it finds one.

        while (ballSpawnLoop)
        {

            int x = Random.Range(3, courseWidth - 3);
            int z = Random.Range(4, 8);

            for (int i = 0; i < activeTiles.Count; i++)
            {
                ITile tileScript = activeTiles[i].GetComponentInChildren<ITile>();

                if (tileScript.GetCoordinates() == new Vector2(x, z))
                {

                    if (tileScript.GetTileType() != "Fairway") continue;



                    Vector3 position = (GetHexCoords(x, z) + startTilePosition) + new Vector3(0, 0, -.25f);


                    position = tileScript.GetPosition() + new Vector3(0, 0, -.25f);

                    ball = Instantiate(ballPrefab, position, Quaternion.identity);

                    currentBallTile = activeTiles[i];

                    ballController = ball.GetComponent<BallController>();

                    ballController.SetHexGrid(activeTiles[i]);

                    Debug.Log("ball is: " + ball);

                    Debug.Log("clickDetection: " + clickDetector);

                    clickDetector.SetBall(ball);

                    Debug.Log("ballTileSet");

                   ballSpawnLoop = false;

                    


                }

            }

        }
 

    }


    public void SwapActiveHex(GameObject chosenHex)
    {

        if (chosenHex == currentBallTile) return;
       
        //Here would be where we could use the interface to check if the chosen hex is of a certain type (or we refer to the ball).
        //This could then trigger different states of the game/ball (i.e. stage is over, ball bounces, ball rolls etc)

       // ballController.Jump(currentBallTile,chosenHex);

        ITile tileScript = currentBallTile.GetComponentInChildren<ITile>();
        tileScript.GetCoordinates();



        currentBallTile = chosenHex;

        Flip(currentBallTile);

        ballController.SetHexGrid(currentBallTile);

        //If hex is the holeTile. Start End Hole Cycle.
        if (currentBallTile == holeTile)
        {

            HoleCycle();

        }
       

       // currentBallTile = chosenHex;

      //  ballController.SetHexGrid(currentBallTile);

    }

    private void HoleCycle()
    {
        //Plays celebrationAnimation.
      //  Celebrate(RetreiveCurrentBallTile());

        holeNum++;

        //Making next hole level.
        //It is here where we increase hole level, play success animations, calculate next hole coords and generate the next hole.

        int holeStartRow = Random.Range(5, 7);

        //here we could get the row of the next tee off, then find the position of the current hole tile and then the yCoord and then multiply by tile size.
        ITile currentBallTile = holeTile.GetComponentInChildren<ITile>();

        parOffset = (int)currentBallTile.GetCoordinates().y - holeStartRow;

        holeSpawnRow = mapHeight - holeStartRow;


        var holeGen = GenerateHoleCoords(mapWidth);
        nextHole = new Vector2(holeGen.Item1, holeGen.Item2);

        holeSpawnRow = (int)currentBallTile.GetCoordinates().y-(holeStartRow + ((mapHeight+1) - holeGen.Item2));



        if (holeNum > 0)
        {
          //  ShiftHexPositions();
          MoveHexes();
            //Move the holes

        }

    }

    public GameObject GetHex(int x, int y)
    {

        GameObject retHex = null;

        for(int i = 0;i < activeTiles.Count; i++)
        {

            ITile tile = activeTiles[i].GetComponentInChildren<ITile>();

            if(tile.GetCoordinates().x == x && tile.GetCoordinates().y == y)
            {

                retHex = activeTiles[i];

            } 
        }
       
     
        return retHex;

    }

    private bool CheckHexPoolWater(int x, int z ,Vector3 position, string tileType, float height)
    {
        bool ret = false;

        for (int i = pooledTiles.Count - 1; i >= 0; i--)
        {
            ITile pooledTile = pooledTiles[i].GetComponentInChildren<ITile>();
            if (pooledTile.GetTileType() == tileType)
            {

                pooledTiles[i].SetActive(true);

                //Debug.Log("Object UnPooled at: " + x + ", " + z + ". Type: " + tileType);

                if(height != 0)
                {
                    if(tileType == "Water")
                    {

                        position.z = (Mathf.Lerp(0f, 0.05f, height / noiseThreshold));

                    }
                }

                if(tileType == "Hole")
                {

                    Debug.Log("Found Hole in Pool!");

                    pooledTile.OnClick();
                    holeTile = pooledTiles[i];



                }

             
                pooledTiles[i].transform.position = position;

              //  Debug.Log("Pooled yVal: " + z);
                pooledTile.AssignCoordinate(x, (z));

                GameObject pooledObj = pooledTiles[i].gameObject;

                if(holeNum != 0 && isMoving)
                {
                    //Setting parent of moving pool to moving container.
                    pooledObj.transform.parent = movingContainer.transform;
                  //  pooledObj.transform.position = Vector3.zero;
                }

                pooledTiles.RemoveAt(i);

                activeTiles.Add(pooledObj);

                ret = true;
                return ret;
              
                

            }
        }

        return ret;
    }

    public List<Vector2> GetNeighbourListCoordinates(GameObject holeHex)
    {

        //This function is used for pathfinding, used for determining the number of tiles covered for each hit.
        //This function returns a list of neighbours that surround a specific tile.



        List<Vector2> neighbourList = new List<Vector2>();

       ITile tile = holeHex.GetComponentInChildren<ITile>();




        bool oddRow = tile.GetCoordinates().x % 2 == 1;


        //Finding directly adjacent neighbours.
        if (tile.GetCoordinates().y - 1 >= 0)
        {
            //Underneath
            Vector2 bottom = new Vector2((int)(tile.GetCoordinates().x), (int)(tile.GetCoordinates().y - 1));
            neighbourList.Add(bottom);
          

        }

        if (tile.GetCoordinates().y + 1 <= GetCourseHeight())
        {
            //Above
            Vector2 above = new Vector2((int)(tile.GetCoordinates().x), (int)(tile.GetCoordinates().y + 1));
            neighbourList.Add(above);

        }

        if (tile.GetCoordinates().x - 1 >= 0)
        {
            //Left
            Vector2 left = new Vector2((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y));
            neighbourList.Add(left);

        }

        if (tile.GetCoordinates().x + 1 < GetCourseWidth())
        {
            //Right
            Vector2 right = new Vector2((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y));
            neighbourList.Add(right);

        }

        if (oddRow)
        {

            if (tile.GetCoordinates().y + 1 < GetCourseHeight() && tile.GetCoordinates().x + 1 < GetCourseWidth())
            {
                //Diagonal Top Right
                Vector2 topRight = new Vector2((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y + 1));
                neighbourList.Add(topRight);

            }

            if (tile.GetCoordinates().y + 1 < GetCourseHeight() && tile.GetCoordinates().x - 1 >= 0)
            {
                //Diagonal Top Left
                Vector2 topLeft = new Vector2((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y + 1));
                neighbourList.Add(topLeft);

            }
        }
        else
        {

            if (tile.GetCoordinates().y - 1 >= 0 && tile.GetCoordinates().x - 1 >= 0)
            {
                //Diagonal Bot Left
                Vector2 botLeft = new Vector2((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y - 1));
                neighbourList.Add(botLeft);

              

            }

            if (tile.GetCoordinates().y - 1 >= 0 && tile.GetCoordinates().x + 1 < GetCourseWidth())
            {
                //Diagonal bot right

                Vector2 botRight = new Vector2((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y - 1));
                neighbourList.Add(botRight);

       

            }



        }


        return neighbourList;
    }



    public int GetCourseWidth()
    {

        return courseWidth;

    }

    public int GetCourseHeight()
    {

        return mapHeight;

    }


    public GameObject RetreiveBallObj()
    {

        return ball;

    }

    public GameObject RetreiveCurrentBallTile()
    {

        return currentBallTile;

    }

    public List<GameObject> GetActiveTiles()
    {
        return activeTiles;
    }

    private void Flip(GameObject tile)
    {
        //Could add flip animation here, tanks frames currently though (not sure why).


      //  tile.transform.DOPunchRotation(new Vector3 (0,35,0),.8f,9);
       // tile.transform.DOPunchPosition(new Vector3 (0,0,-.2f),0.8f,9);

        //tile.transform.D

    }


    
    private void Celebrate(GameObject tile)
    {
        // Celebration animation for completing hole. May have to add object pooling for sprites.

        Debug.Log("Celebrate! " + celebrationSprites.Count());

        int numOfSparkles = Random.Range(3, 6);

        for(int i = 1; i <= numOfSparkles; i++)
        {
            int sparkle = Random.Range(0, 5);

            GameObject sparkleSprite = Instantiate(celebrationSprites[sparkle]);

           // sparkleSprite.transform.parent = tile.transform;

            sparkleSprite.transform.position = tile.transform.position + new Vector3 (0,-.5f,-1f);

            float randTranslateX = Random.Range(0f, .4f);

            if (i % 2 == 1)
            {
                randTranslateX = -randTranslateX;
            }
            

            Vector3 targetPosition = sparkleSprite.transform.position + new Vector3( randTranslateX,0.8f,0); 

            sparkleSprite.transform.DOMove(targetPosition,.4f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => {

                    Destroy(sparkleSprite);
            
            
              });




        }



    }


    private void GenerateClouds()
    {





    }




}
