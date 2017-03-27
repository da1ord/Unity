﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairRenderer : MonoBehaviour
{
    public Image topLeaf_;
    public Image bottomLeaf_;
    public Image leftLeaf_;
    public Image rightLeaf_;
    Vector3 topLeafPosition_;
    Vector3 bottomLeafPosition_;
    Vector3 leftLeafPosition_;
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

        // Top leaf
        topLeaf_.transform.position = new Vector3( topLeafPosition_.x, topLeafPosition_.y - leafSize + spread, topLeafPosition_.z );
        // Bottom leaf
        bottomLeaf_.transform.position = new Vector3( bottomLeafPosition_.x, bottomLeafPosition_.y + leafSize - spread, bottomLeafPosition_.z );
        // Left leaf
        leftLeaf_.transform.position = new Vector3( leftLeafPosition_.x + leafSize - spread, leftLeafPosition_.y, leftLeafPosition_.z );
        // Right leaf
        rightLeaf_.transform.position = new Vector3( rightLeafPosition_.x - leafSize + spread, rightLeafPosition_.y, rightLeafPosition_.z );
    }
}