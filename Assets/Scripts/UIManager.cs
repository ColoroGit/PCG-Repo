using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Contadores elementos")]
    [SerializeField] TMP_Text HCounterText;
    [SerializeField] TMP_Text OCounterText;
    [SerializeField] TMP_Text CCounterText;
    [SerializeField] TMP_Text NaCounterText;
    [SerializeField] TMP_Text ClCounterText;

    // Variables internas
    int HCounter;
    int OCounter;
    int CCounter;
    int NaCounter;
    int ClCounter;


    [Header("Botones de uso")]
    [SerializeField] Button H2OButton; // HOO
    [SerializeField] Button CO2Button; // COO
    [SerializeField] Button HClButton; // HB
    [SerializeField] Button CH4Button; // CHHHH
    [SerializeField] Button NaClButton; // NB
    [SerializeField] Button NaOHButton; // NOH

    [Header("Referencia a torretas")]
    [SerializeField] GameObject TurretSpots;
    [SerializeField] GameObject TurretPrefab;
    void Start()
    {
        HCounter = int.Parse(HCounterText.text); // Cambiar a que se iguale al resultado del script de puzzle en vez del valor de los textos
        OCounter = int.Parse(OCounterText.text);
        CCounter = int.Parse(CCounterText.text);
        NaCounter = int.Parse(NaCounterText.text);
        ClCounter = int.Parse(ClCounterText.text);

        H2OButton.onClick.AddListener(H2OOnButtonClick);
        CO2Button.onClick.AddListener(CO2OnButtonClick);
        HClButton.onClick.AddListener(HClButtonOnButtonClick);
        CH4Button.onClick.AddListener(CH4ButtonOnButtonClick);
        NaClButton.onClick.AddListener(NaClButtonOnButtonClick);
        NaOHButton.onClick.AddListener(NaOHButtonOnButtonClick);
    }

    void Update()
    {
        
    }

    private void H2OOnButtonClick()
    {
        if(HCounter >= 2 && OCounter >= 1)
        {
            HCounter -= 2;
            OCounter -= 1;

            PlaceTurret("HOO");
            UpdateElementsCounter();
        }
    }

    private void CO2OnButtonClick()
    {
        if(CCounter >= 1 && OCounter >= 2)
        {
            CCounter -= 1;
            OCounter -= 2;

            PlaceTurret("COO");
            UpdateElementsCounter();
        }
    }

    private void HClButtonOnButtonClick()
    {
        if (HCounter >= 1 && ClCounter >= 1)
        {
            HCounter -= 1;
            ClCounter -= 1;

            PlaceTurret("HB");
            UpdateElementsCounter();
        }
    }

    private void CH4ButtonOnButtonClick()
    {
        if(CCounter >= 1 && HCounter >= 4)
        {
            CCounter -= 1;
            HCounter -= 4;

            PlaceTurret("CHHHH");
            UpdateElementsCounter();
        }
    }

    private void NaClButtonOnButtonClick()
    {
        if(NaCounter >= 1 && ClCounter >= 1)
        {
            NaCounter -= 1;
            ClCounter -= 1;

            PlaceTurret("NB");
            UpdateElementsCounter();
        }
    }
    private void NaOHButtonOnButtonClick()
    {
        if(NaCounter >= 1 && OCounter >= 1 && HCounter >= 1)
        {
            NaCounter -= 1;
            OCounter -= 1;
            HCounter -= 1;

            PlaceTurret("NOH");
            UpdateElementsCounter();
        }
    }

    void UpdateElementsCounter()
    {
        HCounterText.text = HCounter.ToString();
        OCounterText.text = OCounter.ToString();
        CCounterText.text = CCounter.ToString();
        NaCounterText.text = NaCounter.ToString();
        ClCounterText.text = ClCounter.ToString();
    }

    void PlaceTurret(string type)
    {
        Transform currentTransform = TurretSpots.transform;

        for(int i = 0; i < currentTransform.childCount; i++)
        {
            if(currentTransform.childCount == 0)
            {
                GameObject turret = Instantiate(TurretPrefab, currentTransform);
                turret.GetComponent<Turret>().type = type;
                break;
            }
        }
    }
}
