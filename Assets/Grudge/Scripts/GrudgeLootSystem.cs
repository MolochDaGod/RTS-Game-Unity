using UnityEngine;
using System.Collections.Generic;
using Grudge.Equipment;
using Grudge.Weapons;
using Grudge.Races;

namespace Grudge.Loot
{
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "Grudge/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        public class LootEntry
        {
            public EquipmentItem equipmentItem;
            public WeaponItem weaponItem;
            [Range(1, 10)]
            public int minTier = 1;
            [Range(1, 10)]
            public int maxTier = 5;
            [Range(0, 100)]
            public float dropChance = 10f;
            public int minPlayerLevel = 1;
        }

        public string tableName;
        public List<LootEntry> possibleLoot = new List<LootEntry>();

        [Header("Drop Settings")]
        public int minDrops = 1;
        public int maxDrops = 3;
        public float luckMultiplier = 1.0f;

        public List<LootDrop> RollLoot(int playerLevel, float luckBonus = 0f)
        {
            List<LootDrop> drops = new List<LootDrop>();
            
            int dropCount = Random.Range(minDrops, maxDrops + 1);
            
            for (int i = 0; i < dropCount; i++)
            {
                foreach (var entry in possibleLoot)
                {
                    // Check level requirement
                    if (playerLevel < entry.minPlayerLevel) continue;

                    // Roll for drop
                    float roll = Random.Range(0f, 100f);
                    float adjustedChance = entry.dropChance * (1 + luckBonus) * luckMultiplier;

                    if (roll < adjustedChance)
                    {
                        // Determine tier based on player level
                        int tier = CalculateTierFromLevel(playerLevel, entry.minTier, entry.maxTier);

                        LootDrop drop = new LootDrop
                        {
                            equipmentItem = entry.equipmentItem,
                            weaponItem = entry.weaponItem,
                            tier = tier
                        };

                        drops.Add(drop);
                        break; // Only one item per roll
                    }
                }
            }

            return drops;
        }

