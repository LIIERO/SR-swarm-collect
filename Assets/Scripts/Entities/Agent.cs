using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using System.Linq;

public class Agent : MonoBehaviour, IDetectable
{
    public int ID {  get; private set; }
    public AgentMode Mode { get; private set; }

    public static int nextId = 0; // For simple initialization of unique id
    private IVessel ballBasket;

    public float viewRange = 10.0f;
    public float movementSpeed = 2.0f;

    private Dictionary<string, List<IDetectable>> currentObjectsInRange = new();
    //private List<ICollectable> collectablesInRange = new();
    //private List<Agent> agentsInRange = new();
    private ICollectable collectableToGet;

    public Vector3 GetPosition() { return transform.position; }
    public string GetTag() { return tag; }

    // Start is called before the first frame update
    private void Start()
    {
        ID = nextId;
        nextId += 1;
        //Debug.Log("ID set: " + ID.ToString());

        Mode = AgentMode.idle;
        ballBasket = FindObjectOfType<Basket>();
    }

    // Update is called once per frame
    private void FixedUpdate() // Przyda³a by siê maszyna stanów ale mo¿e nie warto
    {
        currentObjectsInRange = GetObjectsInRange(viewRange);
        //Debug.Log(currentObjectsInRange["Ball"][0].name);

        // Update state (very shitty state machine replacement)
        if (Mode == AgentMode.idle) // TODO: Po³¹czyæ idle i searching ze sob¹
        {
            // Assign a new ball to collect TODO: what if there are no balls to be seen?
            if (currentObjectsInRange.ContainsKey("Ball")) // Ball found in the field of view
            {
                collectableToGet = ReserveCollectable(currentObjectsInRange["Ball"]);
                Mode = AgentMode.gettingBall;
                if (collectableToGet == null) // Nothing unreserved was found
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
            transform.position = Vector3.MoveTowards(transform.position, collectableToGet.GetPosition(), movementSpeed * Time.fixedDeltaTime); // Temporary
        }
        if (Mode == AgentMode.bringingBallBack)
        {
            // TODO A* where the goal is the ballBasket and everything else is an obstacle
            transform.position = Vector3.MoveTowards(transform.position, ballBasket.GetPosition(), movementSpeed * Time.fixedDeltaTime); // Temporary
        }
        if (Mode == AgentMode.searching)
        {
            // TODO
            Mode = AgentMode.idle; // Delete this
        }

        //Debug.Log("Agent " + ID.ToString() + ": " + Mode.ToString());
    }

    private void OnCollisionEnter(Collision collision) // Get the ball
    {
        if (Mode == AgentMode.gettingBall)
        {
            //if (!collision.gameObject.TryGetComponent<ICollectable>(out var detectedCollectable)) return;
            ICollectable detectedCollectable = GetInterfaceOfObject<ICollectable>(collision.gameObject);
            if (detectedCollectable != collectableToGet) return;

            detectedCollectable.Collect(GetComponent<IDetectable>());
            Mode = AgentMode.bringingBallBack;
        }

        else if (Mode == AgentMode.bringingBallBack)
        {
            IVessel detectedVessel = GetInterfaceOfObject<IVessel>(collision.gameObject);
            if (detectedVessel != ballBasket) return;

            Mode = AgentMode.idle;
            detectedVessel.Fill(collectableToGet); // throw the ball in the basket
            collectableToGet = null;
            // Ball counter inside the basket
        }
    }

    // TODO: Store information about how far each object is?
    // Returns a dictionary where key is the tag of an object, and value is a list of objects with that tag that were seen
    private Dictionary<string, List<IDetectable>> GetObjectsInRange(float range)
    {
        Dictionary<string, List<IDetectable>> tagObjListPairs = new();

        int maxColliders = 20;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, range, colliders); // Fill colliders array

        for (int i = 0; i < numColliders; i++)
        {
            //if (!colliders[i].TryGetComponent<IDetectable>(out var detectedObject)) continue;
            IDetectable detectedObject = GetInterfaceOfObject<IDetectable>(colliders[i].gameObject);
            if (detectedObject == null) continue;
            string objTag = detectedObject.GetTag();

            if (objTag != "Untagged")
            {
                if (tagObjListPairs.ContainsKey(objTag))
                {
                    tagObjListPairs[objTag].Add(detectedObject);
                }
                else
                {
                    tagObjListPairs.Add(objTag, new List<IDetectable>() { detectedObject });
                } 
            }
         
        }

        return tagObjListPairs;
    }

    private ICollectable ReserveCollectable(List<IDetectable> collectableList)
    {
        foreach (ICollectable collectable in collectableList.Cast<ICollectable>()) // TODO: Go through the balls from closest to furthest
        {
            if (!collectable.IsReserved() && !collectable.IsCollected())
            {
                collectable.Reserve();
                return collectable;
            }
        }
        return null;
    }

    private T GetInterfaceOfObject<T>(GameObject obj) // TODO: Daæ do do jakiejœ statycznej klasy "Utils" ¿eby wszyscy mogli korzystaæ
    {
        obj.TryGetComponent<T>(out T interf);
        return interf;
    }
}
