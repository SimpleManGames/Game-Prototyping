using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class CombineChildrenMesh : MonoBehaviour
{
    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Combiner.Combine(gameObject, transform.GetChild(i).gameObject);
            Destroy(transform.GetChild(i));
        }
    }
}