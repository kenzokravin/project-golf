using UnityEngine;

public interface ITile
{
    string GetTileType();

    void AssignCoordinate(float x, float y);

    void OnClick();

    void OnConfirm();

    Vector2 GetCoordinates();

    Vector3 GetPosition();

    void SetGCost(int gCost);

    int GetGCost();

    void SetFCost(int fCost);
    int GetFCost();

    void SetHCost(int fCost);

    void CalculateFCost();

    void SetCameFromTile(ITile hex);

    ITile GetCameFromTile();

}
