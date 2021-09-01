using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private BuildingPlacer _buildingPlacer;

    public Color validTextColor;
    public Color invalidTextColor;

    [Header("Building")]
    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;

    [Header("Game Resources")]
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public GameObject gameResourceCostPrefab;

    [Header("Info Panel")]
    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    [Header("Units Selection")]
    public GameObject selectedUnitMenu;
    public GameObject selectedUnitMenuUpgradeButton;
    public GameObject selectedUnitMenuDestroyButton;
    private Text _selectedUnitTitleText;
    private Text _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitAttackParametersParent;
    private Transform _selectedUnitActionButtonsParent;
    private Unit _selectedUnit;
    public GameObject uiLabelPrefab;
    public GameObject unitSkillButtonPrefab;

    public Transform selectionGroupsParent;

    public Transform selectedUnitsListParent;
    public GameObject selectedUnitDisplayPrefab;

    private Dictionary<string, Button> _buildingButtons;
    private Dictionary<InGameResource, Text> _resourceTexts;

    [Header("Placed Building Production")]
    public RectTransform placedBuildingProductionRectTransform;

    [Header("Game Settings Panel")]
    public GameObject gameSettingsPanel;
    public Transform gameSettingsMenusParent;
    public Text gameSettingsContentName;
    public Transform gameSettingsContentParent;
    public GameObject gameSettingsMenuButtonPrefab;
    public GameObject gameSettingsParameterPrefab;
    public GameObject sliderPrefab;
    public GameObject togglePrefab;
    private Dictionary<string, GameParameters> _gameParameters;

    [Header("Main Menu Panel")]
    public GameObject mainMenuPanel;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();

        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("UnitSpecific/Content/GeneralInfo/Title").GetComponent<Text>();
        _selectedUnitLevelText = selectedUnitMenuTransform.Find("UnitSpecific/Content/GeneralInfo/Level").GetComponent<Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("UnitSpecific/Content/ResourcesProduction");
        _selectedUnitAttackParametersParent = selectedUnitMenuTransform.Find("UnitSpecific/Content/AttackParameters");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform.Find("UnitSpecific/SpecificActions");

        placedBuildingProductionRectTransform.gameObject.SetActive(false);

        gameSettingsPanel.SetActive(false);
        // read the list of game parameters and store them as a dict
        // (then setup the UI panel)
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();
        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;
        _SetupGameSettingsPanel();

        mainMenuPanel.SetActive(false);

        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<InGameResource, Text>();
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab);
            display.name = pair.Key.ToString();
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            _SetResourceText(pair.Key, pair.Value.Amount);
            display.transform.SetParent(resourcesUIParent, false);
        }

        // create buttons for each building type
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            BuildingData data = Globals.BUILDING_DATA[i];
            GameObject button = Instantiate(buildingButtonPrefab);
            button.name = data.unitName;
            if (data.sprite != null)
            {
                button.transform.Find("Icon").GetComponent<Image>().sprite = data.sprite;
                button.transform.Find("Text").gameObject.SetActive(false);
            }
            else
            {
                button.transform.Find("Icon").gameObject.SetActive(false);
                button.transform.Find("Text").GetComponent<Text>().text = data.unitName;
            }
            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            button.transform.SetParent(buildingMenu, false);
            _buildingButtons[data.code] = b;
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        ShowInfoPanel(false);
        _ShowSelectedUnitMenu(false);
        // hide all selection group buttons
        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.AddListener("SelectUnit", _OnSelectUnit);
        EventManager.AddListener("DeselectUnit", _OnDeselectUnit);
        EventManager.AddListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.RemoveListener("SelectUnit", _OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", _OnDeselectUnit);
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }

    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }

    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !gameSettingsPanel.activeSelf;
        gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PauseGame" : "ResumeGame");
    }

    public void ToggleMainMenuPanel()
    {
        bool showMainMenuPanel = !mainMenuPanel.activeSelf;
        mainMenuPanel.SetActive(showMainMenuPanel);
        EventManager.TriggerEvent(showMainMenuPanel ? "PauseGame" : "ResumeGame");
    }

    public void HoverLevelUpButton()
    {
        if (_selectedUnit.LevelMaxedOut) return;
        _UpdateSelectedUnitLevelUpInfoPanel();
        ShowInfoPanel(true);
        _SetSelectedUnitMenu(_selectedUnit, true);
    }

    public void UnhoverLevelUpButton()
    {
        if (_selectedUnit.LevelMaxedOut) return;
        ShowInfoPanel(false);
        _SetSelectedUnitMenu(_selectedUnit);
    }

    public void ClickLevelUpButton()
    {
        _selectedUnit.LevelUp();
        _SetSelectedUnitMenu(_selectedUnit, !_selectedUnit.LevelMaxedOut);
        if (_selectedUnit.LevelMaxedOut)
        {
            selectedUnitMenuUpgradeButton.transform.Find("Text").GetComponent<Text>().text = "Maxed out";
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = false;
            ShowInfoPanel(false);
        }
        else
        {
            _UpdateSelectedUnitLevelUpInfoPanel();
            _CheckBuyLimits();
        }
    }

    public void LoadGame()
    {

    }

    public void SaveGame()
    {

    }

    public void ResumeGame()
    {
        mainMenuPanel.SetActive(false);
        EventManager.TriggerEvent("ResumeGame");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }

    private void _AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void _CheckBuyLimits()
    {
        // chek if level up button is disabled or not
        if (
            _selectedUnit != null &&
            _selectedUnit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId &&
            !_selectedUnit.LevelMaxedOut &&
            Globals.CanBuy(_selectedUnit.LevelUpData.cost)
        )
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = true;

        // check if building buttons are disabled or not
        _OnCheckBuildingButtons();

        // check if buy/upgrade is affordable: update text colors
        if (infoPanel.activeSelf)
        {
            foreach (Transform resourceDisplay in _infoPanelResourcesCostParent)
            {
                InGameResource resourceCode = (InGameResource)System.Enum.Parse(
                    typeof(InGameResource),
                    resourceDisplay.Find("Icon").GetComponent<Image>().sprite.name
                );
                Text txt = resourceDisplay.Find("Text").GetComponent<Text>();
                int resourceAmount = int.Parse(txt.text);
                if (Globals.GAME_RESOURCES[resourceCode].Amount < resourceAmount)
                    txt.color = invalidTextColor;
                else
                    txt.color = validTextColor;
            }
        }
    }

    private void _UpdateSelectedUnitLevelUpInfoPanel()
    {
        int nextLevel = _selectedUnit.Level + 1;
        SetInfoPanel(
            "Level up",
            $"Upgrade unit to level {nextLevel}",
            _selectedUnit.LevelUpData.cost
        );
    }

    private void _SetResourceText(InGameResource resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }

    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES)
            _SetResourceText(pair.Key, pair.Value.Amount);

        _CheckBuyLimits();
    }

    private void _OnUpdatePlacedBuildingProduction(object data)
    {
        object[] values = (object[]) data;
        Dictionary<InGameResource, int> production = (Dictionary<InGameResource, int>) values[0];
        Vector3 pos = (Vector3) values[1];

        // clear current list
        foreach (Transform child in placedBuildingProductionRectTransform.gameObject.transform)
            Destroy(child.gameObject);

        // add one "resource cost" prefab per resource
        GameObject g;
        Transform t;
        foreach (KeyValuePair<InGameResource, int> pair in production)
        {
            g = Instantiate(gameResourceCostPrefab) as GameObject;
            t = g.transform;
            t.Find("Text").GetComponent<Text>().text = $"+{pair.Value}";
            t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            t.SetParent(placedBuildingProductionRectTransform.transform, false);
        }

        // resize container to fit the right number of lines
        placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);

        // place container top-right of the "phantom" building
        placedBuildingProductionRectTransform.anchoredPosition =
            (Vector2) Camera.main.WorldToScreenPoint(pos) / GameManager.instance.canvasScaleFactor
            + Vector2.right * 40f
            + Vector2.up * 10f;
    }

    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy();
    }

    private void _OnHoverBuildingButton(object data)
    {
        SetInfoPanel((UnitData) data);
        ShowInfoPanel(true);
    }

    private void _OnUnhoverBuildingButton()
    {
        ShowInfoPanel(false);
    }

    private void _OnSelectUnit(object data)
    {
        Unit unit = (Unit) data;
        AddSelectedUnitToUIList(unit);
        _SetSelectedUnitMenu(unit);
        _ShowSelectedUnitMenu(true);
    }

    private void _OnDeselectUnit(object data)
    {
        Unit unit = (Unit) data;
        RemoveSelectedUnitFromUIList(unit.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
            _SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);
    }

    private void _OnPlaceBuildingOn()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(true);
    }

    private void _OnPlaceBuildingOff()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }

    public void SetInfoPanel(UnitData data)
    {
        SetInfoPanel(data.unitName, data.description, data.cost);
    }
    public void SetInfoPanel(string title, string description, List<ResourceValue> resourceCosts)
    {
        // update texts
        _infoPanelTitleText.text = title;
        _infoPanelDescriptionText.text = description;
        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);
        if (resourceCosts.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in resourceCosts)
            {
                g = Instantiate(gameResourceCostPrefab) as GameObject;
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
                // check to see if resource requirement is not
                // currently met - in that case, turn the text into the "invalid"
                // color
                if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<Text>().color = invalidTextColor;
                t.SetParent(_infoPanelResourcesCostParent, false);
            }
        }
    }

    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }

    public void AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected,
        // increase the counter
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            Text t = alreadyInstantiatedChild.Find("Count").GetComponent<Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        // else create a brand new counter initialized with a count of 1
        else
        {
            GameObject g = Instantiate(selectedUnitDisplayPrefab);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<Text>().text = "1";
            t.Find("Name").GetComponent<Text>().text = unit.Data.unitName;
            t.SetParent(selectedUnitsListParent, false);
        }
    }

    public void RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;
        Text t = listItem.Find("Count").GetComponent<Text>();
        int count = int.Parse(t.text);
        count -= 1;
        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }

    private void _SetSelectedUnitMenu(Unit unit, bool showUpgrade = false)
    {
        _selectedUnit = unit;

        bool unitIsMine = unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId;

        // update texts
        _selectedUnitTitleText.text = unit.Data.unitName;
        _selectedUnitLevelText.text = $"Level {unit.Level}";

        // RESOURCE PRODUCTION --------------------------------------------
        // clear resource production
        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);
        // reinstantiate new ones (if I own the unit)
        if (unitIsMine && unit.Production.Count > 0)
        {
            GameObject g; Transform t;
            foreach (KeyValuePair<InGameResource, int> resource in unit.Production)
            {
                g = Instantiate(gameResourceCostPrefab);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = showUpgrade
                    ? $"<color=#00ff00>+{_selectedUnit.LevelUpData.newProduction[resource.Key]}</color>"
                    : $"+{resource.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");
                t.SetParent(_selectedUnitResourcesProductionParent, false);
            }
        }

        // ATTACK PARAMETERS ----------------------------------------------
        // clear attack parameters
        foreach (Transform child in _selectedUnitAttackParametersParent)
            Destroy(child.gameObject);
        // reinstantiate new ones (if I own the unit)
        if (unitIsMine)
        {
            GameObject g;
            g = Instantiate(uiLabelPrefab);
            g.GetComponent<Text>().text = showUpgrade
                    ? $"Damage: <color=#00ff00>{_selectedUnit.LevelUpData.newAttackDamage}</color>"
                    : $"Damage: {unit.AttackDamage}";
            g.transform.SetParent(_selectedUnitAttackParametersParent, false);

            g = Instantiate(uiLabelPrefab);
            g.GetComponent<Text>().text = showUpgrade
                    ? $"Range: <color=#00ff00>{(int) _selectedUnit.LevelUpData.newAttackRange}</color>"
                    : $"Range: {(int) unit.AttackRange}";
            g.transform.SetParent(_selectedUnitAttackParametersParent, false);
        }

        // SKILLS ---------------------------------------------------------
        // clear skills
        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);
        // reinstantiate new ones (if I own the unit)
        if (unitIsMine && unit.SkillManagers.Count > 0)
        {
            GameObject g; Transform t; Button b;
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = Instantiate(unitSkillButtonPrefab);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<Text>().text = unit.SkillManagers[i].skill.skillName;
                t.SetParent(_selectedUnitActionButtonsParent, false);
                _AddUnitSkillButtonListener(b, i);
            }
        }

        // if unit is mine, check if it can level up
        if (unitIsMine)
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = Globals.CanBuy(_selectedUnit.LevelUpData.cost);

        // hide upgrade/destroy buttons if I don't own the building
        selectedUnitMenuUpgradeButton.SetActive(unitIsMine);
        selectedUnitMenuDestroyButton.SetActive(unitIsMine);
    }

    private void _ShowSelectedUnitMenu(bool show)
    {
        selectedUnitMenu.SetActive(show);
    }

    private void _SetupGameSettingsPanel()
    {
        GameObject g; string n;
        List<string> availableMenus = new List<string>();
        foreach (GameParameters parameters in _gameParameters.Values)
        {
            // ignore game parameters assets that don't have
            // any parameter to show
            if (parameters.FieldsToShowInGame.Count == 0) continue;

            g = Instantiate(gameSettingsMenuButtonPrefab);
            n = parameters.GetParametersName();
            g.transform.Find("Text").GetComponent<Text>().text = n;
            _AddGameSettingsPanelMenuListener(g.GetComponent<Button>(), n);
            g.transform.SetParent(gameSettingsMenusParent, false);
            availableMenus.Add(n);
        }

        // if possible, set the first menu as the currently active one
        if (availableMenus.Count > 0)
            _SetGameSettingsContent(availableMenus[0]);
    }

    private void _AddGameSettingsPanelMenuListener(Button b, string menu)
    {
        b.onClick.AddListener(() => _SetGameSettingsContent(menu));
    }

    private void _SetGameSettingsContent(string menu)
    {
        gameSettingsContentName.text = menu;

        foreach (Transform child in gameSettingsContentParent)
            Destroy(child.gameObject);

        GameParameters parameters = _gameParameters[menu];
        System.Type ParametersType = parameters.GetType();
        GameObject gWrapper, gEditor;
        RectTransform rtWrapper, rtEditor;
        int i = 0;
        float contentWidth = 534f;
        float parameterNameWidth = 240f;
        float fieldHeight = 32f;
        foreach (string fieldName in parameters.FieldsToShowInGame)
        {
            gWrapper = Instantiate(gameSettingsParameterPrefab);
            gWrapper.transform.Find("Text").GetComponent<Text>().text = Utils.CapitalizeWords(fieldName);

            gEditor = null;
            FieldInfo field = ParametersType.GetField(fieldName);
            if (field.FieldType == typeof(bool))
            {
                gEditor = Instantiate(togglePrefab);
                Toggle t = gEditor.GetComponent<Toggle>();
                t.isOn = (bool) field.GetValue(parameters);
                t.onValueChanged.AddListener(delegate {
                    _OnGameSettingsToggleValueChanged(parameters, field, fieldName, t);
                });
            }
            else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
            {
                bool isRange = System.Attribute.IsDefined(field, typeof(RangeAttribute), false);
                if (isRange)
                {
                    RangeAttribute attr = (RangeAttribute)System.Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                    gEditor = Instantiate(sliderPrefab);
                    Slider s = gEditor.GetComponent<Slider>();
                    s.minValue = attr.min;
                    s.maxValue = attr.max;
                    s.wholeNumbers = field.FieldType == typeof(int);
                    s.value = field.FieldType == typeof(int)
                        ? (int) field.GetValue(parameters)
                        : (float)field.GetValue(parameters);
                    s.onValueChanged.AddListener(delegate
                    {
                        _OnGameSettingsSliderValueChanged(parameters, field, fieldName, s);
                    });
                }
            }
            gWrapper.transform.SetParent(gameSettingsContentParent, false);
            rtWrapper = gWrapper.GetComponent<RectTransform>();
            rtWrapper.anchoredPosition = new Vector2(1f, -i * fieldHeight);
            rtWrapper.sizeDelta = new Vector2(contentWidth, fieldHeight);

            if (gEditor != null)
            {
                gEditor.transform.SetParent(gWrapper.transform, false);
                rtEditor = gEditor.GetComponent<RectTransform>();
                rtEditor.anchoredPosition = new Vector2((parameterNameWidth + 16f), 0f);
                rtEditor.sizeDelta = new Vector2(rtWrapper.sizeDelta.x - (parameterNameWidth + 16f), fieldHeight);
            }

            i++;
        }

        RectTransform rt = gameSettingsContentParent.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = i * fieldHeight;
        rt.sizeDelta = size;
    }

    private void _OnGameSettingsToggleValueChanged(
        GameParameters parameters,
        FieldInfo field,
        string gameParameter,
        Toggle change
    )
    {
        field.SetValue(parameters, change.isOn);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.isOn);
    }

    private void _OnGameSettingsSliderValueChanged(
        GameParameters parameters,
        FieldInfo field,
        string gameParameter,
        Slider change
    )
    {
        if (field.FieldType == typeof(int))
            field.SetValue(parameters, (int) change.value);
        else
            field.SetValue(parameters, change.value);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.value);
    }
}
