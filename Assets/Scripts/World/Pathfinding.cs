using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{

    [SerializeField] GameObject currentBallTile;
    [SerializeField] GameObject ball;
    BallController ballController;
    private List<ITile> openList;
    private List<ITile> closedList;

    private HexGrid hexGrid;

    private const int MOVE_STRAIGHT_COST = 10;


    private void Start()
    {
        ballController = GetComponent<BallController>();
        hexGrid = GetComponent<HexGrid>();

    }





    public List<ITile> GetNeighbourList(ITile currentHex)
    {

        //This function is used for pathfinding, used for determining the number of tiles covered for each hit.
        //This function returns a list of neighbours that surround a specific tile.



        List<ITile> neighbourList = new List<ITile>();

        ITile tile = currentHex;




        bool oddRow = tile.GetCoordinates().x % 2 == 1;


        //Finding directly adjacent neighbours.
        if (tile.GetCoordinates().y - 1 >= 0)
        {
            //Underneath
            neighbourList.Add(hexGrid.GetHex((int)(tile.GetCoordinates().x), (int)(tile.GetCoordinates().y - 1)).GetComponent<ITile>());

        }

        if (tile.GetCoordinates().y + 1 <= hexGrid.GetCourseHeight())
        {
            //Above
            neighbourList.Add(hexGrid.GetHex((int)(tile.GetCoordinates().x), (int)(tile.GetCoordinates().y + 1)).GetComponent<ITile>());

        }

        if (tile.GetCoordinates().x - 1 >= 0)
        {
            //Left
            neighbourList.Add(hexGrid.GetHex((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y)).GetComponent<ITile>());

        }

        if (tile.GetCoordinates().x + 1 < hexGrid.GetCourseWidth())
        {
            //Right
            neighbourList.Add(hexGrid.GetHex((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y)).GetComponent<ITile>());

        }

        if (oddRow)
        {

            if(tile.GetCoordinates().y + 1 < hexGrid.GetCourseHeight() && tile.GetCoordinates().x + 1 < hexGrid.GetCourseWidth())
            {
                //Diagonal Top Right
                neighbourList.Add((hexGrid.GetHex((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y + 1))).GetComponent<ITile>());

            }

            if (tile.GetCoordinates().y + 1 < hexGrid.GetCourseHeight() && tile.GetCoordinates().x - 1 >= 0)
            {
                //Diagonal Top Left
                neighbourList.Add((hexGrid.GetHex((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y + 1))).GetComponent<ITile>());

            }
        } else
        {

            if (tile.GetCoordinates().y - 1 >= 0 && tile.GetCoordinates().x - 1 >= 0)
            {
                //Diagonal Bot Left
                neighbourList.Add((hexGrid.GetHex((int)(tile.GetCoordinates().x - 1), (int)(tile.GetCoordinates().y - 1))).GetComponent<ITile>());

            }

            if (tile.GetCoordinates().y - 1 >= 0 && tile.GetCoordinates().x + 1 < hexGrid.GetCourseWidth())
            {
                //Diagonal bot right
                neighbourList.Add((hexGrid.GetHex((int)(tile.GetCoordinates().x + 1), (int)(tile.GetCoordinates().y - 1))).GetComponent<ITile>());

            }



        }


        return neighbourList;
    }


    public List<ITile> FindPath(int startX, int startY, int endX, int endY)
    {

        
        ITile startTile = hexGrid.GetHex(startX, startY).GetComponent<ITile>();
        ITile endTile = hexGrid.GetHex(endX, endY).GetComponent<ITile>();


        openList = new List<ITile>() { startTile };
        //may have to add start node here (into openList).

        closedList = new List<ITile>();

        List<GameObject> tilesInGrid = hexGrid.GetActiveTiles();

        for(int x = 0; x < hexGrid.GetCourseWidth(); x++)
        {
            for(int y = 0; y < hexGrid.GetCourseHeight(); y++)
            {
              //  Debug.Log("Finding Hex at x: " + x + ", y: " + y);

                ITile hex = hexGrid.GetHex(x, y).GetComponent<ITile>();

                hex.SetGCost(int.MaxValue);
                hex.CalculateFCost();
                hex.SetCameFromTile(null);


               




            }
        }

        startTile.SetGCost(0);
        startTile.SetHCost(CalculateDistanceCost(startTile, endTile));
        startTile.CalculateFCost();

        while(openList.Count > 0)
        {
            ITile currentTile = GetLowestFCostHex(openList);

            if(currentTile == endTile)
            {
                return CalculatePath(endTile);

            }


            openList.Remove(currentTile);
            closedList.Add(currentTile);


            foreach (ITile tile in GetNeighbourList(currentTile))
            {


                if (closedList.Contains(tile)) continue;

                int tentativeGCost = currentTile.GetGCost() + CalculateDistanceCost(currentTile,tile);

                if(tentativeGCost < tile.GetGCost())
                {

                    tile.SetCameFromTile(currentTile);
                    tile.SetGCost(tentativeGCost);
                    tile.SetHCost(CalculateDistanceCost(tile, endTile));
                    tile.CalculateFCost();

                    if(!openList.Contains(tile))
                    {

                        openList.Add(tile);


                    }
                }
            }
        }

        //Out of Nodes on openList

        return null;


    }


    private List<ITile> CalculatePath(ITile endTile)
    {
       List<ITile> path = new List<ITile>();

        path.Add(endTile);

        ITile currentTile = endTile;

        while (currentTile.GetCameFromTile() != null)
        {

            path.Add(currentTile.GetCameFromTile());
            currentTile = currentTile.GetCameFromTile();    


        }

        path.Reverse();
        return path;

    }



    private int CalculateDistanceCost(ITile a, ITile b)
    {

        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(a.GetPosition(), b.GetPosition()));

      /*  int xDistance = Mathf.Abs((int)(a.GetCoordinates().x) - (int)(b.GetCoordinates().x));
        int yDistance = Mathf.Abs((int)(a.GetCoordinates().y) - (int)(b.GetCoordinates().y));

        int remaining = Mathf.Abs(xDistance - yDistance);
*/
    }

    private ITile GetLowestFCostHex(List<ITile> pathHexList)
    {

        ITile lowestFCostHex = pathHexList[0];

        for (int i = 1; i < pathHexList.Count; i++)
        {
            if(pathHexList[i].GetFCost() < lowestFCostHex.GetFCost())
            {
                lowestFCostHex = pathHexList[i];
            }


        }

        return lowestFCostHex;

    }

    public int GetPathLength(List<ITile> path)
    {
        int pathLength = 0;

        for (int i=0; i < path.Count; i++)
        {

            pathLength++;



        }

        return pathLength - 1;


    }



}
