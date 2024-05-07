using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;

public class Agent : MonoBehaviour, IDetectable
{
    public int ID {  get; private set; }
    public AgentMode Mode { get; private set; }

    public static int nextId = 0; // For simple initialization of unique id
    private IVessel ballBasket;

    public float viewRange = 10.0f;
    public float movementSpeed = 2.0f;


    private AIPath aiPath;
    private Vector3 startPosition; // Start position of the agent
    private Quaternion startRotation;
    private bool acquisitionActive = false; // Flag indicating whether acquisition is running


    private Dictionary<string, List<IDetectable>> currentObjectsInRange = new();
    //private List<ICollectable> collectablesInRange = new();
    //private List<Agent> agentsInRange = new();
    private ICollectable collectableToGet;

    public Vector3 GetPosition() { return transform.position; }
    public string GetTag() { return tag; }

    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        ID = nextId;
        nextId += 1;
        //Debug.Log("ID set: " + ID.ToString());

        Mode = AgentMode.idle;
        ballBasket = FindObjectOfType<Basket>();

        startPosition = transform.position; // Save the start position and rotation
        startRotation = transform.rotation;
    }

    // Update is called once per frame
    private void FixedUpdate() // Przyda�a by si� maszyna stan�w ale mo�e nie warto
    {
        if (!acquisitionActive) // If acquisition is paused, return to start position
        {
            if (Vector3.Distance(transform.position, startPosition) <= 0.1f)
            {
                aiPath.maxSpeed = 0f; // Zatrzymaj agenta
                transform.rotation = startRotation; // ustaw agenta do pierwotnej pozycji rotacji
                transform.position = startPosition;
            }
            else
            {
                aiPath.destination = startPosition;
                aiPath.maxSpeed = movementSpeed; // Ustaw pr�dko�� agenta na normaln�
            }

            return;
        }


        currentObjectsInRange = GetObjectsInRange(viewRange);
        //Debug.Log(currentObjectsInRange["Ball"][0].name);

        // Update state (very shitty state machine replacement)
        if (Mode == AgentMode.idle) // TODO: Po��czy� idle i searching ze sob�
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
        else if (Mode == AgentMode.gettingBall)
        {
            // TODO A* where the goal is the ballToGet and everything else is an obstacle
            /*transform.position = Vector3.MoveTowards(transform.position, collectableToGet.GetPosition(), movementSpeed * Time.fixedDeltaTime); // Temporary*/
            aiPath.destination = collectableToGet.GetPosition();
            aiPath.maxSpeed = movementSpeed;

        }
        else if (Mode == AgentMode.bringingBallBack)
        {
            // TODO A* where the goal is the ballBasket and everything else is an obstacle
            /* transform.position = Vector3.MoveTowards(transform.position, ballBasket.GetPosition(), movementSpeed * Time.fixedDeltaTime); // Temporary*/
            aiPath.destination = ballBasket.GetPosition();
            aiPath.maxSpeed = movementSpeed;
        }
        else if (Mode == AgentMode.searching)
        {
            // TODO
            Mode = AgentMode.idle; // Delete this
            aiPath.destination = startPosition; // temp
            aiPath.maxSpeed = movementSpeed; // temp
        }
        
        //Debug.Log("Agent " + ID.ToString() + ": " + Mode.ToString());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Mode == AgentMode.gettingBall)
        {
            //if (!collision.gameObject.TryGetComponent<ICollectable>(out var detectedCollectable)) return;
            ICollectable detectedCollectable = GetInterfaceOfObject<ICollectable>(other.gameObject);
            if (detectedCollectable != collectableToGet) return;

            detectedCollectable.Collect(GetComponent<IDetectable>());
            Mode = AgentMode.bringingBallBack;
         
        } 
        else if (Mode == AgentMode.bringingBallBack)
        {
            IVessel detectedVessel = GetInterfaceOfObject<IVessel>(other.gameObject);
            if (detectedVessel != ballBasket) return;

            Mode = AgentMode.idle;
            detectedVessel.Fill(collectableToGet); // throw the ball in the basket
            if (detectedVessel.GetFilledCollectableCount() >= Ball.Count) // Check if every ball is collected
            {
                Debug.Log("+++++++++KONIEC+++++++++");
                EventManager.InvokeAcquisitionPauseEvent();
                EventManager.InvokeAcquisitionEndEvent();
            }

            collectableToGet = null;
        }
    }


    // TODO: Store information about how far each object is?
    // Returns a dictionary where key is the tag of an object, and value is a list of objects with that tag that were seen
    private Dictionary<string, List<IDetectable>> GetObjectsInRange(float range)
    {
        Dictionary<string, List<IDetectable>> tagObjListPairs = new();

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider collider in colliders)
        {
            //if (!colliders[i].TryGetComponent<IDetectable>(out var detectedObject)) continue;
            IDetectable detectableObject = collider.GetComponent<IDetectable>();
            if (detectableObject != null)
            {
                string tag = detectableObject.GetTag();
                if (!tagObjListPairs.ContainsKey(tag))
                {
                    tagObjListPairs[tag] = new List<IDetectable>();
                }
                tagObjListPairs[tag].Add(detectableObject);
            }
        }

        return tagObjListPairs;
    }

    private ICollectable ReserveCollectable(List<IDetectable> collectableList)
    {
        foreach (ICollectable collectable in collectableList)
        {
            if (!collectable.IsReserved() && !collectable.IsCollected())
            {
                collectable.Reserve();
                return collectable;
            }
        }
        return null;
    }

    private T GetInterfaceOfObject<T>(GameObject obj) // TODO: Da� do do jakiej� statycznej klasy "Utils" �eby wszyscy mogli korzysta�
    {
        obj.TryGetComponent<T>(out T interf);
        return interf;
    }


    // Method to pause and resume acquisition
    private void StartAcquisition() { acquisitionActive = true; }
    private void PauseAcquisition() { acquisitionActive = false; }

    private void OnEnable()
    {
        EventManager.AcquisitionStartEvent += StartAcquisition;
        EventManager.AcquisitionPauseEvent += PauseAcquisition;
    }

    private void OnDisable()
    {
        EventManager.AcquisitionStartEvent -= StartAcquisition;
        EventManager.AcquisitionPauseEvent -= PauseAcquisition;
    }
}
