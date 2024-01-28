using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Node : MonoBehaviour
{

    [Range(0, 5)]
    public int dir;
    public bool randomizeDir = false;
    public bool lockY = false;

    public Hex hex
    {
        get
        {
            return transform.position.ToHex();
        }
    }

    public Hex localHex
    {
        get
        {
            return transform.localPosition.ToHex();
        }
    }

    public void ApplyTransform()
    {
        if (randomizeDir)
        {
            Hex hex = this.hex;
            int i = hex.q * 100 + hex.r;
            dir = ((i % 6) + 6) % 6;
        }
        float y = lockY ? 0f : transform.localPosition.y;
        Vector3 newPos = this.localHex.ToWorld(y);
        transform.localPosition = newPos;
        transform.localRotation = Quaternion.Euler(0, -60f * dir, 0);
    }

#if UNITY_EDITOR
    protected virtual void Update()
    {
        if (!Application.isPlaying)
        {
            ApplyTransform();
            // Hack to never re-apply dir to instances
            this.dir += 1;
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            this.dir = (dir - 1) % 6;
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position, hex.ToString());
    }
#endif

}