using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteShapeGenerator : MonoBehaviour
{
    protected SpriteShapeController controller;

    protected virtual void Awake ()
    {
        controller = GetComponent<SpriteShapeController>();
    }


}
