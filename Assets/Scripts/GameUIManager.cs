using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public int totalPoints;
    public int currentShotCount;
    public int par;
    private TMP_Text currentShotsText;
    private TMP_Text totalPointsText;
    private TMP_Text currentParText;

    [SerializeField] private GameObject totalScoreBack;
    private RectTransform totalPointsRect;
    private float totalPointsBackWidth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentShotCount = 0;
        totalPoints = 0;
        par = 0;
        totalPointsBackWidth = 125f;
       

        currentShotsText = GameObject.Find("Current Shots").GetComponent<TMP_Text>();
        totalPointsText = GameObject.Find("Total Points").GetComponent<TMP_Text>();
        currentParText = GameObject.Find("Par Number").GetComponent<TMP_Text>();

        totalPointsRect = totalScoreBack.GetComponent<RectTransform>();
        totalPointsRect.sizeDelta = new Vector2(totalPointsBackWidth, totalPointsRect.sizeDelta.y);
        //Turning all counts to 0.
        resetAllGameCounters();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addToCurrentShots(int shotsToAdd)
    {

        currentShotCount += shotsToAdd;

        currentShotsText.text = currentShotCount.ToString();

        

    }

    public void addToTotalPoints(int shotsToAdd)
    {

        totalPoints += shotsToAdd;

        if(totalPoints >= 10)
        {

            totalPointsRect.sizeDelta = new Vector2(160f, totalPointsRect.sizeDelta.y);
            //If the number gets to four digits, set font size smaller.
            //Could do the same for five digits.

           // totalPointsText.fontSize 

        } 
        else if (totalPoints >= 100)
        {
            totalPointsRect.sizeDelta = new Vector2(230f, totalPointsRect.sizeDelta.y);
        } 
        else if (totalPoints >= 1000)
        {

            totalPointsRect.sizeDelta = new Vector2(200f, totalPointsRect.sizeDelta.y);

        }


        totalPointsText.text = totalPoints.ToString();

    }

    public void changePar(int parNum)
    {

        par = parNum;

        currentParText.text = par.ToString();
    }

    public void resetAllGameCounters()
    {

        currentShotCount=0;
        totalPoints=0;
        par=0;

        currentShotsText.text = currentShotCount.ToString();
        totalPointsText.text = totalPoints.ToString();
        currentParText.text = par.ToString();

    }

    public int GetCurrentShots()
    {

        return currentShotCount;

    }


}
