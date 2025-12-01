# Weapon Creation System

## Overview
The **WeaponCreation** namespace provides a flexible, framework-independent weapon system for RPG games. It includes weapon types, rarity tiers, elemental attributes, and damage calculation with critical hits.

---

## Table of Contents
- [Features](#features)
- [Quick Start / Usage](#quick-start--usage)
  - [Basic Weapon Creation](#basic-weapon-creation)
  - [Elemental Weapon](#elemental-weapon)
  - [Testing Weapon in Debug Mode](#testing-weapon-in-debug-mode)
  - [Configuring Attack Speed](#configuring-attack-speed)
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
- **Multiple Weapon Types**: Sword, Axe, Bow, Staff
- **Rarity System**: Common, Uncommon, Rare, Epic, Legendary (with damage multipliers)
- **Elemental System**: None, Fire, Ice, Lightning, Earth
- **Dynamic Damage Calculation**: Min/Max damage ranges with random rolls
- **Critical Hit System**: Configurable critical chance and multiplier
- **Attack Speed**: Customizable attacks per second
- **Debug Tools**: Built-in attack simulation for testing

---

## Quick Start / Usage

### Basic Weapon Creation
```csharp
using WeaponCreation;

// Create a common sword
var sword = Weapon.CreateWeapon(
    name: "Iron Sword",
    type: WeaponType.Sword,
    rarity: Rarity.Common,
    damage: 50f,
    critChance: 0.1f,    // 10% crit chance
    critMult: 2.0f,      // 2x damage on crit
    weight: 5f
);

Console.WriteLine(sword);
// Output: Iron Sword [Common Sword] - Dmg: 50 (40.0-60.0) Weight: 5 Element: None
```

### Elemental Weapon
```csharp
var fireStaff = Weapon.CreateWeapon(
    name: "Staff of Flames",
    type: WeaponType.Staff,
    rarity: Rarity.Epic,
    damage: 75f,
    critChance: 0.25f,    // 25% crit chance
    critMult: 2.5f,       // 2.5x damage on crit
    weight: 3f,
    element: WeaponElement.Fire
);

Console.WriteLine(fireStaff);
// Output: Staff of Flames [Epic Staff] - Dmg: 131.25 (105.0-157.5) Weight: 3 Element: Fire
```

### Testing Weapon in Debug Mode
```csharp
#if DEBUG
var legendaryBow = Weapon.CreateWeapon(
    name: "Dragon Slayer Bow",
    type: WeaponType.Bow,
    rarity: Rarity.Legendary,
    damage: 100f,
    critChance: 0.3f,     // 30% crit chance
    critMult: 3.0f,       // 3x damage on crit
    weight: 4f,
    element: WeaponElement.Lightning
);

// Simulate 10 attacks
legendaryBow.AttackLoop(10);
#endif
```

### Configuring Attack Speed
```csharp
var quickDagger = Weapon.CreateWeapon(
    name: "Swift Dagger",
    type: WeaponType.Sword,
    rarity: Rarity.Rare,
    damage: 30f,
    critChance: 0.2f,
    critMult: 2.0f,
    weight: 1.5f
);

// Set to 2 attacks per second
quickDagger.AttackSpeedPerSecond = 2.0f;
```

---

## System Mechanics

### Damage System

The weapon uses a sophisticated damage calculation system:

1. **Base Damage**: The fundamental damage value
2. **Rarity Modifier**: Applied to base damage based on weapon rarity
3. **Damage Range**: Actual damage rolls between MinDamage (80% of ActualDamage) and MaxDamage (120% of ActualDamage)
4. **Critical Hits**: Random chance to multiply damage by CriticalMultiplier

**Formula:**
```
ActualDamage = BaseDamage × RarityModifier
MinDamage = ActualDamage × 0.8
MaxDamage = ActualDamage × 1.2
FinalDamage = Random(MinDamage, MaxDamage) × (IsCrit ? CriticalMultiplier : 1.0)
```

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
```csharp
public Weapon(
    string name,
    WeaponType type,
    Rarity rarity,
    float damage,
    float criticalChance,
    float criticalMultiplier,
    float weight,
    WeaponElement element = WeaponElement.None)
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `BaseDamage` | float | Base damage before rarity modifier |
| `CriticalChance` | float | Probability of critical hit (0.0 - 1.0) |
| `CriticalMultiplier` | float | Damage multiplier on critical hit |
| `AttackSpeedPerSecond` | float | Number of attacks per second |
| `ActualDamage` | float | Damage after applying rarity modifier |

#### Methods

##### CreateWeapon (Static Factory Method)
Creates a new weapon instance.

```csharp
public static Weapon CreateWeapon(
    string name,
    WeaponType type,
    Rarity rarity,
    float damage,
    float critChance,
    float critMult,
    float weight,
    WeaponElement element = WeaponElement.None)
```

**Parameters:**
- `name`: Name of the weapon
- `type`: Weapon type (Sword, Axe, Bow, Staff)
- `rarity`: Rarity tier (Common - Legendary)
- `damage`: Base damage value
- `critChance`: Critical hit chance (e.g., 0.15 for 15%)
- `critMult`: Critical damage multiplier (e.g., 2.0 for double damage)
- `weight`: Weight of the weapon
- `element`: Elemental attribute (default: None)

**Returns:** New `Weapon` instance

##### ToString
Returns a formatted string with weapon information.

**Example Output:**
```
Flaming Sword [Legendary Sword] - Dmg: 100 (80.0-120.0) Weight: 5 Element: Fire
```

##### AttackLoop (DEBUG only)
Simulates multiple attacks for testing purposes.

```csharp
public void AttackLoop(int loops)
```

**Parameters:**
- `loops`: Number of attacks to simulate

**Output:** Console output showing:
- Individual attack damage and critical hit status
- Total critical hits out of total attacks
- Base damage, min/max damage range
- Highest possible damage with critical hit

### Enums

#### WeaponType
Defines the type of weapon.
```csharp
public enum WeaponType
{
    Sword,
    Axe,
    Bow,
    Staff
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
2. **Static Factory Method**: `CreateWeapon()` provides clear weapon creation
3. **Strategy Pattern**: Rarity modifiers using switch expressions
4. **Encapsulation**: Private implementation details with public interface

### Integration with Other Systems

#### Enemy Weakness System
The `WeaponElement` enum integrates with the enemy system to apply weakness multipliers:

```csharp
// Create a fire-element weapon
var flamingSword = Weapon.CreateWeapon(
    name: "Flaming Blade",
    type: WeaponType.Sword,
    rarity: Rarity.Rare,
    damage: 80f,
    critChance: 0.15f,
    critMult: 2.0f,
    weight: 5f,
    element: WeaponElement.Fire
);

// Enemy weak to fire takes bonus damage
// See Enemies.cs TakeDamage() method for implementation
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
