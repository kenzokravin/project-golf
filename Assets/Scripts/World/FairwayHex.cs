using UnityEngine;

public class FairwayHex : MonoBehaviour, ITile
{
    [SerializeField] private string tileType;


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

}
