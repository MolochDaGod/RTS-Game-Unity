using UnityEngine;
using System.Collections.Generic;
using Grudge.Equipment;
using Grudge.Weapons;
using Grudge.Races;

/// <summary>
/// Extended hero unit with full Grudge equipment, weapon, and attribute systems
/// </summary>
public class GrudgeHeroUnit : UnitHero
{
    [Header("Hero Identity")]
    public RaceType race;
    public ClassType heroClass;
    public string heroName;
    public int heroLevel = 1;
    [Range(1, 10)]
    public int equipmentTier = 1;

    [Header("Equipment System")]
    public EquipmentItem[] equippedItems = new EquipmentItem[8];
    
    [Header("Weapon System")]
    public WeaponItem primaryWeapon;
    public WeaponItem secondaryWeapon;
    private GameObject primaryWeaponObject;
    private GameObject secondaryWeaponObject;

    [Header("Attribute System")]
    public AttributeStats currentAttributes = new AttributeStats();
    public int unspentAttributePoints = 0;
    public int attributePointsPerLevel = 7;

    [Header("Derived Stats")]
    public DerivedStats derivedStats;
    
    [Header("Set Bonuses")]
    private Dictionary<EquipmentSet, int> setCounters = new Dictionary<EquipmentSet, int>();
    private List<SetBonusDefinition> activeSetBonuses = new List<SetBonusDefinition>();

    [Header("Visual")]
    public Transform weaponSocketRight;
    public Transform weaponSocketLeft;
    public SkinnedMeshRenderer[] armorMeshRenderers;

    [Header("Databases")]
    public EquipmentDatabase equipmentDatabase;
    public WeaponDatabase weaponDatabase;
    public RaceDatabase raceDatabase;

    // Stat caching for performance
    private bool statsDirty = true;
    private float cachedMaxHP;
    private float cachedMaxMP;
    private float cachedDamage;
    private float cachedDefense;
    private float cachedAttackSpeed;

    protected override void Start()
    {
        base.Start();
        InitializeHero();
    }

    public void InitializeHero()
    {
        // Load race and class base attributes
        if (raceDatabase != null)
        {
            var raceDef = raceDatabase.GetRace(race);
            var classDef = raceDatabase.GetClass(heroClass);

            if (raceDef != null && classDef != null)
            {
                currentAttributes = new AttributeStats();
                currentAttributes.Add(raceDef.baseAttributes);
                currentAttributes.Add(classDef.baseAttributes);
            }
        }

        // Calculate unspent points based on level
        unspentAttributePoints = heroLevel * attributePointsPerLevel;

        RecalculateAllStats();
        UpdateVisuals();
    }

    #region Equipment Management

    public bool CanEquipItem(EquipmentItem item)
    {
        if (item == null) return false;
        
        // Check material restrictions based on class
        switch (heroClass)
        {
            case ClassType.MagePriest:
                return item.material == ArmorMaterial.Cloth || item.material == ArmorMaterial.Gem;
            case ClassType.RangerScout:
                return item.material == ArmorMaterial.Leather || item.material == ArmorMaterial.Cloth;
            case ClassType.Warrior:
                return item.material == ArmorMaterial.Metal || item.material == ArmorMaterial.Leather;
            case ClassType.WorgShapeshifter:
                return item.material == ArmorMaterial.Leather;
            default:
                return true;
        }
    }

    public void EquipItem(EquipmentItem item)
    {
        if (!CanEquipItem(item)) 
        {
            Debug.LogWarning($"Cannot equip {item.displayName} - material restriction for {heroClass}");
            return;
        }

        int slotIndex = (int)item.slot;
        
        // Unequip existing item in slot
        if (equippedItems[slotIndex] != null)
        {
            UnequipItem(item.slot);
        }

        equippedItems[slotIndex] = item;
        statsDirty = true;
        
        UpdateSetBonuses();
        RecalculateAllStats();
        UpdateEquipmentVisual(item);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        int slotIndex = (int)slot;
        if (equippedItems[slotIndex] == null) return;

        equippedItems[slotIndex] = null;
        statsDirty = true;
        
        UpdateSetBonuses();
        RecalculateAllStats();
        RemoveEquipmentVisual(slot);
    }

