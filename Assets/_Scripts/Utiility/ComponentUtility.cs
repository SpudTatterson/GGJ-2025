using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public static class ComponentUtility
{
    public static List<T> GetComponentsInBox<T>(Vector3 position, Vector3 halfExtents, LayerMask layerMask = default)
    {
        HashSet<T> hitObjects = new HashSet<T>();
        Collider[] hits = layerMask == default
        ? Physics.OverlapBox(position, halfExtents)
        : Physics.OverlapBox(position, halfExtents, Quaternion.identity, layerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            T t = hits[i].GetComponentInParent<T>();
            if (t == null) continue;
            hitObjects.Add(t);
        }
        return new List<T>(hitObjects);
    }
    public static List<T> GetComponentsInRadius<T>(Vector3 position, float radius, LayerMask layerMask = default)
    {
        HashSet<T> hitObjects = new HashSet<T>();
        Collider[] hits = layerMask == default
        ? Physics.OverlapSphere(position, radius)
        : Physics.OverlapSphere(position, radius, layerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            GameObject hitObject = hits[i].gameObject;
            T t = hitObject.GetComponentInParent<T>();
            if (t == null) continue;
            hitObjects.Add(t);
        }
        return new List<T>(hitObjects);
    }
    public static T GetComponentInRadius<T>(Vector3 position, float radius, LayerMask layerMask = default)
    {
        HashSet<T> hitObjects = new HashSet<T>();
        Collider[] hits = layerMask == default
        ? Physics.OverlapSphere(position, radius)
        : Physics.OverlapSphere(position, radius, layerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            T t = hits[i].GetComponentInParent<T>();
            if (t == null) continue;
            hitObjects.Add(t);
        }
        if (hitObjects.Count != 0) return hitObjects.First();
        else return default;
    }
}
