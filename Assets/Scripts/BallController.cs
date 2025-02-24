using UnityEngine;
using System.Collections.Generic;

public class BallController : MonoBehaviour
{
    [SerializeField] float hitHeight = 5f;
    [SerializeField] float hitSpeed = 1f;

    [SerializeField] Vector3 startPoint;
    [SerializeField] Vector3 endPoint;

    [SerializeField] Vector3 ballSurfaceHeight = new Vector3 (0,0,-0.25f);
    [SerializeField] private float minHeight = 0.5f;
    [SerializeField] private float maxHeight = 6f;
    private float dynamicHeight;

    [SerializeField] float t = 0f;
    [SerializeField] bool isMoving = false;
    [SerializeField] GameObject ballTile;
    [SerializeField] private Pathfinding pathfinder;

    void Start()
    {
        pathfinder = GameObject.FindGameObjectWithTag("MapBuilder").GetComponent<Pathfinding>();
    }

    // Update is called once per frame
    void Update()
    {
        Travel();



    }


    public void CalculateHit()
    {

      //  List<ITile> path = pathfinder.FindPath(Mathf.RoundToInt(ballTile.GetCoordinates().x), Mathf.RoundToInt(ballTile.GetCoordinates().y), Mathf.RoundToInt(selectedTile.GetCoordinates().x), Mathf.RoundToInt(selectedTile.GetCoordinates().y));

      //  Debug.Log("Length is: " + pathfinder.GetPathLength(path));




    }





    public void Jump(GameObject tileFrom, GameObject tileTo)
    {

        t = 0f;
        hitSpeed = 4f;
        isMoving = true;

        startPoint = tileFrom.transform.position + ballSurfaceHeight;
        endPoint = tileTo.transform.position + ballSurfaceHeight;


        float distance = Vector3.Distance(startPoint, endPoint);
        dynamicHeight = Mathf.Clamp(distance * hitHeight, minHeight, maxHeight);



       // Debug.Log("start: " + startPoint + ". end: " + endPoint);

    }

    public Vector3 GetFlightPoint(Vector3 tileFrom, Vector3 tileTo)
    {

        Vector3 midPoint = Vector3.Lerp(tileFrom , tileTo , t);

        float parabolicHeight = 4 * dynamicHeight * (t - t * t);

        return new Vector3(midPoint.x,midPoint.y,midPoint.z - parabolicHeight);
    }

    private void Travel()
    {

        if(isMoving)
        {

            hitSpeed *= .98f;
            t += Time.deltaTime * hitSpeed;
            t = Mathf.Clamp01(t);
            

            Vector3 pos = GetFlightPoint(startPoint, endPoint);
            transform.position = pos;

          //  Debug.Log("Position: " + gameObject.transform.position);

            if (Vector3.Distance(gameObject.transform.position, endPoint) < 0.01f)
            {
             //   Debug.Log("They are close enough!");
                gameObject.transform.position = endPoint;
                isMoving = false;
            }


        } else
        {
            //Keeps ball object tracking it's current tile.
            gameObject.transform.position = ballTile.transform.position + ballSurfaceHeight;

        }


    }

    public void SetHexGrid(GameObject hGrid)
    {
        ballTile = hGrid;
    }



}
