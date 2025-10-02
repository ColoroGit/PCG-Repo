using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject objective;
    public int DMG;
    public string type;

    // Update is called once per frame
    void Update()
    {
        if (objective != null)
        {
            Vector2.MoveTowards(transform.position, objective.transform.position, Time.deltaTime * 10);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == objective)
        {
            objective.GetComponent<Virus>().ProcessHit(DMG, type);
            Destroy(gameObject);
        }
    }
}
