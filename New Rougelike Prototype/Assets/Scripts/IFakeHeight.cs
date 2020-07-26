using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFakeHeight
{
    FakeHeightObject FakeHeight { get; set; }
    void OnFakeHeightTriggerEnter(Collider2D collider);
    void OnFakeHeightTriggerExit(Collider2D collider);
    void OnFakeHeightTriggerStay(Collider2D collider);
}
