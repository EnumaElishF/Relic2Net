using JKFrame;
using UnityEngine;

public class PlayerClientController : CharacterClientControllerBase<PlayerController>,IPlayerClientController
{
    public Transform cameraLookatTarget { get; private set; }
    public Transform cameraFollowTarget { get; private set; }
    public Transform floatInfoPoint { get; private set; }
    private PlayerFloatInfo floatInfo;
    public bool canControl; //玩家是否可以控制


    public override void FirstInit(PlayerController newPlayer) //第一次被添加组件调用
    {
        base.FirstInit(newPlayer);
        //下面是玩家客户端专享：
        //直接查游戏对象CameraLookat来赋值
        cameraLookatTarget = transform.Find("CameraLookat");
        cameraFollowTarget = transform.Find("CameraFollow");
        floatInfoPoint = transform.Find("FloatPoint");
        mainController.View.footStepAction += View_footStepAction;
    }

    public override void Init()
    {
        if (mainController.IsOwner) //本地玩家看不到自己的名字
        {
            if (floatInfo != null) floatInfo.gameObject.SetActive(false);
        }
        else
        {
            if (floatInfo == null) floatInfo = ResSystem.InstantiateGameObject<PlayerFloatInfo>(floatInfoPoint, "PlayerFloatInfo");
            floatInfo.UpdateName(mainController.playerName.Value.ToString());
        }
        //客户端生成角色后自己手动更新一些血量显示
        OnHpChanged(0, mainController.currentHp.Value);
    }
    /// <summary>
    /// 脚步声
    /// </summary>
    private void View_footStepAction()
    {
        AudioClip audioClip = ClientGlobal.Instance.Config.playerFootStepAudios[Random.Range(0, ClientGlobal.Instance.Config.playerFootStepAudios.Length)];
        AudioSystem.PlayOneShot(audioClip, transform.position);
    }

    #region 网络相关 ：生成和销毁

    protected override void OnHpChanged(float previousValue, float newValue)
    {
        float fillAmount = newValue / ClientGlobal.Instance.Config.playerMaxHp;
        if (mainController.IsOwner) //设置IsOwner识别本地玩家，以做区分，本地玩家在左上角展示血条，那么其他玩家是头顶展示的。
        {
            //HP值绑定上HP窗口
            UISystem.Show<UI_PlayerInfoWindow>().UpdateHP(fillAmount);
        }
        else
        {
            floatInfo.UpdateHp(fillAmount);
        }

    }

    #endregion


    #region 检测输入  
    //最后输入
    private Vector3 lastInputDir = Vector3.zero;
    private void Update()
    {
        if (!mainController.IsSpawned && gameObject.activeInHierarchy) 
        {
            //玩家如果退出登录,用这个可以销毁，但是我们有AOI相关的东西，不能仅仅把游戏对象销毁就结束。 
            NetManager.Instance.DestroyObject(mainController.NetworkObject);
        }
        ; //看看本地的玩家是不是宿主
        if (!mainController.IsOwner) return; //只有IsOwner当前本地玩家时才InitLocalPlayer、检测输入
        switch (mainController.currentState.Value)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
                UpdateMoveInput();
                UpdateJumpInput();
                UpdateAttackInput();
                break;
            case PlayerState.Jump:
                UpdateMoveInput(); //空中移动要监听移动的输入的
                break;
            case PlayerState.AirDown:
                UpdateMoveInput();
                UpdateAttackInput();
                break;
            case PlayerState.Attack:
                UpdateMoveInput();//更新攻击时转向和移动
                UpdateAttackInput(); //空中移动要监听移动的输入的
                break;
        }
    }
    private void UpdateMoveInput()
    {
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
        Debug.Log("测试inputDir=" + inputDir);
        mainController.SendInputMoveDirServerRpc(Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir);
    }
    private void UpdateJumpInput()
    {
        if (!canControl) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mainController.SendJumpInputServerRpc();
        }
    }
    private bool lastAttackInput = false;
    private void UpdateAttackInput()
    {
        if (!canControl)
        {
            if (lastAttackInput)
            {
                SetAttackInput(false);
            }
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            SetAttackInput(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SetAttackInput(false);
        }
    }
    private void SetAttackInput(bool value)
    {
        mainController.SendAttackInputServerRpc(value);
        lastAttackInput = value;
    }
    #endregion


}
