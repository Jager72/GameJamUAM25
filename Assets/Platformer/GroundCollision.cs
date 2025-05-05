using UnityEngine;

public class GroundCollision : MonoBehaviour
{
    PlatformerPlayerControll go;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        go = transform.parent.GetComponent<PlatformerPlayerControll>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("xd");
        CheckGroundContact(collider, true);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        CheckGroundContact(collider, false);
    }

    private void CheckGroundContact(Collider2D col, bool isEntering)
    {
        ContactPoint2D[] pts = new ContactPoint2D[10];
        int count = col.GetContacts(pts);
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Point stats: " + pts[i].normal.y + " x: " + pts[i].normal.x);
            if (pts[i].normal.y > 0.7f)
            {
                go.isGrounded = isEntering;
                return;
            }
        }
        if (!isEntering) go.isGrounded = false;
    }
}
