using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;

public class UIManager : MonoBehaviour
{
    private Button selectedMoleculeButton;
    private ColorBlock defaultColorBlock;
    private bool colorBlockInitialized = false;

    [Header("Puzzle y EA")]
    [SerializeField] Puzzle puzzle;
    [SerializeField] EA ea;

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

    [Header("Sliders, Seed y respectivas etiquetas")]
    [SerializeField] Slider virusAmountSlider;
    [SerializeField] TMP_Text virusAmountLabel;
    [SerializeField] Slider resistanceAmountSlider;
    [SerializeField] TMP_Text resistanceAmountLabel;
    [SerializeField] Slider difficultySlider;
    [SerializeField] TMP_Text difficultyLabel;
    [SerializeField] Slider extensionWeightSlider;
    [SerializeField] TMP_Text extensionWeightLabel;
    [SerializeField] Slider coverageWeightSlider;
    [SerializeField] TMP_Text coverageWeightLabel;
    [SerializeField] Slider turnsWeightSlider;
    [SerializeField] TMP_Text turnsWeightLabel;
    [SerializeField] Slider turnsDensitySlider;
    [SerializeField] TMP_Text turnsDensityLabel;
    [SerializeField] TMP_InputField SeedText;

    [Header("Referencia a torretas")]
    public int turretsBuilt = 0;
    [SerializeField] GameObject TurretPrefab;
    [SerializeField] GameObject cleaner;

    void Start()
    {
        HCounter = int.Parse(HCounterText.text);
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

    public void Generate()
    {
        foreach (Transform child in cleaner.transform)
        {
            Destroy(child.gameObject);
        }

        turretsBuilt = 0;
        UnityEngine.Random.InitState(int.Parse(SeedText.text));
        ea.Generate();
        puzzle.Generate();
    }

    private void SelectMoleculeButton(Button button)
    {
        if (!colorBlockInitialized)
        {
            defaultColorBlock = button.colors;
            colorBlockInitialized = true;
        }

        selectedMoleculeButton = button;
        ColorBlock cb = button.colors;
        cb.normalColor = Color.yellow;
        cb.selectedColor = Color.yellow;
        button.colors = cb;

        // Deshabilita los otros botones
        foreach (var b in new[] { H2OButton, CO2Button, HClButton, CH4Button, NaClButton, NaOHButton })
        {
            if (b != button)
                b.interactable = false;
        }
    }

    private void ResetMoleculeButtons()
    {
        if (selectedMoleculeButton != null)
        {
            selectedMoleculeButton.colors = defaultColorBlock;
            selectedMoleculeButton = null;
        }
        foreach (var b in new[] { H2OButton, CO2Button, HClButton, CH4Button, NaClButton, NaOHButton })
        {
            b.interactable = true;
        }
    }

    private void H2OOnButtonClick()
    {
        if (HCounter >= 2 && OCounter >= 1 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(H2OButton);
            StartCoroutine(PlaceTurretCoroutine("HOO", H2OButton));
        }
    }

    private void CO2OnButtonClick()
    {
        if (CCounter >= 1 && OCounter >= 2 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(CO2Button);
            StartCoroutine(PlaceTurretCoroutine("COO", CO2Button));
        }
    }

    private void HClButtonOnButtonClick()
    {
        if (HCounter >= 1 && ClCounter >= 1 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(HClButton);
            StartCoroutine(PlaceTurretCoroutine("HB", HClButton));
        }
    }

    private void CH4ButtonOnButtonClick()
    {
        if (CCounter >= 1 && HCounter >= 4 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(CH4Button);
            StartCoroutine(PlaceTurretCoroutine("CHHHH", CH4Button));
        }
    }

    private void NaClButtonOnButtonClick()
    {
        if (NaCounter >= 1 && ClCounter >= 1 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(NaClButton);
            StartCoroutine(PlaceTurretCoroutine("NB", NaClButton));
        }
    }

    private void NaOHButtonOnButtonClick()
    {
        if (NaCounter >= 1 && OCounter >= 1 && HCounter >= 1 && turretsBuilt < puzzle.virusAmount)
        {
            SelectMoleculeButton(NaOHButton);
            StartCoroutine(PlaceTurretCoroutine("NOH", NaOHButton));
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

    public void InitializeCounters(Dictionary<string, int> elements)
    {
        foreach (KeyValuePair<string, int> element in elements)
        {
            switch (element.Key)
            {
                case "H":
                    HCounter = element.Value;
                    break;
                case "O":
                    OCounter = element.Value;
                    break;
                case "C":
                    CCounter = element.Value;
                    break;
                case "N":
                    NaCounter = element.Value;
                    break;
                case "B":
                    ClCounter = element.Value;
                    break;
                default:
                    Debug.Log("Elemento no reconocido");
                    break;
            }
        }

        UpdateElementsCounter();
    }

    IEnumerator PlaceTurretCoroutine(string type, Button moleculeButton)
    {
        bool waitingForClick = true;

        while (waitingForClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    if (selectedMoleculeButton == moleculeButton)
                    {
                        break;
                    }
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject clickedObj = hit.collider.gameObject;
                        if (clickedObj.name.StartsWith("MapRoad_"))
                        {
                            // No hacer nada, seguir esperando
                            Debug.Log("No se puede colocar una torreta en el camino.");
                        }
                        else
                        {
                            Debug.Log("Turreta colocada de tipo: " + type);
                            DepleteAtoms(type);
                            GameObject turret = Instantiate(TurretPrefab, cleaner.transform);
                            turret.transform.position = hit.point + Vector3.back;
                            turret.GetComponent<Turret>().type = type;
                            turretsBuilt++;
                            break;
                        }
                    }
                }
            }
            yield return null;
        }

        ResetMoleculeButtons();
    }

