using UnityEngine;

public interface ITile
{
    string GetTileType();

    void AssignCoordinate(float x, float y);

    void OnClick();

    Vector2 GetCoordinates();
    
}
