namespace WeaponCreation;

public static class LootGenerator
{
    private static readonly Random _rng = new();
    
    private static Dictionary<WeaponPrefixStats.StatType, (float Min, float Max)> _weaponStats = new()
    {
        { WeaponPrefixStats.StatType.MaxHealth, (10f, 100f) },
        { WeaponPrefixStats.StatType.AttackDamage, (5f, 50f) },
        { WeaponPrefixStats.StatType.CriticalChance, (0.01f, 0.25f) },
        { WeaponPrefixStats.StatType.CriticalDamage, (1.5f, 3.0f) }
    };
    
    public static void RollStatsForWeapon(Weapon weapon)
    {
        int rollCount = GetRollCount(weapon.RarityType);

        for (int i = 0; i < rollCount; i++)
        {
            WeaponPrefixStats newStat = GenerateStat();
            
            // LOGIC: Check for duplicates and stack them
            var existing = weapon.BonusStats
                .FirstOrDefault(s => s.statType == newStat.statType && s.modifierType == newStat.modifierType);

            if (existing != null)
            {
                existing.value += newStat.value; // Stack the value
            }
            else
            {
                weapon.BonusStats.Add(newStat); // Add new stat
            }
        }
        
        weapon.UpdateNameWithPrefixes();
    }

    private static WeaponPrefixStats GenerateStat()
    {
        // 1. Pick a random Stat Type from our Dictionary keys
        var statTypes = _weaponStats.Keys.ToList();
        var selectedStat = statTypes[_rng.Next(statTypes.Count)];

        // 2. Pick a random Modifier
        var modifiers = Enum.GetValues<WeaponPrefixStats.ModifierType>();
        var selectedMod = modifiers[_rng.Next(modifiers.Length)];

        // 3. Calculate Value based on the Config Dictionary
        (float min, float max) = _weaponStats[selectedStat];
        
        // Standard C# math for random float between min and max
        float randomVal = min + ((float)_rng.NextDouble() * (max - min));

        // 4. Adjustments for specific modifier types (Pure C# logic)
        if (selectedMod == WeaponPrefixStats.ModifierType.PercentageAdd)
        {
            // If the range was 10-50, make it 10%-50% (0.1 - 0.5)
            randomVal /= 100f; 
        }
        else if (selectedMod == WeaponPrefixStats.ModifierType.Multiplier)
        {
            randomVal = Math.Clamp(1.0f + (randomVal / 100f), 1.0f, 2.0f); 
        }

        return new WeaponPrefixStats(selectedStat, selectedMod, randomVal);
    }

    private static int GetRollCount(Rarity rarity) => rarity switch
    {
        Rarity.Common => 0,
        Rarity.Uncommon => 1,
        Rarity.Rare => 2,
        Rarity.Epic => 3,
        Rarity.Legendary => 4,
        _ => 0
    };
}