    private void DepleteAtoms(string type)
    {
        switch (type)
        {
            case "HOO":
                HCounter -= 2;
                OCounter -= 1;
                break;
            case "COO":
                CCounter -= 1;
                OCounter -= 2;
                break;
            case "HB":
                HCounter -= 1;
                ClCounter -= 1;
                break;
            case "CHHHH":
                CCounter -= 1;
                HCounter -= 4;
                break;
            case "NB":
                NaCounter -= 1;
                ClCounter -= 1;
                break;
            case "NOH":
                NaCounter -= 1;
                OCounter -= 1;
                HCounter -= 1;
                break;
            default:
                Debug.Log("Tipo de molécula no reconocido para depletar átomos.");
                break;
        }

        UpdateElementsCounter();
    }

    public void OnVirusAmountSliderChanged()
    {
        puzzle.SetVirusAmount(virusAmountSlider.value);
        virusAmountLabel.text = $": {virusAmountSlider.value:F0}";
    }

    public void OnResistanceAmountSliderChanged()
    {
        puzzle.SetResistanceAmount(resistanceAmountSlider.value);
        resistanceAmountLabel.text = $": {resistanceAmountSlider.value:F1}";
    }

    public void OnDifficultySliderChanged()
    {
        puzzle.SetDifficulty(difficultySlider.value);
        difficultyLabel.text = $": {difficultySlider.value:F2}";
    }

    public void OnExtensionWeightSliderChanged()
    {
        ea.SetExtensionWeight(extensionWeightSlider.value);
        extensionWeightLabel.text = $": {extensionWeightSlider.value:F2}";
    }

    public void OnCoverageWeightSliderChanged()
    {
        ea.SetCoverageWeight(coverageWeightSlider.value);
        coverageWeightLabel.text = $": {coverageWeightSlider.value:F2}";
    }

    public void OnTurnsWeightSliderChanged()
    {
        ea.SetTurnsWeight(turnsWeightSlider.value);
        turnsWeightLabel.text = $": {turnsWeightSlider.value:F2}";
    }

    public void OnTurnsDensitySliderChanged()
    {
        ea.SetTurnsDensity(turnsDensitySlider.value);
        turnsDensityLabel.text = $": {turnsDensitySlider.value:F2}";
    }
    public void OnSeedChanged() { UnityEngine.Random.InitState(int.Parse(SeedText.text)); }
}
