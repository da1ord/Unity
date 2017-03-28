using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairRenderer : MonoBehaviour
{
    // Top leaf image
    public Image topLeaf_;
    // Bottom leaf image
    public Image bottomLeaf_;
    // Left leaf image
    public Image leftLeaf_;
    // Right leaf image
    public Image rightLeaf_;

    // Top leaf image position
    Vector3 topLeafPosition_;
    // Bottom leaf image position
    Vector3 bottomLeafPosition_;
    // Left leaf image position
    Vector3 leftLeafPosition_;
    // Right leaf image position
    Vector3 rightLeafPosition_;

    void Start()
    {
        topLeafPosition_ = topLeaf_.transform.position;
        bottomLeafPosition_ = bottomLeaf_.transform.position;
        leftLeafPosition_ = leftLeaf_.transform.position;
        rightLeafPosition_ = rightLeaf_.transform.position;
    }

    void OnGUI()
    {
        // Leaf size
        float leafSize = 20;
        // Spread coefficient
        float spread = PlayerShooting.aimSpread_ * leafSize;

        // Top leaf positioning
        topLeaf_.transform.position = new Vector3( topLeafPosition_.x, topLeafPosition_.y - leafSize + spread, topLeafPosition_.z );
        // Bottom leaf positioning
        bottomLeaf_.transform.position = new Vector3( bottomLeafPosition_.x, bottomLeafPosition_.y + leafSize - spread, bottomLeafPosition_.z );
        // Left leaf positioning
        leftLeaf_.transform.position = new Vector3( leftLeafPosition_.x + leafSize - spread, leftLeafPosition_.y, leftLeafPosition_.z );
        // Right leaf positioning
        rightLeaf_.transform.position = new Vector3( rightLeafPosition_.x - leafSize + spread, rightLeafPosition_.y, rightLeafPosition_.z );
    }
}
