// Should not be dependent on Frameworks or Unity.

namespace WeaponCreation
{
    /// <summary>
    /// Weapon Types (Sword, Axe, etc)
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Axe,
        Bow,
        Staff
    }

    /// <summary>
    /// Weapon Rarity Types (Common, Rare, etc)
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    /// <summary>
    /// Weapon Element Types (Fire, Ice, etc)
    /// </summary>
    public enum WeaponElement
    {
        None,
        Fire,
        Ice,
        Lightning,
        Earth
    }
    
    public class Weapon(
        string name,
        WeaponType type,
        Rarity rarity,
        float damage,
        float criticalChance,
        float criticalMultiplier,
        float weight,
        WeaponElement element = WeaponElement.None)
    {
        private string Name { get; } = name;
        private WeaponType Type { get; } = type;
        private Rarity RarityType { get; } = rarity;
        private WeaponElement Element { get; } = element;
        private float Weight { get; } = weight;
        public float BaseDamage { get; set; } = damage; // Base damage before rarity modifier
        public float CriticalChance { get; set; } = criticalChance; // 10% default
        public float CriticalMultiplier { get; set; } = criticalMultiplier; // 2x damage on crit
        public float AttackSpeedPerSecond { get; set; } = 1.0f; // 1 attack per second

        public float ActualDamage // Damage after applying Rarity modifier
        {
            get => BaseDamage * (int)GetDamageModifier(RarityType); // Adjusted to int for clearer scaling
            set => BaseDamage = value / (int)GetDamageModifier(RarityType); // Reverse calculation to set BaseDamage
        }

        private float MinDamage => ActualDamage * 0.8f;   
        private float MaxDamage => ActualDamage * 1.2f; 

        /// <summary>
        /// Modifier for Damage based on Rarity
        /// </summary>
        /// <param name="rarity">Common - Legendary</param>
        /// <returns></returns>
        private static float GetDamageModifier(Rarity rarity) => rarity switch
        {
            Rarity.Common => 1.0f, 
            Rarity.Uncommon => 1.25f,
            Rarity.Rare => 1.5f,
            Rarity.Epic => 1.75f,
            Rarity.Legendary => 2.0f,
            _ => 1.0f // Default case
        };

        public override string ToString()
        {
            return $"{Name} [{RarityType} {Type}] - Dmg: {ActualDamage} ({MinDamage:F1}-{MaxDamage:F1}) Weight: {Weight} Element: {Element} Price: {CalculateSalePrice()}, Sell Price: {CalculateSellPrice()}";
        }

        private int CalculateDamage()
        {
            int damage = RollDamage();
            if (IsCriticalHit())
            {
                damage = (int)(damage * CriticalMultiplier);
            }
            return damage;
        }
        
        private int RollDamage()
        {
            return Random.Shared.Next((int)MinDamage, (int)MaxDamage + 1);
        }
        
        private bool IsCriticalHit()
        {
            return Random.Shared.NextDouble() < CriticalChance;
        }
        
        /// <summary>
        /// Create Weapon - Static Factory Method
        /// </summary>
        /// <param name="name">Name of the Weapon.</param>
        /// <param name="type">Weapon Type (Bow, Staff...)</param>
        /// <param name="rarity">Common - Legendary.</param>
        /// <param name="damage">Damage of the Weapon.</param>
        /// <param name="weight">Weight of the Weapon.</param>
        /// <param name="element">Element Type, Default is None.</param>
        /// <returns></returns>
        public static Weapon CreateWeapon(string name, WeaponType type, Rarity rarity, float damage, float critChance, float critMult, float weight,
            WeaponElement element = WeaponElement.None)
        {
            return new Weapon(name, type, rarity, damage, critChance, critMult, weight, element);
        }
        
        #region Price Calculation
        
        /// <summary>
        /// Calculates the multiplier based on Rarity.
        /// Calls in Price Calculation.
        /// </summary>
        /// <returns></returns>
        private int CalculateSaleMultiplierWithRarity()
        {
            return RarityType switch
            {
                // Change these values to adjust pricing scale
                // Multiply by base price
                Rarity.Common => 1,
                Rarity.Uncommon => 2,
                Rarity.Rare => 3,
                Rarity.Epic => 5,
                Rarity.Legendary => 8,
                _ => 1
            };
        }
        
        /// <summary>
        /// Calculates the Sale Price of the Weapon.
        /// Formula is Sale Price = (Actual Damage * 10 + Weight * 5) * Rarity Multiplier.
        /// </summary>
        /// <returns></returns>
        private int CalculateSalePrice()
        {
            // Base price calculation
            int basePrice = (int)(ActualDamage * 10 + Weight * 5);
            // Apply rarity multiplier
            int rarityMultiplier = CalculateSaleMultiplierWithRarity();
            return basePrice * rarityMultiplier;
        }
        
        // Sell Price
        /// <summary>
        /// Formula is Sell Price = Sale Price * (BasePriceDivide / 2).
        /// Sale Price is calculated from CalculateSalePrice().
        /// </summary>
        /// <returns></returns>
        private int CalculateSellPrice()
        {
            int salePrice = CalculateSalePrice();
            const float basePriceDivide = 0.15f;
            return (int)(salePrice * basePriceDivide);
        }
        
        #endregion

        #if DEBUG
        /// <summary>
        /// Debug method to simulate multiple attacks.
        /// 
        /// </summary>
        public void AttackLoop(int loops)
        {
            // Simulate multiple attacks to see damage distribution and crits
            int howManyAttacks = (int)(AttackSpeedPerSecond);
            int howManyCrits = 0;
            int baseDamage = (int)ActualDamage;
            int minDamage = (int)MinDamage;
            int maxDamage = (int)MaxDamage;
            int highestPossibleDamage = (int)(MaxDamage * CriticalMultiplier);
            
            for (var i = 0; i < loops; i++)
            {
                int damage = CalculateDamage();
                for (var j = 0; j < howManyAttacks; j++)
                {
                    if (IsCriticalHit()) howManyCrits++;
                }
                Console.WriteLine($"Calculated Damage: {damage}, Attack #{i + 1}, Critical: {(damage > MaxDamage ? "Yes" : "No")}");
            }
            Console.WriteLine(howManyCrits + " Critical Hits landed out of " + (loops * howManyAttacks) + " attacks.");
            Console.WriteLine(baseDamage + " Base Damage, Min Damage: " + minDamage + ", Max Damage: " + maxDamage);
            Console.WriteLine("Highest Possible Damage (Max * Crit Mult): " + highestPossibleDamage);
        }
        #endif
    }
}