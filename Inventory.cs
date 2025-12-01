namespace WeaponCreation;

/// <summary>
/// Player Inventory to manage weapons.
/// </summary>
public class Inventory
{
    // List to hold weapons in inventory slots
    private List<Weapon> _slots;
    // Maximum number of slots in the inventory
    private int _maxSlots;
    
    // Currently equipped weapon
    public Weapon? EquippedWeapon { get; private set; }
    
    /// <summary>
    /// Inventory Constructor
    /// </summary>
    /// <param name="maxSlots">How many slots the Player has to hold Weapons.</param>
    public Inventory(int maxSlots)
    {
        _maxSlots = maxSlots;
        _slots = new List<Weapon>(maxSlots);
    }
    
    /// <summary>
    /// Adds a weapon to the inventory if there is space.
    /// Uses the max slots defined in the constructor.
    /// </summary>
    /// <param name="weapon">Weapon that is stored in the Player inventory.</param>
    /// <returns></returns>
    public bool AddWeapon(Weapon weapon)
    {
        if (_slots.Count >= _maxSlots)
        {
            Console.WriteLine("Inventory is full. Cannot add weapon.");
            return false;
        }
        
        _slots.Add(weapon);
        Console.WriteLine($"Added {weapon.Name} to inventory.");
        return true;
    }
    
    /// <summary>
    /// Removes the specified weapon from the inventory.
    /// </summary>
    /// <param name="weapon">The Weapon that you want to remove.</param>
    /// <returns></returns>
    public bool RemoveWeapon(Weapon weapon)
    {
        if (_slots.Remove(weapon))
        {
            Console.WriteLine($"Removed {weapon.Name} from inventory.");
            return true;
        }
        
        Console.WriteLine($"{weapon.Name} not found in inventory.");
        return false;
    }
    
    /// <summary>
    /// Equips a weapon from the inventory by index.
    /// </summary>
    /// <param name="index">To store the Weapon.</param>
    public void EquipWeapon(int index) 
    {
        if (index < 0 || index >= _slots.Count) return;
    
        // Equip the weapon at the specified index
        EquippedWeapon = _slots[index];
        Console.WriteLine($"[Inventory] You equipped: {EquippedWeapon.Name}");
    }
    
    /// <summary>
    /// Unequips the currently equipped weapon.
    /// </summary>
    public void UnequipWeapon()
    {
        // EquippedWeapon is set to null
        if (EquippedWeapon == null) return;
        Console.WriteLine($"[Inventory] You unequipped: {EquippedWeapon?.Name}");
        EquippedWeapon = null; 
    }
    
    /// <summary>
    /// Prints all the weapons in the inventory.
    /// </summary>
    public void DisplayInventory()
    {
        Console.WriteLine("Inventory:");
        foreach (var weapon in _slots)
        {
            Console.WriteLine($"- {weapon.Name}, Rarity: {weapon.RarityType}, Level: {weapon.Level} (Damage: {weapon.ActualDamage}, Element: {weapon.Element}), {weapon.ToString()}");
        }
    }
}