using UnityEngine;
public interface IPlayerServerController : ICharacterServerController
{

    public void ReceiveMoveInput(Vector3 moveDir);
    public void ReceiveJumpInput();
    public void ReceiveAttackInput(bool value);
}
