using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject labelPrefab; // Para visualizar qué virus es, prefab de text tmp
    [SerializeField] private Vector3 labelOffset = new Vector3(0, 1.5f, 0);
    private TMP_Text labelInstance;

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

        if (labelPrefab == null)
        {
            Debug.LogWarning($"[Virus] {name}: labelPrefab no asignado.");
        }
        else
        {
            var labelObj = Instantiate(labelPrefab, transform);
            labelObj.transform.localPosition = labelOffset;

            // 2) Buscar el TMP_Text en RAÍZ o HIJOS (activo/inactivo)
            labelInstance = labelObj.GetComponentInChildren<TMP_Text>(true);

            if (labelInstance == null)
            {
                Debug.LogError($"[Virus] {name}: labelPrefab no contiene un TMP_Text en la raíz ni en sus hijos.");
            }
        }

        if (labelInstance != null)
        {
            labelInstance.text = type ?? "";
        }
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

        // label siempre mira a la cámara
        if (labelInstance != null && Camera.main != null)
        {
            labelInstance.transform.rotation = Camera.main.transform.rotation;
        }
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

