using UnityEngine;

public class WaterHex : MonoBehaviour, ITile
{
    [SerializeField] private string tileType;
    [SerializeField] private Vector2 coordinates;


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

    }


    public Vector2 GetCoordinates()
    {
        return coordinates;

    }
}
