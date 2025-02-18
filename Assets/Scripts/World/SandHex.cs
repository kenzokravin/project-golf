using UnityEngine;

public class SandHex : MonoBehaviour, ITile
{
    [SerializeField] private string tileType;
    [SerializeField] private Vector2 coordinates;
    [SerializeField] private bool selected = false;


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

        if(selected)
        {
            Debug.Log(gameObject.name + " was clicked!");
            // Perform any action like changing color
            GetComponent<Renderer>().material.color = Color.red;
        } else
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



}
