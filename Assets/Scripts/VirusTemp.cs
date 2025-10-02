using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusTemp : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private List<string> resistenceToElements;

    public string Name => name;
    public List<string> ResistenceToElements => resistenceToElements;
}
