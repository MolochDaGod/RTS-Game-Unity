using UnityEngine;
using System.Collections.Generic;
using Grudge.Equipment;
using Grudge.Weapons;
using Grudge.Races;

namespace Grudge.Factory
{
    /// <summary>
    /// Factory for creating Grudge heroes, elite units, and common units
    /// </summary>
    public class GrudgeUnitFactory : MonoBehaviour
    {
        [Header("Databases")]
        public EquipmentDatabase equipmentDatabase;
        public WeaponDatabase weaponDatabase;
        public RaceDatabase raceDatabase;

        [Header("Prefab Templates")]
        public GameObject heroUnitPrefab;
        public GameObject eliteUnitPrefab;
        public GameObject commonUnitPrefab;

        [Header("Spawn Settings")]
        public Transform spawnParent;
        public bool autoInitialize = true;

        private static GrudgeUnitFactory instance;
        public static GrudgeUnitFactory Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (autoInitialize)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (equipmentDatabase != null) equipmentDatabase.Initialize();
            if (weaponDatabase != null) weaponDatabase.Initialize();
            if (raceDatabase != null) raceDatabase.Initialize();
        }

        #region Hero Creation

        /// <summary>
        /// Create a fully customizable hero unit
        /// </summary>
        public GrudgeHeroUnit CreateHero(
            RaceType race,
            ClassType heroClass,
            string heroName,
            int level = 1,
            int tier = 1,
            Vector3 position = default)
        {
            if (heroUnitPrefab == null)
            {
                Debug.LogError("Hero unit prefab not assigned!");
                return null;
            }

            GameObject heroObj = Instantiate(heroUnitPrefab, position, Quaternion.identity, spawnParent);
            GrudgeHeroUnit hero = heroObj.GetComponent<GrudgeHeroUnit>();

            if (hero == null)
            {
                hero = heroObj.AddComponent<GrudgeHeroUnit>();
            }

            // Set hero identity
            hero.race = race;
            hero.heroClass = heroClass;
            hero.heroName = heroName;
            hero.heroLevel = level;
            hero.equipmentTier = tier;

            // Set databases
            hero.equipmentDatabase = equipmentDatabase;
            hero.weaponDatabase = weaponDatabase;
            hero.raceDatabase = raceDatabase;

            // Initialize hero stats
            hero.InitializeHero();

            return hero;
        }

        /// <summary>
        /// Create hero with pre-configured loadout
        /// </summary>
        public GrudgeHeroUnit CreateHeroWithLoadout(
            HeroLoadout loadout,
            Vector3 position = default)
        {
            var hero = CreateHero(
                loadout.race,
                loadout.heroClass,
                loadout.heroName,
                loadout.level,
                loadout.tier,
                position
            );

            if (hero == null) return null;

            // Equip items
            if (loadout.equipmentIds != null)
            {
                for (int i = 0; i < loadout.equipmentIds.Length; i++)
                {
                    if (string.IsNullOrEmpty(loadout.equipmentIds[i])) continue;
                    
                    var item = equipmentDatabase.GetEquipmentById(loadout.equipmentIds[i]);
                    if (item != null)
                    {
                        hero.EquipItem(item);
                    }
                }
            }

            // Equip weapons
            if (!string.IsNullOrEmpty(loadout.primaryWeaponId))
            {
                var weapon = weaponDatabase.GetWeaponById(loadout.primaryWeaponId);
                if (weapon != null) hero.EquipWeapon(weapon, true);
            }

            if (!string.IsNullOrEmpty(loadout.secondaryWeaponId))
            {
                var weapon = weaponDatabase.GetWeaponById(loadout.secondaryWeaponId);
                if (weapon != null) hero.EquipWeapon(weapon, false);
            }

            // Allocate attributes
            if (loadout.attributes != null)
            {
                hero.currentAttributes = new AttributeStats(
                    loadout.attributes.strength,
                    loadout.attributes.intellect,
                    loadout.attributes.vitality,
                    loadout.attributes.dexterity,
                    loadout.attributes.endurance,
                    loadout.attributes.wisdom,
                    loadout.attributes.agility,
                    loadout.attributes.tactics
                );
                hero.RecalculateAllStats();
            }

            return hero;
        }

        /// <summary>
        /// Create a random hero for PvE content
        /// </summary>
        public GrudgeHeroUnit CreateRandomHero(
            int level,
            int tier,
            Vector3 position = default,
            Faction? forceFaction = null)
        {
            // Random race
            RaceType race;
            if (forceFaction.HasValue)
            {
                var factionRaces = raceDatabase.GetRacesByFaction(forceFaction.Value);
                race = factionRaces[Random.Range(0, factionRaces.Count)].raceType;
            }
            else
            {
                race = (RaceType)Random.Range(0, System.Enum.GetValues(typeof(RaceType)).Length);
            }

            // Random class
            ClassType heroClass = (ClassType)Random.Range(0, System.Enum.GetValues(typeof(ClassType)).Length);

            // Generate random name
            string[] namePrefi= { "Grim", "Dark", "Blood", "Shadow", "Iron", "Wrath", "Dusk", "Ember" };
            string[] nameSuffix = { "fang", "bane", "slayer", "guard", "blade", "heart", "fury", "storm" };
            string heroName = namePrefix[Random.Range(0, namePrefix.Length)] + 
                            nameSuffix[Random.Range(0, nameSuffix.Length)];

            var hero = CreateHero(race, heroClass, heroName, level, tier, position);

            // Equip random gear appropriate for tier
            EquipRandomGear(hero, tier);

            return hero;
        }

