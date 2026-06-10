using UnityEngine;

public class  WiggleAhh: MonoBehaviour
{
    public Transform visualRoot;

    public float moveWidth = 1.5f;
    public float moveSpeed = 2f;

    public float rotateAngle = 12f;
    public float rotateSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 visualStartLocalPosition;

    void Start()
    {
        startPosition = transform.position;

        if (visualRoot != null)
        {
            visualStartLocalPosition = visualRoot.localPosition;
        }
    }

    void Update()
    {
        float moveX = Mathf.Sin(Time.time * moveSpeed) * moveWidth;
        float rotateZ = Mathf.Sin(Time.time * rotateSpeed) * rotateAngle;

        if (visualRoot != null)
        {
            visualRoot.localPosition = visualStartLocalPosition + new Vector3(moveX, 0, 0);
            visualRoot.localRotation = Quaternion.Euler(0, 0, rotateZ);
        }
        else
        {
            transform.position = startPosition + new Vector3(moveX, 0, 0);
            transform.rotation = Quaternion.Euler(0, 0, rotateZ);
        }
    }
}