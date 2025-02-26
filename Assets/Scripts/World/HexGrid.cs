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

    [SerializeField] private List<GameObject> activeTiles;
    [SerializeField] private List<GameObject> pooledTiles;

    [SerializeField] private GameObject movingCont;
    public float maxHexHeight;


    void Start()
    {


        var AllMBs = GetComponents<MonoBehaviour>();
        foreach (var mb in AllMBs)
        {
            Debug.Log("HEXGRID Found a " + mb.GetType());
        }


        holeNum = 0;
        InitializeMapGrid();
        clickDetector = GetComponent<ClickDetector>();
        pooledTiles = new List<GameObject>();
        isInitializing = false;
        poolCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        

    }

    public void PoolHex(GameObject hexToPool, ITile tile)
    {

        //This will be called in tile script when they reach the bounds of the camera.

        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i] == hexToPool)
            {
                activeTiles.RemoveAt(i);

                pooledTiles.Add(hexToPool);
                hexToPool.SetActive(false);
                break;

            }
        }

        //Could call spawn next hex here?
        //This would mean for each despawned hex, a new hex is spawned.
        //  ITile tile = hexToPool.GetComponentInChildren<ITile>();

        if (holeNum != 0)
        {
            SpawnNextHex(tile);
        }

    }


    private void MoveHexes()
    {

        //Order of Operations for lazy loading of hexes (hoping to increase performance.
        //1. Cycle through and move each hex (using a tween or otherwise) down to new position.
        //2. For each hex despawned, spawn a new one above and move into position.
        //3. To despawn, add a method that checks if the tile is below the camera bounds. (this might have to be in the tile script. We can then pool the hex.)

        //Maybe to reduce impact of tweens, could place all active tiles into empty, tween empty, then remove empty (or reset it to 0,0 and then add all new active tiles?)

        //Another note: To ensure pooled Tiles are available, we could Instantiate 2 holes at start, storing the 2nd hole hexes as pooled. Then when they are required, activate them and send move them into position.



        GameObject movingContainer = Instantiate(movingCont,gameObject.transform);


        for (int i = activeTiles.Count -1; i >= 0; i--)
        {
            // activeTiles[i].transform.DOMove(new Vector3(activeTiles[i].transform.position.x, (activeTiles[i].transform.position.y - (GetCameraBounds().y)), 0), 2f);

            activeTiles[i].transform.SetParent(movingContainer.transform);

        }

        movingContainer.transform.DOMove(new Vector3(0, -(GetCameraBounds().y), 0), 2f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
                // Unparent objects
                foreach (GameObject obj in activeTiles)
              {
                  obj.transform.SetParent(gameObject.transform);
              }

                // Destroy the container
                Destroy(movingContainer);
          });



    }

    public void SpawnNextHex(ITile despawnedTile)
    {

        //This method will be called in each tile script when they are despawned.

        //float yCoord = despawnedTile.GetCoordinates().y + mapHeight;

        //Generating the hexCoords using an adjusted y position (given the coords of the despawned tile).
        Vector3 hexCoords = GetHexCoords((int)despawnedTile.GetCoordinates().x, ((int)despawnedTile.GetCoordinates().y + (holeNum != 0 ? mapHeight : 0))) + startGenPosition;


        //This will spawn the next Hex
        CheckWaterValue(hexCoords, (int)despawnedTile.GetCoordinates().x, (int)despawnedTile.GetCoordinates().y);
       

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

        //Generating waterValue to determine if water hex or not.
        float waterValue = Mathf.PerlinNoise(((hexCoords.x) * noiseSeed) / noiseFrequency, ((hexCoords.y + noiseOffset) * noiseSeed) / noiseFrequency);

        if (hexCoords.x - (tileHorizWidth * 0.75f) <= -camBounds.x * .5f || hexCoords.x + (tileHorizWidth * 0.75f) >= camBounds.x * .5f)
        {
            //This is setting the width tiles to the water tiles.

            Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));

            //  Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

            if (CheckHexPoolWater(xCoord, yCoord, position, "Water", 0))
            {

                // continue;
                return;
            }


            GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0, 0, 90));
            ITile tileScript = tile.GetComponent<ITile>();
            tileScript.AssignCoordinate(xCoord, (yCoord + (holeNum != 0 ? mapHeight : 0)));
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

                ITile tileWaterScript = tileWater.GetComponent<ITile>();
                tileWaterScript.AssignCoordinate(xCoord, (yCoord + (holeNum != 0 ? mapHeight : 0)));

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
       //     return;
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

        Debug.Log("Lowest Y: " + lowestY);

        maxHexHeight = tileSize * mapHeight;


        Vector3 startPosition = new Vector3(startX, -camBounds.y * .5f, 0);

        startTilePosition = startPosition;
        startGenPosition = startPosition;   


        var result = GenerateHoleCoords(tileHorizontalNum);

        Vector2 holeCoords = new Vector2(result.Item1,result.Item2);
        Debug.Log("Hole Coords: " + holeCoords);

        float noiseOffset = holeNum * mapHeight;



        for (int z = 0; z < mapHeight * 2; z++)
        {


            for (int x = 0; x < mapWidth; x++) 
            {

                //Checking the hole coords to ensure that it isn't overwritten.

                Vector3 hexCoords = GetHexCoords(x, (z + (holeNum != 0 ? mapHeight : 0))) + startPosition;


                if (noiseSeed == -1)
               {
                    noiseSeed = Random.Range(0, 10000);

               }

               

                //Right now there is no way to have a seeded map, to do this, we would have to translate/offset the perlin noise each time it is regen. Thus maintaining the same seed.


                float waterValue = Mathf.PerlinNoise(((hexCoords.x) * noiseSeed) / noiseFrequency, ((hexCoords.y + noiseOffset) * noiseSeed) / noiseFrequency);





                if (Mathf.Approximately(holeCoords.x, x) && Mathf.Approximately(holeCoords.y, (z + (holeNum != 0 ? mapHeight : 0))))
                {

                    Debug.Log("Hole Generated: " + x + ", " + z);

                  //  Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

                    //-------------------------------
                    //Need to add hole type script to hole.
                    if (CheckHexPoolWater(x, z, hexCoords, "Hole",waterValue))
                    {

                       continue;
                   }

                    InitHole(startPosition, result.Item1,result.Item2, waterValue);


                 //   Debug.Log("Matched: " + new Vector2(x, (z + (holeNum != 0 ? mapHeight : 0))));

                    continue;
                }



                if (hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));

                  //  Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

                    if (CheckHexPoolWater(x, z, position,"Water",0))
                    {

                       continue;
                    }


                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0,0,90));
                    ITile tileScript = tile.GetComponent<ITile>();
                    tileScript.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));
                   // tileScript.SetUpperBounds(maxHexHeight);
                    tile.transform.parent = transform;

                    ITile tileChildScript = tile.GetComponentInChildren<ITile>();
                    tileChildScript.SetUpperBounds(maxHexHeight);

                    activeTiles.Add(tile);



                } else {


                    if(hexCoords.y > (camBounds.y * 0.5f) - (4*tileHeight))
                    {

                        //Debug.Log("ABOVE Line");

                    }
              


                

                    bool isWater = waterValue < noiseThreshold;

                    if (isWater)
                    {

                        Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0.02f,0.07f,waterValue/noiseThreshold)));

                        //May be issue with array size causing memory issues.

                        //Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");

                        if (CheckHexPoolWater(x,z,positionWater,"Water",0))
                        {

                            continue;
                        }
                        


                            GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.Euler(0, 0, 90));
                            tileWater.transform.parent = transform;

                            ITile tileWaterScript = tileWater.GetComponent<ITile>();
                            tileWaterScript.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));
                         tileWaterScript.SetUpperBounds(maxHexHeight);

                        ITile tileChildScript = tileWater.GetComponentInChildren<ITile>();
                        tileChildScript.SetUpperBounds(maxHexHeight);


                        activeTiles.Add(tileWater);

                            continue;

                        

                    } else
                    {

                    //    Debug.Log($"Current Z: {z}, Adjusted Z: {z + (holeNum != 0 ? mapHeight : 0)}");
                        InitLandTile(hexCoords.x, hexCoords.y, waterValue, x, z);

                    }
                   
                }
            }

        }


        //If the hole is not the start hole then do not spawn the ball
        if(holeNum == 0 ? true : false) BallSpawn();


        isInitializing = false;

       // poolCounter++;



    }

    private void InitHole(Vector3 startPosition, int x,int y, float height)
    {


        //Generate a Y Value between 23-28 (The green zone) and randomly select an x coord.
        //Generate Tile based on this coord and the start position.
        //Store coord values and skip generate when the generation loop encounters the hole value.

        //Will have to find a range that is x tiles
        float tileHeight = (Mathf.Lerp(0f, 0.06f, height / (1 - noiseThreshold)));

        Vector3 hexCoords = GetHexCoords(x, y) + startPosition;


        GameObject hole = Instantiate(holePrefab, new Vector3(hexCoords.x,hexCoords.y,-tileHeight), Quaternion.Euler(-90, 0, 0));
        holeTile = hole;
       ITile tileScript = hole.GetComponent<ITile>();
        //tileScript.SetUpperBounds(maxHexHeight);
        tileScript.AssignCoordinate(x, y);

        ITile tileChildScript = hole.GetComponentInChildren<ITile>();
        tileChildScript.SetUpperBounds(maxHexHeight);


        activeTiles.Add(hole);

      

    }

    private (int,int) GenerateHoleCoords(int tileNumberHorizontal)
    {

        int x = Random.Range(5, tileNumberHorizontal - 5);
        int z = Random.Range(22, 27);

       // Vector2 holeCoords = new Vector2(x, z + (mapHeight * (holeNum > 0 ? 1 : 0)));

        
        return (x, z + (holeNum != 0 ? mapHeight : 0));

    }



    private Vector3 GetHexCoords(int x, int z)
    {
        float xPos = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        float zPos = z * tileSize + ((x % 2 == 1) ? tileSize * .5f : 0);

        return new Vector3(xPos, zPos,0);

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
            Debug.Log("Pooled Land ObJ spawned, with a noise value: " + landValue);
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
        tileScript.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));

        foreach (Transform child in tile.transform.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Hex"))
            {
                Debug.Log(child);
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
                ITile tileScript = activeTiles[i].GetComponent<ITile>();

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
        holeNum++;

        //Making next hole level.
        //Maybe not use hole number to scale y? If it goes to hole two, it will use the double multiply



     //   MakeMapGrid();


        if (holeNum > 0)
        {
          //  ShiftHexPositions();
          MoveHexes();
            //Move the holes

        }

    }


    private void ShiftHexPositions()
    {


        //Get first hex y position. Compare it to the newly regenerated hex position.
        // Move all positions down by that value (or to that position, using y value of current hole + holeheight).
        //Then we assign the new coordinate values to the new set of tiles (by subtracting the added holeheight from y, whilst keeping x).
        

        float courseHeightMove = mapHeight * tileSize;


        for (int i = activeTiles.Count - 1; i >=0;i--)
        {

            ITile activeTile = activeTiles[i].GetComponent<ITile>();


            activeTile.AssignCoordinate((int)activeTile.GetCoordinates().x, (int)activeTile.GetCoordinates().y - mapHeight);


            activeTiles[i].transform.position = activeTiles[i].transform.position + new Vector3(0, -courseHeightMove, 0);



         //   poolCounter++;

            if (activeTiles[i].GetComponent<ITile>().GetCoordinates().y < 0)
            {


              //  Debug.Log("Object Pooled with Coords: (" + activeTile.GetCoordinates().x + "," + activeTile.GetCoordinates().y + ").");

                GameObject tileToPool = activeTiles[i];
                activeTiles.RemoveAt(i);

                pooledTiles.Add(tileToPool);
                tileToPool.SetActive(false);
               
               
            }

        }



    }


    public GameObject GetHex(int x, int y)
    {

        GameObject retHex = null;

        for(int i = 0;i < activeTiles.Count; i++)
        {

            ITile tile = activeTiles[i].GetComponent<ITile>();

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
            ITile pooledTile = pooledTiles[i].GetComponent<ITile>();
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
                    pooledTile.OnClick();
                    holeTile = pooledTiles[i];



                }

             
                pooledTiles[i].transform.position = position;
                pooledTile.AssignCoordinate(x, (z + (holeNum != 0 ? mapHeight : 0)));

                GameObject pooledObj = pooledTiles[i].gameObject;

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

       ITile tile = holeHex.GetComponent<ITile>();




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




}
