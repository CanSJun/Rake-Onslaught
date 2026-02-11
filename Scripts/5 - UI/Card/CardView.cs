using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frame;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _description;

    [SerializeField] private CanvasGroup _canvasGroup;

    public RectTransform Rect => (RectTransform)transform;

    public void Bind(CardData data)
    {
        _titleText.text = data.title;
        _description.text = data.description;
        _icon.enabled = data.icon != null;
        _icon.sprite = data.icon;
    }

    public void SetVisual(float alpha, bool interactable)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;
    }
}
