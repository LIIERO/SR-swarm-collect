using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;

public class Agent : MonoBehaviour
{
    public int ID {  get; private set; }
    public AgentMode Mode { get; private set; }

    public static int nextId = 0; // For simple initialization of unique id
    private static readonly string ballBasketName = "BallBasket";
    private GameObject ballBasket;

    public float viewRange = 10.0f;
    public float movementSpeed = 2.0f;

    private Dictionary<string, List<GameObject>> currentObjectsInRange = new();
    private GameObject ballToGet;

    // Start is called before the first frame update
    private void Start()
    {
        ID = nextId;
        nextId += 1;
        Debug.Log("ID set: " + ID.ToString());

        Mode = AgentMode.idle;
        ballBasket = GameObject.Find(ballBasketName);
    }

    // Update is called once per frame
    private void FixedUpdate() // Przyda³a by siê maszyna stanów ale mo¿e nie warto
    {
        currentObjectsInRange = GetObjectsInRange(viewRange);
        //Debug.Log(currentObjectsInRange["Ball"][0].name);

        // Update state (very shitty state machine replacement)
        if (Mode == AgentMode.idle)
        {
            // Assign a new ball to collect TODO: what if there are no balls to be seen?
            if (currentObjectsInRange.ContainsKey("Ball")) // Ball found in the field of view
            {
                ballToGet = ReserveBall(currentObjectsInRange["Ball"]);
                Mode = AgentMode.gettingBall;
                if (ballToGet == null) // Nothing unreserved was found
                    Mode = AgentMode.searching;
            }
            else // No ball found
            {
                Mode = AgentMode.searching;
            }
        }
        if (Mode == AgentMode.gettingBall)
        {
            // TODO A* where the goal is the ballToGet and everything else is an obstacle
            transform.position = Vector3.MoveTowards(transform.position, ballToGet.transform.position, movementSpeed * Time.fixedDeltaTime); // Temporary
        }
        if (Mode == AgentMode.bringingBallBack)
        {
            // TODO A* where the goal is the ballBasket and everything else is an obstacle
            transform.position = Vector3.MoveTowards(transform.position, ballBasket.transform.position, movementSpeed * Time.fixedDeltaTime); // Temporary
        }
        if (Mode == AgentMode.searching)
        {
            // TODO
            Mode = AgentMode.idle; // Delete this
        }

        Debug.Log("Agent " + ID.ToString() + ": " + Mode.ToString());
    }

    private void OnCollisionEnter(Collision collision) // Get the ball
    {
        if (Mode == AgentMode.gettingBall && collision.gameObject ==  ballToGet)
        {
            ballToGet = null;
            Destroy(collision.gameObject);
            Mode = AgentMode.bringingBallBack;
        }

        if (Mode == AgentMode.bringingBallBack && collision.gameObject == ballBasket)
        {
            Mode = AgentMode.idle;
            // TODO: Increment ball counter or something (by invoking an event)
        }
    }

    // TODO: Store information about how far each object is?
    // Returns a dictionary where key is the tag of an object, and value is a list of objects with that tag that were seen
    private Dictionary<string, List<GameObject>> GetObjectsInRange(float range)
    {
        Dictionary<string, List<GameObject>> tagObjListPairs = new();

        int maxColliders = 20;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, range, colliders); // Fill colliders array

        for (int i = 0; i < numColliders; i++)
        {
            if (!colliders[i].CompareTag("Untagged"))
            {
                string objTag = colliders[i].gameObject.tag;
                if (tagObjListPairs.ContainsKey(objTag))
                {
                    tagObjListPairs[objTag].Add(colliders[i].gameObject);
                }
                else
                {
                    tagObjListPairs.Add(objTag, new List<GameObject>() { colliders[i].gameObject });
                } 
            }
         
        }

        return tagObjListPairs;
    }

    private GameObject ReserveBall(List<GameObject> ballList)
    {
        foreach (GameObject ball in ballList) // TODO: Go through the balls from closest to furthest
        {
            Ball ballScript = ball.GetComponent<Ball>();
            if (ballScript.Reserved == false)
            {
                ballScript.Reserved = true;
                return ball;
            }
        }
        return null;
    }
}
