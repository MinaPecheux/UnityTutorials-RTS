using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private int _myPlayerId;

    public Color validTextColor;
    public Color invalidTextColor;

    [Header("Game Resources")]
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public GameObject gameResourceCostPrefab;

    [Header("Info Panel")]
    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    [Header("Construction Menu")]
    public GameObject constructionMenu;
    private Button[] _constructionSlotButtons;

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

    [Header("Units Selection")]
    public Transform unitFormationTypesParent;
    private Image[] _unitFormationTypeImages;
    private Color _unitFormationTypeActiveColor = Color.white;
    private Color _unitFormationTypeInactiveColor = Color.gray;

    [Header("Placed Building Production")]
    public RectTransform placedBuildingProductionRectTransform;

    [Header("Technology Tree")]
    public GameObject techTreePanel;
    private Transform _techTreeViewParent;
    private Transform _techTreeCosts;
    private const float _TECH_TREE_VIEW_WIDTH = 966f;
    private bool _initializedTechnologyTree;
    private bool _showingTechTreePanel;
    private Dictionary<string, Image> _techTreeProgressBars;

    [Header("Game Settings Panel")]
    public GameObject gameSettingsPanel;
    public Transform gameSettingsMenusParent;
    public Text gameSettingsContentName;
    public Transform gameSettingsContentParent;
    public GameObject gameSettingsMenuButtonPrefab;
    public GameObject gameSettingsParameterPrefab;
    public GameObject sliderPrefab;
    public GameObject togglePrefab;
    public GameObject inputMappingPrefab;
    public GameObject inputBindingPrefab;
    private Dictionary<string, GameParameters> _gameParameters;

    [Header("Main Menu Panel")]
    public GameObject mainMenuPanel;

    [Header("Misc")]
    public Image playerIndicatorImage;
    private Dictionary<InGameResource, Text> _resourceTexts;

    private void Awake()
    {
        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");

        _constructionSlotButtons = new Button[3];
        Transform constructionSlots = constructionMenu.transform.Find("BuildSlots");
        for (int i = 0; i < 3; i++)
            _constructionSlotButtons[i] = constructionSlots.GetChild(i).Find("Button").GetComponent<Button>();
        constructionMenu.SetActive(false);

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("UnitSpecific/Content/GeneralInfo/Title").GetComponent<Text>();
        _selectedUnitLevelText = selectedUnitMenuTransform.Find("UnitSpecific/Content/GeneralInfo/Level").GetComponent<Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("UnitSpecific/Content/ResourcesProduction");
        _selectedUnitAttackParametersParent = selectedUnitMenuTransform.Find("UnitSpecific/Content/AttackParameters");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform.Find("UnitSpecific/SpecificActions");

        _unitFormationTypeImages = new Image[] {
            unitFormationTypesParent.Find("FormationNone").GetComponent<Image>(),
            unitFormationTypesParent.Find("FormationLine").GetComponent<Image>(),
            unitFormationTypesParent.Find("FormationGrid").GetComponent<Image>(),
            unitFormationTypesParent.Find("FormationXCross").GetComponent<Image>()
        };
        _OnUpdatedUnitFormationType();

        placedBuildingProductionRectTransform.gameObject.SetActive(false);

        _initializedTechnologyTree = false;
        _showingTechTreePanel = false;
        techTreePanel.SetActive(_showingTechTreePanel);
        _techTreeViewParent = techTreePanel.transform.Find("Panel/Content/Scroll View/Viewport/Content");
        _techTreeCosts = techTreePanel.transform.Find("Panel/Content/Costs");

        gameSettingsPanel.SetActive(false);
        // read the list of game parameters and store them as a dict
        // (then setup the UI panel)
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();
        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;
        _SetupGameSettingsPanel();

        mainMenuPanel.SetActive(false);

        ShowInfoPanel(false);
        _ShowSelectedUnitMenu(false);
        // hide all selection group buttons
        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);
    }

    private void Start()
    {
        _myPlayerId = GameManager.instance.gamePlayersParameters.myPlayerId;

        // set player indicator color to match my player color
        Color c = GameManager.instance.gamePlayersParameters.players[_myPlayerId].color;
        c = Utils.LightenColor(c, 0.2f);
        playerIndicatorImage.color = c;

        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<InGameResource, Text>();
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerId])
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key.ToString();
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            _SetResourceText(pair.Key, pair.Value.Amount);
        }

        _CheckBuyLimits();
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdatedResources", _OnUpdatedResources);
        EventManager.AddListener("UpdatedConstructors", _OnUpdatedConstructors);
        EventManager.AddListener("UpdatedPlacedBuildingPosition", _OnUpdatedPlacedBuildingPosition);
        EventManager.AddListener("HoveredSkillButton", _OnHoveredSkillButton);
        EventManager.AddListener("UnhoveredSkillButton", _OnUnhoveredSkillButton);
        EventManager.AddListener("SelectedUnit", _OnSelectedUnit);
        EventManager.AddListener("DeselectedUnit", _OnDeselectedUnit);
        EventManager.AddListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", _OnPlaceBuildingOff);
        EventManager.AddListener("SetPlayer", _OnSetPlayer);
        EventManager.AddListener("UpdatedUnitFormationType", _OnUpdatedUnitFormationType);
        EventManager.AddListener("HoveredTechnologyNode", _OnHoveredTechnologyNode);
        EventManager.AddListener("UnhoveredTechnologyNode", _OnUnhoveredTechnologyNode);
        EventManager.AddListener("StartedTechTreeNodeUnlock", _OnStartedTechTreeNodeUnlock);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdatedResources", _OnUpdatedResources);
        EventManager.RemoveListener("UpdatedConstructors", _OnUpdatedConstructors);
        EventManager.RemoveListener("UpdatedPlacedBuildingPosition", _OnUpdatedPlacedBuildingPosition);
        EventManager.RemoveListener("HoveredSkillButton", _OnHoveredSkillButton);
        EventManager.RemoveListener("UnhoveredSkillButton", _OnUnhoveredSkillButton);
        EventManager.RemoveListener("SelectedUnit", _OnSelectedUnit);
        EventManager.RemoveListener("DeselectedUnit", _OnDeselectedUnit);
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
        EventManager.RemoveListener("SetPlayer", _OnSetPlayer);
        EventManager.RemoveListener("UpdatedUnitFormationType", _OnUpdatedUnitFormationType);
        EventManager.RemoveListener("HoveredTechnologyNode", _OnHoveredTechnologyNode);
        EventManager.RemoveListener("UnhoveredTechnologyNode", _OnUnhoveredTechnologyNode);
        EventManager.RemoveListener("StartedTechTreeNodeUnlock", _OnStartedTechTreeNodeUnlock);
    }

    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }

    public void ToggleTechnologyTreePanel()
    {
        _showingTechTreePanel = !_showingTechTreePanel;
        if (_showingTechTreePanel && !_initializedTechnologyTree)
        {
            _techTreeProgressBars = TechnologyTreeVisualizer.DrawTree(
                _techTreeViewParent, _TECH_TREE_VIEW_WIDTH);
            _initializedTechnologyTree = true;
        }
        techTreePanel.SetActive(_showingTechTreePanel);
        EventManager.TriggerEvent(_showingTechTreePanel ? "PausedGame" : "ResumedGame");
    }

    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !gameSettingsPanel.activeSelf;
        gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PausedGame" : "ResumedGame");
    }

    public void ToggleMainMenuPanel()
    {
        bool showMainMenuPanel = !mainMenuPanel.activeSelf;
        mainMenuPanel.SetActive(showMainMenuPanel);
        EventManager.TriggerEvent(showMainMenuPanel ? "PausedGame" : "ResumedGame");
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

    public void SetUnitFormationType(int type)
    {
        Globals.UNIT_FORMATION_TYPE = (UnitFormationType)type;
        _OnUpdatedUnitFormationType();
    }

    public void LoadGame()
    {

    }

    public void SaveGame()
    {
        DataHandler.SaveGameData();
    }

    public void ResumeGame()
    {
        mainMenuPanel.SetActive(false);
        EventManager.TriggerEvent("ResumedGame");
    }

    public void QuitGame()
    {
        SaveGame();
        CoreBooter.instance.LoadMenu();
    }

    private void _AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void _CheckBuyLimits()
    {
        // check if level up button is disabled or not
        if (
            _selectedUnit != null &&
            _selectedUnit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId &&
            !_selectedUnit.LevelMaxedOut &&
            Globals.CanBuy(_selectedUnit.LevelUpData.cost)
        )
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = true;

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
                if (Globals.GAME_RESOURCES[_myPlayerId][resourceCode].Amount < resourceAmount)
                    txt.color = invalidTextColor;
                else
                    txt.color = validTextColor;
            }
        }

        // check if tech is affordable: update text colors
        if (_showingTechTreePanel)
        {
            foreach (Transform resourceDisplay in _techTreeCosts)
            {
                InGameResource resourceCode = (InGameResource)System.Enum.Parse(
                    typeof(InGameResource),
                    resourceDisplay.Find("Icon").GetComponent<Image>().sprite.name
                );
                Text txt = resourceDisplay.Find("Text").GetComponent<Text>();
                int resourceAmount = int.Parse(txt.text);
                if (Globals.GAME_RESOURCES[_myPlayerId][resourceCode].Amount < resourceAmount)
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

    private void _OnUpdatedResources()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerId])
            _SetResourceText(pair.Key, pair.Value.Amount);

        _CheckBuyLimits();
    }

    private void _OnUpdatedConstructors(object data)
    {
        _SetConstructionMenu((Building)data);
    }

    private void _OnUpdatedPlacedBuildingPosition(object data)
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
            g = GameObject.Instantiate(
                gameResourceCostPrefab,
                placedBuildingProductionRectTransform.transform);
            t = g.transform;
            t.Find("Text").GetComponent<Text>().text = $"+{pair.Value}";
            t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
        }

        // resize container to fit the right number of lines
        placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);

        // place container top-right of the "phantom" building
        placedBuildingProductionRectTransform.anchoredPosition =
            (Vector2) Camera.main.WorldToScreenPoint(pos) / GameManager.instance.canvasScaleFactor
            + Vector2.right * 60f;
    }

    private void _OnHoveredSkillButton(object data)
    {
        SkillData s = (SkillData)data;
        string title = s.skillName;
        string desc = s.description;
        List<ResourceValue> cost = new List<ResourceValue>();
        if (
            s.type == SkillType.INSTANTIATE_CHARACTER ||
            s.type == SkillType.INSTANTIATE_BUILDING
        )
        {
            float costReducerBuy =
                TechnologyNodeActioners.GetMultiplier("cost_reducer_buy");
            foreach (ResourceValue rv in s.unitReference.cost)
                cost.Add(new ResourceValue(
                    rv.code,
                    (int) (rv.amount * costReducerBuy)));
        }
        SetInfoPanel(title, desc, cost);
        ShowInfoPanel(true);
    }

    private void _OnUnhoveredSkillButton()
    {
        ShowInfoPanel(false);
    }

    private void _OnSelectedUnit(object data)
    {
        Unit unit = (Unit) data;
        AddSelectedUnitToUIList(unit);
        if (unit.IsAlive)
        {
            _SetSelectedUnitMenu(unit);
            _ShowSelectedUnitMenu(true);
        }
        else if (unit is Building b)
        {
            _SetConstructionMenu(b);
            _ShowConstructionMenu(true);
        }
    }

    private void _OnDeselectedUnit(object data)
    {
        Unit unit = (Unit) data;
        RemoveSelectedUnitFromUIList(unit.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
        {
            Unit u = Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit;
            if (u.IsAlive)
            {
                _SetSelectedUnitMenu(u);
                _ShowSelectedUnitMenu(true);
            }
            else if (u is Building b)
            {
                _SetConstructionMenu(b);
                _ShowConstructionMenu(true);
            }
        }
    }

    private void _OnPlaceBuildingOn()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(true);
    }

    private void _OnPlaceBuildingOff()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }

    private void _OnSetPlayer(object data)
    {
        int playerId = (int)data;
        _myPlayerId = playerId;
        Color c = GameManager.instance.gamePlayersParameters.players[_myPlayerId].color;
        c = Utils.LightenColor(c, 0.2f);
        playerIndicatorImage.color = c;
        _OnUpdatedResources();
        _CheckBuyLimits();
    }

    private void _OnUpdatedUnitFormationType()
    {
        for (int i = 0; i < 4; i++)
        {
            if ((int)Globals.UNIT_FORMATION_TYPE == i)
                _unitFormationTypeImages[i].color = _unitFormationTypeActiveColor;
            else
                _unitFormationTypeImages[i].color = _unitFormationTypeInactiveColor;
        }
    }

    private void _OnStartedTechTreeNodeUnlock(object data)
    {
        string nodeCode = (string)data;
        StartCoroutine(_UnlockingTechTreeNode(nodeCode));
    }

    private void _OnHoveredTechnologyNode(object data)
    {
        _SetTechTreeNodeCosts((TechnologyNodeData)data);
        _techTreeCosts.gameObject.SetActive(true);
    }

    private void _OnUnhoveredTechnologyNode()
    {
        _techTreeCosts.gameObject.SetActive(false);
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
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
                // check to see if resource requirement is not currently met -
                // in that case, turn the text into the "invalid" color
                if (Globals.GAME_RESOURCES[_myPlayerId][resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<Text>().color = invalidTextColor;
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
            GameObject g = GameObject.Instantiate(selectedUnitDisplayPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<Text>().text = "1";
            t.Find("Name").GetComponent<Text>().text = unit.Data.unitName;
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

    private void _SetConstructionMenu(Building building)
    {
        int nConstructors = building.Constructors.Count;
        for (int i = 0; i < 3; i++)
        {
            _constructionSlotButtons[i].gameObject.SetActive(i < nConstructors);
            _SetConstructionMenuSlotListener(
                _constructionSlotButtons[i], building, i);
        }
    }

    private void _SetConstructionMenuSlotListener(Button b, Building building, int i)
    {
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => { building.RemoveConstructor(i); });
    }

    private void _ShowConstructionMenu(bool show)
    {
        selectedUnitMenu.SetActive(false);
        constructionMenu.SetActive(show);
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
                g = GameObject.Instantiate(gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = showUpgrade
                    ? $"<color=#00ff00>+{_selectedUnit.LevelUpData.newProduction[resource.Key]}</color>"
                    : $"+{resource.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");
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
            g = GameObject.Instantiate(uiLabelPrefab, _selectedUnitAttackParametersParent);
            g.GetComponent<Text>().text = showUpgrade
                    ? $"Damage: <color=#00ff00>{_selectedUnit.LevelUpData.newAttackDamage}</color>"
                    : $"Damage: {unit.AttackDamage}";

            g = GameObject.Instantiate(uiLabelPrefab, _selectedUnitAttackParametersParent);
            g.GetComponent<Text>().text = showUpgrade
                    ? $"Range: <color=#00ff00>{(int) _selectedUnit.LevelUpData.newAttackRange}</color>"
                    : $"Range: {(int) unit.AttackRange}";
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
                g = GameObject.Instantiate(unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                g.GetComponent<SkillButton>().Initialize(unit.SkillManagers[i].skill);
                t.Find("Text").GetComponent<Text>().text = unit.SkillManagers[i].skill.skillName;
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
        constructionMenu.SetActive(false);
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

            g = GameObject.Instantiate(gameSettingsMenuButtonPrefab, gameSettingsMenusParent);
            n = parameters.GetParametersName();
            g.transform.Find("Text").GetComponent<Text>().text = n;
            _AddGameSettingsPanelMenuListener(g.GetComponent<Button>(), n);
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
        foreach (string fieldName in parameters.FieldsToShowInGame)
        {
            gWrapper = GameObject.Instantiate(gameSettingsParameterPrefab, gameSettingsContentParent);
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
                        ? (int)field.GetValue(parameters)
                        : (float)field.GetValue(parameters);
                    s.onValueChanged.AddListener(delegate
                    {
                        _OnGameSettingsSliderValueChanged(parameters, field, fieldName, s);
                    });
                }
            }
            else if (field.FieldType.IsArray && field.FieldType.GetElementType() == typeof(InputBinding))
            {
                gEditor = Instantiate(inputMappingPrefab);
                InputBinding[] bindings = (InputBinding[])field.GetValue(parameters);
                for (int b = 0; b < bindings.Length; b++)
                {
                    GameObject g = GameObject.Instantiate(inputBindingPrefab, gEditor.transform);
                    g.transform.Find("Text").GetComponent<Text>().text = bindings[b].displayName;
                    g.transform.Find("Key/Text").GetComponent<Text>().text = bindings[b].key;
                    _AddInputBindingButtonListener(
                        g.transform.Find("Key").GetComponent<Button>(),
                        gEditor.transform,
                        (GameInputParameters)parameters,
                        b
                    );
                }
            }
            if (gEditor != null)
                gEditor.transform.SetParent(gWrapper.transform, false);
        }
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

    private void _AddInputBindingButtonListener(Button b, Transform inputBindingsParent, GameInputParameters inputParams, int bindingIndex)
    {
        b.onClick.AddListener(() =>
        {
            Text keyText = b.transform.Find("Text").GetComponent<Text>();
            StartCoroutine(_WaitingForInputBinding(inputParams, inputBindingsParent, bindingIndex, keyText));
        });
    }

    private IEnumerator _WaitingForInputBinding(GameInputParameters inputParams, Transform inputBindingsParent, int bindingIndex, Text keyText)
    {
        keyText.text = "<?>";

        GameManager.instance.waitingForInput = true;
        GameManager.instance.pressedKey = string.Empty;

        yield return new WaitUntil(() => !GameManager.instance.waitingForInput);

        string key = GameManager.instance.pressedKey;

        // if input was already assign to another key,
        // the previous assignment is removed
        (int prevBindingIndex, InputBinding prevBinding) =
            GameManager.instance.gameInputParameters.GetBindingForKey(key);
        if (prevBinding != null)
        {
            prevBinding.key = string.Empty;
            inputBindingsParent.GetChild(prevBindingIndex).Find("Key/Text").GetComponent<Text>().text = string.Empty;
        }

        inputParams.bindings[bindingIndex].key = key;

        keyText.text = key;
    }

    private void _SetTechTreeNodeCosts(TechnologyNodeData node)
    {
        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _techTreeCosts)
            Destroy(child.gameObject);

        if (node.researchCost.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in node.researchCost)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _techTreeCosts);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
                // check to see if resource requirement is not currently met -
                // in that case, turn the text into the "invalid" color
                if (Globals.GAME_RESOURCES[_myPlayerId][resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<Text>().color = invalidTextColor;
            }
        }
    }

    private IEnumerator _UnlockingTechTreeNode(string code)
    {
        TechnologyNodeData node = TechnologyNodeData.TECH_TREE_NODES[code];
        int myPlayerId = GameManager.instance.gamePlayersParameters.myPlayerId;
        foreach (ResourceValue resource in node.researchCost)
            Globals.GAME_RESOURCES[myPlayerId][resource.code].AddAmount(-resource.amount);
        _OnUpdatedResources();

        float t = 0f;
        while (t < node.researchTime)
        {
            if (_showingTechTreePanel)
                _techTreeProgressBars[code].fillAmount = t / node.researchTime;
            t += Time.deltaTime;
            yield return null;
        }

        node.Unlock();

        // clear tech tree
        foreach (Transform child in _techTreeViewParent)
            Destroy(child.gameObject);

        // re-draw tree
        _techTreeProgressBars = TechnologyTreeVisualizer.DrawTree(
                _techTreeViewParent, _TECH_TREE_VIEW_WIDTH);
    }
}
