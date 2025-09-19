using System.Collections.Generic;

public class BagData //背包的格子是有固定的数量上限，空格子的表现为 itemList[i] = null
{
    public List<ItemDataBase> itemList = new List<ItemDataBase>();
}