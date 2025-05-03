using UnityEngine;
using System.Collections.Generic;

public class Item
{
    public enum ItemType
    {
        None,
        Weapon,
        Armor,
        Helmet,
        Boots,
        Accessory,
        Consumable,
        Material
    }

    public string Name { get; private set; }
    public Sprite Icon { get; private set; }
    public ItemType Type { get; private set; }

    // Словарь для хранения иконок
    private static Dictionary<string, Sprite> itemIcons;

    public Item(string name, Sprite icon, ItemType type = ItemType.None)
    {
        Name = name;
        Icon = icon;
        Type = type;
    }

    // Инициализация словаря иконок
    public static void InitializeIcons(Sprite healingPotion, Sprite shieldPotion, Sprite manaCrystal, Sprite healthGem, Sprite ironArmor, Sprite steelHelmet, Sprite leatherBoots)
    {
        itemIcons = new Dictionary<string, Sprite>
        {
            { "Healing Potion", healingPotion },
            { "Shield Potion", shieldPotion },
            { "Mana Crystal", manaCrystal },
            { "Health Gem", healthGem },
            { "Iron Armor", ironArmor },
            { "Steel Helmet", steelHelmet },
            { "Leather Boots", leatherBoots }
        };
    }

    // Метод для восстановления предмета после десериализации
    public static Item FromSerialized(string name, string type)
    {
        ItemType itemType;
        if (System.Enum.TryParse(type, out itemType))
        {
            Sprite icon = itemIcons != null && itemIcons.ContainsKey(name) ? itemIcons[name] : null;
            return new Item(name, icon, itemType);
        }
        Debug.LogError($"Не удалось десериализовать тип предмета: {type}");
        return new Item(name, null, ItemType.None); // Значение по умолчанию
    }
}