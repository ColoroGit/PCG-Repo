using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject virusPrefab;

    [SerializeField]
    private int virusToSpawn;

    private List<string> virusTypes; 

    public void StartRound(List<string> _virusTypes)
    {
        virusTypes = _virusTypes;
        StartCoroutine(SpawnVirus());
    }

    private IEnumerator SpawnVirus()
    {
        for (int i = 0; i < virusToSpawn; i++)
        {
            string type = virusTypes[i % virusTypes.Count];
            GameObject virus = Instantiate(virusPrefab, transform.position, Quaternion.identity);
            Virus v = virus.GetComponent<Virus>();
            v.direction = new Vector3(1, 0, 0);
            v.type = type;
            yield return new WaitForSeconds(2);
        }
    }
}
