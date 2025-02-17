using UnityEngine;

public class HexGrid : MonoBehaviour
{

    public int mapWidth = 10;
    public int mapHeight = 10;
    public float tileSize = 1;
    public GameObject tilePrefab;
    public GameObject outTilePrefab;

    public Camera mainCam;
    
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

        int width = 10;
        int height = 6;
        float cellsize = 1f;



    }


    void MakeMapGrid()
    {

        if (tilePrefab == null)
        {
            return;
        }

        SpriteRenderer renderer = tilePrefab.GetComponentInChildren<SpriteRenderer>();

        
            Vector3 size = renderer.bounds.size;
            float tileWidth = size.x;
            float tileHeight = size.y;
            tileSize = tileHeight;

            Vector2 camBounds = GetCameraBounds();

        

            mapWidth = Mathf.FloorToInt((camBounds.x) / (tileWidth * 0.75f));
            mapHeight = Mathf.FloorToInt(camBounds.y / (tileHeight * 0.75f));

        float widthCutoffCheck = 0;
        int tileHorizontalNum = 0;


        float xStartPositionCoord = 0 * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        float xEndPositionCoord = 0;


        for (int x = 0; x < mapWidth; x++)
        {
            float xPos = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);

            widthCutoffCheck = xPos;
            tileHorizontalNum++;
            xEndPositionCoord = x * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        }


        float xTotalWidth = xEndPositionCoord - xStartPositionCoord;

        xTotalWidth = xTotalWidth / 2;

        // Calculate the starting X position (shift grid right to center)
        float startX = -xTotalWidth ; // Shifting right for even spacing

        // Shift left to center the grid
        // startX = -((gridWidth - tileWidth) * 0.5f);

        Debug.Log("TileNumberHorizontal: " + tileHorizontalNum);

        Debug.Log("Starting X position: " + startX);


        Debug.Log(camBounds.x);
        Debug.Log("mapW: " + mapWidth);


        //Debug.Log(tileHeight);



        Vector3 startPosition = new Vector3(startX, -camBounds.y * .5f, 0);

        for (int x = 0; x < mapWidth; x++)
        {


            for(int z = 0; z < mapHeight; z++)
            {



                Vector3 hexCoords = GetHexCoords(x, z) + startPosition;

                if(hexCoords.x - (tileWidth *0.75f) <= -camBounds.x *.5f || hexCoords.x + (tileWidth * 0.75f) >= camBounds.x * .5f)
                {

                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, 0);

                    GameObject tile = Instantiate(outTilePrefab, position, Quaternion.identity);
                    tile.transform.parent = transform;



                } else {


                    Vector3 position = new Vector3(hexCoords.x, hexCoords.y, 0);

                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                    tile.transform.parent = transform;




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



}
