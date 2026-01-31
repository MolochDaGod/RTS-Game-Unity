using UnityEngine;
using System.Collections.Generic;

namespace Grudge.Equipment
{
    public enum EquipmentSet
    {
        Bloodfeud,
        Wraithfang,
        Oathbreaker,
        Kinrend,
        Dusksinger,
        Emberclad
    }

    public enum EquipmentSlot
    {
        Helm = 0,
        Shoulder = 1,
        Chest = 2,
        Hands = 3,
        Feet = 4,
        Ring = 5,
        Necklace = 6,
        Relic = 7
    }

    public enum ArmorMaterial
    {
        Cloth,
        Leather,
        Metal,
        Gem
    }

    [System.Serializable]
    public class EquipmentStats
    {
        public float hpBase;
        public float hpPerTier;
        public float manaBase;
        public float manaPerTier;
        public float critBase;
        public float critPerTier;
        public float blockBase;
        public float blockPerTier;
        public float defenseBase;
        public float defensePerTier;

        public float GetHP(int tier) => hpBase + (hpPerTier * tier);
        public float GetMana(int tier) => manaBase + (manaPerTier * tier);
        public float GetCrit(int tier) => critBase + (critPerTier * tier);
        public float GetBlock(int tier) => blockBase + (blockPerTier * tier);
        public float GetDefense(int tier) => defenseBase + (defensePerTier * tier);
    }

    [CreateAssetMenu(fileName = "New Equipment Item", menuName = "Grudge/Equipment Item")]
    public class EquipmentItem : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public EquipmentSlot slot;
        public ArmorMaterial material;
        public EquipmentSet equipmentSet;

        [Header("Lore")]
        [TextArea(3, 6)]
        public string lore;

        [Header("Stats")]
        public EquipmentStats stats;

        [Header("Effects")]
        [TextArea(2, 4)]
        public string passive;
        public string attribute;
        [TextArea(2, 4)]
        public string effect;
        [TextArea(2, 4)]
        public string proc;
        [TextArea(2, 4)]
        public string setBonus;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefabVisual;
        public Material materialOverride;
        
        [Header("Crafting")]
        public int craftingCost = 100;
        public string[] requiredMaterials;
    }

    [System.Serializable]
    public class SetBonusDefinition
    {
        public EquipmentSet set;
        public int pieceCount; // 2, 4, 6, or 8
        public string bonusDescription;
        public StatModifier[] statModifiers;
        public string specialEffect;
    }

    [System.Serializable]
    public class StatModifier
    {
        public enum StatType
        {
            HP,
            Mana,
            Damage,
            Defense,
            CritChance,
            CritDamage,
            AttackSpeed,
            MoveSpeed,
            BlockChance,
            Lifesteal,
            ManaRegen,
            HPRegen
        }

        public StatType stat;
        public float value;
        public bool isPercentage;
    }

    [CreateAssetMenu(fileName = "Equipment Database", menuName = "Grudge/Equipment Database")]
    public class EquipmentDatabase : ScriptableObject
    {
        public List<EquipmentItem> allEquipment;
        public List<SetBonusDefinition> setBonuses;

        private Dictionary<string, EquipmentItem> equipmentLookup;

        public void Initialize()
        {
            equipmentLookup = new Dictionary<string, EquipmentItem>();
            foreach (var item in allEquipment)
            {
                if (!equipmentLookup.ContainsKey(item.id))
                {
                    equipmentLookup[item.id] = item;
                }
            }
        }

        public EquipmentItem GetEquipmentById(string id)
        {
            if (equipmentLookup == null) Initialize();
            return equipmentLookup.ContainsKey(id) ? equipmentLookup[id] : null;
        }

        public List<EquipmentItem> GetEquipmentBySet(EquipmentSet set)
        {
            return allEquipment.FindAll(e => e.equipmentSet == set);
        }

        public List<EquipmentItem> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return allEquipment.FindAll(e => e.slot == slot);
        }

        public List<SetBonusDefinition> GetSetBonuses(EquipmentSet set)
        {
            return setBonuses.FindAll(b => b.set == set);
        }
    }
}
