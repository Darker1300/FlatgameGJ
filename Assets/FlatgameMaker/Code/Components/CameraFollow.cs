using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FollowType
{
    basic,
    //smoothed
}
[ExecuteInEditMode]
public class CameraFollow : MonoBehaviour
{

    public FollowType followType;
    float zPos;
    Transform target;

    void Start()
    {
        zPos = transform.position.z;
    }

    void Update()
    {
        Area area = AreaManager.Instance.GetCurrentArea();
        if (area != null) { 
            target = area.AreaPlayer.transform;
        }
        if (target != null)
        {
            Vector2 targPos = target.position;
            switch (followType)
            {
                default:
                    transform.position = (Vector3)targPos + (Vector3.forward * zPos);
                    break;
            }
        }
    }
}
