using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject virusPrefab;

    [SerializeField]
    private int virusToSpawn;

    [SerializeField]
    private GameObject cleaner;

    public List<string> virusTypes;

    public List<Vector3> path;

    public void StartRound()
    {
        if (virusTypes.Count == 0) { return; }
        StartCoroutine(SpawnVirus());
    }

    private IEnumerator SpawnVirus()
    {
        for (int i = 0; i < virusToSpawn; i++)
        {
            string type = virusTypes[i % virusTypes.Count];
            GameObject virus = Instantiate(virusPrefab, cleaner.transform);
            virus.transform.position = transform.position;
            Virus v = virus.GetComponent<Virus>();
            v.direction = new Vector3(1, 0, 0);
            v.type = type;
            v.path = path;
            yield return new WaitForSeconds(2);
        }
    }
}
