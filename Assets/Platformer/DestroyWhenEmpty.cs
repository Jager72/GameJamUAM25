using UnityEngine;

public class DestroyWhenEmpty : MonoBehaviour
{
    void Update()
    {
        if (transform.childCount == 0)
            Destroy(gameObject);
    }
}