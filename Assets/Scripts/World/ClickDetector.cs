using UnityEngine;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private GameObject clickedTile;
    [SerializeField] private bool selected = false;
    [SerializeField] private BallController ball;

    private Pathfinding pathfinder;
    private HexGrid hexGrid;

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
        ball = hexGrid.RetreiveBallObj().GetComponent<BallController>();
        pathfinder = GetComponent<Pathfinding>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
               // Debug.Log("Clicked on: " + hit.collider.gameObject.name);

                // If the object has a specific script
                ITile clickable = hit.collider.gameObject.GetComponent<ITile>();
                if (clickable != null)
                {
                    if(selected)
                    {

                        if(hit.collider.gameObject == clickedTile)
                        {

                           
                            
                            ITile confirmTile = clickedTile.GetComponent<ITile>();

                            confirmTile.OnConfirm();

                            ball.Jump(hexGrid.RetreiveCurrentBallTile(), clickedTile);

                            hexGrid.SwapActiveHex(hit.collider.gameObject);

                            selected = false;

                        }
                        else
                        {

                            ITile previousTile = clickedTile.GetComponent<ITile>();
                            previousTile.OnClick();
                            selected = false;

                        }



                    }

                   
                    selected = true;
                    clickable.OnClick();

                    

                    clickedTile = hit.collider.gameObject;

                    ITile ballTile = hexGrid.RetreiveCurrentBallTile().GetComponent<ITile>();
                   // ITile selectedTile = clickedTile.GetComponent<ITile>();

                 //   Debug.Log("ballTile is: " + hexGrid.RetreiveCurrentBallTile() + " at: "  + Mathf.RoundToInt(ballTile.GetCoordinates().x) + "," + Mathf.RoundToInt(ballTile.GetCoordinates().y) + ". Selected is: " + 
                   //     clickedTile +" at: " + Mathf.RoundToInt(selectedTile.GetCoordinates().x) + ", " + Mathf.RoundToInt(selectedTile.GetCoordinates().y));

                    List<ITile> path = pathfinder.FindPath(Mathf.RoundToInt(ballTile.GetCoordinates().x), Mathf.RoundToInt(ballTile.GetCoordinates().y), Mathf.RoundToInt(clickable.GetCoordinates().x), Mathf.RoundToInt(clickable.GetCoordinates().y));

                    Debug.Log("Length is: " + pathfinder.GetPathLength(path));

                }
            }
        }
    }
}
