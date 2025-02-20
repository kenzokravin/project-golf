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



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
            Debug.Log(gameObject.name + " was clicked!");
            // Perform any action like changing color
            GetComponent<Renderer>().material.color = Color.red;


        }
        else
        {

            Debug.Log(gameObject.name + " was un-clicked!");
            // Perform any action like changing color
            GetComponent<Renderer>().material.color = Color.yellow;

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
            GetComponent<Renderer>().material.color = Color.red;


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
