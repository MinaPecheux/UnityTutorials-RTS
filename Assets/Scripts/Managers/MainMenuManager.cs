using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static readonly Color[] _playerColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.magenta,
        Color.white,
        Color.gray,
    };

    [Header("Prefabs")]
    public GameObject menuScenePickPrefab;
    public GameObject playerPickerPrefab;

    [Header("UI")]
    public Transform newGameScrollview;
    public Image newGameDetailMapCapture;
    public Text newGameDetailInfoText;
    public Transform newGamePlayersList;
    public Button startGameButton;

    private MapData _selectedMap;
    private List<bool> _activePlayers;
    private Dictionary<int, PlayerData> _playersData;
    private List<Color> _availableColors;

    private void Start()
    {
        _PopulateMapsList();
    }

    #region New Game
    private void _PopulateMapsList()
    {
        MapData[] maps = Resources.LoadAll<MapData>("ScriptableObjects/Maps");
        Transform t; Sprite s;
        foreach (MapData map in maps)
        {
            GameObject g = Instantiate(menuScenePickPrefab, newGameScrollview);
            t = g.transform;
            s = Resources.Load<Sprite>($"MapCaptures/{map.sceneName}");
            t.Find("MapCapture").GetComponent<Image>().sprite = s;
            t.Find("Data/Name").GetComponent<Text>().text = map.mapName;
            t.Find("Data/Desc").GetComponent<Text>().text =
                $"{map.GetMapSizeType()} map, max {map.maxPlayers} players";
            _AddScenePickListener(g.GetComponent<Button>(), map, s);
        }
    }

    private void _SelectMap(MapData map, Sprite mapSprite)
    {
        _selectedMap = map;
        startGameButton.interactable = true;

        _availableColors = new List<Color>(_playerColors);

        foreach (Transform child in newGamePlayersList)
            Destroy(child.gameObject);

        newGameDetailMapCapture.sprite = mapSprite;
        newGameDetailInfoText.text = $"{map.mapName} <size=20>({map.mapSize}x{map.mapSize})</size>";

        _activePlayers = new List<bool>(map.maxPlayers);
        _playersData = new Dictionary<int, PlayerData>(map.maxPlayers);
        string name;
        for (int i = 0; i < map.maxPlayers; i++)
        {
            name = i == 0 ? "Player" : $"Enemy {i}";
            _activePlayers.Add(false);
            _playersData[i] = new PlayerData(name, _playerColors[i]);

            Transform player = Instantiate(playerPickerPrefab, newGamePlayersList).transform;
            player.Find("Name/InputField").GetComponent<InputField>().text = name;
            Image colorSprite = player.Find("Color/Content").GetComponent<Image>();
            colorSprite.color = _playersData[i].color;

            Transform colorsPicker = player.Find("Color/ColorsPicker");
            Transform picker;

            player.Find("Color/Content").GetComponent<Button>().onClick.AddListener(() =>
            {
                for (int j = 0; j < _playerColors.Length; j++)
                {
                    picker = colorsPicker.Find("Background").GetChild(j);
                    picker.GetComponent<Button>().interactable =
                        _availableColors.Contains(_playerColors[j]);
                }
                colorsPicker.gameObject.SetActive(true);
            });

            for (int j = 0; j < _playerColors.Length; j++)
            {
                picker = colorsPicker.Find("Background").GetChild(j);
                picker.GetComponent<Image>().color = _playerColors[j];
                _AddScenePickPlayerColorListener(
                    colorsPicker, colorSprite,
                    picker.GetComponent<Button>(),
                    i, j);
            }

            _AddScenePickPlayerInputListener(
                player.Find("Name/InputField").GetComponent<InputField>(),
                i);

            if (i <= 1)
            {
                player.Find("Toggle").GetComponent<Button>().interactable = false;
                _TogglePlayer(player, i);
            }
            else
            {
                _AddScenePickPlayerToggleListener(
                    player.Find("Toggle").GetComponent<Button>(),
                    player,
                    i);
            }
        }
    }

    private void _SetPlayerColor(
        Color color, int i, Image colorSprite, bool autoAdd = true)
    {
        if (autoAdd && _playersData[i].color != null)
            _availableColors.Add(_playersData[i].color);
        _availableColors.Remove(color);
        _playersData[i].color = color;
        colorSprite.color = color;
    }

    private void _SetPlayerName(string value, int i)
    {
        _playersData[i].name = value;
    }

    private void _TogglePlayer(Transform t, int i)
    {
        bool active = !_activePlayers[i];
        _activePlayers[i] = active;
        float from = active ? 42 : 0;
        float to = active ? 0 : 42;
        t.Find("Toggle/Checkmark").gameObject.SetActive(active);
        StartCoroutine(_TogglingPlayer(t.Find("Color/Block")
            .GetComponent<RectTransform>(), from, to, 42));
        StartCoroutine(_TogglingPlayer(t.Find("Name/Block")
            .GetComponent<RectTransform>(), from, to, 274));

        // check for player colors duplicates
        Color c = _playersData[i].color;
        if (active)
        {
            if (_availableColors.Contains(c))
                _availableColors.Remove(c);
            else
                _SetPlayerColor(
                    _availableColors[0], i,
                    t.Find("Color/Content").GetComponent<Image>(),
                    false);
        }
        else
        {
            if (!_availableColors.Contains(c))
                _availableColors.Add(c);
        }

        // check there are at least 2 players active
        startGameButton.interactable =
            _activePlayers.Where(a => a).Count() >= 2;
    }

    private IEnumerator _TogglingPlayer(RectTransform rt, float from, float to, float width)
    {
        rt.sizeDelta = new Vector2(to, from);
        float t = 0f;
        while (t < 0.5f)
        {
            rt.sizeDelta = new Vector2(Mathf.Lerp(from, to, t * 2f), width);
            t += Time.deltaTime;
            yield return null;
        }
        rt.sizeDelta = new Vector2(to, width);
    }

    private void _AddScenePickListener(Button b, MapData map, Sprite mapSprite)
    {
        b.onClick.AddListener(() => _SelectMap(map, mapSprite));
    }

    private void _AddScenePickPlayerColorListener(Transform colorsPicker, Image colorSprite, Button b, int i, int c)
    {
        b.onClick.AddListener(() => {
            _SetPlayerColor(_playerColors[c], i, colorSprite);
            colorsPicker.gameObject.SetActive(false);
        });
    }

    private void _AddScenePickPlayerInputListener(InputField f, int i)
    {
        f.onValueChanged.AddListener((string value) => _SetPlayerName(value, i));
    }

    private void _AddScenePickPlayerToggleListener(Button b, Transform t, int i)
    {
        b.onClick.AddListener(() => _TogglePlayer(t, i));
    }

    #endregion

}
