using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject currentVirus;

    public string type;
    public float recoil;

    private Coroutine shooting;

    private void Update()
    {
        if (currentVirus != null)
        {
            shooting = StartCoroutine(Shoot());
        }
        else if (shooting != null)
        {
            StopCoroutine(shooting);
            shooting = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("virus"))
        {
            currentVirus = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == currentVirus)
        {
            currentVirus = null;
        }
    }

    private IEnumerator Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().type = type;
        yield return new WaitForSeconds(recoil);
    }
}
