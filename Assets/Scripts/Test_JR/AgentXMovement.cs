using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentXMovement : MonoBehaviour
{
    public float speed = 1f;
    private Vector3 direction;
    private float changeDirectionTime = 2f;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        // Ustaw pocz�tkowy kierunek ruchu
        direction = GetRandomDirection();
        timer = changeDirectionTime;
    }

    // Update is called once per frame
    void Update()
    {
        // Zmniejsz licznik czasu
        timer -= Time.deltaTime;

        // Je�li up�yn�� czas do zmiany kierunku
        if (timer <= 0)
        {
            // Wylosuj nowy kierunek
            direction = GetRandomDirection();
            // Zresetuj timer
            timer = changeDirectionTime;
        }

        // Przesu� agenta w wybranym kierunku
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // Metoda do losowania kierunku
    Vector3 GetRandomDirection()
    {
        // Wylosuj liczb� od 0 do 3
        int randomNum = Random.Range(0, 4);
        // Zwr�� odpowiedni wektor kierunku
        switch (randomNum)
        {
            case 0:
                return Vector3.forward; // Ruch do przodu
            case 1:
                return Vector3.back; // Ruch do ty�u
            case 2:
                return Vector3.left; // Ruch w lewo
            case 3:
                return Vector3.right; // Ruch w prawo
            default:
                return Vector3.forward; // Domy�lnie ruch do przodu
        }
    }
}

