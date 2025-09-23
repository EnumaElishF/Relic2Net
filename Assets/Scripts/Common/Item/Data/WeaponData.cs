
public class WeaponData : ItemDataBase
{
    //public override ItemDataBase Copy()
    //{
    //    return new WeaponData { id = id };
    //}
    public override ItemType GetItemType()
    {
        return ItemType.Weapon;
    }
}
