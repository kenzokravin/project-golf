using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{

    [SerializeField] public int mapWidth = 10;
    [SerializeField] public int mapHeight = 10;
    public float tileSize = 1;
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

    [SerializeField] int holeNum;
    [SerializeField] private bool isInitializing;
    [SerializeField] private int poolCounter;

    [SerializeField] GameObject holeTile;
    [SerializeField] private int courseWidth;
    [SerializeField] private Vector3 startTilePosition;


    public Camera mainCam;

    //Noise Settings
    [SerializeField] private float noiseFrequency = 100f;
    [SerializeField] private float noiseThreshold = .5f;
    [SerializeField] private float noiseSeed = 1234567;
    [SerializeField] private float landNoiseSeed = 1234567;

    [SerializeField] private List<GameObject> activeTiles;
    [SerializeField] private List<GameObject> pooledTiles;



    void Start()
    {
        holeNum = 0;
        MakeMapGrid();
        pooledTiles = new List<GameObject>();
        isInitializing = false;
        poolCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        

    }

    private void Awake()
    {

      
    }


    void MakeMapGrid()
    {
        isInitializing = true;

        if (fairwayPrefab == null)
        {
            return;
        }

        MeshRenderer renderer = fairwayPrefab.GetComponentInChildren<MeshRenderer>();

        
            Vector3 size = renderer.bounds.size;
            float tileWidth = size.x;
            float tileHeight = size.y;
            tileSize = tileHeight;

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




        Vector3 startPosition = new Vector3(startX, -camBounds.y * .5f, 0);

        startTilePosition = startPosition;


        var result = GenerateHoleCoords(tileHorizontalNum);

        Vector2 holeCoords = new Vector2(result.Item1,result.Item2);
        Debug.Log("Hole Coords: " + holeCoords);




        for (int z = 0; z < mapHeight; z++)
        {


            for (int x = 0; x < mapWidth; x++) 
            {

                //Checking the hole coords to ensure that it isn't overwritten.

                Vector3 hexCoords = GetHexCoords(x, z + (mapHeight* (holeNum != 0 ? 1 : 0))) + startPosition;


                float waterValue = Mathf.PerlinNoise((hexCoords.x * noiseSeed) / noiseFrequency, (hexCoords.y * noiseSeed) / noiseFrequency);


                if (holeCoords == new Vector2(x, z + (mapHeight * (holeNum != 0 ? 1 : 0))))
                {

                    //-------------------------------
                    //Need to add hole type script to hole.
                    if (CheckHexPoolWater(x, z, hexCoords, "Hole",waterValue))
                    {

                        continue;
                    }



                    InitHole(startPosition, result.Item1,result.Item2, waterValue);



                    Debug.Log("Matched: " + new Vector2(x, z + (mapHeight * (holeNum != 0 ? 1 : 0))));

                    continue;
                }



                if (hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));


                    if (CheckHexPoolWater(x, z, position,"Water",0))
                    {

                        continue;
                    }


                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0,0,90));
                    ITile tileScript = tile.GetComponent<ITile>();
                    tileScript.AssignCoordinate(x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));
                    tile.transform.parent = transform;

                    activeTiles.Add(tile);



                } else {


                    if(hexCoords.y > (camBounds.y * 0.5f) - (4*tileHeight))
                    {

                        //Debug.Log("ABOVE Line");

                    }
              


                    if (noiseSeed == -1)
                    {
                        noiseSeed = Random.Range(0, 10000);
                    
                    }

                    bool isWater = waterValue < noiseThreshold;
                    if (isWater)
                    {

                        Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0.02f,0.07f,waterValue/noiseThreshold)));

                        //May be issue with array size causing memory issues.

                       if(CheckHexPoolWater(x,z,positionWater,"Water",0))
                        {

                            continue;
                        }
                        


                            GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.Euler(0, 0, 90));
                            tileWater.transform.parent = transform;

                            ITile tileWaterScript = tileWater.GetComponent<ITile>();
                            tileWaterScript.AssignCoordinate(x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));

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


        GameObject hole = Instantiate(holePrefab, new Vector3(hexCoords.x,hexCoords.y,-tileHeight), Quaternion.Euler(0, 0, 90));
        holeTile = hole;
       ITile tileScript = hole.GetComponent<ITile>();
        tileScript.AssignCoordinate(x, y);
        activeTiles.Add(hole);

      

    }

    private (int,int) GenerateHoleCoords(int tileNumberHorizontal)
    {

        int x = Random.Range(5, tileNumberHorizontal - 5);
        int z = Random.Range(22, 27);

       // Vector2 holeCoords = new Vector2(x, z + (mapHeight * (holeNum > 0 ? 1 : 0)));

        
        return (x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));

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
            Debug.Log("Land Seed: " + landNoiseSeed);
        }


        string landTile = null;


        float landValue = Mathf.PerlinNoise((xCoord * landNoiseSeed) / noiseFrequency, (yCoord * landNoiseSeed) / noiseFrequency);
        float height = 0;

        if(landValue < 0.25)
        {

            landTile = "Sand";
           height = (Mathf.Lerp(0f, 0.06f, waterValue / (1-noiseThreshold)));

        } else if (landValue >= 0.25 && landValue < 0.6)
        {
            landTile = "Fairway";
            height = (Mathf.Lerp(0f, 0.06f, waterValue / (1 - noiseThreshold)));

        } else
        {

            landTile = "Rough";
            height = (Mathf.Lerp(0f, 0.06f, waterValue / (1 - noiseThreshold)));
          //  Debug.Log("height: " + height + " .WaterVal: " + waterValue + " . Land Val: " + landValue);

            

        }

        Vector3 position = new Vector3(xCoord, yCoord, -height);

        //Checking if land tile is available in object pool by checking for the tile type.
        //This could be done better by cycling through before and passing the tile into this method. (reducing number of for loop).
        
        for(int i = 0; i < pooledTiles.Count; i++)
        {
            ITile pooledTile = pooledTiles[i].GetComponent<ITile>();
            if (pooledTile.GetTileType() == landTile)
            {

                pooledTiles[i].SetActive(true);
                pooledTiles[i].transform.position = position;
                pooledTile.AssignCoordinate(x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));

                activeTiles.Add(pooledTiles[i]);

                pooledTiles.RemoveAt(i);
                return;


            }
        }


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


        GameObject tile = Instantiate(landHex, position, Quaternion.Euler(0, 0, 90));
        ITile tileScript = tile.GetComponent<ITile>();
        tileScript.AssignCoordinate(x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));

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

        ITile tileScript = currentBallTile.GetComponent<ITile>();
        tileScript.GetCoordinates();

        currentBallTile = chosenHex;
        ballController.SetHexGrid(currentBallTile);

        //If hex is the holeTile. Start End Hole Cycle.
        if (chosenHex == holeTile)
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
        MakeMapGrid();


        if (holeNum > 0)
        {
            ShiftHexPositions();
            //Move the holes

           // PoolInactiveTiles();

        }

    }


    private void ShiftHexPositions()
    {


        //Get first hex y position. Compare it to the newly regenerated hex position.
        // Move all positions down by that value (or to that position, using y value of current hole + holeheight).
        //Then we assign the new coordinate values to the new set of tiles (by subtracting the added holeheight from y, whilst keeping x).
        

        float courseHeightMove = mapHeight * tileSize;


        for (int i = 0; i < activeTiles.Count;i++)
        {

            ITile activeTile = activeTiles[i].GetComponent<ITile>();


            activeTile.AssignCoordinate((int)activeTile.GetCoordinates().x, (int)activeTile.GetCoordinates().y - mapHeight);


            activeTiles[i].transform.position = activeTiles[i].transform.position + new Vector3(0, -courseHeightMove, 0);



            poolCounter++;

            if (activeTiles[i].GetComponent<ITile>().GetCoordinates().y < 0)
            {


                Debug.Log("Object Pooled with Coords: (" + activeTile.GetCoordinates().x + "," + activeTile.GetCoordinates().y + ").");

                pooledTiles.Add(activeTiles[i]);
                activeTiles[i].SetActive(false);

                //Can't remove from active tiles as it affects the index of the list.
                activeTiles.RemoveAt(i);
                i--;
            }
            else
            {
                Debug.Log("Object NOT Pooled with Coords: (" + activeTile.GetCoordinates().x + "," + activeTile.GetCoordinates().y + ").");

            }


        }



    }

    private void PoolInactiveTiles()
    {

        //This could perhaps work better, to prevent constant for loop cycling (maybe better for performance?)
        //We could set it so each tile does it individual and then calls the pool?

        //Height limit could use the start position to set 
        //float heightLimit = -8f;

        //Right now, the issue with pooling, is that we are spawning and then pooling.
        //We are also using Coordinate system values which is good, but the order of operations could be fucked.

       

        for (int i = 0;i < activeTiles.Count;i++)
        {

            if(activeTiles[i].GetComponent<ITile>().GetCoordinates().y < 0)
            {
                //When the tile hits the height limit, then it will be pooled. (which is below the y coord of 0.)
                //Removing it from the active tiles and hiding it.

                Debug.Log("Object Pooled");





               pooledTiles.Add(activeTiles[i]);
               activeTiles[i].SetActive(false);

                activeTiles.RemoveAt(i);
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

    private bool CheckHexPoolWater(int x, int z ,Vector3 positionWater, string tileType, float height)
    {
        bool ret = false;

        for (int i = 0; i < pooledTiles.Count; i++)
        {
            ITile pooledTile = pooledTiles[i].GetComponent<ITile>();
            if (pooledTile.GetTileType() == tileType)
            {

                pooledTiles[i].SetActive(true);

                if(height != 0)
                {
                    positionWater.z = -(Mathf.Lerp(0f, 0.05f, height / noiseThreshold));
                }

             
                pooledTiles[i].transform.position = positionWater;
                pooledTile.AssignCoordinate(x, z + (mapHeight * (holeNum != 0 ? 1 : 0)));

                activeTiles.Add(pooledTiles[i]);

                pooledTiles.RemoveAt(i);

                ret = true;

            }
        }

        return ret;
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
