using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combiner
{
    //
    // Summary:
    //      Takes in a source to add to the target
    public static GameObject Combine(GameObject target, GameObject source)
    {
        Dictionary<string, Transform> catalog = new Dictionary<string, Transform>() { { target.name, target.transform } };
        SkinnedMeshRenderer[] meshes = source.GetComponentsInChildren<SkinnedMeshRenderer>();
        GameObject targetMesh = AddChild(source, target.transform);

        foreach (SkinnedMeshRenderer sourceRenderer in meshes)
        {
            SkinnedMeshRenderer targetRenderer = targetMesh.GetComponent<SkinnedMeshRenderer>();
            targetRenderer.sharedMesh = sourceRenderer.sharedMesh;
            targetRenderer.materials = sourceRenderer.materials;

            targetRenderer.bones = TranslateTransforms(sourceRenderer.bones, catalog);
        }

        return target;
    }

    private static GameObject AddChild(GameObject source, Transform transform)
    {
        GameObject target = new GameObject(source.name);
        target.transform.parent = transform;

        target.transform.localPosition = source.transform.localPosition;
        target.transform.localRotation = source.transform.localRotation;
        target.transform.localScale = source.transform.localScale;

        return target;
    }

    private static Transform[] TranslateTransforms(Transform[] sources, Dictionary<string, Transform> transformCatalog)
    {
        Transform[] targets = new Transform[sources.Length];
        for (int index = 0; index < sources.Length; index++)
            transformCatalog.TryGetValue(sources[index].name, out targets[index]);

        return targets;
    }
}