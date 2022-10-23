using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTest : MonoBehaviour
{
    public Material mat;

    public void randomMatColor()
    {
        mat.SetColor("_Color", new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }
}
