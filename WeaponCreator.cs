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
        public float CriticalDamage { get; set; }
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
                    _cachedDamage = CalculateFinalDamage();
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
            CriticalDamage = critMult;
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
                this.Range, this.Weight, this.CriticalChance, this.CriticalDamage,
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
                damage = (int)(damage * CriticalDamage);
            }
            return damage;
        }
        
        public float GetEffectiveStat(float baseValue, WeaponPrefixStats.StatType typeToCheck)
        {
            float total = baseValue;

            // Filter the list once for the stats we care about
            var relevantStats = BonusStats.Where(s => s.statType == typeToCheck).ToList();

            // 1. Apply Flat Modifiers (e.g., +5 Damage)
            // It's usually best to add flat values before multiplying percentages
            float flatSum = relevantStats
                .Where(s => s.modifierType == WeaponPrefixStats.ModifierType.Flat)
                .Sum(s => s.value);
    
            total += flatSum;

            // 2. Apply Percentage Additive (e.g., +10% Damage + 20% Damage = +30% Total)
            float percentSum = relevantStats
                .Where(s => s.modifierType == WeaponPrefixStats.ModifierType.PercentageAdd)
                .Sum(s => s.value);
    
            if (percentSum != 0)
            {
                total *= (1.0f + percentSum);
            }

            // 3. Apply Multipliers (e.g., Double Damage)
            var multipliers = relevantStats
                .Where(s => s.modifierType == WeaponPrefixStats.ModifierType.Multiplier);
        
            foreach (var stat in multipliers)
            {
                total *= stat.value;
            }

            return total;
        }
        
        public float CalculateFinalDamage()
        {
            // Step A: Calculate the Stat (Base + Prefixes)
            float effectiveBaseDamage = GetEffectiveStat(this.BaseDamage, WeaponPrefixStats.StatType.AttackDamage);
            
            // Step B: Get Scaling Multipliers
            // Note: Assuming these methods exist in your class based on your previous code
            float rarityMod = GetDamageModifier(this.RarityType); 
            float levelMod = GetDamageFromLevel(this.Level, this.GrowthRatePerLevel);

            // Step C: Calculate Final
            return effectiveBaseDamage * rarityMod * levelMod;
        }
        
        public float CalculateFinalCriticalChance()
        {
            return GetEffectiveStat(this.CriticalChance, WeaponPrefixStats.StatType.CriticalChance);
        }

        public float CalculateFinalCriticalDamage()
        {
            return GetEffectiveStat(this.CriticalDamage, WeaponPrefixStats.StatType.CriticalDamage);
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
            return $"[{Id}] {Name} {Level} ({RarityType} {Type}) | DMG: {BaseDamage:F0} | ELM: {Element} | VAL: {CalculateSalePrice()}g, SELL: {CalculateSellPrice()}g{GetBonusStatsString()}";
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
        public void UpdateNameWithPrefixes()
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
                    _ => "Sharp"
                },
        
                WeaponPrefixStats.StatType.MaxHealth => stat.modifierType switch
                {
                    WeaponPrefixStats.ModifierType.Multiplier => "Immortal",
                    WeaponPrefixStats.ModifierType.PercentageAdd => "Vital",
                    _ => "Sturdy"
                },
                
                WeaponPrefixStats.StatType.CriticalChance => stat.modifierType switch
                {
                    WeaponPrefixStats.ModifierType.Multiplier => "Assassin's",
                    WeaponPrefixStats.ModifierType.PercentageAdd => "Keen",
                    _ => "Precise"
                },
                
                WeaponPrefixStats.StatType.CriticalDamage => stat.modifierType switch
                {
                    WeaponPrefixStats.ModifierType.Multiplier => "Brutal",
                    WeaponPrefixStats.ModifierType.PercentageAdd => "Fierce",
                    _ => "Powerful"
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

        #region Testing Methods

        /// <summary>

        public void TestWeaponCalculations(Weapon weapon)
        {
            Console.WriteLine($"--- Testing Weapon: {weapon.Name} ---");
            Console.WriteLine($"Base Damage: {weapon.BaseDamage}");
            
            // Calculate level scaling
            float levelMod = GetDamageFromLevel(weapon.Level, weapon.GrowthRatePerLevel);
            Console.WriteLine($"Level Modifier ({weapon.Level}): {levelMod:F2}x");
            
            // Calculate rarity scaling
            float rarityMod = GetDamageModifier(weapon.RarityType);
            Console.WriteLine($"Rarity Modifier ({weapon.RarityType}): {rarityMod:F2}x");
            
            // Calculate effective base damage with prefixes
            float effectiveBaseDamage = weapon.GetEffectiveStat(weapon.BaseDamage, WeaponPrefixStats.StatType.AttackDamage);
            Console.WriteLine($"Effective Base Damage (with prefixes):{weapon.BaseDamage} + {(float)WeaponPrefixStats.StatType.AttackDamage} = {effectiveBaseDamage:F2}");
            
            // Calculate final damage
            float finalDamage = effectiveBaseDamage * rarityMod * levelMod;
            Console.WriteLine($"Final Damage Calculation: {effectiveBaseDamage:F2} * {rarityMod:F2} * {levelMod:F2} = {finalDamage:F2}");
            Console.WriteLine($"Actual Damage (cached): {weapon.ActualDamage:F2}");
            
            float effectiveCritChance = weapon.GetEffectiveStat(weapon.CriticalChance, WeaponPrefixStats.StatType.CriticalChance);
            Console.WriteLine($"Effective Critical Chance: {weapon.CriticalChance:P2} -> {effectiveCritChance:P2}");
            float effectiveCritDamage = weapon.GetEffectiveStat(weapon.CriticalDamage, WeaponPrefixStats.StatType.CriticalDamage);
            Console.WriteLine($"Effective Critical Damage: {weapon.CriticalDamage:F2}x -> {effectiveCritDamage:F2}x");
            
            // Price calculations
            int salePrice = weapon.CalculateSalePrice();
            int sellPrice = weapon.CalculateSellPrice();
            Console.WriteLine($"Sale Price: {salePrice}g, Sell Price: {sellPrice}g");
            
            Console.WriteLine("Bonus Stats:");
            foreach (var stat in weapon.BonusStats)
            {
                Console.WriteLine($"- {stat}");
            }
            Console.WriteLine("---------------------------");
        }

        #endregion
    }
}