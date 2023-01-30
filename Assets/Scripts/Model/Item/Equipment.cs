using Model;

public enum EquipmentType
{
    None,
    Helmet, // 头盔
    Armor, //护甲
    Boots, //鞋子
    Gloves, //手套
    Ornaments, //饰品
    OffHand, //副手武器 
    MainHand //主手武器
}

public interface IEquipable {
    string EquipDescription();
    EquipmentType Type();
    void PutOn(EquipmentInventory inventory);
    void PutOff(EquipmentInventory inventory);
}

public class EquipmentItemStack : UniqueItemStack
{
    public EquipmentType equipmentType;

    public override bool StoreItem(Item other, int num)
    {
        if (other.ItemType == ItemType.Equipment && (EquipmentType)other.SubItemType == equipmentType)
        {
            if(base.StoreItem(other, num)) {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }
}