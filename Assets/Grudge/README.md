# Grudge RTS Integration System

Complete integration of Grudge-Builder's hero, equipment, weapon, and race systems into the Unity RTS game.

## 📁 Folder Structure

```
Assets/Grudge/
├── Prefabs/          # Character, equipment, and weapon prefabs
│   ├── Heroes/
│   ├── Equipment/
│   └── Weapons/
├── ScriptableObjects/ # Data assets
│   ├── Equipment/
│   ├── Weapons/
│   └── Races/
├── Materials/        # Armor materials
│   ├── Cloth/
│   ├── Leather/
│   └── Metal/
├── Animations/       # Weapon-specific animations
└── Scripts/          # Core systems
    ├── GameModes/
    └── Editor/
```

## 🎮 Core Systems

### 1. Equipment System
**File**: `GrudgeEquipmentData.cs`

- **6 Equipment Sets**: Bloodfeud, Wraithfang, Oathbreaker, Kinrend, Dusksinger, Emberclad
- **8 Equipment Slots**: Helm, Shoulder, Chest, Hands, Feet, Ring, Necklace, Relic
- **3 Armor Materials**: Cloth (mage), Leather (rogue), Metal (tank)
- **Tier Scaling**: Equipment stats scale from Tier 1-10
- **Set Bonuses**: 2pc, 4pc, 6pc, 8pc bonuses

**Usage**:
```csharp
// Create equipment item
var helm = ScriptableObject.CreateInstance<EquipmentItem>();
helm.displayName = "Bloodfeud Helm";
helm.slot = EquipmentSlot.Helm;
helm.material = ArmorMaterial.Metal;
helm.equipmentSet = EquipmentSet.Bloodfeud;

// Equip on hero
hero.EquipItem(helm);
```

### 2. Weapon System
**File**: `GrudgeWeaponData.cs`

- **23 Weapon Types**: Melee, Ranged, Magic (Staves & Tomes)
- **Weapon Categories**: 1H, 2H, Ranged 2H
- **Abilities**: Basic, selectable, signature, and passive abilities
- **Crafting Professions**: Miner, Forester, Engineer, Mystic

**Usage**:
```csharp
// Create weapon
var sword = ScriptableObject.CreateInstance<WeaponItem>();
sword.displayName = "Bloodfeud Blade";
sword.weaponType = WeaponType.Sword;
sword.category = WeaponCategory.OneHand;

// Equip on hero
hero.EquipWeapon(sword, isPrimary: true);
```

### 3. Race & Class System
**File**: `GrudgeRaceData.cs`

- **6 Races**: Human, Barbarian (Crusade), Undead, Orc (Legion), Elf, Dwarf (Fabled)
- **4 Classes**: Worg Shapeshifter, Warrior, Mage Priest, Ranger Scout
- **8 Attributes**: STR, INT, VIT, DEX, END, WIS, AGI, TAC
- **Derived Stats**: Calculated from attributes (HP, Mana, Damage, Defense, etc.)

**Stat Formulas**:
```
HP = (STR × 5) + (VIT × 25)
Mana = (INT × 9) + (WIS × 6)
Physical Damage = STR × 1.25
Magical Damage = INT × 1.5
Physical Defense = (STR × 4) + (VIT × 1.5) + (END × 5)
Magical Defense = (INT × 2) + (WIS × 5.5)
```

### 4. Hero Unit System
**File**: `GrudgeHeroUnit.cs`

Extended `UnitHero` with:
- Equipment management (8 slots)
- Weapon management (primary/secondary)
- Attribute system with point allocation
- Set bonus tracking
- Stat caching for performance
- Visual equipment updates

**Usage**:
```csharp
// Access hero
GrudgeHeroUnit hero = GetComponent<GrudgeHeroUnit>();

// Allocate attributes
hero.AllocateAttribute("strength", 5);
hero.AllocateAttribute("vitality", 3);

// Level up
hero.GainLevel(); // +7 attribute points

// Get stats
string stats = hero.GetStatsDebugString();
Debug.Log(stats);
```

### 5. Unit Factory
**File**: `GrudgeUnitFactory.cs`

Singleton factory for spawning units:

