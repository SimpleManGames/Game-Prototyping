using Core.Managers;
using Core.XmlDatabase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MechCreaterManager : Singleton<MechCreaterManager>
{
    public GameObject defaultToggleValues;

    private Canvas UI;
    private GameObject demoMech;
    private PartInfo[] parts;
    public List<PartInfo> selectedParts;

    public override void Awake()
    {
        base.Awake();
        UI = GameObject.Find("Mech Creation UI").GetComponent<Canvas>();
        demoMech = GameObject.Find("Creation Mech");
    }

    public void Start()
    {
        parts = Database.Instance.GetEntries<PartInfo>();
        CreateToggleGroup(PartType.Head);
        CreateToggleGroup(PartType.Core);
        CreateToggleGroup(PartType.Arms);
        CreateToggleGroup(PartType.Legs);
        CreateToggleGroup(PartType.Booster);
    }

    private void CreateToggleGroup(PartType type)
    {
        GameObject panel = GameObject.Find(type.ToString() + " Panel");
        ToggleGroup group = panel.AddComponent<ToggleGroup>();
        group.allowSwitchOff = true;

        foreach (PartInfo part in parts.Where(p => p.Type == type))
        {
            GameObject newToggleObject = Instantiate(defaultToggleValues);
            newToggleObject.transform.SetParent(panel.transform);
            Toggle toggle = newToggleObject.GetComponent<Toggle>();
            toggle.isOn = false;

            toggle.group = group;
            toggle.onValueChanged.AddListener((bool value) => 
            {
                if (value)
                {
                    GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) => { Instantiate(prefab, demoMech.transform); });
                    selectedParts.Add(part);
                }
                else
                {
                    GameObject partToBeDeleted = demoMech.transform.Find(part.Name + "(Clone)")?.gameObject;

                    if (partToBeDeleted == null)
                        return;

                    selectedParts.Remove(part);
                    Destroy(partToBeDeleted);
                }
            });

            toggle.GetComponentInChildren<Text>().text = part.Name;
        }

        panel.SetActive(false);
    }
}