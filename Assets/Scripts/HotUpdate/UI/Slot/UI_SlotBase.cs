using JKFrame;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 其他格子都以这个为基础，首要做为抽象的基类
/// </summary>
public abstract class UI_SlotBase : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    protected static UI_SlotBase enteredSlot; //目前鼠标进入的格子

    [SerializeField] protected Image frameImage;
    [SerializeField] protected Sprite normalFrame;
    [SerializeField] protected Sprite selectedFrame;
    [SerializeField] protected Text keyCodeText; //快捷窗口中时显示快捷键的
    public int dataIndex { get; private set; } //格子的index
    protected Action<int> onUseAction;
    protected Action<UI_SlotBase, UI_SlotBase> onDragToNewSlotAction; //从A拖拽到B
    public IItemWindow ownerWindow { get; private set; }
    //默认data，config为null，可以表示不传入值是没有问题的
    public virtual void Init(IItemWindow ownerWindow,ItemDataBase data, ItemConfigBase config,int dataIndex, Action<int> onUseAction,Action<UI_SlotBase,UI_SlotBase> onDragToNewSlotAction)
    {
        this.ownerWindow = ownerWindow;
        this.dataIndex = dataIndex;
        this.onUseAction = onUseAction;
        this.onDragToNewSlotAction = onDragToNewSlotAction;
        if (keyCodeText != null) keyCodeText.gameObject.SetActive(false);//keyCode默认不显示
        OnPointerExit(null);
        OnInit();
    }
    public virtual void OnInit()
    {

    }
    public virtual void SetShortcutKeyCode(int num) //键盘数字
    {
        keyCodeText.gameObject.SetActive(true);
        keyCodeText.text = num.ToString();
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //鼠标选中后，修改图标边框
        frameImage.sprite = selectedFrame;
        enteredSlot = this;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        frameImage.sprite = normalFrame;
        enteredSlot = null;
    }
    public virtual void Destroy()
    {
        this.GameObjectPushPool();
        if (enteredSlot == this) enteredSlot = null;
    }
    /// <summary>
    /// 鼠标点击
    /// </summary>
    /// <param name="eventData">PointerEventData是Unity底层的事件数据</param>
    /// virtual实现多态性让子类可重写
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //鼠标右键意味着使用物品
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            Use();
        }
    }
    public void Use()
    {
        onUseAction?.Invoke(dataIndex);
    }
}
public abstract class UI_SlotBase<D,C> : UI_SlotBase, IBeginDragHandler, IDragHandler, IEndDragHandler where D : ItemDataBase where C : ItemConfigBase
{
    [SerializeField] protected Image iconImage;
    protected D itemData;
    protected C itemConfig;
    /// <summary>
    /// 虽然说初始化的部分有泛型，但是不一定拿到的数据类型是准的，我们还是给明确一个类型基类，然后在内部转泛型
    /// </summary>
    public override void Init(IItemWindow ownerWindow, ItemDataBase data, ItemConfigBase config, int index, Action<int> onUseAction, Action<UI_SlotBase, UI_SlotBase> onDragToNewSlotAcion)
    {
        this.itemData = (D)data;
        this.itemConfig = (C)config;
        base.Init(ownerWindow,data, config, index, onUseAction, onDragToNewSlotAcion);
    }

    public override void OnInit()
    {
        iconImage.sprite = itemConfig.icon;
    }
    public override void Destroy()
    {
        if (itemConfig != null && enteredSlot == this)
        {
            UISystem.Close<UI_ItemInfoPopupWindow>();
        }
        itemData = null;
        itemConfig = null;
        base.Destroy();
    }
    /// <summary>
    /// 鼠标进入
    /// </summary>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (itemConfig != null)
        {
            UISystem.Show<UI_ItemInfoPopupWindow>().Show(transform.position, ((RectTransform)transform).sizeDelta.y / 2, itemConfig);
        }
    }
    /// <summary>
    /// 鼠标出去
    /// </summary>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (itemConfig != null && enteredSlot == this)
        {
            UISystem.Close<UI_ItemInfoPopupWindow>();
        }
        base.OnPointerExit(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        iconImage.transform.SetParent(UISystem.DragLayer);
    }

    public void OnDrag(PointerEventData eventData)
    {
        iconImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        iconImage.transform.SetParent(transform);
        iconImage.transform.SetAsFirstSibling();//设置到最顶级
        iconImage.transform.localPosition = Vector3.zero;
        // 对方格子是有意义的并且不是自己
        if (enteredSlot != null && enteredSlot != this)
        {
            onDragToNewSlotAction?.Invoke(this, enteredSlot);
        }

    }
}


