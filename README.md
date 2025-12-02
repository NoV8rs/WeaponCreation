# Weapon Creation System

## Overview
The **WeaponCreation** namespace provides a flexible, framework-independent weapon system for RPG games. It includes weapon types, rarity tiers, elemental attributes, and damage calculation with critical hits.

---

## Table of Contents
- [Features](#features)
- [Quick Start / Usage](#quick-start--usage)
  - [Loading Weapons from CSV](#loading-weapons-from-csv)
  - [Accessing and Using Individual Weapons](#accessing-and-using-individual-weapons)
  - [Creating Weapon Copies at Different Levels](#creating-weapon-copies-at-different-levels)
  - [Rolling Random Stats for Weapons](#rolling-random-stats-for-weapons)
  - [Testing Weapon in Debug Mode](#testing-weapon-in-debug-mode)
- [System Mechanics](#system-mechanics)
  - [Damage System](#damage-system)
  - [Rarity Multipliers](#rarity-multipliers)
  - [Elemental Attributes](#elemental-attributes)
- [API Reference](#api-reference)
  - [Weapon Class](#weapon-class)
    - [Constructor](#constructor)
    - [Properties](#properties)
    - [Methods](#methods)
  - [Enums](#enums)
    - [WeaponType](#weapontype)
    - [Rarity](#rarity)
    - [WeaponElement](#weaponelement)
  - [WeaponPrefixStats Class](#weaponprefixstats-class)
- [Architecture & Design](#architecture--design)
  - [Framework Independence](#framework-independence)
  - [Random Number Generation](#random-number-generation)
  - [Performance Considerations](#performance-considerations)
  - [Design Patterns Used](#design-patterns-used)
  - [Integration with Other Systems](#integration-with-other-systems)
- [Roadmap](#roadmap)
- [Version History](#version-history)

---

## Features
- **CSV-Based Weapon Loading**: Easy weapon creation and balancing through CSV files
- **Multiple Weapon Types**: Sword, Axe, Bow, Staff, Dagger, Hammer
- **Rarity System**: Common, Uncommon, Rare, Epic, Legendary (with damage multipliers)
- **Elemental System**: None, Fire, Ice, Lightning, Earth
- **Level Scaling**: Weapons scale exponentially with configurable growth rates
- **Dynamic Damage Calculation**: Min/Max damage ranges with random rolls
- **Critical Hit System**: Configurable critical chance and multiplier
- **Bonus Stats System**: Random stat rolling based on weapon rarity with prefix names
- **Weapon Copying**: Create scaled copies of weapons at different levels
- **Economy System**: Automated price calculation for buying and selling
- **Debug Tools**: Built-in attack simulation for testing

---

## Quick Start / Usage

### Loading Weapons from CSV
Weapons are created by loading them from a CSV file. This is the primary way to create weapons.

```csharp
using WeaponCreation;

// Load all weapons from the CSV file
List<Weapon> weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");

// Display all loaded weapons
foreach (var weapon in weapons)
{
    Console.WriteLine(weapon);
}
// Output: [W001] Iron Sword 50 (Common Sword) | DMG: 2815 | ELM: None | VAL: 28290g, SELL: 4243g
```

### Accessing and Using Individual Weapons
```csharp
// Load weapons from CSV
var weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");

// Get a specific weapon by index or search
var ironSword = weapons.First(w => w.Name == "Iron Sword");
var flameAxe = weapons.First(w => w.Id == "W002");

Console.WriteLine($"Weapon: {ironSword.Name}");
Console.WriteLine($"Damage: {ironSword.ActualDamage}");
Console.WriteLine($"Level: {ironSword.Level}");
```

### Creating Weapon Copies at Different Levels
```csharp
// Load weapons from CSV
var weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");
var baseWeapon = weapons.First(w => w.Name == "Iron Sword");

// Create a copy of the weapon at a higher level
var leveledWeapon = baseWeapon.CreateCopyAtLevel(100);

Console.WriteLine(leveledWeapon);
// The weapon will have scaled damage based on its growth rate
```

### Rolling Random Stats for Weapons
```csharp
// Load weapons
var weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");

// Roll random bonus stats based on weapon rarity
// Common = 0 stats, Uncommon = 1, Rare = 2, Epic = 3, Legendary = 4
foreach (var weapon in weapons)
{
    Weapon.RollStatsForWeapon(weapon);
    Console.WriteLine(weapon); // Will show bonus stats if any
}
```

### Testing Weapon in Debug Mode
```csharp
#if DEBUG
// Load weapons
var weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");
var testWeapon = weapons.First(w => w.Id == "W002"); // Flame Axe

// Simulate 10 attacks to test damage and crit rates
testWeapon.AttackLoop(10);
// Output shows hit count, crit count, and estimated value
#endif
```

---

## System Mechanics

### Damage System

The weapon uses a sophisticated damage calculation system with multiple scaling factors:

1. **Base Damage**: The fundamental damage value from the CSV
2. **Rarity Modifier**: Applied to base damage based on weapon rarity
3. **Level Scaling**: Damage increases exponentially based on weapon level and growth rate
4. **Damage Range**: Actual damage rolls between MinDamage (80% of ActualDamage) and MaxDamage (120% of ActualDamage)
5. **Critical Hits**: Random chance to multiply damage by CriticalMultiplier

**Formulas:**
```
LevelScaling = GrowthRate ^ (Level - 1)
ActualDamage = BaseDamage × RarityModifier × LevelScaling
MinDamage = ActualDamage × 0.8
MaxDamage = ActualDamage × 1.2
FinalDamage = Random(MinDamage, MaxDamage) × (IsCrit ? CriticalMultiplier : 1.0)
```

**Example:**
- BaseDamage: 10
- Rarity: Legendary (2.0x multiplier)
- Level: 50
- GrowthRate: 1.1
- Result: ActualDamage = 10 × 2.0 × (1.1^49) ≈ 2,815 damage

### Rarity Multipliers

Weapon rarity determines damage multipliers applied to base damage:
- **Common**: 1.0x damage
- **Uncommon**: 1.25x damage
- **Rare**: 1.5x damage
- **Epic**: 1.75x damage
- **Legendary**: 2.0x damage

### Elemental Attributes

Elemental attributes enable integration with enemy weakness systems. The `WeaponElement` enum supports the following elements:
- **None**: No elemental attribute
- **Fire**: Fire damage (effective against ice enemies)
- **Ice**: Ice damage (effective against fire enemies)
- **Lightning**: Lightning damage (effective against water enemies)
- **Earth**: Earth damage (effective against flying enemies)

---

## API Reference

### Weapon Class

#### Constructor
The constructor is **private** and used internally. Weapons are created through CSV loading.

```csharp
private Weapon(
    string id,
    string name,
    WeaponType type,
    Rarity rarity,
    float damage,
    float range,
    float weight,
    float critChance,
    float critMult,
    WeaponElement element,
    float growthPerLevel,
    int level)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique identifier for the weapon (e.g., "W001") |
| `Name` | string | Name of the weapon (updates with prefixes from bonus stats) |
| `Type` | WeaponType | Type of weapon (Sword, Axe, Bow, Staff, Dagger, Hammer) |
| `RarityType` | Rarity | Rarity tier of the weapon |
| `Element` | WeaponElement | Elemental attribute of the weapon |
| `BaseDamage` | float | Base damage before modifiers |
| `CriticalChance` | float | Probability of critical hit (0.0 - 1.0) |
| `CriticalMultiplier` | float | Damage multiplier on critical hit |
| `AttackSpeedPerSecond` | float | Number of attacks per second (default: 1.0) |
| `Level` | int | Current level of the weapon |
| `GrowthRatePerLevel` | float | Damage scaling factor per level |
| `ActualDamage` | float | Damage after applying rarity, level scaling (cached) |
| `BonusStats` | List\<WeaponPrefixStats\> | List of bonus stats applied to the weapon |
| `Range` | float | Range of the weapon (may be removed later) |
| `Weight` | float | Weight of the weapon (may be removed later) |

#### Methods

##### LoadWeaponsFromCsv (Static - Primary Creation Method)
Loads weapons from a CSV file. This is the main way to create weapons.

```csharp
public static List<Weapon> LoadWeaponsFromCsv(string path = "weapons.csv")
```

**Parameters:**
- `path`: Path to the CSV file containing weapon data (default: "weapons.csv")

**Returns:** `List<Weapon>` containing all loaded weapons

**CSV Format:**
```
ID,Name,Type,Rarity,BaseDamage,Range,Weight,CritChance,CritMult,Element,GrowthRate,Level
W001,Iron Sword,Sword,Common,10,1.5,5.0,0.1,2.0,None,1.1,50
```

##### CreateCopyAtLevel
Creates a copy of the weapon at a specific level. Great for scaling enemies or loot drops.

```csharp
public Weapon CreateCopyAtLevel(int targetLevel)
```

**Parameters:**
- `targetLevel`: The level for the new weapon copy

**Returns:** New `Weapon` instance at the specified level

##### RollStatsForWeapon (Static)
Rolls random bonus stats for a weapon based on its rarity.

```csharp
public static void RollStatsForWeapon(Weapon weapon)
```

**Parameters:**
- `weapon`: The weapon to roll stats for

**Stat Count by Rarity:**
- Common: 0 stats
- Uncommon: 1 stat
- Rare: 2 stats
- Epic: 3 stats
- Legendary: 4 stats

##### CalculateBonusStat
Calculates the bonus value for a specific stat type based on the weapon's bonus stats.

```csharp
public float CalculateBonusStat(WeaponPrefixStats.StatType statToCalc, float playerBaseValue)
```

**Parameters:**
- `statToCalc`: The type of stat to calculate
- `playerBaseValue`: The base value from the player

**Returns:** Final calculated stat value after applying all modifiers

##### CalculateSalePrice / CalculateSellPrice
Calculate the sale and sell prices of the weapon.

```csharp
public int CalculateSalePrice()  // Price to buy from shop
public int CalculateSellPrice()  // Price when selling (15% of sale price)
```

##### ToString
Returns a formatted string with weapon information.

**Example Output:**
```
[W002] Flame Axe 5 (Legendary Axe) | DMG: 125 | ELM: Fire | VAL: 1260g, SELL: 189g
```

##### AttackLoop (DEBUG only)
Simulates multiple attacks for testing purposes.

```csharp
public void AttackLoop(int loops)
```

**Parameters:**
- `loops`: Number of attacks to simulate

**Output:** Console output showing hit count, crit count, and estimated value

### Enums

#### WeaponType
Defines the type of weapon.
```csharp
public enum WeaponType
{
    Sword,
    Axe,
    Bow,
    Staff,
    Dagger,
    Hammer
}
```

#### Rarity
Defines weapon rarity with corresponding damage multipliers:
- **Common**: 1.0x damage
- **Uncommon**: 1.25x damage
- **Rare**: 1.5x damage
- **Epic**: 1.75x damage
- **Legendary**: 2.0x damage

```csharp
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
```

#### WeaponElement
Defines elemental attributes for weapons (used with enemy weakness system).
```csharp
public enum WeaponElement
{
    None,
    Fire,
    Ice,
    Lightning,
    Earth
}
```

### WeaponPrefixStats Class

The `WeaponPrefixStats` class defines bonus stats that can be rolled on weapons. These stats modify player attributes when the weapon is equipped.

#### StatType Enum
```csharp
public enum StatType
{
    MaxHealth,
    AttackDamage
}
```

#### ModifierType Enum
```csharp
public enum ModifierType
{
    Flat,           // Adds a flat value (e.g., +20 Health)
    PercentageAdd,  // Adds a percentage (e.g., +25% Health)
    Multiplier      // Multiplies the value (e.g., 1.5x Health)
}
```

#### Properties
- `statType`: The type of stat being modified (MaxHealth or AttackDamage)
- `modifierType`: How the stat is modified (Flat, PercentageAdd, or Multiplier)
- `value`: The numeric value of the modifier

#### Calculation Order
When multiple modifiers affect the same stat:
```
FinalValue = (BaseValue + FlatSum) × (1 + PercentSum) × (MultiplierProduct)
```

**Example:**
- Base Health: 100
- Flat: +20
- Percentage: +25% (0.25)
- Multiplier: 1.5x
- Result: (100 + 20) × (1 + 0.25) × 1.5 = 225 Health

---

## Architecture & Design

### Framework Independence
- **No Dependencies**: This system is designed to work without Unity, Unreal Engine, or any specific framework
- **Pure C# Implementation**: Uses only standard .NET libraries
- **Portable**: Can be integrated into any C# project (console apps, games, etc.)

### Random Number Generation
Uses `Random.Shared` (available in .NET 6+) for thread-safe random number generation without seeding issues.

### Performance Considerations
- **Struct Enums**: Lightweight enum types for performance
- **Property Caching**: Calculated values use properties with getter logic
- **Minimal Allocations**: Value types and pre-calculated modifiers

### Design Patterns Used

1. **Primary Constructor**: Modern C# 12+ primary constructor syntax
2. **CSV Data Loading Pattern**: Weapons are loaded from external data files for easy balancing
3. **Static Factory Method**: `LoadWeaponsFromCsv()` provides centralized weapon creation
4. **Strategy Pattern**: Rarity modifiers using switch expressions
5. **Encapsulation**: Private constructor and implementation details with public interface

### Integration with Other Systems

#### Enemy Weakness System
The `WeaponElement` enum integrates with the enemy system to apply weakness multipliers:

```csharp
// Load weapons from CSV
var weapons = Weapon.LoadWeaponsFromCsv("weapons.csv");

// Get a fire-element weapon
var flameAxe = weapons.First(w => w.Element == WeaponElement.Fire);

// Enemy weak to fire takes bonus damage
// See Enemies.cs TakeDamage() method for implementation
Console.WriteLine($"{flameAxe.Name} deals {flameAxe.Element} damage");
```

---

## Roadmap

Potential additions to the system:
- Weapon durability system
- Enchantment/upgrade system
- Weapon skill requirements
- Special abilities per weapon type
- Damage over time (DOT) effects for elements
- Weapon sets with bonuses

---

## Version History

**Current Version**: 1.0
- Initial implementation with core weapon system
- Rarity tiers with damage modifiers
- Elemental system
- Critical hit calculations
- Debug testing tools

---