    public EquipmentItem GetEquippedItem(EquipmentSlot slot)
    {
        int slotIndex = (int)slot;
        return equippedItems[slotIndex];
    }

    #endregion

    #region Weapon Management

    public void EquipWeapon(WeaponItem weapon, bool isPrimary = true)
    {
        if (weapon == null) return;

        if (isPrimary)
        {
            if (primaryWeapon != null) UnequipWeapon(true);
            primaryWeapon = weapon;
            AttachWeaponModel(weapon, weaponSocketRight, ref primaryWeaponObject);
        }
        else
        {
            // Check if can dual wield
            if (weapon.category == WeaponCategory.TwoHand)
            {
                Debug.LogWarning("Cannot equip 2H weapon in offhand");
                return;
            }

            if (secondaryWeapon != null) UnequipWeapon(false);
            secondaryWeapon = weapon;
            AttachWeaponModel(weapon, weaponSocketLeft, ref secondaryWeaponObject);
        }

        statsDirty = true;
        RecalculateAllStats();
        UpdateWeaponSkills();
    }

    public void UnequipWeapon(bool isPrimary)
    {
        if (isPrimary)
        {
            primaryWeapon = null;
            if (primaryWeaponObject != null)
            {
                Destroy(primaryWeaponObject);
                primaryWeaponObject = null;
            }
        }
        else
        {
            secondaryWeapon = null;
            if (secondaryWeaponObject != null)
            {
                Destroy(secondaryWeaponObject);
                secondaryWeaponObject = null;
            }
        }

        statsDirty = true;
        RecalculateAllStats();
    }

    private void AttachWeaponModel(WeaponItem weapon, Transform socket, ref GameObject weaponObj)
    {
        if (weapon.weaponPrefab == null || socket == null) return;

        weaponObj = Instantiate(weapon.weaponPrefab, socket);
        weaponObj.transform.localPosition = weapon.attachmentPosition;
        weaponObj.transform.localRotation = Quaternion.Euler(weapon.attachmentRotation);
        weaponObj.transform.localScale = weapon.attachmentScale;
    }

    private void UpdateWeaponSkills()
    {
        // Map weapon abilities to skill slots
        // This will be handled by WeaponSkillMapper
        if (primaryWeapon != null && primaryWeapon.abilities != null && primaryWeapon.abilities.Length > 0)
        {
            // Update skill references based on weapon abilities
            // TODO: Integrate with existing skill system
        }
    }

    #endregion

    #region Stat Calculation

