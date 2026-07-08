using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    [SerializeField] RectTransform rt;
    [SerializeField] float moveSpeed = 1;

    float xMove = 0;

    private void Update()
    {
        xMove -= Time.deltaTime * moveSpeed;
        rt.anchoredPosition = new Vector3(xMove, 0, 0);
        if (xMove < -1656)
            xMove += 1656;

        if (Input.GetKeyDown(KeyCode.A))
        {
            xMove = -1600;
        }
    }
}
