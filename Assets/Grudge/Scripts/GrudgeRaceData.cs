using UnityEngine;
using System.Collections.Generic;

namespace Grudge.Races
{
    public enum Faction
    {
        Crusade,
        Legion,
        Fabled
    }

    public enum RaceType
    {
        Human,
        Barbarian,
        Undead,
        Orc,
        Elf,
        Dwarf
    }

    public enum ClassType
    {
        WorgShapeshifter,
        Warrior,
        MagePriest,
        RangerScout
    }

    [System.Serializable]
    public class AttributeStats
    {
        public int strength;
        public int intellect;
        public int vitality;
        public int dexterity;
        public int endurance;
        public int wisdom;
        public int agility;
        public int tactics;

        public AttributeStats()
        {
            strength = intellect = vitality = dexterity = 0;
            endurance = wisdom = agility = tactics = 0;
        }

        public AttributeStats(int str, int intel, int vit, int dex, int end, int wis, int agi, int tac)
        {
            strength = str;
            intellect = intel;
            vitality = vit;
            dexterity = dex;
            endurance = end;
            wisdom = wis;
            agility = agi;
            tactics = tac;
        }

        public void Add(AttributeStats other)
        {
            strength += other.strength;
            intellect += other.intellect;
            vitality += other.vitality;
            dexterity += other.dexterity;
            endurance += other.endurance;
            wisdom += other.wisdom;
            agility += other.agility;
            tactics += other.tactics;
        }

        public int GetTotal()
        {
            return strength + intellect + vitality + dexterity + 
                   endurance + wisdom + agility + tactics;
        }
    }

    [System.Serializable]
    public class DerivedStats
    {
        public float health;
        public float mana;
        public float physicalDamage;
        public float magicalDamage;
        public float physicalDefense;
        public float magicalDefense;
        public float critChance;
        public float attackSpeed;
        public float moveSpeed;
        public float hpRegen;
        public float manaRegen;
        public float blockChance;
        public float evasion;
        public float accuracy;

        public static DerivedStats CalculateFromAttributes(AttributeStats attributes)
        {
            DerivedStats stats = new DerivedStats();
            
            // Health calculation
            stats.health = (attributes.strength * 5f) + (attributes.vitality * 25f);
            
            // Mana calculation
            stats.mana = (attributes.intellect * 9f) + (attributes.wisdom * 6f);
            
            // Damage
            stats.physicalDamage = attributes.strength * 1.25f;
            stats.magicalDamage = attributes.intellect * 1.5f;
            
            // Defense
            stats.physicalDefense = (attributes.strength * 4f) + (attributes.vitality * 1.5f) + (attributes.endurance * 5f);
            stats.magicalDefense = (attributes.intellect * 2f) + (attributes.wisdom * 5.5f);
            
            // Combat stats
            stats.critChance = attributes.dexterity * 0.3f;
            stats.attackSpeed = attributes.dexterity * 0.2f;
            stats.moveSpeed = attributes.agility * 0.15f;
            stats.blockChance = attributes.endurance * 0.175f;
            stats.evasion = attributes.agility * 0.225f;
            stats.accuracy = attributes.dexterity * 0.25f;
            
            // Regen
            stats.hpRegen = attributes.vitality * 0.06f;
            stats.manaRegen = attributes.wisdom * 0.1f;
            
            return stats;
        }
    }

    [CreateAssetMenu(fileName = "New Race", menuName = "Grudge/Race Definition")]
    public class RaceDefinition : ScriptableObject
    {
        [Header("Identity")]
        public RaceType raceType;
        public string displayName;
        public Faction faction;

        [Header("Description")]
        [TextArea(3, 6)]
        public string description;

        [Header("Base Stats")]
        public AttributeStats baseAttributes;

        [Header("Visuals")]
        public Sprite portrait;
        public GameObject[] basePrefabs;
        public GameObject[] mountedPrefabs;
        public GameObject[] upgradedPrefabs;
        public string spriteSetName;

        [Header("Racial Abilities")]
        [TextArea(2, 4)]
        public string racialPassive;
        [TextArea(2, 4)]
        public string racialAbility;

        [Header("Voice & Audio")]
        public AudioClip[] selectionSounds;
        public AudioClip[] movementSounds;
        public AudioClip[] attackSounds;
        public AudioClip[] deathSounds;
    }

    [CreateAssetMenu(fileName = "New Class", menuName = "Grudge/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        [Header("Identity")]
        public ClassType classType;
        public string displayName;
        public string role;

        [Header("Description")]
        [TextArea(4, 8)]
        public string description;

        [Header("Base Stats")]
        public AttributeStats baseAttributes;

        [Header("Starting Equipment")]
        public string startingWeaponType;
        public string[] startingAbilities;

        [Header("Visuals")]
        public Sprite icon;
        public string spriteSetOverride;
        public RuntimeAnimatorController animatorController;

        [Header("Progression")]
        public int baseHP = 100;
        public int baseMana = 100;
        public float hpPerLevel = 10f;
        public float manaPerLevel = 5f;
    }

    [CreateAssetMenu(fileName = "Race Database", menuName = "Grudge/Race Database")]
    public class RaceDatabase : ScriptableObject
    {
        public List<RaceDefinition> allRaces;
        public List<ClassDefinition> allClasses;

        private Dictionary<RaceType, RaceDefinition> raceLookup;
        private Dictionary<ClassType, ClassDefinition> classLookup;

        public void Initialize()
        {
            raceLookup = new Dictionary<RaceType, RaceDefinition>();
            foreach (var race in allRaces)
            {
                if (!raceLookup.ContainsKey(race.raceType))
                {
                    raceLookup[race.raceType] = race;
                }
            }

            classLookup = new Dictionary<ClassType, ClassDefinition>();
            foreach (var classItem in allClasses)
            {
                if (!classLookup.ContainsKey(classItem.classType))
                {
                    classLookup[classItem.classType] = classItem;
                }
            }
        }

        public RaceDefinition GetRace(RaceType race)
        {
            if (raceLookup == null) Initialize();
            return raceLookup.ContainsKey(race) ? raceLookup[race] : null;
        }

        public ClassDefinition GetClass(ClassType classType)
        {
            if (classLookup == null) Initialize();
            return classLookup.ContainsKey(classType) ? classLookup[classType] : null;
        }

        public List<RaceDefinition> GetRacesByFaction(Faction faction)
        {
            return allRaces.FindAll(r => r.faction == faction);
        }
    }
}
