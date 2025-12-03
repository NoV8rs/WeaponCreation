namespace WeaponCreation;

public static class LootGenerator
{
    private static readonly Random _rng = new();
    
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
        var statTypes = Enum.GetValues<WeaponPrefixStats.StatType>();
        var selectedStat = statTypes[_rng.Next(statTypes.Length)];

        var allowedModifiers = WeaponPrefixStats.GetAllowedModifiers(selectedStat);
        var selectedMod = allowedModifiers[_rng.Next(allowedModifiers.Count)];

        (float min, float max) = WeaponPrefixStats.GetRange(selectedStat, selectedMod);
        float randomVal = min + (float)_rng.NextDouble() * (max - min);


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