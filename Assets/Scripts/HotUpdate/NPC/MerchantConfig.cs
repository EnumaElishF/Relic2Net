using JKFrame;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/MerchantConfig")]
public class MerchantConfig : ConfigBase
{
    //当前设计是把需要加入商店的Config数据，其他商店需要的类，手动在Config上加。  -后续有需求，再改为自动配置
    public List<ItemConfigBase> items = new List<ItemConfigBase>();

}
