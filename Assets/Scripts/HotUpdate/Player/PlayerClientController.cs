using JKFrame;
using UnityEngine;

public class PlayerClientController : MonoBehaviour,IPlayerClientController
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
    private PlayerClientConfig clientConfig;
    private SkillConfig currentSkillConfig;
    public void FirstInit(PlayerController newPlayer) //第一次被添加组件调用
    {
        this.mainController = newPlayer;
        newPlayer.SetClientController(this);
        //直接查游戏对象CameraLookat来赋值
        cameraLookatTarget = transform.Find("CameraLookat");
        cameraFollowTarget = transform.Find("CameraFollow");
        floatInfoPoint = transform.Find("FloatPoint");
        clientConfig = ResSystem.LoadAsset<PlayerClientConfig>(nameof(PlayerClientConfig));
        mainController.view.footStepAction += View_footStepAction;
    }

    public void Init()
    {
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

    #region 网络相关 ：生成和销毁
    public void OnNetworkSpawn()
    {
        
    }

    public void OnNetworkDespawn()
    {
        
    }
    #endregion

    #region 检测输入  
    //最后输入
    private Vector3 lastInputDir = Vector3.zero;
    private void Update()
    {
        if (!mainController.IsSpawned) return; //看看本地的玩家是不是宿主
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

    #region 战斗
    public void StartSkill(int skillIndex)
    {
        currentSkillConfig = mainController.skillConfigList[skillIndex];
        PlaySkillEffect(currentSkillConfig.releaseEffect);
    }
    public void StartSkillHit()
    {
        PlaySkillEffect(currentSkillConfig.startHitEffect);
    }
    private void PlaySkillEffect(SkillEffect skillEffect)
    {
        if (skillEffect == null) return;
        if (skillEffect.audio != null)
        {
            AudioSystem.PlayOneShot(skillEffect.audio, transform.position);
        }
        if (skillEffect.prefab != null)
        {
            GameObject effectObj = ClientUtility.GetOrInstantiate(skillEffect.prefab, null);
            //效果的坐标，他是要考虑坐标在套入角色内，以及单独拿出外，坐标是会变换的，毕竟父级不一样
            //效果的坐标，这里函数存的是对于在角色上是0,0,0的坐标，拉出来，无父级的时候是什么坐标。这样一个一直相对变化值
            //将「角色本地坐标系的偏移量」转换为「世界坐标系的绝对位置」，让效果贴合角色指定位置
            effectObj.transform.position = mainController.view.transform.TransformPoint(skillEffect.offset);
            //效果的旋转，道理和上面类似，但是容易一些，我们用四元数相乘的逻辑，做角度叠加。 相对偏移角度skillEffect.rotation
            //将「角色的基础旋转」与「效果的相对旋转」叠加，让效果朝向符合预期（如技能发射方向、特效朝向）
            effectObj.transform.rotation = mainController.view.transform.rotation * Quaternion.Euler(skillEffect.rotation);
            //因为角色模型不会变大变小，所以效果的相对缩放这个方面倒不用考虑其他计算，直接用设定就行
            effectObj.transform.localScale = skillEffect.scale;
        }
    }
    /// <summary>
    /// 脚步声
    /// </summary>
    private void View_footStepAction()
    {
        AudioClip audioClip = clientConfig.footStepAudios[Random.Range(0, clientConfig.footStepAudios.Length)];
        AudioSystem.PlayOneShot(audioClip, transform.position);
    }

    public void PlaySkillHitEffect(Vector3 point)
    {
        //TODO
    }
    #endregion

}
