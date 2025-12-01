using System.Globalization;

namespace WeaponCreation
{
    // --- ENUMS ---
    public enum WeaponType { Sword, Axe, Bow, Staff, Dagger, Hammer }
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum WeaponElement { None, Fire, Ice, Lightning, Earth }

    public class Weapon
    {
        // --- PROPERTIES ---
        #region Properties
        // Unique Identifier for the Weapon
        public string Id { get; private set; }
        // Name of the Weapon
        public string Name { get; private set; }
        private string _originalName => Name; // For reference if needed
        // Type of the Weapon
        public WeaponType Type { get; private set; }
        // Rarity of the Weapon
        public Rarity RarityType { get; private set; }
        // Elemental Type of the Weapon
        public WeaponElement Element { get; private set; }
        
        // List of all the bonus stats applied to the weapon
        public List<WeaponPrefixStats> BonusStats { get; private set; } = new List<WeaponPrefixStats>();
        public float Range { get; private set; } // Might Remove later
        public float Weight { get; private set; } // Might Remove later

        // Base Damage before modifiers or scaling
        public float BaseDamage { get; set; }
        // Critical Chance (e.g., 0.15 = 15% chance)
        public float CriticalChance { get; set; }
        // Critical Multiplier (e.g., 1.5 = 50% more damage on crit)
        public float CriticalMultiplier { get; set; }
        // Attack Speed (Attacks per Second)
        public float AttackSpeedPerSecond { get; set; } = 1.0f;

        // Level of the Weapon.
        public int Level { get; set; } = 1;
        
        // Growth rate for damage scaling per level
        public float GrowthRatePerLevel { get; set; } = 1.1f;
        
        // Cached the Damage after calculations
        private float _cachedDamage = -1f;

        // Actual Damage is the damage after applying rarity and level scaling.
        public float ActualDamage
        {
            get
            {
                if (_cachedDamage < 0)
                {
                    _cachedDamage = BaseDamage * GetDamageModifier(RarityType) * GetDamageFromLevel(Level, GrowthRatePerLevel);
                }
                return _cachedDamage;
            }
        }
        
        // Damage Range for variability in attacks
        private float MinDamage => ActualDamage * 0.8f;
        private float MaxDamage => ActualDamage * 1.2f;
        #endregion

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
            // Initialize the list to hold weapons, then adds them from CSV
            var weaponList = new List<Weapon>();
            // Get the full path for logging
            string fullPath = Path.GetFullPath(path);

            Console.WriteLine($"[System] Loading Weapons from: {fullPath}...");

            if (!File.Exists(path))
            {
                Console.WriteLine($"[Error] File not found at {fullPath}");
                return weaponList;
            }

            // Read all lines from the CSV
            string[] lines = File.ReadAllLines(path);

            // Loop starts at 1 to skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                // Handles empty lines.
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Split CSV line into columns
                string[] data = line.Split(',');
                
                // Basic validation
                if (data.Length < 12) // Expecting at least 12 columns
                {
                    Console.WriteLine($"[Warning] Skipping invalid row {i} (Not enough columns): {line}");
                    continue;
                }