```csharp
// Get factory instance
var factory = GrudgeUnitFactory.Instance;

// Create basic hero
var hero = factory.CreateHero(
    RaceType.Human,
    ClassType.Warrior,
    "Grimfang",
    level: 10,
    tier: 3,
    position: Vector3.zero
);

// Create hero with loadout
HeroLoadout loadout = new HeroLoadout
{
    heroName = "Bloodbane",
    race = RaceType.Orc,
    heroClass = ClassType.Warrior,
    level = 20,
    tier = 5,
    primaryWeaponId = "sword-bloodfeud"
};
var loadoutHero = factory.CreateHeroWithLoadout(loadout);

// Create random hero for PvE
var enemyHero = factory.CreateRandomHero(
    level: 15,
    tier: 4,
    position: spawnPoint,
    forceFaction: Faction.Legion
);

// Create common unit
var soldier = factory.CreateCommonUnit(
    RaceType.Human,
    WeaponType.Sword,
    upgradeLevel: 2
);
```

### 6. Loot & Progression System
**File**: `GrudgeLootSystem.cs`

**Loot Tables**:
```csharp
// Create loot table
var lootTable = ScriptableObject.CreateInstance<LootTable>();
lootTable.minDrops = 1;
lootTable.maxDrops = 3;

// Roll loot
var drops = lootTable.RollLoot(playerLevel: 25, luckBonus: 0.1f);

// Distribute to player
LootManager lootManager = GetComponent<LootManager>();
lootManager.DistributeLoot(lootTable, playerLevel: 25);
```

**Crafting**:
```csharp
// Create recipe
var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
recipe.resultEquipment = bloodfeudHelm;
recipe.resultTier = 5;
recipe.profession = CraftingProfession.Miner;

// Craft item
LootManager lootManager = GetComponent<LootManager>();
bool success = lootManager.CraftItem(recipe, hero);
```

**Experience**:
```csharp
ExperienceSystem xpSystem = new ExperienceSystem();
xpSystem.baseXPRequired = 100;
xpSystem.xpScalingFactor = 1.5f;

// Add XP
bool leveledUp;
xpSystem.AddExperience(hero, 500, out leveledUp);

if (leveledUp)
{
    Debug.Log($"{hero.heroName} leveled up to {hero.heroLevel}!");
}
```

## 🎯 Quick Start Guide

### Step 1: Setup Databases

1. Create Equipment Database:
   - Right-click in Project → Create → Grudge → Equipment Database
   - Name it "GrudgeEquipmentDB"
   - Populate with equipment items

2. Create Weapon Database:
   - Right-click → Create → Grudge → Weapon Database  
   - Name it "GrudgeWeaponDB"
   - Populate with weapons

3. Create Race Database:
   - Right-click → Create → Grudge → Race Database
   - Name it "GrudgeRaceDB"
   - Add race and class definitions

### Step 2: Setup Factory

1. Create empty GameObject: "GrudgeUnitFactory"
2. Add `GrudgeUnitFactory` component
3. Assign databases to factory
4. Assign prefab templates:
   - Hero Unit Prefab (with GrudgeHeroUnit component)
   - Elite Unit Prefab
   - Common Unit Prefab

### Step 3: Create Your First Hero

```csharp
using Grudge.Factory;
using Grudge.Races;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        var factory = GrudgeUnitFactory.Instance;
        
        // Create hero
        var hero = factory.CreateHero(
            RaceType.Human,
            ClassType.Warrior,
            "TestHero",
            level: 1,
            tier: 1,
            position: transform.position
        );
        
        // Equip starting gear
        var weapon = factory.weaponDatabase.GetWeaponById("sword-bloodfeud");
        hero.EquipWeapon(weapon, true);
        
        Debug.Log(hero.GetStatsDebugString());
    }
}
```

## 🏗️ Game Mode Templates

### Campaign Mode
```csharp
using Grudge.Factory;
using Grudge.Loot;

public class CampaignMission : MonoBehaviour
{
    public LootTable missionLootTable;
    public GrudgeUnitFactory factory;
    public LootManager lootManager;
    
    public void OnMissionComplete()
    {
        // Award loot
        lootManager.DistributeLoot(
            missionLootTable, 
            playerLevel: 25, 
            luckBonus: 0.1f
        );
        
        // Award XP
        ExperienceSystem xp = new ExperienceSystem();
        foreach (var hero in playerHeroes)
        {
            bool leveledUp;
            xp.AddExperience(hero, 1000, out leveledUp);
        }
    }
}
```

