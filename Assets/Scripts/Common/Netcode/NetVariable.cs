using Unity.Netcode;
/// <summary>
/// 避免，Netcode目前存在热更新程序集中的网络变量在同步中的错误问题
/// </summary>
/// <typeparam name="T"></typeparam>
public class NetVariable<T> : NetworkVariable<T>
{
    /// <summary>
    /// Constructor for <see cref="NetworkVariable{T}"/>
    /// </summary>
    /// <param name="value">initial value set that is of type T</param>
    /// <param name="readPerm">the <see cref="NetworkVariableReadPermission"/> for this <see cref="NetworkVariable{T}"/></param>
    /// <param name="writePerm">the <see cref="NetworkVariableWritePermission"/> for this <see cref="NetworkVariable{T}"/></param>
    public NetVariable(T value = default,
        NetworkVariableReadPermission readPerm = DefaultReadPerm,
        NetworkVariableWritePermission writePerm = DefaultWritePerm)
        : base( value,readPerm, writePerm)
    {

    }

    public override bool IsDirty()
    {
        //本项目中客户端没有修改网络变量的权力，所以直接过滤，避免NetworkVariableSerialization<T>.AreEqual ==null
        if (NetworkVariableSerialization<T>.AreEqual == null)
        {
            return false;
        }
        return base.IsDirty();
    }
}