                try
                {
                    // See the weapons.csv for the order of these parameters
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
                    weaponList.Add(newWeapon); // Add to the list
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
        /// <param name="targetLevel">What should the Weapon Level be.</param>
        /// <returns>New Weapons.</returns>
        public Weapon CreateCopyAtLevel(int targetLevel)
        {
            var newWeapon = new Weapon(
                this.Id, this.Name, this.Type, this.RarityType, this.BaseDamage,
                this.Range, this.Weight, this.CriticalChance, this.CriticalMultiplier,
                this.Element, this.GrowthRatePerLevel, targetLevel
            );

            // Initialize the list
            newWeapon.BonusStats = new List<WeaponPrefixStats>();
            return newWeapon;
        }

        // --- CALCULATION METHODS ---
        #region Calculations
        private static float GetDamageModifier(Rarity rarity) => rarity switch
        {
            Rarity.Common => 1.0f,
            Rarity.Uncommon => 1.25f,
            Rarity.Rare => 1.5f,
            Rarity.Epic => 1.75f,
            Rarity.Legendary => 2.0f,
            _ => 1.0f
        };
        
        private static float GetDamageFromLevel(int level, float growthRate)
        {
            if (level <= 1 || growthRate <= 0)
                return 1f; // Level 1 (or invalid growth) should return base damage only

            return (float)Math.Pow(growthRate, level - 1);
        }

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
        
        public float CalculateBonusStat(WeaponPrefixStats.StatType statToCalc, float playerBaseValue)
        {
            float flatSum = 0;
            float percentSum = 0;
            float multiplierProduct = 1;

            // Find only modifiers that affect the requested stat (e.g., MaxHealth)
            foreach (var mod in BonusStats.Where(m => m.statType == statToCalc))
            {
                switch (mod.modifierType)
                {
                    case WeaponPrefixStats.ModifierType.Flat:
                        flatSum += mod.value;
                        break;
                    case WeaponPrefixStats.ModifierType.PercentageAdd:
                        percentSum += mod.value;
                        break;
                    case WeaponPrefixStats.ModifierType.Multiplier:
                        multiplierProduct *= mod.value;
                        break;
                }
            }

            // Formula: (Base + Flat) * (1 + Sum%) * (ProductX)
            return (playerBaseValue + flatSum) * (1.0f + percentSum) * multiplierProduct;
        }
        #endregion
        
        // --- HELPERS METHODS ---
        #region Helpers
        /// <summary>
        /// Parses a string to a float value.
        /// If the string cannot be parsed, returns 0.0f.
        /// </summary>
        /// <param name="value">The string representation of the float value to parse.</param>
        /// <returns>The parsed float value, or 0.0f if parsing fails.</returns>
        private static float ParseFloat(string value)
        {
            // Tries to parse. If it fails (bad text), returns 0.0f
            if (float.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;
            return 0.0f; 
        }

        /// <summary>
        /// Parses a string value into an integer. If parsing fails, it returns a default value of 1.
        /// </summary>
        /// <param name="value">The string value to be parsed into an integer.</param>
        /// <returns>The parsed integer value. Returns 1 if the parsing fails.</returns>
        private static int ParseInt(string value)
        {
            if (int.TryParse(value.Trim(), out int result))
                return result;
            return 1; // Default level 1 if parsing fails
        }

        /// <summary>
        /// Used to parse Enums from string values in CSV.
        /// </summary>
        /// <param name="value">The collum of the data that you want to parse.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Returns the data, if not returns the first enum item if fails.</returns>
        private static T ParseEnum<T>(string value) where T : struct
        {
            if (Enum.TryParse<T>(value.Trim(), true, out T result))
                return result; 
            return default(T); // Returns first enum item (e.g. Sword or Common) if fails
        }

        /// <summary>
        /// Gets the weapon info as a formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{Id}] {Name} {Level} ({RarityType} {Type}) | DMG: {ActualDamage:F0} | ELM: {Element} | VAL: {CalculateSalePrice()}g, SELL: {CalculateSellPrice()}g{GetBonusStatsString()}";
        }
        
        /// <summary>
        /// Gets a formatted string of the bonus stats.
        /// </summary>
        /// <returns>Returns the Stats to String.</returns>
        private string GetBonusStatsString()
        {
            if (BonusStats.Count == 0) return "";
            return " | Bonus: " + string.Join(", ", BonusStats);
        }
        
        /// <summary>
        /// Gives the weapon a name based on its prefixes.
        /// </summary>
        private void UpdateNameWithPrefixes()
        {
            // 1. Reset name to original so we don't get "Sharp Sharp Sword"
            string prefix = "";
    
            // 2. Loop through stats to find a suitable prefix
            // For simplicity, we just take the prefix of the LAST added stat, 
            // but you could prioritize Rarity or specific stats.
            if (BonusStats.Count > 0)
            {
                // Get the last stat added (or search for the highest value)
                var dominantStat = BonusStats.Last(); 
        
                prefix = GetAdjectiveForStat(dominantStat);
            }

            // 3. Combine them
            if (!string.IsNullOrEmpty(prefix))
            {
                Name = $"{prefix} {_originalName}";
            }
            else
            {
                Name = _originalName;
            }
        }
        
        /// <summary>
        /// IF you want to combine multiple prefixes into the name.
        /// E.g., "Sharp Vital Sword" for +Damage and +Health.
        /// </summary>
        /// <returns></returns>
        //private void UpdateNameWithPrefixes()
        //{
        //    StringBuilder fullPrefix = new StringBuilder();

        //    foreach (var stat in BonusStats)
        //    {
        //        fullPrefix.Append(GetAdjectiveForStat(stat) + " ");
        //    }

        //    // TrimEnd removes the trailing space
        //    Name = $"{fullPrefix.ToString().TrimEnd()} {_originalName}";
        //}

        /// <summary>
        /// Get the adjective based on the stat type and modifier.
        /// </summary>
        /// <param name="stat">Gets the stat for the adjective name for the weapon.</param>
        /// <returns></returns>
        private string GetAdjectiveForStat(WeaponPrefixStats stat)
        {
            return stat.statType switch
            {
                WeaponPrefixStats.StatType.AttackDamage => stat.modifierType switch 
                {
                    WeaponPrefixStats.ModifierType.Multiplier => "Deadly",
                    WeaponPrefixStats.ModifierType.PercentageAdd => "Jagged",
                    _ => "Sharp" // Flat damage
                },
        
                WeaponPrefixStats.StatType.MaxHealth => stat.modifierType switch
                {
                    WeaponPrefixStats.ModifierType.Multiplier => "Immortal",
                    WeaponPrefixStats.ModifierType.PercentageAdd => "Vital",
                    _ => "Sturdy"
                },
        
                _ => "Enchanted" // Fallback
            };
        }
        #endregion
        
        #if DEBUG
        /// <summary>
        /// Attack Loop for testing damage and crit rates.
        /// </summary>
        /// <param name="loops">How many loops do you need.</param>
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
        #endif
        
        #region Stat Rolling for Weapons
        // Simple RNG for stat rolling
        private static Random _rng = new Random(); 
        
        /// <summary>
        /// Rolls random stats for the weapon based on its Rarity.
        /// Common = 0 stats, Uncommon = 1 stat, Rare = 2 stats, Epic = 3 stats, Legendary = 4 stats.
        /// </summary>
        /// <param name="weapon">Weapon that is rolling stats.</param>
        public static void RollStatsForWeapon(Weapon weapon)
        {
            // Determine how many stats based on Rarity?
            int rollCount = weapon.RarityType switch
            {
                Rarity.Common => 0,
                Rarity.Uncommon => 1,
                Rarity.Rare => 2,
                Rarity.Epic => 3,
                Rarity.Legendary => 4,
                _ => 0
            };

            for (int i = 0; i < rollCount; i++)
            {
                weapon.BonusStats.Add(GenerateRandomStat());
            }
            
            weapon.UpdateNameWithPrefixes();
        }
        
        /// <summary>
        /// This is the stats to be rolled for the weapon.
        /// Add more stats as needed.
        /// </summary>
        /// <returns>Stats to be rolled.</returns>
        private static WeaponPrefixStats GenerateRandomStat()
        {
            // Pick Random Stat (Health or Damage)
            var stats = Enum.GetValues<WeaponPrefixStats.StatType>();
            var statType = stats[_rng.Next(stats.Length)];

            // Pick Random Modifier (Flat, %, etc)
            var mods = Enum.GetValues<WeaponPrefixStats.ModifierType>();
            var modType = mods[_rng.Next(mods.Length)];

            float flatValue = 0f;
            float minPercent = 0.2f, maxPercent = 0.5f;
            float minMult = 1.1f, maxMult = 1.5f;
            
            if (modType == WeaponPrefixStats.ModifierType.Flat)
            {
                if (statType == WeaponPrefixStats.StatType.MaxHealth) flatValue = _rng.Next(10, 50); // +10 to +50 HP
                else if (statType == WeaponPrefixStats.StatType.AttackDamage) flatValue = _rng.Next(20, 20); // +2 to +10 DMG
            }
            else if (modType == WeaponPrefixStats.ModifierType.PercentageAdd)
            {
                flatValue = minPercent + ((float)_rng.NextDouble() * (maxPercent - minPercent)); // 20% to 50% based on the minPercent/maxPercent

            }
            else // Multiplier
            {
                flatValue = minMult + ((float)_rng.NextDouble() * (maxMult - minMult)); // 1.0x to 1.5x based on the minMult/maxMult
            }

            return new WeaponPrefixStats(statType, modType, flatValue);
        }
        #endregion
    }
}