        private int CalculateTierFromLevel(int playerLevel, int minTier, int maxTier)
        {
            // Tier progression: every 10 levels = +1 tier
            int tierFromLevel = Mathf.FloorToInt(playerLevel / 10f) + 1;
            return Mathf.Clamp(tierFromLevel, minTier, maxTier);
        }
    }

    [System.Serializable]
    public class LootDrop
    {
        public EquipmentItem equipmentItem;
        public WeaponItem weaponItem;
        public int tier;
        public int quantity = 1;

        public bool IsEquipment => equipmentItem != null;
        public bool IsWeapon => weaponItem != null;

        public string GetDisplayName()
        {
            if (IsEquipment) return $"{equipmentItem.displayName} (Tier {tier})";
            if (IsWeapon) return $"{weaponItem.displayName} (Tier {tier})";
            return "Unknown Item";
        }
    }

    [CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Grudge/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Result")]
        public EquipmentItem resultEquipment;
        public WeaponItem resultWeapon;
        [Range(1, 10)]
        public int resultTier = 1;

        [Header("Requirements")]
        public int requiredLevel = 1;
        public CraftingProfession profession;
        public int professionLevel = 1;

        [Header("Materials")]
        public MaterialCost[] materialCosts;

        [Header("Crafting Time")]
        public float craftingTime = 5f;

        public bool CanCraft(GrudgeHeroUnit hero, Dictionary<string, int> playerMaterials)
        {
            // Check level
            if (hero.heroLevel < requiredLevel) return false;

            // Check materials
            foreach (var cost in materialCosts)
            {
                if (!playerMaterials.ContainsKey(cost.materialId)) return false;
                if (playerMaterials[cost.materialId] < cost.amount) return false;
            }

            return true;
        }

        public LootDrop Craft(Dictionary<string, int> playerMaterials)
        {
            // Consume materials
            foreach (var cost in materialCosts)
            {
                if (playerMaterials.ContainsKey(cost.materialId))
                {
                    playerMaterials[cost.materialId] -= cost.amount;
                }
            }

            LootDrop result = new LootDrop
            {
                equipmentItem = resultEquipment,
                weaponItem = resultWeapon,
                tier = resultTier,
                quantity = 1
            };

            return result;
        }
    }

    [System.Serializable]
    public class MaterialCost
    {
        public string materialId;
        public string materialName;
        public int amount;
    }

    /// <summary>
    /// Manages player inventory and loot distribution
    /// </summary>
    public class LootManager : MonoBehaviour
    {
        public EquipmentDatabase equipmentDatabase;
        public WeaponDatabase weaponDatabase;

        // Player inventory
        private Dictionary<string, int> materials = new Dictionary<string, int>();
        private List<LootDrop> unequippedItems = new List<LootDrop>();

        #region Loot Distribution

        public void DistributeLoot(LootTable table, int playerLevel, float luckBonus = 0f)
        {
            var drops = table.RollLoot(playerLevel, luckBonus);
            
            foreach (var drop in drops)
            {
                AddToInventory(drop);
                OnLootDropped?.Invoke(drop);
            }

            if (drops.Count > 0)
            {
                Debug.Log($"Received {drops.Count} items!");
            }
        }

        public void AddToInventory(LootDrop drop)
        {
            unequippedItems.Add(drop);
        }

        public void RemoveFromInventory(LootDrop drop)
        {
            unequippedItems.Remove(drop);
        }

        public List<LootDrop> GetInventory()
        {
            return new List<LootDrop>(unequippedItems);
        }

        #endregion

        #region Materials

        public void AddMaterial(string materialId, int amount)
        {
            if (!materials.ContainsKey(materialId))
            {
                materials[materialId] = 0;
            }
            materials[materialId] += amount;
        }

        public bool HasMaterial(string materialId, int amount)
        {
            return materials.ContainsKey(materialId) && materials[materialId] >= amount;
        }

        public int GetMaterialCount(string materialId)
        {
            return materials.ContainsKey(materialId) ? materials[materialId] : 0;
        }

        public Dictionary<string, int> GetAllMaterials()
        {
            return new Dictionary<string, int>(materials);
        }

        #endregion

        #region Crafting

        public bool CraftItem(CraftingRecipe recipe, GrudgeHeroUnit hero)
        {
            if (!recipe.CanCraft(hero, materials))
            {
                Debug.LogWarning("Cannot craft item - requirements not met");
                return false;
            }

            var result = recipe.Craft(materials);
            AddToInventory(result);
            
            OnItemCrafted?.Invoke(result);
            Debug.Log($"Crafted: {result.GetDisplayName()}");
            
            return true;
        }

        #endregion

        #region Equipment Management

        public bool EquipItemOnHero(LootDrop drop, GrudgeHeroUnit hero)
        {
            if (drop.IsEquipment && drop.equipmentItem != null)
            {
                hero.EquipItem(drop.equipmentItem);
                hero.equipmentTier = drop.tier;
                RemoveFromInventory(drop);
                return true;
            }
            else if (drop.IsWeapon && drop.weaponItem != null)
            {
                hero.EquipWeapon(drop.weaponItem, true);
                hero.equipmentTier = drop.tier;
                RemoveFromInventory(drop);
                return true;
            }

            return false;
        }

        #endregion

        #region Events

        public System.Action<LootDrop> OnLootDropped;
        public System.Action<LootDrop> OnItemCrafted;

        #endregion
    }

    /// <summary>
    /// Experience and leveling system for heroes
    /// </summary>
    [System.Serializable]
    public class ExperienceSystem
    {
        [Header("Level Curve")]
        public int baseXPRequired = 100;
        public float xpScalingFactor = 1.5f;
        public int maxLevel = 100;

        public int GetXPForLevel(int level)
        {
            if (level >= maxLevel) return int.MaxValue;
            return Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpScalingFactor, level - 1));
        }

        public bool AddExperience(GrudgeHeroUnit hero, int xp, out bool leveledUp)
        {
            leveledUp = false;

            if (hero.heroLevel >= maxLevel)
            {
                return false;
            }

            // Add XP
            int xpNeeded = GetXPForLevel(hero.heroLevel + 1);
            
            // Simple XP tracking (you'd want to add a currentXP field to GrudgeHeroUnit)
            // For now, check if xp is enough to level
            if (xp >= xpNeeded)
            {
                hero.GainLevel();
                leveledUp = true;
                
                // Recursive call for remaining XP
                int remainingXP = xp - xpNeeded;
                if (remainingXP > 0)
                {
                    bool additionalLevel;
                    AddExperience(hero, remainingXP, out additionalLevel);
                }
            }

            return leveledUp;
        }

        public int GetXPFromKill(Unit enemy)
        {
            // Base XP from enemy tier and level
            int baseXP = 50;
            int tierBonus = enemy.tier * 10;
            return baseXP + tierBonus;
        }
    }
}
