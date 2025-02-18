using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private GameObject clickedTile;
    [SerializeField] private bool selected = false;
    

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
                        ITile previousTile = clickedTile.GetComponent<ITile>();
                        previousTile.OnClick();
                        selected = false;

                    }

                   
                    selected = true;
                    clickable.OnClick();
                    clickedTile = hit.collider.gameObject;
                }
            }
        }
    }
}
