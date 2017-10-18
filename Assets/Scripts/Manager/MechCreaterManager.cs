using Core.Managers;
using Core.XmlDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers
{
    public class MechCreaterManager : MonoBehaviour
    {
        public GameObject defaultToggleValues;
        public InputField mechNameSaveField;

        private Canvas UI;
        private GameObject demoMech;
        private PartInfo[] parts;
        private List<PartInfo> selectedParts;

        public void Awake()
        {
            UI = GameObject.Find("Mech Creation UI").GetComponent<Canvas>();
            demoMech = GameObject.Find("Creation Mech");

            selectedParts = new List<PartInfo>();

            PlayerManager.LoadPlayer();
            PlayerManager.OnPlayerLoadOK += LoadPlayer;
        }

        private void LoadPlayer(int id, string playerName, string data)
        {
            int count = 6;
            int k = 0;

            List<string> partStrings = data.ToLookup(c => Mathf.Floor(k++ / count)).Select(e => new String(e.ToArray())).ToList();

            foreach (string partName in partStrings)
            {
                foreach (PartInfo part in parts.Where(p => p.Name == partName))
                {
                    GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) => { Instantiate(prefab, demoMech.transform); });
                    selectedParts.Add(part);
                }
            }
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

        public void SaveMech()
        {
            PlayerManager.SavePlayer(mechNameSaveField.text, CreateByteArrayFromSelectParts());
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
                        var check = selectedParts.Where(p => p.Type == part.Type).ToArray();
                        if (check.Length > 0)
                        {
                            Destroy(demoMech.transform.Find(check[0].Name + "(Clone)")?.gameObject);
                            selectedParts.Remove(check[0]);
                        }

                        GameManager.Instance.ResourceManager.LoadAssetAsync<GameObject>(part.Prefab.Entry, (prefab) => { Instantiate(prefab, demoMech.transform); });
                        selectedParts.Add(part);
                    }
                    //else
                    //{
                    //    GameObject partToBeDeleted = demoMech.transform.Find(part.Name + "(Clone)")?.gameObject;

                    //    if (partToBeDeleted == null)
                    //        return;

                    //    Destroy(partToBeDeleted);

                    //    if (selectedParts.Find(f => f == part) == null)
                    //        return;

                    //    selectedParts.Remove(part);
                    //}
                });

                toggle.GetComponentInChildren<Text>().text = part.Name;
            }

            panel.SetActive(false);
        }

        private byte[] CreateByteArrayFromSelectParts()
        {
            return selectedParts.Select(p => p.Name).SelectMany(s => Encoding.ASCII.GetBytes(s)).ToArray();
        }

        [Obsolete]
        private List<string> GetPartsFromByteArray(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        }
    }
}