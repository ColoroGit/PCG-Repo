using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawnManager : MonoBehaviour
{
    public GameObject virusPrefab;

    public int virusToSpawn;

    public string type; //Traer desde el Puzzle

    private void Start()
    {
        StartCoroutine(SpawnVirus());
    }

    private IEnumerator SpawnVirus()
    {
        while (virusToSpawn > 0)
        {
            GameObject virus = Instantiate(virusPrefab, transform.position, Quaternion.identity);
            Virus v = virus.GetComponent<Virus>();
            v.direction = new Vector3(1, 0, 0);
            v.type = type;

            yield return new WaitForSeconds(2);
        }
    }
}
