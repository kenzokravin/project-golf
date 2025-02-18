using UnityEngine;

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

    public Camera mainCam;

    //Noise Settings
    [SerializeField] private float noiseFrequency = 100f;
    [SerializeField] private float noiseThreshold = .5f;
    [SerializeField] private float noiseSeed = 1234567;
    [SerializeField] private float landNoiseSeed = 1234567;



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


        Vector2 holeCoords = InitHole(startPosition, tileHorizontalNum);
        Debug.Log("Hole Coords: " + holeCoords);


        for (int x = 0; x < mapWidth; x++)
        {


            for(int z = 0; z < mapHeight; z++)
            {

                //Checking the hole coords to ensure that it isn't overwritten.

                if(holeCoords == new Vector2(x,z))
                {

                    Debug.Log("Matched: " + new Vector2(x, z));

                    continue;
                }



                Vector3 hexCoords = GetHexCoords(x, z) + startPosition;


                float waterValue = Mathf.PerlinNoise((hexCoords.x * noiseSeed) / noiseFrequency, (hexCoords.y * noiseSeed) / noiseFrequency);


                if (hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, (Mathf.Lerp(0f, 0.05f, waterValue / noiseThreshold)));

                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.Euler(0,0,90));
                    ITile tileScript = tile.GetComponent<ITile>();
                    tileScript.AssignCoordinate(x, z);
                    tile.transform.parent = transform;



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


                        continue;

                    } else
                    {


                        InitLandTile(hexCoords.x, hexCoords.y, waterValue, x, z);


                  

                    }
                   
                }
            }

        }

        






    }

    private Vector2 InitHole(Vector3 startPosition, int tileNumberHorizontal)
    {


        //Generate a Y Value between 23-28 (The green zone) and randomly select an x coord.
        //Generate Tile based on this coord and the start position.
        //Store coord values and skip generate when the generation loop encounters the hole value.

        //Will have to find a range that is x tiles

        int x = Random.Range(5, tileNumberHorizontal-5);
        int z = Random.Range(22, 27);

        Vector2 holeCoords = new Vector2(x, z);
        Vector3 hexCoords = GetHexCoords(x, z) + startPosition;


        GameObject holeTile = Instantiate(holePrefab, new Vector3(hexCoords.x,hexCoords.y,0), Quaternion.Euler(0, 0, 90));
       // ITile tileScript = holeTile.GetComponent<ITile>();
      //  tileScript.AssignCoordinate(x, z);

        return holeCoords;

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

        } else if (landValue >= 0.25 && landValue < 0.5)
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
        tile.transform.parent = transform;



    }



}
