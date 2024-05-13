using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;
using UnityEditor.Search;

public class Agent : MonoBehaviour, IDetectable
{
    public int ID {  get; private set; }
    public AgentMode Mode { get; private set; }

    public static int nextId = 0; // For simple initialization of unique id
    private IVessel ballBasket;
    
    public float viewRange = 1.0f;
    public float movementSpeed = 2.0f;
    
    private Vector3 searchDirection;
    private float changeSearchDirectionTime = 2f;
    private float searchTimer;


    private AIPath aiPath;
    private Vector3 startPosition; // Start position of the agent
    private Quaternion startRotation;
    private bool acquisitionActive = false; // Flag indicating whether acquisition is running
    private bool isInitialized = false;


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
        aiPath.maxSpeed = movementSpeed;

        ID = nextId;
        nextId += 1;
        //Debug.Log("ID set: " + ID.ToString());

        Mode = AgentMode.idle;
        ballBasket = FindObjectOfType<Basket>();

        startPosition = transform.position; // Save the start position and rotation
        startRotation = transform.rotation;
    }

    // Update is called once per frame
    private void FixedUpdate() // Przyda³a by siê maszyna stanów ale mo¿e nie warto
    {
        if (!isInitialized) return;

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
                aiPath.maxSpeed = movementSpeed; // Ustaw prêdkoœæ agenta na normaln¹
            }

            return;
        }


        currentObjectsInRange = GetObjectsInRange(viewRange);
        //Debug.Log(currentObjectsInRange["Ball"][0].name);

        // Update state (very shitty state machine replacement)
        if (Mode == AgentMode.idle) // TODO: Po³¹czyæ idle i searching ze sob¹
        {
            // Assign a new ball to collect TODO: what if there are no balls to be seen?
            if (currentObjectsInRange.ContainsKey("Ball")) // Ball found in the field of view
            {
                //Debug.LogWarning(currentObjectsInRange["Ball"].Count);
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
            Mode = AgentMode.idle; // To jest potrzebne ¿eby agent mia³ szansê wykryæ obiekty i wyjœæ ze stanu szukania

            // Zmniejsz licznik czasu
            searchTimer -= Time.deltaTime;

            // Jeœli up³yn¹³ czas do zmiany kierunku
            if (searchTimer <= 0)
            {
                // Wylosuj nowy kierunek
                searchDirection = GetRandomDirection();


                // Ja tymczasowo da³em kierunek szukania razy 5 jako pozycjê do której zmierza, pobaw siê tym
                aiPath.destination = searchDirection * 5; // tutaj trzeba daæ POZYCJÊ do której agent ma iœæ jak szuka
                aiPath.maxSpeed = movementSpeed;

                // Zresetuj timer
                searchTimer = changeSearchDirectionTime;
            }


            // Metoda do losowania kierunku
            Vector3 GetRandomDirection()
            {
                // Wylosuj liczbê od 0 do 3
                int randomNum = Random.Range(0, 4);
                // Zwróæ odpowiedni wektor kierunku
                switch (randomNum)
                {
                    case 0:
                        return Vector3.forward; // Ruch do przodu
                    case 1:
                        return Vector3.back; // Ruch do ty³u
                    case 2:
                        return Vector3.left; // Ruch w lewo
                    case 3:
                        return Vector3.right; // Ruch w prawo
                    default:
                        return Vector3.forward; // Domyœlnie ruch do przodu
                }
            }  
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

    private T GetInterfaceOfObject<T>(GameObject obj) // TODO: Daæ do do jakiejœ statycznej klasy "Utils" ¿eby wszyscy mogli korzystaæ
    {
        obj.TryGetComponent<T>(out T interf);
        return interf;
    }


    private void Initialize(int maxAgents)
    {
        if (ID >= maxAgents)
        {
            Destroy(gameObject);
        } else
        {
            isInitialized = true;
        }
    }

    // Method to pause and resume acquisition
    private void StartAcquisition() { acquisitionActive = true; }
    private void PauseAcquisition() { acquisitionActive = false; }

    private void OnEnable()
    {
        EventManager.AgentInitializationEvent += Initialize;
        EventManager.AcquisitionStartEvent += StartAcquisition;
        EventManager.AcquisitionPauseEvent += PauseAcquisition;
    }

    private void OnDisable()
    {
        EventManager.AgentInitializationEvent -= Initialize;
        EventManager.AcquisitionStartEvent -= StartAcquisition;
        EventManager.AcquisitionPauseEvent -= PauseAcquisition;
    }
}
