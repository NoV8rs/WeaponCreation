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


    /// <summary>
    /// Represents a rule that defines how a modifier is applied and normalized for a given stat type.
    /// </summary>
    private sealed record ModifierRule(ModifierType Type, Func<float, float> NormalizeValue);

    /// <summary>
    /// Values ranges for each stat type and modifier combination.
    /// </summary>
    private static readonly Dictionary<(StatType Stat, ModifierType Modifier), (float Min, float Max)> _statValueRanges = new()
    {
        {(StatType.MaxHealth, ModifierType.Flat), (10f, 100f)},
        {(StatType.MaxHealth, ModifierType.PercentageAdd), (0.5f, 0.75f)},
        {(StatType.AttackDamage, ModifierType.Flat), (5f, 50f)},
        {(StatType.AttackDamage, ModifierType.PercentageAdd), (0.5f, 1.0f)},
        {(StatType.AttackDamage, ModifierType.Multiplier), (1.1f, 3.0f)},
        {(StatType.CriticalChance, ModifierType.Flat), (0.05f, 0.25f)},
        {(StatType.CriticalChance, ModifierType.PercentageAdd), (0.25f, 0.5f)},
        {(StatType.CriticalDamage, ModifierType.Flat), (0.2f, 1.0f)},
        {(StatType.CriticalDamage, ModifierType.PercentageAdd), (0.25f, 0.75f)},
        {(StatType.CriticalDamage, ModifierType.Multiplier), (1.25f, 4.0f)}
    };

    /// <summary>
    /// Adds rules for each stat type and modifier combination.
    /// Like clamping values to certain ranges and normalizing percentages.
    /// </summary>
    private static readonly Dictionary<StatType, List<ModifierRule>> _statModifierRules = new()
    {
        // This code rounds the values to the nearest hundredth for each stat type and modifier combination.
        {
            StatType.MaxHealth,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.PercentageAdd, value => MathF.Round(value, 2))
            }
        },
        {
            StatType.AttackDamage,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.PercentageAdd, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.Multiplier, value => MathF.Round(value <= 1.0f ? 1.1f : value, 2))
            }
        },
        {
            StatType.CriticalChance,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.PercentageAdd, value => MathF.Round(value, 2))
            }
        },
        {
            StatType.CriticalDamage,
            new()
            {
                new ModifierRule(ModifierType.Flat, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.PercentageAdd, value => MathF.Round(value, 2)),
                new ModifierRule(ModifierType.Multiplier, value => MathF.Round(value <= 1.0f ? 1.25f : value, 2))
            }
        }
    };

    public static IReadOnlyList<ModifierType> GetAllowedModifiers(StatType statType)
    {
        if (_statModifierRules.TryGetValue(statType, out var rules) && rules.Count > 0)
        {
            return rules.Select(rule => rule.Type).Distinct().ToList();
        }

        throw new ArgumentOutOfRangeException(nameof(statType), $"Stat type '{statType}' is not configured for prefix modifiers.");
    }
    
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
    
    public static (float Min, float Max) GetRange(StatType statType, ModifierType modifierType)
    {
        if (_statValueRanges.TryGetValue((statType, modifierType), out var range))
        {
            return range;
        }

        throw new ArgumentOutOfRangeException(nameof(statType),
            $"No range configured for stat '{statType}' with modifier '{modifierType}'.");
    }
    
    /// <summary>
    /// Prints the modifier in a rounded format for display.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        float roundedValue = MathF.Round(value, 2);
        float roundedPercent = MathF.Round(value * 100f, 2);
        
        if (statType == StatType.CriticalChance && modifierType == ModifierType.Flat)
        {
            return $"+ {roundedPercent:F}% {statType}";
        }

        return modifierType switch
        {
            ModifierType.Flat => $"+ {roundedValue} {statType}",
            ModifierType.PercentageAdd => $"+ {roundedPercent}% {statType}",
            ModifierType.Multiplier => $"{roundedValue}x {statType}",
            _ => "Unknown Modifier"
        };
    }

    #region TestingHelpers

    /// <summary>
    /// Testing helper to print min and max values for the stat and modifier.
    /// </summary>
    public void PrintModifierMinMax()
    {
        var (min, max) = GetRange(statType, modifierType);
        Console.WriteLine($"Stat: {statType}, Modifier: {modifierType}, Value: {value:F} , Min: {min}, Max: {max}");
    }

    #endregion
}