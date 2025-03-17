using UnityEngine;

public class FairwayHex : MonoBehaviour, ITile
{
    [SerializeField] private string tileType;
    [SerializeField] private Vector2 coordinates;
    [SerializeField] private bool selected = false;

    [SerializeField] private int gCost;
    [SerializeField] private int fCost;
    [SerializeField] private int hCost;

    [SerializeField] public ITile cameFromTile;

    public Camera cam;
    private HexGrid grid;
    public float lowerBounds;
    public float upperBounds;
    Vector2 camBounds;

    private bool mapShifting;


    void Start()
    {
        mapShifting = false;

        grid = GameObject.FindGameObjectWithTag("MapBuilder").GetComponent<HexGrid>();
        cam = GameObject.FindGameObjectWithTag("CameraWorldBuilder").GetComponent<Camera>();

        Vector2 camBounds = GetCameraBounds();

       // Debug.Log("Cam Bounds for pool despawn: " + -(camBounds.y * .5f));
        lowerBounds = -(camBounds.y * .5f) - 0.01f;
       // upperBounds = (camBounds.y * .5f);
    }

    // Update is called once per frame
    void Update()
    {
       CheckDespawn();
    }

    public void SetUpperBounds(float height)
    {
        upperBounds = height;
    }

    private void CheckDespawn()
    {

        //Need to set lower limit of world position (could be done by setting the lower limit value when instantiated) (this would make it dependent on the user screen and also would mean that hexes aren't deactivated (which would lose their coords)).
        //For moving the hole down though, this might be difficult. As it won't have a full shift of all hexes by a set amount. This is because of different par holes.
        //Or would it? Not sure. This might not affect it because the limit is dependent on world position, not coords. World limit has to be less than the lowest coord (where y=0 for hex coord) 


        if (gameObject.transform.parent.gameObject.transform.position.y < lowerBounds || gameObject.transform.parent.gameObject.transform.position.y > upperBounds)
        {
          //     Debug.Log("upperBounds: " + upperBounds + " with a y of: " + gameObject.transform.parent.gameObject.transform.position.y);

            grid.PoolHex(gameObject.transform.parent.gameObject, this);
        }
        //  grid.PoolHex(gameObject.transform.parent.gameObject,this);


    }


    private Vector2 GetCameraBounds()
    {
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        return new Vector2(camWidth, camHeight);


    }

    public string GetTileType()
    {
        return tileType;

    }

    public void AssignCoordinate(float x, float y)
    {
        coordinates = new Vector2(x, y);

    }

    public void OnClick()
    {
        selected = !selected;

        if (selected)
        {
         //   Debug.Log(gameObject.name + " was clicked!");
            // Perform any action like changing color
           // GetComponent<Renderer>().material.color = Color.red;


        }
        else
        {

           // Debug.Log(gameObject.name + " was un-clicked!");
            // Perform any action like changing color
           // GetComponent<Renderer>().material.color = Color.yellow;

        }

    }


    public Vector2 GetCoordinates()
    {
        return coordinates;

    }


    public Vector3 GetPosition()
    {

        return gameObject.transform.position;

    }

    public void OnConfirm()
    {

        if (selected)
        {

            Debug.Log(gameObject.name + " was clicked!");
            // Perform any action like changing color
        //    GetComponent<Renderer>().material.color = Color.red;


        }



    }


    public void SetGCost(int cost)
    {
        gCost = cost;


    }

    public void SetFCost(int cost)
    {

        fCost = cost;

    }


    public void SetHCost(int cost)
    {

        hCost = cost;

    }

    public int GetGCost()
    {
        return gCost;


    }

    public int GetFCost()
    {
        return fCost;


    }

    public int GetHCost()
    {
        return hCost;


    }



    public void CalculateFCost()
    {

        fCost = gCost + hCost;


    }

    public void SetCameFromTile(ITile hex)
    {

        cameFromTile = hex;


    }

    public ITile GetCameFromTile()
    {

        return cameFromTile;


    }



}
