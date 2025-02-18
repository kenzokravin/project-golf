using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private GameObject clickedTile;
    [SerializeField] private bool selected = false;

    private HexGrid hexGrid;

    private void Start()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Clicked on: " + hit.collider.gameObject.name);

                // If the object has a specific script
                ITile clickable = hit.collider.gameObject.GetComponent<ITile>();
                if (clickable != null)
                {
                    if(selected)
                    {

                        if(hit.collider.gameObject == clickedTile)
                        {


                            ITile confirmTile = clickedTile.GetComponent<ITile>();
                            confirmTile.OnClick();
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

                    hexGrid.SwapActiveHex(hit.collider.gameObject);

                    clickedTile = hit.collider.gameObject;
                }
            }
        }
    }
}
