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



    void Start()
    {

        MakeMapGrid();
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


        for (int x = 0; x < mapWidth; x++)
        {


            for(int z = 0; z < mapHeight; z++)
            {

                //Checking the hole coords to ensure that it isn't overwritten.

               



                Vector3 hexCoords = GetHexCoords(x, z) + startPosition;


                float waterValue = Mathf.PerlinNoise((hexCoords.x * noiseSeed) / noiseFrequency, (hexCoords.y * noiseSeed) / noiseFrequency);


                if (holeCoords == new Vector2(x, z))
                {

                    InitHole(startPosition, result.Item1,result.Item2, waterValue);



                    Debug.Log("Matched: " + new Vector2(x, z));

                    continue;
                }



                if (hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));

                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0,0,90));
                    ITile tileScript = tile.GetComponent<ITile>();
                    tileScript.AssignCoordinate(x, z);
                    tile.transform.parent = transform;

                    activeTiles.Add(tile);



                } else {


                    if(hexCoords.y > (camBounds.y * 0.5f) - (4*tileHeight))
                    {

                        //Debug.Log("ABOVE Line");

                    }
                   // Debug.Log("Conditions for Green: " +( (camBounds.y * 0.5f) - (4 * tileHeight)));


                    if (noiseSeed == -1)
                    {
                        noiseSeed = Random.Range(0, 10000);
                        //Debug.Log("Land Seed: " + noiseSeed);
                    }




                 //   Debug.Log("water Val: " + waterValue);
                 //   Debug.Log("xPerlin: " + hexCoords.x / noiseFrequency + " . Yperlin: " + hexCoords.y / noiseFrequency);

                    bool isWater = waterValue < noiseThreshold;
                    if (isWater)
                    {

                        Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f,0.05f,waterValue/noiseThreshold)));

                        GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.Euler(0, 0, 90));
                        tileWater.transform.parent = transform;
                        activeTiles.Add(tileWater);

                        continue;

                    } else
                    {


                        InitLandTile(hexCoords.x, hexCoords.y, waterValue, x, z);


                  

                    }
                   
                }
            }

        }


        BallSpawn();





    }

    private void InitHole(Vector3 startPosition, int x,int y, float height)
    {


        //Generate a Y Value between 23-28 (The green zone) and randomly select an x coord.
        //Generate Tile based on this coord and the start position.
        //Store coord values and skip generate when the generation loop encounters the hole value.

        //Will have to find a range that is x tiles
        float tileHeight = (Mathf.Lerp(0f, 0.06f, height / (1 - noiseThreshold)));

        Vector3 hexCoords = GetHexCoords(x, y) + startPosition;


        GameObject holeTile = Instantiate(holePrefab, new Vector3(hexCoords.x,hexCoords.y,-tileHeight), Quaternion.Euler(0, 0, 90));
       // ITile tileScript = holeTile.GetComponent<ITile>();
      //  tileScript.AssignCoordinate(x, z);

      

    }

    private (int,int) GenerateHoleCoords(int tileNumberHorizontal)
    {

        int x = Random.Range(5, tileNumberHorizontal - 5);
        int z = Random.Range(22, 27);

        Vector2 holeCoords = new Vector2(x, z);

        
        return (x, z);

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

        GameObject landtile;

        float landValue = Mathf.PerlinNoise((xCoord * landNoiseSeed) / noiseFrequency, (yCoord * landNoiseSeed) / noiseFrequency);
        float height = 0;

        if(landValue < 0.25)
        {

            landtile = sandPrefab;
           height = (Mathf.Lerp(0f, 0.06f, waterValue / (1-noiseThreshold)));

        } else if (landValue >= 0.25 && landValue < 0.6)
        {
            landtile = fairwayPrefab;
           height = (Mathf.Lerp(0f, 0.06f, waterValue / (1 - noiseThreshold)));

        } else
        {

            landtile = roughPrefab;
           height = (Mathf.Lerp(0f, 0.06f, waterValue / (1 - noiseThreshold)));
          //  Debug.Log("height: " + height + " .WaterVal: " + waterValue + " . Land Val: " + landValue);

            

        }


        Vector3 position = new Vector3(xCoord, yCoord, -height);

        GameObject tile = Instantiate(landtile, position, Quaternion.Euler(0, 0, 90));
        ITile tileScript = tile.GetComponent<ITile>();
        tileScript.AssignCoordinate(x, z);

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

                   ballSpawnLoop = false;

                    


                }

            }

        }
 

    }


    public void SwapActiveHex(GameObject chosenHex)
    {

       
       
        //Here would be where we could use the interface to check if the chosen hex is of a certain type (or we refer to the ball).
        //This could then trigger different states of the game/ball (i.e. stage is over, ball bounces, ball rolls etc)

        ballController.Jump(currentBallTile,chosenHex);


        currentBallTile = chosenHex;

    }









}
