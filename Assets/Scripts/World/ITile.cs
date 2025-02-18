using UnityEngine;

public interface ITile
{
    string GetTileType();

    void AssignCoordinate(float x, float y);

    void OnClick();

    void OnConfirm();

    Vector2 GetCoordinates();

    Vector3 GetPosition();
    
}
