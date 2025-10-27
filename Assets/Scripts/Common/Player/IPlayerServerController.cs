using UnityEngine;
public interface IPlayerServerController
{
    public void OnNetworkSpawn();
    public void OnNetworkDespawn();
    public void ReceiveMoveInput(Vector3 moveDir);
    public void ReceiveJumpInput();

}