        private void EquipRandomGear(GrudgeHeroUnit hero, int tier)
        {
            if (hero == null) return;

            // Determine armor material based on class
            ArmorMaterial material = ArmorMaterial.Leather;
            switch (hero.heroClass)
            {
                case ClassType.MagePriest:
                    material = ArmorMaterial.Cloth;
                    break;
                case ClassType.Warrior:
                    material = ArmorMaterial.Metal;
                    break;
                case ClassType.RangerScout:
                case ClassType.WorgShapeshifter:
                    material = ArmorMaterial.Leather;
                    break;
            }

            // Pick random set
            EquipmentSet randomSet = (EquipmentSet)Random.Range(0, System.Enum.GetValues(typeof(EquipmentSet)).Length);

            // Equip full set
            var setItems = equipmentDatabase.GetEquipmentBySet(randomSet);
            foreach (var item in setItems)
            {
                if (item.material == material)
                {
                    hero.EquipItem(item);
                }
            }

            // Equip random weapon appropriate for class
            WeaponType weaponType = GetRandomWeaponForClass(hero.heroClass);
            var weapons = weaponDatabase.GetWeaponsByType(weaponType);
            if (weapons.Count > 0)
            {
                var randomWeapon = weapons[Random.Range(0, weapons.Count)];
                hero.EquipWeapon(randomWeapon, true);
            }
        }

        private WeaponType GetRandomWeaponForClass(ClassType classType)
        {
            switch (classType)
            {
                case ClassType.Warrior:
                    WeaponType[] warriorWeapons = { WeaponType.Sword, WeaponType.Axe, WeaponType.Hammer1h, 
                                                   WeaponType.Greatsword, WeaponType.Greataxe, WeaponType.Hammer2h };
                    return warriorWeapons[Random.Range(0, warriorWeapons.Length)];
                
                case ClassType.MagePriest:
                    WeaponType[] mageWeapons = { WeaponType.FireStaff, WeaponType.FrostStaff, WeaponType.NatureStaff,
                                                WeaponType.HolyStaff, WeaponType.ArcaneStaff, WeaponType.LightningStaff };
                    return mageWeapons[Random.Range(0, mageWeapons.Length)];
                
                case ClassType.RangerScout:
                    WeaponType[] rangerWeapons = { WeaponType.Bow, WeaponType.Crossbow, WeaponType.Gun,
                                                  WeaponType.Dagger };
                    return rangerWeapons[Random.Range(0, rangerWeapons.Length)];
                
                case ClassType.WorgShapeshifter:
                    WeaponType[] worgWeapons = { WeaponType.Axe, WeaponType.Dagger };
                    return worgWeapons[Random.Range(0, worgWeapons.Length)];
                
                default:
                    return WeaponType.Sword;
            }
        }

        #endregion

        #region Common Unit Creation

        /// <summary>
        /// Create a basic common unit (soldier)
        /// </summary>
        public Unit CreateCommonUnit(
            RaceType race,
            WeaponType weaponType,
            int upgradeLevel = 0,
            Vector3 position = default)
        {
            if (commonUnitPrefab == null)
            {
                Debug.LogError("Common unit prefab not assigned!");
                return null;
            }

            GameObject unitObj = Instantiate(commonUnitPrefab, position, Quaternion.identity, spawnParent);
            Unit unit = unitObj.GetComponent<Unit>();

            if (unit == null)
            {
                unit = unitObj.AddComponent<Unit>();
            }

            // Set base stats based on race
            ApplyRaceStatsToUnit(unit, race, upgradeLevel);

            return unit;
        }

        private void ApplyRaceStatsToUnit(Unit unit, RaceType race, int upgradeLevel)
        {
            var raceDef = raceDatabase.GetRace(race);
            if (raceDef == null) return;

            // Base stats
            unit.maxHp = 100 + (upgradeLevel * 20);
            unit.hp = unit.maxHp;
            unit.damage = 10 + (upgradeLevel * 3);
            unit.defense = 5 + (upgradeLevel * 2);
            unit.attackSpeed = 1.0f;
            unit.walkSpeed = 5f;
            unit.runSpeed = 8f;

            // Apply racial modifiers
            var attrs = raceDef.baseAttributes;
            unit.maxHp += attrs.vitality * 10;
            unit.damage += attrs.strength * 2;
            unit.defense += attrs.endurance;
            unit.attackSpeed += attrs.dexterity * 0.05f;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get army composition point cost
        /// </summary>
        public int CalculateArmyPointCost(List<GrudgeHeroUnit> heroes, List<Unit> units)
        {
            int cost = 0;
            
            foreach (var hero in heroes)
            {
                if (hero != null) cost += hero.GetPointCost();
            }

            foreach (var unit in units)
            {
                if (unit != null) cost += GetUnitPointCost(unit);
            }

            return cost;
        }

        private int GetUnitPointCost(Unit unit)
        {
            return 25 + (unit.tier * 10);
        }

        #endregion
    }

    #region Supporting Classes

    [System.Serializable]
    public class HeroLoadout
    {
        public string heroName;
        public RaceType race;
        public ClassType heroClass;
        public int level = 1;
        public int tier = 1;
        public AttributeStats attributes;
        public string[] equipmentIds = new string[8];
        public string primaryWeaponId;
        public string secondaryWeaponId;
    }

    #endregion
}
