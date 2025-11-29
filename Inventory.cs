namespace WeaponCreation;

public class Inventory
{
    private List<Weapon> _slots;
    private int _maxSlots;
    
    public Weapon EquippedWeapon { get; private set; }
    
    public Inventory(int maxSlots)
    {
        _maxSlots = maxSlots;
        _slots = new List<Weapon>(maxSlots);
    }
    
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
    
    public void EquipWeapon(int index) 
    {
        if (index < 0 || index >= _slots.Count) return;
    
        EquippedWeapon = _slots[index];
        Console.WriteLine($"[Inventory] You equipped: {EquippedWeapon.Name}");
    }
    
    public void DisplayInventory()
    {
        Console.WriteLine("Inventory:");
        foreach (var weapon in _slots)
        {
            Console.WriteLine($"- {weapon.Name}, Level: {weapon.Level} (Damage: {weapon.ActualDamage}, Element: {weapon.Element})");
        }
    }
    
    public int Count => _slots.Count;
}