using System;

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
    
    /// <summary>
    /// Stat types that can be modified by weapon prefixes.
    /// </summary>
    public enum StatType
    {
        MaxHealth,
        AttackDamage,
        CriticalChance,
        CriticalDamage,
    }
    
    // Properties
    public StatType statType { get; set; }
    public ModifierType modifierType { get; set; }
    public float value { get; set; }
    
    private sealed record ModifierRule(ModifierType Type, Func<float, float> NormalizeValue);

    private static readonly Dictionary<StatType, List<ModifierRule>> _statModifierRules = new()
    {
        {
            StatType.MaxHealth,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => MathF.Max(0f, value)),
                new ModifierRule(ModifierType.PercentageAdd, value => Math.Clamp(value, -0.5f, 0.75f))
            }
        },
        {
            StatType.AttackDamage,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => value),
                new ModifierRule(ModifierType.PercentageAdd, value => Math.Clamp(value, 0.5f, 1.0f)),
                new ModifierRule(ModifierType.Multiplier, value => Math.Clamp(value <= 1.0f ? 1.1f : value, 1.1f, 3.0f))
            }
        },
        {
            StatType.CriticalChance,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => Math.Clamp(value / 100f, 0.05f, 0.25f)),
                new ModifierRule(ModifierType.PercentageAdd, value => Math.Clamp(value / 100f, 0.25f, 0.5f))
            }
        },
        {
            StatType.CriticalDamage,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => Math.Clamp(value, 0.2f, 1.0f)),
                new ModifierRule(ModifierType.PercentageAdd, value => Math.Clamp(value, 0.25f, 0.75f)),
                new ModifierRule(ModifierType.Multiplier, value => Math.Clamp(value <= 1.0f ? 1.25f : value, 1.25f, 4.0f))
            }
        }
    };

    private static ModifierRule ResolveModifierRule(StatType statType, ModifierType requested)
    {
        if (!_statModifierRules.TryGetValue(statType, out var rules) || rules.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(statType), $"Stat type '{statType}' is not configured for prefix modifiers.");
        }

        return rules.FirstOrDefault(rule => rule.Type == requested) ?? rules[0];
    }
    
    /// <summary>
    /// Weapon Prefix Stats Constructor
    /// </summary>
    /// <param name="statType">Type of stats that can be on the Weapon.</param>
    /// <param name="modifierType">Percent, Add, and Mult.</param>
    /// <param name="value">The value for the increases by the modifier.</param>
    public WeaponPrefixStats(StatType statType, ModifierType modifierType, float value)
    {
        this.statType = statType;
        var rule = ResolveModifierRule(statType, modifierType);
        this.modifierType = rule.Type;
        this.value = rule.NormalizeValue(value);
    }
    
    /// <summary>
    /// Prints the modifier in a rounded format for display.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        float roundedValue = MathF.Round(value, 2);
        float roundedPercent = MathF.Round(value * 100f, 2);

        return modifierType switch
        {
            ModifierType.Flat => $"+ {roundedValue} {statType}",
            ModifierType.PercentageAdd => $"+ {roundedPercent}% {statType}",
            ModifierType.Multiplier => $"{roundedValue}x {statType}",
            _ => "Unknown Modifier"
        };
    }
}