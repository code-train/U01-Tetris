using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockO : Block
{
    public override bool AntiClockwiseRotation()
    {
        return false;
    }

    public override bool ClockwiseRotation()
    {
        return false;
    }
}
