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
    [SerializeField] protected Image frameImage;
    [SerializeField] protected Sprite normalFrame;
    [SerializeField] protected Sprite selectedFrame;
    [SerializeField] protected Text keyCodeText; //快捷窗口中时显示快捷键的
    public int bagIndex { get; private set; } //格子的index
    protected Action<int> onUseAction;

    //默认data，config为null，可以表示不传入值是没有问题的
    public virtual void Init(ItemDataBase data, ItemConfigBase config,int index, Action<int> onUseAction)
    {
        this.bagIndex = index;
        this.onUseAction = onUseAction;
        if (keyCodeText != null) keyCodeText.gameObject.SetActive(false);//keyCode默认不显示
        OnPointerExit(null);
    }
    public virtual void SetShortKeyCode(int num) //键盘数字
    {
        keyCodeText.text = num.ToString();
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //鼠标选中后，修改图标边框
        frameImage.sprite = selectedFrame;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        frameImage.sprite = normalFrame;
    }
    public virtual void Destroy()
    {
        this.GameObjectPushPool();
    }
    /// <summary>
    /// 鼠标点击
    /// </summary>
    /// <param name="eventData">PointerEventData是Unity底层的事件数据</param>
    /// virtual实现多态性让子类可重写
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //鼠标右键意味着使用物品
        if(onUseAction != null && eventData.button == PointerEventData.InputButton.Right)
        {
            transform.GetSiblingIndex();
            onUseAction?.Invoke(bagIndex);
        }
    }
}
public abstract class UI_SlotBase<D,C> : UI_SlotBase where D : ItemDataBase where C : ItemConfigBase
{
    [SerializeField] protected Image iconImage;
    protected D itemData;
    protected C itemConfig;
    /// <summary>
    /// 虽然说初始化的部分有泛型，但是不一定拿到的数据类型是准的，我们还是给明确一个类型基类，然后在内部转泛型
    /// </summary>
    /// <param name="data"></param>
    /// <param name="config"></param>
    public override void Init(ItemDataBase data, ItemConfigBase config, int index, Action<int> onUseAction)
    {
        base.Init(data,config,index,onUseAction);
        this.itemData = (D)data;
        this.itemConfig = (C)config;
        iconImage.sprite = config.icon;
    }
    public override void Destroy()
    {
        base.Destroy();
        itemData = null;
        itemConfig = null;
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
        if (itemConfig != null)
        {
            UISystem.Close<UI_ItemInfoPopupWindow>();
        }
        base.OnPointerExit(eventData);
    }

}


