using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Transform leftHand, rightHand, leftFoot, rightFoot;
    public Vector3 leftHandOrigin, rightHandOrigin, leftFootOrigin, rightFootOrigin;

    public float animationSpeed, animationDuration;
    private float animationState = 0f;
    void Start()
    {
        leftHandOrigin = leftHand.localPosition;
        rightHandOrigin = rightHand.localPosition;
        leftFootOrigin = leftFoot.localPosition;
        rightFootOrigin = rightFoot.localPosition;
    }

    void Update()
    {
        animationState += Time.deltaTime * animationSpeed / animationDuration;
        if (animationState > 1f) animationState -= 1f;

        float yOffset = 3 * animationState;
        if (yOffset > 1.5f) yOffset = 3f - yOffset;
        Vector3 leftOffset = new Vector3(2f, 1f + yOffset);
        Vector3 rightOffset = new Vector3(-2f, 1f + yOffset);
        leftHand.localPosition = leftHandOrigin + leftOffset;
        rightHand.localPosition = rightHandOrigin + rightOffset;
    }
}
