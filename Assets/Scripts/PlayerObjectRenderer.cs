using PurrNet;
using UnityEngine;

public class PlayerObjectRenderer : NetworkBehaviour
{

    private Vector3 prevPos;
    private Vector3 lookDir;
    private int updateFrame = 1;

    void Start()
    {
        prevPos = transform.parent.forward * 0.05f;
    }

    [ObserversRpc(runLocally:true, bufferLast:true)]
    private void UpdateLookDirRPC(Vector3 movePos)
    {
        
        transform.forward = movePos;

    }

    private void UpdateLookDir(Vector3 movePos)
    {
        
        transform.forward = movePos;

    }

    public void SetLookDir(Vector3 movePos)
    {
        
        movePos.y = 0;

        if(movePos.magnitude > 0.05f){

            if(networkManager) UpdateLookDirRPC(movePos);
            else UpdateLookDir(movePos);

        }

    }
}
