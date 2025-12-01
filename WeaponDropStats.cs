namespace WeaponCreation;

public class WeaponDropStats
{
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
    
    public WeaponDropStats(StatType statType, ModifierType modifierType, float value)
    {
        this.statType = statType;
        this.modifierType = modifierType;
        this.value = value;
    }
    
    public override string ToString()
    {
        return modifierType switch
        {
            ModifierType.Flat => $"+{value} {statType}",
            ModifierType.PercentageAdd => $"+{value * 100:0}% {statType}",
            ModifierType.Multiplier => $"{value}x {statType}",
            _ => "Unknown Modifier"
        };
    }
}