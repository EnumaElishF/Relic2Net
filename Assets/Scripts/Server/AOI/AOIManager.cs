using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class AOIManager : SingletonMono<AOIManager>
{
    private readonly static Vector2Int defaultCoord;

    [SerializeField] private float chunkSize = 50; //最远可以看到的其他玩家或者服务端对象
    [SerializeField] private int visualChunkRange = 1;//如果是1，就是刚好周围的一圈：九宫格
    // <chunkCoord,clinetIDs>   记录这一块有哪些客户端玩家
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();
    // <chunkCoord,serverObjectIDs>    记录这一块有哪些服务端对象
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic = new Dictionary<Vector2Int, HashSet<NetworkObject>>();

    static AOIManager()
    {
        // int.MinValue（即 -2147483648） defaultCoord作为一个不可能有的地图块逻辑，用在初始化的时候
        defaultCoord = new Vector2Int(int.MinValue, int.MinValue);
    }
    public Vector2Int GetCoordByWorldPostion(Vector3 worldPostion)
    {
        return new Vector2Int((int)(worldPostion.x / chunkSize), (int)(worldPostion.z / chunkSize));
    }

    public void Init()
    {
        Debug.Log("初始化AOI");
        //初始化，传chunkSize到公共程序
        
    }

    #region Client

    /// <summary>
    /// 初始化客户端
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="chunkCoord"></param>
    public void InitClient(ulong clientID, Vector2Int chunkCoord)
    {
        //这里的实际决定权取决于后续代码的oldCoord，也就是这里的defaultCoord
        UpdateClientChunkCoord(clientID, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// 更新玩家在AOI地图上的坐标   (检查chunk块)
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="oldCoord">"coord" 是坐标 "coordinate" 的缩写</param>
    /// <param name="newCoord"></param>
    public void UpdateClientChunkCoord(ulong clientID, Vector2Int oldCoord, Vector2Int newCoord)
    {
        Debug.Log("!!!开始进行了更新玩家在AOI地图上的坐标");
        if (oldCoord == newCoord) return;

        // 从旧的地图块中移除
        RemoveClient(clientID, oldCoord);

        //判断是否跨地图块移动  (跨地图块的话就是类似传送的情况)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子移动的极限距离,所以是传送性质的位移
        {
            //跨块移动
            Debug.Log("跨块移动");
            //单个格子的斜角度位移肯定是小于1.5f的，超过了就说明超出格子的极限距离 √2 = 1.414
            for (int x = -visualChunkRange; x <= visualChunkRange; x++) //-1,0,1的九宫格
            {
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    //虽然看着算法的时间复杂度比较大，但是因为面对的九个格子+客户端数量限制，实际上不会很大
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y); //!!注意这里构成了 初始的跨快初始化没能看到互相的客户端 的错误原因，写成oldCoord.x了。另外newCoord最外层方法没有赋值默认取V2的(0,0)
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }

        }
        else //正常一个格子的移动距离
        {
            Debug.Log("非跨块移动");
            //非跨块移动，考虑上下左右，以及多个斜方向移动（注：斜方向的则会被分解掉，像是为组合的左上，这种情况)
            // 上，旧的最下面一行隐藏，新的最上一行显示
            if (newCoord.y > oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y - visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y + visualChunkRange);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            // 下，旧的最下面一行显示，新的最上一行隐藏
            else if (newCoord.y < oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y + visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y - visualChunkRange);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }

            // 左，旧的最右边面一列隐藏，新的最左边一列显示
            if (newCoord.x < oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x - visualChunkRange, newCoord.y + i);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            // 右，旧的最右边面一列显示，新的最左边一列隐藏
            else if (newCoord.x > oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x - visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + visualChunkRange, newCoord.y + i);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
        }

        Debug.Log("!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDs)为" + (!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDsTest)));
        //把客户端加入当前新的块----先从旧的地图块中移除，又把客户端加入当前新的块。这样最内侧的方法也保证了自己不会存自己的情况
        if (!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDs))
        {
            newCoordClientIDs = ResSystem.GetOrNew<HashSet<ulong>>();
            Debug.Log("chunkClientDic.Add(newCoord, newCoordClientIDs); newCoord为"+ newCoord+ "newCoordClientIDs为"+ newCoordClientIDs);
            chunkClientDic.Add(newCoord, newCoordClientIDs);
        }
        newCoordClientIDs.Add(clientID);
    }


    // 某个客户端和某个区域的客户端们全部互相 可见
    private void ShowClientForChunkClients(ulong clientID, Vector2Int chunkCoord)
    {
        //clientID测试没有问题，玩家1和玩家2的ID按从1到2升序
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong newClientID in clientIDs)
            {
                Debug.Log("newClientID为" + newClientID);
                //本人的clientID对比，遍历到的每个其他客户端的ID。然后走互相可见的逻辑
                ClientMutualShow(clientID, newClientID);
            }
        }
    }
    // 某个客户端和某个区域的客户端们全部互相 不可见
    private void HideClientForChunkClients(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong newClientID in clientIDs)
            {
                ClientMutualHide(clientID, newClientID);
            }
        }
    }
    /// <summary>
    /// 移除客户端
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="coord"></param>
    public void RemoveClient(ulong clientID, Vector2Int coord)
    {
        if (chunkClientDic.TryGetValue(coord, out HashSet<ulong> clientIDs)) //得到ulong的id
        {
            //如果当前坐标下没有玩家，则回收容器。
            if (clientIDs.Remove(clientID) && clientIDs.Count == 0)
            {
                clientIDs.ObjectPushPool();
                chunkClientDic.Remove(coord);
            }
        }
    }

    /// <summary>
    /// 客户端互相可见
    /// </summary>
    /// <param name="clientA"></param>
    /// <param name="clientB"></param>
    private void ClientMutualShow(ulong clientA, ulong clientB)
    {
        Debug.Log("clientA == clientB测试" + (clientA == clientB));
        if (clientA == clientB) return;
        //检查NetManager.Instance.SpawnManager.OwnershipToObjectsTable
        Debug.Log("NetManager.Instance.SpawnManager.OwnershipToObjectsTable为" + NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDicTest));
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            Debug.Log("进入A可见B执行逻辑前");
            // A可见B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkShow(clientB);
            }
            // B可见A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkShow(clientA);
            }
        }
    }
    /// <summary>
    /// 客户端互相不可见
    /// </summary>
    /// <param name="clientA"></param>
    /// <param name="clientB"></param>
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            // A可见B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkHide(clientB); //如果可见就隐藏
            }
            // B可见A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkHide(clientA);
            }
        }
    }


    /// <summary>
    /// 某个区域的全部服务端对象 对某个客户端 可见
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="chunkCoord"></param>
    private void ShowChunkServerObjectForClient(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            foreach (NetworkObject serverObject in serverObjects)
            {
                if (!serverObject.IsNetworkVisibleTo(clientID)) //如果是不可见的，就给他改成 可见
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }

    /// <summary>
    /// 某个区域的全部服务端对象 对某个客户端 不可见
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="chunkCoord"></param>
    private void HideChunkServerObjectForClient(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            foreach (NetworkObject serverObject in serverObjects)
            {
                if (serverObject.IsNetworkVisibleTo(clientID)) //如果是可见的，就给他改成 不可见
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    /// <summary>
    /// 为某个地图块下的全部的客户端，显示与隐藏的控制 {既有区域内客户端互相是否可见，又有服务端上的对象对客户端的是否可见}
    /// </summary>
    private void ShowAndHideForChunk(ulong clientID, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowClientForChunkClients(clientID, showChunkCoord);
        HideClientForChunkClients(clientID, hideChunkCoord);
        ShowChunkServerObjectForClient(clientID, showChunkCoord);
        HideChunkServerObjectForClient(clientID, hideChunkCoord);
    }
    #endregion



    #region Server

    /// <summary>
    /// 初始化服务器对象
    /// </summary>
    /// <param name="serverObject"></param>
    /// <param name="chunkCoord"></param>
    public void InitServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        UpdateServerObjectChunkCoord(serverObject, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// 更新服务器对象在AOI地图上的坐标   (检查chunk块)
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="oldCoord">"coord" 是坐标 "coordinate" 的缩写</param>
    /// <param name="newCoord"></param>
    public void UpdateServerObjectChunkCoord(NetworkObject serverObject, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) return;
        // 从旧的地图块中移除
        RemoveServerObject(serverObject, oldCoord);

        //服务端的对象也要移动，也需要更新格子
        //判断是否跨地图块移动  (跨地图块的话就是类似传送的情况)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子移动的极限距离,所以是传送性质的位移
        {
            //跨块移动
            //单个格子的斜角度位移肯定是小于1.5f的，超过了就说明超出格子的极限距离 √2 = 1.414
            for (int x = -visualChunkRange; x <= visualChunkRange; x++) //-1,0,1的九宫格
            {
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    //虽然看着算法的时间复杂度比较大，但是因为面对的九个格子+客户端数量限制，实际上不会很大
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y);
                    ShowAndHideChunkClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
        }
        else //正常一个格子的移动距离
        {
            //非跨块移动，考虑上下左右，以及多个斜方向移动（注：斜方向的则会被分解掉，像是为组合的左上，这种情况)
            // 上，旧的最下面一行隐藏，新的最上一行显示
            if (newCoord.y > oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y - visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y + visualChunkRange);
                    ShowAndHideChunkClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
            // 下，旧的最下面一行显示，新的最上一行隐藏
            else if (newCoord.y < oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y + visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y - visualChunkRange);
                    ShowAndHideChunkClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }

            // 左，旧的最右边面一列隐藏，新的最左边一列显示
            if (newCoord.x < oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x - visualChunkRange, newCoord.y + i);
                    ShowAndHideChunkClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
            // 右，旧的最右边面一列显示，新的最左边一列隐藏
            else if (newCoord.x > oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x - visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + visualChunkRange, newCoord.y + i);
                    ShowAndHideChunkClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
        }

        // 把服务端对象加入到当前新块
        if (!chunkServerObjectDic.TryGetValue(newCoord, out HashSet<NetworkObject> serverObjects))
        {
            Debug.Log("把未加入的服务端对象加入到当前新块"); //！注意区分chunkClientDic.Add 服务器上的客户端对象字典 和 chunkServerObjectDic.Add 服务器上的一般对象字典
            serverObjects = ResSystem.GetOrNew<HashSet<NetworkObject>>();
            chunkServerObjectDic.Add(newCoord, serverObjects);
        }
        serverObjects.Add(serverObject);
    }


    public void RemoveServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            serverObjects.Remove(serverObject);
        }
    }

    private void ShowAndHideChunkClientsForServerObject(NetworkObject serverObject, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowChunkClientsForServerObject(serverObject, showChunkCoord);//这个格子下的所有客户端都 能看见我
        HideChunkClientsForServerObject(serverObject, hideChunkCoord);//这个格子下的所有客户端都 不能看见我
    }

    /// <summary>
    /// 为一个服务端对象 显示 某个区域(地图块)下的全部客户端
    /// </summary>
    /// <param name="serverObject"></param>
    /// <param name="chunkCoord"></param>

    private void ShowChunkClientsForServerObject(NetworkObject serverObject,Vector2Int chunkCoord)
    {
        if(chunkClientDic.TryGetValue(chunkCoord,out HashSet<ulong> clientIDs))
        {
            foreach(ulong clientID in clientIDs)
            {
                if (!serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }
    /// <summary>
    /// 为一个服务端对象 隐藏 某个区域(地图块)下的全部客户端
    /// </summary>
    /// <param name="serverObject"></param>
    /// <param name="chunkCoord"></param>
    private void HideChunkClientsForServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong clientID in clientIDs)
            {
                if (serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    #endregion






}
