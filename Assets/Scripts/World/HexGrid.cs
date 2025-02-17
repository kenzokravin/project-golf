using UnityEngine;

public class HexGrid : MonoBehaviour
{

    public int mapWidth = 10;
    public int mapHeight = 10;
    public float tileSize = 1;
    public GameObject fairwayPrefab;
    public GameObject outTilePrefab;
    public GameObject sandPrefab;
    public GameObject roughPrefab;
    public GameObject greenPrefab;

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

        SpriteRenderer renderer = fairwayPrefab.GetComponentInChildren<SpriteRenderer>();

        
            Vector3 size = renderer.bounds.size;
            float tileWidth = size.x;
            float tileHeight = size.y;
            tileSize = tileHeight;

            Vector2 camBounds = GetCameraBounds();

        

            mapWidth = Mathf.FloorToInt((camBounds.x) / (tileWidth * 0.75f)) + 2;
            mapHeight = Mathf.FloorToInt(camBounds.y / (tileHeight * 0.75f));

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


/*
        Debug.Log("TileNumberHorizontal: " + tileHorizontalNum);

        Debug.Log("Starting X position: " + startX);


        Debug.Log(camBounds.x);
        Debug.Log("mapW: " + mapWidth);*/


        //Debug.Log(tileHeight);



        Vector3 startPosition = new Vector3(startX, -camBounds.y * .5f, 0);

        for (int x = 0; x < mapWidth; x++)
        {


            for(int z = 0; z < mapHeight; z++)
            {



                Vector3 hexCoords = GetHexCoords(x, z) + startPosition;





                if(hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {
                    //This is setting the width tiles to the water tiles.

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, 0);

                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.identity);
                    tile.transform.parent = transform;



                } else {


                    if(hexCoords.y > (camBounds.y * 0.5f) - (4*tileHeight))
                    {

                        Debug.Log("ABOVE Line");

                    }
                   // Debug.Log("Conditions for Green: " +( (camBounds.y * 0.5f) - (4 * tileHeight)));


                    if (noiseSeed == -1)
                    {
                        noiseSeed = Random.Range(0, 100000);
                    }



                    float waterValue = Mathf.PerlinNoise((hexCoords.x * noiseSeed)/noiseFrequency,(hexCoords.y * noiseSeed) / noiseFrequency);

                 //   Debug.Log("water Val: " + waterValue);
                 //   Debug.Log("xPerlin: " + hexCoords.x / noiseFrequency + " . Yperlin: " + hexCoords.y / noiseFrequency);

                    bool isWater = waterValue < noiseThreshold;
                    if (isWater)
                    {

                        Vector3 positionWater = new Vector3(hexCoords.x, hexCoords.y, 0);

                        GameObject tileWater = Instantiate(outTilePrefab, positionWater, Quaternion.identity);
                        tileWater.transform.parent = transform;


                        continue;

                    } else
                    {


                        InitLandTile(hexCoords.x, hexCoords.y);


                  

                    }
                    




                }
            }

        }

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

    private void InitLandTile(float xCoord, float yCoord)
    {

        if (landNoiseSeed == -1)
        {
            landNoiseSeed = Random.Range(0, 100000);
        }

        GameObject landtile;

        float landValue = Mathf.PerlinNoise((xCoord * landNoiseSeed) / noiseFrequency, (yCoord * landNoiseSeed) / noiseFrequency);

        if(landValue < 0.2)
        {

            landtile = sandPrefab;

        } else if (landValue >= 0.2 && landValue < 0.7)
        {
            landtile = fairwayPrefab;


        } else
        {

            landtile = roughPrefab;

        }


        Vector3 position = new Vector3(xCoord, yCoord, 0);

        GameObject tile = Instantiate(landtile, position, Quaternion.identity);
        tile.transform.parent = transform;



    }



}
