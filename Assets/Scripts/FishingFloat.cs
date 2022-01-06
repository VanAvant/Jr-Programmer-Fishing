using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingFloat : MonoBehaviour
{
    // Start is called before the first frame update

    private float windowToCatch = 0.4f;
    public GameObject bitingFish;
    public bool canCatchFish = false;

    IEnumerator catchTimer;

    private void Awake()
    {
        catchTimer = StartCatchableTime();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canCatchFish)
            {
                Debug.Log("Caught the fish");
            }
            else
            {
                try
                {
                    bitingFish.GetComponent<NPCFish>().EscapeFromTarget();
                }
                catch
                {
                    //We don't care if biting fish is empty, just means that no fish was in range when player clicked away
                }

                if (bitingFish != null)
                {
                    Debug.Log("Fish got away");
                }
                else
                {
                    Debug.Log("Cancelled fishing");
                }
            }

            StopCoroutine(catchTimer);
            DeleteSelf();
        }
    }

    public void DeleteSelf()
    {
        //GameObject.Destroy(gameObject);
        Destroy(gameObject);
    }

    public void FishHasBitten()
    {
        StartCoroutine(catchTimer);
    }

    private IEnumerator StartCatchableTime()
    {
        canCatchFish = true;

        yield return new WaitForSeconds(windowToCatch);

        canCatchFish = false;
        bitingFish.GetComponent<NPCFish>().EscapeFromTarget();
        Debug.Log("Took too long");
        bitingFish = null; //This does mean that multiple fish can attempt to bite the same bait, may require use of another bool to track whether a fish has bitten or not. 
    }
}
