using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScaleSpriteToFillOrthographicCamera : MonoBehaviour
{
    [Header("Set in Inspector")]
    [Tooltip("If the camToMatch is not orthographic, this code will do nothing.")]
    public Camera   camToMatch;

    // Use this for initialization
    void Start()
    {
        if (camToMatch == null || !camToMatch.orthographic)
        {
            return;
        }

        transform.localScale = Vector3.one;
        Renderer rend = GetComponent<Renderer>();
        Vector3 baseSize = rend.bounds.size;
        Vector3 camSize = baseSize;
        camSize.y = camToMatch.orthographicSize * 2;
        camSize.x = camSize.y * camToMatch.aspect;

        Vector3 scale = camSize.ComponentDivide(baseSize);

        transform.localScale = scale;
    }
}