    public void RecalculateAllStats()
    {
        if (!statsDirty) return;

        // Calculate derived stats from attributes
        derivedStats = DerivedStats.CalculateFromAttributes(currentAttributes);

        // Add equipment bonuses
        float equipmentHP = 0;
        float equipmentMana = 0;
        float equipmentCrit = 0;
        float equipmentBlock = 0;
        float equipmentDefense = 0;

        foreach (var item in equippedItems)
        {
            if (item != null)
            {
                equipmentHP += item.stats.GetHP(equipmentTier);
                equipmentMana += item.stats.GetMana(equipmentTier);
                equipmentCrit += item.stats.GetCrit(equipmentTier);
                equipmentBlock += item.stats.GetBlock(equipmentTier);
                equipmentDefense += item.stats.GetDefense(equipmentTier);
            }
        }

        // Add weapon bonuses
        float weaponDamage = 0;
        float weaponSpeed = 0;
        float weaponCrit = 0;
        float weaponBlock = 0;
        float weaponDefense = 0;

        if (primaryWeapon != null)
        {
            weaponDamage += primaryWeapon.stats.GetDamage(equipmentTier);
            weaponSpeed += primaryWeapon.stats.GetSpeed(equipmentTier);
            weaponCrit += primaryWeapon.stats.GetCrit(equipmentTier);
            weaponBlock += primaryWeapon.stats.GetBlock(equipmentTier);
            weaponDefense += primaryWeapon.stats.GetDefense(equipmentTier);
        }

        if (secondaryWeapon != null)
        {
            weaponDamage += secondaryWeapon.stats.GetDamage(equipmentTier) * 0.5f; // Offhand penalty
            weaponCrit += secondaryWeapon.stats.GetCrit(equipmentTier);
            weaponBlock += secondaryWeapon.stats.GetBlock(equipmentTier);
        }

        // Apply set bonuses
        ApplySetBonusModifiers();

        // Cache final stats
        cachedMaxHP = maxHp + derivedStats.health + equipmentHP;
        cachedMaxMP = maxMp + derivedStats.mana + equipmentMana;
        cachedDamage = damage + derivedStats.physicalDamage + weaponDamage;
        cachedDefense = defense + derivedStats.physicalDefense + equipmentDefense + weaponDefense;
        cachedAttackSpeed = attackSpeed + derivedStats.attackSpeed + weaponSpeed;

        // Apply to base unit stats
        maxHp = cachedMaxHP;
        maxMp = cachedMaxMP;
        damage = cachedDamage;
        defense = cachedDefense;
        attackSpeed = cachedAttackSpeed;
        walkSpeed += derivedStats.moveSpeed;
        runSpeed += derivedStats.moveSpeed * 1.5f;
        hpRegen = derivedStats.hpRegen;
        mpRegen = derivedStats.manaRegen;

        // Clamp current HP/MP to new maximums
        if (hp > maxHp) hp = maxHp;
        if (mp > maxMp) mp = maxMp;

        statsDirty = false;
    }

    private void UpdateSetBonuses()
    {
        setCounters.Clear();
        activeSetBonuses.Clear();

        // Count equipment pieces per set
        foreach (var item in equippedItems)
        {
            if (item != null)
            {
                if (!setCounters.ContainsKey(item.equipmentSet))
                {
                    setCounters[item.equipmentSet] = 0;
                }
                setCounters[item.equipmentSet]++;
            }
        }

        // Activate set bonuses based on piece counts
        if (equipmentDatabase != null)
        {
            foreach (var kvp in setCounters)
            {
                var bonuses = equipmentDatabase.GetSetBonuses(kvp.Key);
                foreach (var bonus in bonuses)
                {
                    if (kvp.Value >= bonus.pieceCount)
                    {
                        activeSetBonuses.Add(bonus);
                    }
                }
            }
        }

        statsDirty = true;
    }

    private void ApplySetBonusModifiers()
    {
        foreach (var bonus in activeSetBonuses)
        {
            if (bonus.statModifiers != null)
            {
                foreach (var modifier in bonus.statModifiers)
                {
                    ApplyStatModifier(modifier);
                }
            }
        }
    }

    private void ApplyStatModifier(StatModifier modifier)
    {
        float value = modifier.value;
        
        switch (modifier.stat)
        {
            case StatModifier.StatType.HP:
                cachedMaxHP += modifier.isPercentage ? (cachedMaxHP * value / 100f) : value;
                break;
            case StatModifier.StatType.Mana:
                cachedMaxMP += modifier.isPercentage ? (cachedMaxMP * value / 100f) : value;
                break;
            case StatModifier.StatType.Damage:
                cachedDamage += modifier.isPercentage ? (cachedDamage * value / 100f) : value;
                break;
            case StatModifier.StatType.Defense:
                cachedDefense += modifier.isPercentage ? (cachedDefense * value / 100f) : value;
                break;
            case StatModifier.StatType.AttackSpeed:
                cachedAttackSpeed += modifier.isPercentage ? (cachedAttackSpeed * value / 100f) : value;
                break;
            // Add more stat types as needed
        }
    }

    public int GetPointCost()
    {
        int baseCost = 100;
        int levelCost = heroLevel * 10;
        int tierCost = equipmentTier * 50;
        int equipmentCost = GetEquipmentValue();

        return baseCost + levelCost + tierCost + equipmentCost;
    }

