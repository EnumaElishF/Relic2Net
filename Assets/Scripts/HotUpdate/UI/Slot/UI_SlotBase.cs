using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 其他格子都以这个为基础，首要做为抽象的基类
/// </summary>
public abstract class UI_SlotBase : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] protected Image frameImage;
    [SerializeField] protected Sprite normalFrame;
    [SerializeField] protected Sprite selectedFrame;

    //默认data，config为null，可以表示不传入值是没有问题的
    public virtual void Init(ItemDataBase data = null, ItemConfigBase config = null)
    {
        OnPointerExit(null);

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //鼠标选中后，修改图标边框
        frameImage.sprite = selectedFrame;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        frameImage.sprite = normalFrame;
    }
    public virtual void Destroy()
    {
        this.GameObjectPushPool();
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
    public override void Init(ItemDataBase data, ItemConfigBase config)
    {
        base.Init(data,config);
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
}


