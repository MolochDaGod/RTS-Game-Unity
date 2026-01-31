using UnityEngine;
using System.Collections.Generic;

namespace Grudge.Weapons
{
    public enum WeaponType
    {
        // 1H Melee
        Sword,
        Axe,
        Dagger,
        Hammer1h,
        
        // 2H Melee
        Greatsword,
        Greataxe,
        Hammer2h,
        
        // Ranged 2H
        Bow,
        Crossbow,
        Gun,
        
        // Staves
        FireStaff,
        FrostStaff,
        NatureStaff,
        HolyStaff,
        ArcaneStaff,
        LightningStaff,
        
        // Tomes (1H Magic)
        FireTome,
        FrostTome,
        NatureTome,
        HolyTome,
        ArcaneTome,
        LightningTome
    }

    public enum WeaponCategory
    {
        OneHand,
        TwoHand,
        Ranged2H
    }

    public enum CraftingProfession
    {
        Miner,
        Forester,
        Engineer,
        Mystic
    }

    [System.Serializable]
    public class WeaponStats
    {
        public float damageBase;
        public float damagePerTier;
        public float speedBase;
        public float speedPerTier;
        public float comboBase;
        public float comboPerTier;
        public float critBase;
        public float critPerTier;
        public float blockBase;
        public float blockPerTier;
        public float defenseBase;
        public float defensePerTier;

        public float GetDamage(int tier) => damageBase + (damagePerTier * tier);
        public float GetSpeed(int tier) => speedBase + (speedPerTier * tier);
        public float GetCombo(int tier) => comboBase + (comboPerTier * tier);
        public float GetCrit(int tier) => critBase + (critPerTier * tier);
        public float GetBlock(int tier) => blockBase + (blockPerTier * tier);
        public float GetDefense(int tier) => defenseBase + (defensePerTier * tier);
    }

    [System.Serializable]
    public class WeaponAbility
    {
        public string abilityName;
        [TextArea(2, 4)]
        public string description;
        public float cooldown = 5f;
        public float manaCost = 20f;
        public Sprite icon;
    }

    [CreateAssetMenu(fileName = "New Weapon", menuName = "Grudge/Weapon Item")]
    public class WeaponItem : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public WeaponType weaponType;
        public WeaponCategory category;

        [Header("Lore")]
        [TextArea(3, 6)]
        public string lore;

        [Header("Stats")]
        public WeaponStats stats;

        [Header("Abilities")]
        [TextArea(2, 4)]
        public string basicAbility;
        
        [Tooltip("Selectable abilities for hotkey slots")]
        public WeaponAbility[] abilities;
        
        [TextArea(2, 4)]
        public string signatureAbility;

        [Header("Passives")]
        [TextArea(2, 4)]
        public string[] passives;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject weaponPrefab;
        public RuntimeAnimatorController animatorController;
        
        [Header("Weapon Attachment")]
        public string boneAttachmentName = "WeaponSocket";
        public Vector3 attachmentPosition;
        public Vector3 attachmentRotation;
        public Vector3 attachmentScale = Vector3.one;

        [Header("Crafting")]
        public CraftingProfession craftedBy;
        public int craftingCost = 200;
        public string[] requiredMaterials;

        [Header("Effects")]
        public GameObject trailEffect;
        public GameObject hitEffect;
        public AudioClip swingSound;
        public AudioClip hitSound;
    }

    [CreateAssetMenu(fileName = "Weapon Database", menuName = "Grudge/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        public List<WeaponItem> allWeapons;

        private Dictionary<string, WeaponItem> weaponLookup;
        private Dictionary<WeaponType, List<WeaponItem>> weaponsByType;

        public void Initialize()
        {
            weaponLookup = new Dictionary<string, WeaponItem>();
            weaponsByType = new Dictionary<WeaponType, List<WeaponItem>>();

            foreach (var weapon in allWeapons)
            {
                if (!weaponLookup.ContainsKey(weapon.id))
                {
                    weaponLookup[weapon.id] = weapon;
                }

                if (!weaponsByType.ContainsKey(weapon.weaponType))
                {
                    weaponsByType[weapon.weaponType] = new List<WeaponItem>();
                }
                weaponsByType[weapon.weaponType].Add(weapon);
            }
        }

        public WeaponItem GetWeaponById(string id)
        {
            if (weaponLookup == null) Initialize();
            return weaponLookup.ContainsKey(id) ? weaponLookup[id] : null;
        }

        public List<WeaponItem> GetWeaponsByType(WeaponType type)
        {
            if (weaponsByType == null) Initialize();
            return weaponsByType.ContainsKey(type) ? weaponsByType[type] : new List<WeaponItem>();
        }

        public List<WeaponItem> GetWeaponsByCategory(WeaponCategory category)
        {
            return allWeapons.FindAll(w => w.category == category);
        }

        public List<WeaponItem> GetWeaponsByCraftingProfession(CraftingProfession profession)
        {
            return allWeapons.FindAll(w => w.craftedBy == profession);
        }
    }
}
