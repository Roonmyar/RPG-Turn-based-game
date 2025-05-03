using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    public Item.ItemType SlotType { get; private set; }
    public Item EquippedItem { get; private set; }
    private GameUIController uiController;
    private Button unequipButton;

    public void Initialize(Item.ItemType slotType, GameUIController controller, Button unequipButton)
    {
        SlotType = slotType;
        uiController = controller;
        this.unequipButton = unequipButton;

        if (unequipButton != null)
        {
            unequipButton.onClick.AddListener(OnUnequipClicked);
            unequipButton.gameObject.SetActive(false); // Исправлено: uneqipButton на unequipButton
            Debug.Log($"Кнопка Unequip настроена для слота {SlotType}");
        }
        else
        {
            Debug.LogWarning($"UnequipButton не найден для слота {SlotType}");
        }

        UpdateUI();
    }

    public void SetItem(Item item)
    {
        EquippedItem = item;
        Debug.Log($"Установлен предмет {item?.Name ?? "null"} в слот {SlotType}");
        UpdateUI();
    }

    private void UpdateUI()
    {
        Image iconImage = transform.Find("SlotIcon")?.GetComponent<Image>();
        TMPro.TMP_Text nameText = transform.Find("SlotName")?.GetComponent<TMPro.TMP_Text>();

        if (EquippedItem != null)
        {
            if (iconImage != null) iconImage.sprite = EquippedItem.Icon;
            else Debug.LogWarning($"SlotIcon не найден для слота {SlotType}");
            if (nameText != null) nameText.text = $"{SlotType}: {EquippedItem.Name}";
            else Debug.LogWarning($"SlotName не найден для слота {SlotType}");
            if (unequipButton != null) unequipButton.gameObject.SetActive(true);
        }
        else
        {
            if (iconImage != null) iconImage.sprite = null;
            else Debug.LogWarning($"SlotIcon не найден для слота {SlotType}");
            if (nameText != null) nameText.text = $"{SlotType}: None";
            else Debug.LogWarning($"SlotName не найден для слота {SlotType}");
            if (unequipButton != null) unequipButton.gameObject.SetActive(false);
        }
    }

    private void OnUnequipClicked()
    {
        if (EquippedItem != null)
        {
            uiController.UnequipItem(this);
        }
    }
}