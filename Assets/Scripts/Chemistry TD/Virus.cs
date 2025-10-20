using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;

    public List<Vector3> path = new();
    private int currentWaypoint = 0;
    private float waypointThreshold = 0.1f;
    public Vector3 direction;

    public string type;
    public int HP;
    public int progress = 0;

    private void Start()
    {
        HP = type.Length * 3;
    }

    private void Update()
    {
        if (path == null || path.Count == 0 || currentWaypoint >= path.Count)
        {
            transform.position += direction * Time.deltaTime * speed;
            progress++;
            return;
        }

        Vector3 target = path[currentWaypoint];
        Vector3 moveDir = (target - transform.position);

        // Solo mueve en las 4 direcciones principales
        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            direction = new Vector3(Mathf.Sign(moveDir.x), 0, 0);
        else
            direction = new Vector3(0, Mathf.Sign(moveDir.y), 0);

        Vector3 nextPosition = transform.position + direction * Time.deltaTime * speed;

        // Si el siguiente movimiento sobrepasa el waypoint, ajusta la posición al waypoint
        if (Vector3.Distance(nextPosition, target) < waypointThreshold || Vector3.Distance(transform.position, target) < waypointThreshold)
        {
            transform.position = target;
            currentWaypoint++;
        }
        else
        {
            transform.position = nextPosition;
        }
        progress++;
    }

    public void ProcessHit(string _type)
    {
        //Esto se podría invertir; por cada elemento distinto que tenga _type, este hará la misma cantidad de daño
        //Y se irá descontando si es que el virus tiene resistencia a algún elemento
        foreach (char c in _type)
        {
            if (!type.Contains(c))
            {
                HP--;
            }
        }

        if (HP <= 0)
            Destroy(gameObject);
    }
}

