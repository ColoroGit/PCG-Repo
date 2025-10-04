using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;

    public List<Vector3> path = new() { new Vector3(20.1f, 9.6f, 0f), new Vector3(20.5f, -6.5f, 0f), new Vector3(43.2f, -6.5f, 0f) };
    private int currentWaypoint = 0;
    private float waypointThreshold = 0.1f;
    public Vector3 direction;

    public string type;
    public int progress = 0; 

    private void Update()
    {
        if (path == null || currentWaypoint >= path.Count)
        {
            transform.position += direction * Time.deltaTime * speed;
            progress++;
            return;
        }

        Vector3 target = path[currentWaypoint];
        Vector3 moveDir = (target - transform.position).normalized;

        // Solo mueve en las 4 direcciones principales
        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            direction = new Vector3(Mathf.Sign(moveDir.x), 0, 0);
        else
            direction = new Vector3(0, Mathf.Sign(moveDir.y), 0);

        transform.position += direction * Time.deltaTime * speed;
        progress++;

        // Si está cerca del waypoint, avanza al siguiente
        if (Vector3.Distance(transform.position, target) < waypointThreshold)
        {
            currentWaypoint++;
        }
    }

    public void ProcessHit(string _type)
    {
        //Esto se podría invertir; por cada elemento distinto que tenga _type, este hará la misma cantidad de daño
        //Y se irá descontando si es que el virus tiene resistencia a algún elemento

        if (_type.Any(c => type.Contains(c))) // No hit
            return;

        Destroy(gameObject);
    }
}
