using UnityEngine;
using UnityEngine.UI;

public class TechnologyNodeVisualizer
{
    private static GameObject _uiPrefab;
    private static GameObject _UI_PREFAB
    {
        get
        {
            if (_uiPrefab == null)
            {
                _uiPrefab = Resources.Load<GameObject>(
                    "Prefabs/UI/TechnologyTree/UITechnologyTreeNode");
            }
            return _uiPrefab;
        }
    }

    private static Color _UNLOCKED_COLOR = new Color(1f, 1f, 0.3f, 1f);

    private TechnologyNodeData _node;
    private Rect _rect;
    private Image _progressBar;
    public Image ProgressBar => _progressBar;

    public TechnologyNodeVisualizer(TechnologyNodeData node, int depth)
    {
        _node = node;

        (Vector2 position, Vector2 size) =
            TechnologyTreeLayouter.InitializeNodeVisualizer(depth);
        _rect = new Rect(position, size);
    }

    public float X
    {
        get => _rect.position.x;
        set
        {
            Vector2 v = _rect.position;
            v.x = value;
            _rect.position = v;
        }
    }
    public float Y
    {
        get => _rect.position.y;
    }
    public float Mod { get; set; }

    public void Draw(Transform parent, float xOffset)
    {
        GameObject g = GameObject.Instantiate(_UI_PREFAB, parent);
        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchoredPosition =
            new Vector2(_rect.position.x + xOffset, -_rect.position.y);
        rt.sizeDelta = _rect.size;
        g.transform.Find("Fill/Name").GetComponent<Text>().text =
            _node.displayName;

        if (!_node.Unlocked)
            g.GetComponent<TechnologyNodeButton>().Initialize(_node);
        else
            Object.Destroy(g.GetComponent<TechnologyNodeButton>());

        Button b = g.GetComponent<Button>();
        _progressBar = g.transform.Find("ProgressBar/Fill").GetComponent<Image>();

        if (_node.Unlocked)
        {
            g.GetComponent<Image>().color = _UNLOCKED_COLOR;
            b.interactable = false;
            ColorBlock cb = b.colors;
            cb.disabledColor = Color.white;
            b.colors = cb;
        }
        else if (!_node.Parent.Unlocked)
        {
            b.interactable = false;
        }

        if (b.interactable)
            b.onClick.AddListener(() =>
            {
                if (_node.Unlocked) return;
                if (!Globals.CanBuy(_node.researchCost)) return;
                g.transform.Find("ProgressBar").gameObject.SetActive(true);
                EventManager.TriggerEvent("StartedTechTreeNodeUnlock", _node.code);
            });
    }
}
