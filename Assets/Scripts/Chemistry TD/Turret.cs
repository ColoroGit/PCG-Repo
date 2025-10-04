using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Virus currentVirus;

    public string type;
    public float recoil;

    public Coroutine shooting;

    private void Update()
    {
        if (currentVirus == null)
        {
            if (shooting != null)
            {
                StopCoroutine(shooting);
                shooting = null;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Virus v = collision.gameObject.GetComponent<Virus>();

        if (v)
        {
            if (currentVirus == null)
            {
                currentVirus = v;
                shooting = StartCoroutine(Shoot());
            }
            else if (v.progress > currentVirus.progress)
            {
                currentVirus = v;
                if (shooting != null)
                {
                    StopCoroutine(shooting);
                }
                shooting = StartCoroutine(Shoot());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentVirus != null && collision.gameObject == currentVirus.gameObject)
        {
            currentVirus = null;
            if (shooting != null)
            {
                StopCoroutine(shooting);
                shooting = null;
            }
        }
    }

    private IEnumerator Shoot()
    {
        while (true)
        {
            yield return new WaitForSeconds(recoil);

            if (currentVirus == null)
                yield break;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Bullet b = bullet.GetComponent<Bullet>();
            b.type = type;
            b.objective = currentVirus.gameObject;
            b.parent = this;
        }
    }
}
