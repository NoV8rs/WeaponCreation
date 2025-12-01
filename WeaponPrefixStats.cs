namespace WeaponCreation;

/// <summary>
/// A class for defining weapon prefix stats and their modifiers.
/// </summary>
public class WeaponPrefixStats
{
    /// <summary>
    /// How the stat is modified. (Added +, Percentage %, Multiplied x)
    /// </summary>
    public enum ModifierType
    {
        Flat,
        PercentageAdd,
        Multiplier
    }

    public enum StatType
    {
        MaxHealth,
        AttackDamage,
    }
    
    public StatType statType { get; set; }
    public ModifierType modifierType { get; set; }
    public float value { get; set; }
    
    public WeaponPrefixStats(StatType statType, ModifierType modifierType, float value)
    {
        this.statType = statType;
        this.modifierType = modifierType;
        this.value = value;
        
        if (modifierType == ModifierType.Multiplier)
        {
            this.value = Math.Clamp(value, 1.1f, 2.0f);
        }
    }
    
    public override string ToString()
    {
        // Rounded to the nearest tenth for display
        float roundedValue = MathF.Round(value, 1);
        float roundedPercent = MathF.Round(value * 100f, 1);

        return modifierType switch
        {
            ModifierType.Flat => $"+{roundedValue} {statType}",
            ModifierType.PercentageAdd => $"+{roundedPercent}% {statType}",
            ModifierType.Multiplier => $"{roundedValue}x {statType}",
            _ => "Unknown Modifier"
        };
    }
}