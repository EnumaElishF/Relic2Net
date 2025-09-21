using JKFrame;
using UnityEngine;

public class LoginSceneManager : MonoBehaviour
{


    void Start()
    {
        UISystem.Show<UI_MainMenuWindow>();

    }
    private void Update()
    {
        //TODO : 临时测试背包显示逻辑
        if (Input.GetKeyDown(KeyCode.B))
        {
            UI_BagWindow bagWindow = UISystem.GetWindow<UI_BagWindow>();
            if(bagWindow == null || !bagWindow.gameObject.activeInHierarchy)
            {
                BagData bagData = new BagData();
                bagData.itemList.Add(new WeaponData() { id = "Weapon_0" }); 
                bagData.itemList.Add(new WeaponData() { id = "Weapon_1" }); 
                bagData.itemList.Add(new ConsumableData() { id = "Consumable_0", count = 1 }); 
                bagData.itemList.Add(new ConsumableData() { id = "Consumable_1", count = 2 }); 
                bagData.itemList.Add(new ConsumableData() { id = "Consumable_2", count = 3 }); 
                bagData.itemList.Add(new ConsumableData() { id = "Consumable_3", count = 4 }); 
                bagData.itemList.Add(new ConsumableData() { id = "Consumable_4", count = 5 }); 
                bagData.itemList.Add(new MaterialData() { id = "Material_0", count = 5 }); 
                bagData.itemList.Add(new MaterialData() { id = "Material_1", count = 7 }); 
                bagData.itemList.Add(new MaterialData() { id = "Material_2", count = 8 }); 
                for(int i = 0; i < 20; i++)
                {
                    bagData.itemList.Add(null);
                }
                UISystem.Show<UI_BagWindow>().Show(bagData);
            }
            else
            {
                UISystem.Close<UI_BagWindow>();
            }
        }
    }


}