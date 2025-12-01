using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace WeaponCreation
{
    // --- ENUMS (Unchanged) ---
    public enum WeaponType { Sword, Axe, Bow, Staff, Dagger, Hammer }
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum WeaponElement { None, Fire, Ice, Lightning, Earth }

    public class Weapon
    {
        // --- PROPERTIES ---
        public string Id { get; private set; }
        public string Name { get; private set; }
        public WeaponType Type { get; private set; }
        public Rarity RarityType { get; private set; }
        public WeaponElement Element { get; private set; }
        
        public List<WeaponDropStats> BonusStats { get; private set; } = new List<WeaponDropStats>();
        public float Range { get; private set; }
        public float Weight { get; private set; }

        // Combat Stats
        public float BaseDamage { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalMultiplier { get; set; }
        public float AttackSpeedPerSecond { get; set; } = 1.0f;

        public int Level { get; set; } = 1;
        public float GrowthRatePerLevel { get; set; } = 1.1f; // 10% growth per level set by default

        // --- CALCULATED PROPERTIES ---
        public float ActualDamage => BaseDamage * GetDamageModifier(RarityType) * GetDamageFromLevel(Level, GrowthRatePerLevel);
        private float MinDamage => ActualDamage * 0.8f;
        private float MaxDamage => ActualDamage * 1.2f;

        // --- CONSTRUCTOR ---
        /// <summary>
        /// This is a private constructor used internally.
        /// Use LoadWeaponsFromCsv to create weapons from CSV data.
        /// </summary>
        /// <param name="id">Weapon ID</param>
        /// <param name="name">Name of the Weapon</param>
        /// <param name="type">Weapon Type (Axes, Swords, Bow)</param>
        /// <param name="rarity">Weapon Rarity (White, Rare, Legendary)</param>
        /// <param name="damage">Base Damage of the Weapon</param>
        /// <param name="range">Not used yet</param>
        /// <param name="weight">Not used yet</param>
        /// <param name="critChance">crit change 1.15 = 15% chance</param>
        /// <param name="critMult">Base Damage mult from this value (Base Damage[20] x critMult[1.5] = 30 damage)</param>
        /// <param name="element">Element type of Weapon (Fire, Shock, Dark)</param>
        /// <param name="growthPerLevel">Value for damage scaling with level of the weapon (Level 1 Weapon damage is 20, growthPerLevel is 1.5, then the level 2 Weapon damage is 30)</param>
        /// <param name="level">Level of the Weapon</param>
        private Weapon(string id, string name, WeaponType type, Rarity rarity, float damage, float range, float weight, float critChance, float critMult, WeaponElement element, float growthPerLevel, int level)
        {
            Id = id;
            Name = name;
            Type = type;
            RarityType = rarity;
            BaseDamage = damage;
            Range = range;
            Weight = weight;
            CriticalChance = critChance;
            CriticalMultiplier = critMult;
            Element = element;
            Level = level;
            GrowthRatePerLevel = growthPerLevel;
        }

        // --- CSV LOADER (Main Creation Method) ---
        /// <summary>
        /// This loads weapons from a CSV file.
        /// Great for bulk creation and balancing.
        /// </summary>
        /// <param name="path">A path to the weapon.csv location.</param>
        /// <returns>List of Weapons.</returns>
        public static List<Weapon> LoadWeaponsFromCsv(string path = "weapons.csv")
        {
            var weaponList = new List<Weapon>();
            string fullPath = Path.GetFullPath(path);

            Console.WriteLine($"[System] Loading Weapons from: {fullPath}...");

            if (!File.Exists(path))
            {
                Console.WriteLine($"[Error] File not found at {fullPath}");
                return weaponList;
            }

            string[] lines = File.ReadAllLines(path);

            // Loop starts at 1 to skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] data = line.Split(',');
                
                if (data.Length < 12)
                {
                    Console.WriteLine($"[Warning] Skipping invalid row {i} (Not enough columns): {line}");
                    continue;
                }

                try
                {
                    Weapon newWeapon = new Weapon(
                        id:             data[0].Trim(),
                        name:           data[1].Trim(),
                        type:           ParseEnum<WeaponType>(data[2]),
                        rarity:         ParseEnum<Rarity>(data[3]),
                        damage:         ParseFloat(data[4]),
                        range:          ParseFloat(data[5]),
                        weight:         ParseFloat(data[6]),
                        critChance:     ParseFloat(data[7]),
                        critMult:       ParseFloat(data[8]),
                        element:        ParseEnum<WeaponElement>(data[9]),
                        growthPerLevel: ParseFloat(data[10]),
                        level:          ParseInt(data[11])
                    );
                    weaponList.Add(newWeapon);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to parse row {i}: {ex.Message}");
                }
            }

            Console.WriteLine($"[System] Successfully loaded {weaponList.Count} weapons.");
            return weaponList;
        }
        
        /// <summary>
        /// This creates a copy of the weapon at a specific level.
        /// Great for scaling enemies or loot drops.
        /// </summary>
        /// <param name="targetLevel"></param>
        /// <returns></returns>
        public Weapon CreateCopyAtLevel(int targetLevel)
        {
            var newWeapon = new Weapon(
                this.Id, this.Name, this.Type, this.RarityType, this.BaseDamage,
                this.Range, this.Weight, this.CriticalChance, this.CriticalMultiplier,
                this.Element, this.GrowthRatePerLevel, targetLevel
            );

            // Initialize the list
            newWeapon.BonusStats = new List<WeaponDropStats>();
            return newWeapon;
        }

        // --- LOGIC METHODS ---

        private static float GetDamageModifier(Rarity rarity) => rarity switch
        {
            Rarity.Common => 1.0f,
            Rarity.Uncommon => 1.25f,
            Rarity.Rare => 1.5f,
            Rarity.Epic => 1.75f,
            Rarity.Legendary => 2.0f,
            _ => 1.0f
        };
        
        private static float GetDamageFromLevel(int level, float growthRate) => (float)Math.Pow(growthRate, level - 1);

        private int CalculateSaleMultiplierWithRarity() => RarityType switch
        {
            Rarity.Common => 1,
            Rarity.Uncommon => 2,
            Rarity.Rare => 3,
            Rarity.Epic => 5,
            Rarity.Legendary => 8,
            _ => 1
        };

        public int CalculateSalePrice()
        {
            int basePrice = (int)(ActualDamage * 10 + Weight * 5);
            return basePrice * CalculateSaleMultiplierWithRarity();
        }

        public int CalculateSellPrice()
        {
            return (int)(CalculateSalePrice() * 0.15f);
        }

        // Combat Logic
        private int CalculateDamage()
        {
            int damage = Random.Shared.Next((int)MinDamage, (int)MaxDamage + 1);
            if (Random.Shared.NextDouble() < CriticalChance)
            {
                damage = (int)(damage * CriticalMultiplier);
            }
            return damage;
        }
        
        public float CalculateBonusStat(WeaponDropStats.StatType statToCalc, float playerBaseValue)
        {
            float flatSum = 0;
            float percentSum = 0;
            float multiplierProduct = 1;

            // Find only modifiers that affect the requested stat (e.g., MaxHealth)
            foreach (var mod in BonusStats.Where(m => m.statType == statToCalc))
            {
                switch (mod.modifierType)
                {
                    case WeaponDropStats.ModifierType.Flat:
                        flatSum += mod.value;
                        break;
                    case WeaponDropStats.ModifierType.PercentageAdd:
                        percentSum += mod.value;
                        break;
                    case WeaponDropStats.ModifierType.Multiplier:
                        multiplierProduct *= mod.value;
                        break;
                }
            }

            // Formula: (Base + Flat) * (1 + Sum%) * (ProductX)
            return (playerBaseValue + flatSum) * (1.0f + percentSum) * multiplierProduct;
        }
        
        // --- Helpers ---
        private static float ParseFloat(string value)
        {
            // Tries to parse. If it fails (bad text), returns 0.0f
            if (float.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;
            return 0.0f; 
        }

        private static int ParseInt(string value)
        {
            if (int.TryParse(value.Trim(), out int result))
                return result;
            return 1; // Default level 1 if parsing fails
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (Enum.TryParse<T>(value.Trim(), true, out T result))
                return result;
            return default(T); // Returns first enum item (e.g. Sword or Common) if fails
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} {Level} ({RarityType} {Type}) | DMG: {ActualDamage:F0} | ELM: {Element} | VAL: {CalculateSalePrice()}g";
        }
        
        public string GetBonusStatsString()
        {
            if (BonusStats.Count == 0) return "";
            return " | Bonus: " + string.Join(", ", BonusStats);
        }

        #if DEBUG
        public void AttackLoop(int loops)
        {
            Console.WriteLine($"--- Simulating {loops} attacks for {Name} ---");
            int crits = 0;
            for (int i = 0; i < loops; i++)
            {
                int dmg = CalculateDamage();
                // Check if damage exceeds max base damage (indicates a crit)
                if (dmg > MaxDamage) crits++;
            }
            Console.WriteLine($"Results: {loops} Hits. {crits} Crits. Est. Value: {CalculateSalePrice()}");
        }
        
        #region Stat Rolling for Weapons (Debug Only)
        private static Random _rng = new Random();

        // Rolls random stats for a weapon based on its rarity
        // Going to change... this to a more robust system later.
        public static void RollStatsForWeapon(Weapon weapon)
        {
            // Determine how many stats based on Rarity?
            int rollCount = weapon.RarityType switch
            {
                Rarity.Common => 0,
                Rarity.Uncommon => 1,
                Rarity.Rare => 2,
                Rarity.Epic => 3,
                Rarity.Legendary => 10,
                _ => 0
            };

            for (int i = 0; i < rollCount; i++)
            {
                weapon.BonusStats.Add(GenerateRandomStat());
            }
        }

        // Going to change this to a more robust system later.
        private static WeaponDropStats GenerateRandomStat()
        {
            // Pick Random Stat (Health or Damage)
            var stats = Enum.GetValues<WeaponDropStats.StatType>();
            var statType = stats[_rng.Next(stats.Length)];

            // Pick Random Modifier (Flat, %, etc)
            var mods = Enum.GetValues<WeaponDropStats.ModifierType>();
            var modType = mods[_rng.Next(mods.Length)];

            float value = 0f;
            
            if (modType == WeaponDropStats.ModifierType.Flat)
            {
                if (statType == WeaponDropStats.StatType.MaxHealth) value = _rng.Next(10, 50); // +10 to +50 HP
                else value = _rng.Next(2, 10); // +2 to +10 Dmg
            }
            else if (modType == WeaponDropStats.ModifierType.PercentageAdd)
            {
                value = (float)_rng.NextDouble() * 0.2f; // 0% to 20%

            }
            else // Multiplier
            {
                value = 1.0f + ((float)_rng.NextDouble() * 0.5f); // 1.0x to 1.5x
            }

            return new WeaponDropStats(statType, modType, value);
        }
        #endregion
        #endif
    }
}