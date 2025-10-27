using JKFrame;
using UnityEngine;

public class PlayerClientController : MonoBehaviour
{
    public Transform cameraLookatTarget { get; private set; }
    public Transform cameraFollowTarget { get; private set; }
    public Transform floatInfoPoint { get; private set; }
    /// <summary>
    /// 本地玩家，主玩家控制器
    /// </summary>
    public PlayerController mainController { get; private set; }
    private PlayerFloatInfo floatInfo;
    public bool canControl; //玩家是否可以控制
    public void FirstInit() //第一次被添加组件调用
    {
        //直接查游戏对象CameraLookat来赋值
        cameraLookatTarget = transform.Find("CameraLookat");
        cameraFollowTarget = transform.Find("CameraFollow");
        floatInfoPoint = transform.Find("FloatPoint");
    }
    public void Init(PlayerController newPlayer)
    {
        this.mainController = newPlayer;
        if (mainController.IsSpawned) //本地玩家看不到自己的名字
        {
            if (floatInfo != null) floatInfo.gameObject.SetActive(false);
        }
        else
        {
            if (floatInfo == null) floatInfo = ResSystem.InstantiateGameObject<PlayerFloatInfo>(floatInfoPoint, "PlayerFloatInfo");
            floatInfo.Init(mainController.playerName.Value.ToString());
        }
    }
    //最后输入
    private Vector3 lastInputDir = Vector3.zero;
    private void Update()
    {
        if (!mainController.IsSpawned) return; //看看本地的玩家是不是宿主

        if (mainController.currentState.Value == PlayerState.None) return;
        //因为玩家发像服务端的移动是如果一个键没有变化就一直向服务端发，
        //---(我们的移动指令设计是这样的，所以鼠标暂停移动，不能直接在这里return断掉键位消息的发送情况，需要下面逻辑判断为不移动)
        Vector3 inputDir = Vector3.zero;
        if (canControl)
        {
            //如果true可以控制，就使用玩家键入的按键。如果false不能控制，那么就等于没按
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputDir = new Vector3(h, 0, v);
        }
        if (inputDir == Vector3.zero && lastInputDir == Vector3.zero) return;//上一次没按，这一次也没按，就直接不用传消息了
        //输入方向
        lastInputDir = inputDir;
        //计算摄像机的旋转，和相对的角色WASD移动的 对应变化
        float cameraEulerAngleY = Camera.main.transform.eulerAngles.y;
        //四元数和向量相乘：让这个向量按照四元数所表达的角度进行旋转后得到一个新的向量
        //移动方向：Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir
        mainController.SendInputMoveDirServerRpc(Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir);
    }


}
