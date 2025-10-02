using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Virus : MonoBehaviour
{
    public int HP;
    public Vector3 direction;
    public string type;

    private void Update()
    {
        transform.position += direction * Time.deltaTime * 3;
    }

    public void ProcessHit(int DMG, string _type)
    {
        //Esto se podría invertir; por cada elemento distinto que tenga _type, este hará la misma cantidad de daño
        //Y se irá descontando si es que el virus tiene resistencia a algún elemento

        if (_type.Any(c => type.Contains(c))) // No hit
            return;

        HP -= DMG;

        if (HP < 0)
        {
            Destroy(gameObject);
        }
    }
}