### PvP Skirmish Mode
```csharp
public class SkirmishMode : MonoBehaviour
{
    public int armyPointBudget = 1000;
    
    public bool ValidateArmy(List<GrudgeHeroUnit> heroes, List<Unit> units)
    {
        var factory = GrudgeUnitFactory.Instance;
        int cost = factory.CalculateArmyPointCost(heroes, units);
        return cost <= armyPointBudget;
    }
}
```

### Survival Mode
```csharp
public class SurvivalMode : MonoBehaviour
{
    public int currentWave = 1;
    public GrudgeUnitFactory factory;
    
    public void SpawnWave()
    {
        int enemyLevel = 10 + (currentWave * 2);
        int enemyTier = Mathf.Min(1 + (currentWave / 5), 10);
        
        for (int i = 0; i < currentWave + 5; i++)
        {
            var enemy = factory.CreateRandomHero(
                enemyLevel,
                enemyTier,
                GetRandomSpawnPoint(),
                Faction.Legion
            );
            enemy.team = Team.T2; // Enemy team
        }
        
        currentWave++;
    }
}
```

## 📊 Unit Archetypes

### Tank (Metal Armor + Shield)
```
Race: Dwarf
Class: Warrior
Equipment: Full Metal Bloodfeud Set (8pc)
Primary: Hammer1h (Ironfist)
Stats: 2500 HP, 50 Defense, 15% Block
Role: Frontline tank, crowd control
```

### DPS (Leather Armor + Dual Daggers)
```
Race: Elf
Class: Ranger Scout
Equipment: Full Leather Wraithfang Set (8pc)
Primary: Dagger (Nightfang)
Secondary: Dagger (Bloodshiv)
Stats: 1200 HP, 45% Crit, 25% Lifesteal
Role: Assassin, single-target burst
```

### Mage (Cloth Armor + Staff)
```
Race: Undead
Class: Mage Priest
Equipment: Full Cloth Emberclad Set (8pc)
Primary: Fire Staff (Emberwrath)
Stats: 800 HP, 2000 Mana, +50% Burn Damage
Role: AoE damage, DoT specialist
```

### Mounted Cavalry
```
Race: Human
Class: Warrior
Equipment: Partial Leather Kinrend Set (4pc)
Primary: Greatsword (Doomspire)
Mount: Warhorse
Stats: +50% Movement Speed, +25% Charge Damage
Role: Mobile striker, flanking
```

## ⚙️ Performance Optimization

### Stat Caching
- Stats only recalculated when `statsDirty` flag is true
- Set when equipment changes, attributes allocated, or level gained
- Cached values used for combat calculations

### LOD System
- Use Unity's LOD groups for character prefabs
- Lower poly meshes for distant units
- Reduce particle effects at distance

### Object Pooling
- Pool common unit prefabs
- Pool particle effects
- Pool weapon trail effects

## 🐛 Debugging

### Hero Stats
```csharp
Debug.Log(hero.GetStatsDebugString());
```

### Equipment Validation
```csharp
if (!hero.CanEquipItem(item))
{
    Debug.LogWarning($"Cannot equip {item.displayName}");
}
```

### Point Cost Calculation
```csharp
int cost = hero.GetPointCost();
Debug.Log($"Hero point cost: {cost}");
```

## 📝 TODO for Full Implementation

- [ ] Import TypeScript equipment/weapon data via JSON
- [ ] Create weapon-specific skill mappers
- [ ] Implement visual equipment attachment system
- [ ] Create UI for inventory and equipment screens
- [ ] Add mount system prefabs
- [ ] Implement weapon trail VFX
- [ ] Add combat sound effects
- [ ] Create character customization UI
- [ ] Implement save/load system for hero loadouts
- [ ] Add multiplayer army synchronization

## 🔗 Related Files

- Plan Document: `../../plan-[id].md`
- Base Unit System: `../../Assets/Units/Unit.cs`
- Base Hero System: `../../Assets/Units/UnitHero.cs`
- TypeScript Source: `../../../Grudge-Builder2/`

## 📚 References

- Equipment Sets: 6 sets × 8 slots × 3 materials = 144 equipment items
- Weapons: 23 weapon types × 6 sets = 138+ weapons
- Races: 6 races × 3 factions
- Classes: 4 classes with unique abilities
- Max Level: 100
- Max Tier: 10

---

**Status**: Core systems implemented and ready for testing
**Version**: 1.0.0
**Last Updated**: 2026-01-31
