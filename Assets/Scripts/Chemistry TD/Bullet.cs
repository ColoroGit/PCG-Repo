using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    public GameObject objective;
    public string type;
    public Turret parent;

    // Update is called once per frame
    void Update()
    {
        if (objective != null)
        {
            Vector3 direction = (objective.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == objective)
        {
            objective.GetComponent<Virus>().ProcessHit(type);
            Destroy(gameObject);
        }
    }
}