    private int GetEquipmentValue()
    {
        int value = 0;
        foreach (var item in equippedItems)
        {
            if (item != null)
            {
                value += item.craftingCost / 10;
            }
        }
        return value;
    }

    #endregion

    #region Attribute Allocation

    public bool AllocateAttribute(string attributeName, int points = 1)
    {
        if (unspentAttributePoints < points) return false;

        switch (attributeName.ToLower())
        {
            case "strength":
                currentAttributes.strength += points;
                break;
            case "intellect":
                currentAttributes.intellect += points;
                break;
            case "vitality":
                currentAttributes.vitality += points;
                break;
            case "dexterity":
                currentAttributes.dexterity += points;
                break;
            case "endurance":
                currentAttributes.endurance += points;
                break;
            case "wisdom":
                currentAttributes.wisdom += points;
                break;
            case "agility":
                currentAttributes.agility += points;
                break;
            case "tactics":
                currentAttributes.tactics += points;
                break;
            default:
                return false;
        }

        unspentAttributePoints -= points;
        statsDirty = true;
        RecalculateAllStats();
        return true;
    }

    public void GainLevel()
    {
        heroLevel++;
        unspentAttributePoints += attributePointsPerLevel;
        
        // Bonus HP/MP per level
        maxHp += 10f;
        maxMp += 5f;
        hp = maxHp;
        mp = maxMp;

        statsDirty = true;
        RecalculateAllStats();
    }

    #endregion

    #region Visual Updates

    private void UpdateVisuals()
    {
        // Update all equipment visuals
        foreach (var item in equippedItems)
        {
            if (item != null)
            {
                UpdateEquipmentVisual(item);
            }
        }

        // Update weapon visuals
        if (primaryWeapon != null && weaponSocketRight != null)
        {
            AttachWeaponModel(primaryWeapon, weaponSocketRight, ref primaryWeaponObject);
        }
        if (secondaryWeapon != null && weaponSocketLeft != null)
        {
            AttachWeaponModel(secondaryWeapon, weaponSocketLeft, ref secondaryWeaponObject);
        }
    }

    private void UpdateEquipmentVisual(EquipmentItem item)
    {
        if (item.prefabVisual != null)
        {
            // TODO: Attach equipment mesh to appropriate body part
            // This will be handled by EquipmentVisualController
        }

        if (item.materialOverride != null && armorMeshRenderers != null)
        {
            // Apply material to armor pieces
            foreach (var renderer in armorMeshRenderers)
            {
                renderer.material = item.materialOverride;
            }
        }
    }

    private void RemoveEquipmentVisual(EquipmentSlot slot)
    {
        // TODO: Remove equipment mesh from body part
    }

    #endregion

    #region Debug & Info

    public string GetStatsDebugString()
    {
        return $@"
=== {heroName} ({race} {heroClass}) ===
Level: {heroLevel} | Tier: {equipmentTier}
HP: {hp:F0}/{maxHp:F0} | MP: {mp:F0}/{maxMp:F0}
Damage: {damage:F1} | Defense: {defense:F1}
Attack Speed: {attackSpeed:F2} | Move Speed: {walkSpeed:F2}

Attributes:
STR: {currentAttributes.strength} | INT: {currentAttributes.intellect}
VIT: {currentAttributes.vitality} | DEX: {currentAttributes.dexterity}
END: {currentAttributes.endurance} | WIS: {currentAttributes.wisdom}
AGI: {currentAttributes.agility} | TAC: {currentAttributes.tactics}
Unspent: {unspentAttributePoints}

Equipped Sets:
{GetSetBonusString()}

Weapons:
Primary: {(primaryWeapon != null ? primaryWeapon.displayName : "None")}
Secondary: {(secondaryWeapon != null ? secondaryWeapon.displayName : "None")}
";
    }

    private string GetSetBonusString()
    {
        string result = "";
        foreach (var kvp in setCounters)
        {
            result += $"{kvp.Key}: {kvp.Value} pieces\n";
        }
        return result;
    }

    #endregion
}
