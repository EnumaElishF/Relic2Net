//使用PlayerManager和PlayerController，作为公共的部分，既会出现在Client又会出现在Server，
//从而要求他们不能去依赖客户端或者服务端的程序集的内容，要打断他们的依赖关系，可以通过事件，去传，从而跨程序集通信
using Cinemachine;
using JKFrame;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// PlayerManager放到了热更新程序集HotUpdate里，那么就作为 只有客户端使用
/// </summary>
public class PlayerManager : SingletonMono<PlayerManager>
{
    [SerializeField] private CinemachineFreeLook cinemachine;
    //取消使用静态变量static,因为跨场景的时候曾经的PlayerController并没有被销毁，而是放对象池中，静态变量指向的变量是全局唯一，且是强绑定
    public PlayerClientController localPlayer { get; private set; } 
    //玩家是否可以控制角色，以后可能受到多个方面的影响，目前只和鼠标显示关联
    public bool playerControlEnable { get; private set; }
    public int UsedWeaponIndex => bagData.usedWeaponIndex;

    private bool requestOpenBagWindow = false;
    public BagData bagData { get; private set; }
    /// <summary>
    /// PlayerManaget在Awake里触发xxx改为手动Init，然后要求PlayerController才在后续方法里面触发
    /// </summary>
    public void Init()
    {
        Debug.Log("客户端的玩家初始化");
        PlayerController.SetGetWeaponFunc(GetWeapon);

        //事件的监听开始
        EventSystem.AddTypeEventListener<SpawnPlayerEvent>(OnSpawnPlayerEvent);
        EventSystem.AddTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);
        //TODO 每次新加，玩家相关回复给客户端的消息
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_GetBagData, OnS_C_GetBagData);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_BagUpdateItem, OnS_C_UpdateItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_ShortcutBarUpdateItem, OnS_C_ShortcutBarUpdateItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_UpdateCoinCount, OnS_C_UpdateCoinCount);

        ClientGlobal.Instance.ActiveMouse = false;
        RequestBagData();
    }


    private void OnDestroy()
    {
        //事件监听取消，针对内部方法
        //检查所有的的EventSystem的事件绑定，因为PlayerManager脚本并不是全局的，他会因为场景卸载而关闭
        //那么就需要把事件取消掉，所以加了下面这个RemoveTypeEventListener移除事件监听
        //以此类似的还有很多，但是像是ClientGlobal这种一直存在，就不需要加下面的处理
        EventSystem.RemoveTypeEventListener<SpawnPlayerEvent>(OnSpawnPlayerEvent);
        EventSystem.RemoveTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);//每次在ClientGlobal的ActiveMouse触发
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_GetBagData, OnS_C_GetBagData);
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_BagUpdateItem, OnS_C_UpdateItem);
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_ShortcutBarUpdateItem, OnS_C_ShortcutBarUpdateItem);

    }

    private void OnSpawnPlayerEvent(SpawnPlayerEvent arg)
    {
        if(!arg.newPlayer.TryGetComponent(out PlayerClientController clientController))
        {
            clientController = arg.newPlayer.gameObject.AddComponent<PlayerClientController>();
            clientController.FirstInit(arg.newPlayer);
        }
        clientController.Init();
        if (arg.newPlayer.IsSpawned)
        {
            InitLocalPlayer(clientController);
        }
    }
    private void OnMouseActiveStateChangedEvent(MouseActiveStateChangedEvent arg)
    {
        //目前只和鼠标是否显示关联
        playerControlEnable = !arg.activeState;
        //cinemachine转向的相机控制
        cinemachine.enabled = playerControlEnable;
        if (localPlayer != null)
        {
            localPlayer.canControl = playerControlEnable;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!ClientUtility.GetWindowActiveState(out UI_GamePopupWindow gamePopupWindow))
            {
                UISystem.Show<UI_GamePopupWindow>();
            }
            else
            {
                UISystem.Close<UI_GamePopupWindow>();
            }
        }

        //把背包消息发给服务器
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!ClientUtility.GetWindowActiveState(out UI_BagWindow bagWindow))
            {
                OpenBagWindow();
            }
            else UISystem.Close<UI_BagWindow>();

        }
    }
    private void OpenBagWindow()
    {
        requestOpenBagWindow = true;
        RequestBagData();
    }

    private void RequestBagData()
    {
        // 请求网络
        int dataVersion = bagData == null ? -1 : bagData.dataVersion;
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_GetBagData, new C_S_GetBagData { dataVersion = dataVersion });
        // 等网络消息回发
    }
    /// <summary>
    /// 服务器把背包的消息发回来
    /// </summary>
    private void OnS_C_GetBagData(ulong serverID, INetworkSerializable serializable)
    {
        S_C_GetBagData message = (S_C_GetBagData)serializable;
        if(message.haveBagData && message.bagData != null)
        {
            this.bagData = message.bagData;
        }
        if (requestOpenBagWindow)
        {
            requestOpenBagWindow = false;
            UISystem.Show<UI_BagWindow>().Show(bagData);
        }
        if (UISystem.GetWindow<UI_ShortcutBarWindow>() == null)
        {
            //UI_ShortcutBarWindow作为常驻窗口，道具快捷栏是一直存在的，只要是没有，那就打开他
            UISystem.Show<UI_ShortcutBarWindow>().Show(bagData);
        }
    }

    public bool IsLoadingCompleted()
    {
        //不为null了，则说明玩家加载完成了
        return localPlayer != null;
    }

    public void InitLocalPlayer(PlayerClientController player)
    {
        localPlayer = player;
        cinemachine.transform.position = localPlayer.transform.position;
        //注: Unity虽然会把这部分热更在打包前把他们剔除不在包体里，但是Unity剔除前依然会检验这部分能不能用
        //尤其是公共部分，一种打包成客户端，一种打包成服务端。需要考虑好这个东西在对立，比如服务端的热更新里会不会引用这个内容。反之，客户端也考虑一下。
        //例如：如果是服务端版的热更新的打包，下面这个是纯客户端才存在的东西，如果不加限制为 客户端的#if,那就会报错
        cinemachine.LookAt = localPlayer.cameraLookatTarget;
        cinemachine.Follow = localPlayer.cameraFollowTarget;
        localPlayer.canControl = playerControlEnable;
    }

    public void UseItem(int slotIndex)
    {
        // 构建使用物品的消息
        C_S_BagUseItem message = new C_S_BagUseItem { bagIndex = slotIndex };
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_BagUseItem, message);
    }
    private void OnS_C_UpdateItem(ulong serverID, INetworkSerializable serializable)
    {
        S_C_BagUpdateItem message = (S_C_BagUpdateItem)serializable;
        // 版本一致则不需要考虑，背包没有数据也不用考虑
        if (bagData == null || bagData.dataVersion == message.bagDataVersion) return;
        ItemDataBase itemData = message.newItemData;
        bagData.itemList[message.itemIndex] = itemData;
        bagData.dataVersion = message.bagDataVersion;
        if(message.usedWeapon)//更新武器
        {
            bagData.usedWeaponIndex = message.itemIndex;
        }
        //如果背包是打开状态则同步给背包
        if(ClientUtility.GetWindowActiveState(out UI_BagWindow bagWindow))
        {
            bagWindow.UpdateItem(message.itemIndex, itemData);
        }
        //道具快捷栏
        if (ClientUtility.GetWindowActiveState(out UI_ShortcutBarWindow shortcutBarWindow))
        {
            shortcutBarWindow.UpdateItemByBagIndex(message.itemIndex, itemData);
        }
        if (ClientUtility.GetWindowActiveState(out UI_CraftWindow craftWindow))
        {
            craftWindow.UpdateCraftArea();
        }
    }
    private GameObject GetWeapon(string weaponName)
    {
        GameObject weaponObj = PoolSystem.GetGameObject(weaponName);
        if (weaponObj == null)
        {
            WeaponConfig weaponConfig = ResSystem.LoadAsset<WeaponConfig>(weaponName);
            weaponObj = Instantiate(weaponConfig.prefab);
            weaponConfig.name = weaponName;
        }

        return weaponObj;
    }

    private void OnS_C_ShortcutBarUpdateItem(ulong serverID, INetworkSerializable serializable)
    {
        S_C_ShortcutBarUpdateItem message = (S_C_ShortcutBarUpdateItem)serializable;
        if (bagData.dataVersion == message.bagDataVersion) return; //没变化就不要去处理下面的内容
        bagData.dataVersion = message.bagDataVersion;
        // 通知快捷栏
        if (ClientUtility.GetWindowActiveState(out UI_ShortcutBarWindow shortcutBarWindow))
        {
            shortcutBarWindow.SetItem(message.shortcutBarIndex, message.bagIndex, bagData);
        }
    }

    public void OpenShop(string merchantConfigName)
    {
        if (!ClientUtility.GetWindowActiveState<UI_ShopWindow>(out _))
        {
            UISystem.Show<UI_ShopWindow>().Show(ResSystem.LoadAsset<MerchantConfig>(merchantConfigName));
            if(!requestOpenBagWindow && !ClientUtility.GetWindowActiveState(out UI_BagWindow bagWindow))
            {
                OpenBagWindow();
            }
        }
    }

    internal void ShopBuyItem(ItemConfigBase targetItemConfig, int bagIndex)
    {
        //金币是否足够
        if(targetItemConfig.price > bagData.coinCount)
        {
            //弹窗金币不足+本地化
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey(ErrorCode.CoinsInsufficient.ToString(), Color.yellow);
            return;
        }
        // 背包空间检测
        ItemDataBase existedItemData = bagData.TryGetItem(targetItemConfig.name, out int existedBagIndex);
        ItemDataBase targetSlotItemData = bagData.itemList[bagIndex];
        bool check; 
        //堆叠物品可以是空位，或是同一个物品
        if(targetItemConfig.GetDefaultItemData() is StackableItemDataBase)
        {
            check = targetSlotItemData == null || existedItemData != null;
            if(existedItemData != null)
            {
                bagIndex = existedBagIndex;
            }
        }
        //武器必须在空位
        else
        {
            check = targetSlotItemData == null;
        }
        if (!check)
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey(ErrorCode.LackOfBagSpace.ToString(), Color.yellow);
            return;
        }
        //发送网络信息
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_ShopBuyItem,
            new C_S_ShopBuyItem { itemID = targetItemConfig.name, bagIndex = bagIndex});
    }

    private void OnS_C_UpdateCoinCount(ulong serverID, INetworkSerializable serializable)
    {
        S_C_UpdateCoinCount message = (S_C_UpdateCoinCount)serializable;
        if (bagData.dataVersion == message.bagDataVersion) return;
        bagData.dataVersion = message.bagDataVersion;
        bagData.coinCount = message.coinCount;
        if(ClientUtility.GetWindowActiveState(out UI_BagWindow bagWindow))
        {
            bagWindow.UpdateCoin(bagData.coinCount);
        }
    }

    public void OpenCraft(string configName)
    {
        //放弃out参数，避免后续用了不该用的变量！做好优化
        if (!ClientUtility.GetWindowActiveState<UI_CraftWindow>(out _))
        {
            UISystem.Show<UI_CraftWindow>().Show(ResSystem.LoadAsset<CrafterConfig>(configName));
            if (!requestOpenBagWindow && !ClientUtility.GetWindowActiveState(out UI_BagWindow bagWindow))
            {
                OpenBagWindow();
            }
        }
    }
}

