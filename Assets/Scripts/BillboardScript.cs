using UnityEngine;

public class BillboardScript : MonoBehaviour
{

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPosition = Camera.main.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);
    }
}
