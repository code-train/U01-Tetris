using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [SerializeField] protected Vector3 rotatePoint;

    private Vector3 axis = new Vector3(0, 0, 1);

    //      0
    //   3     1
    //      2
    //
    // 0 - spawn state
    // 1 - rotate right
    // 2 - 2 successive rotations in either direction form spawn
    // 3 - rotate left
    protected int state = 0;
    private Vector2Int[] m_direction = new Vector2Int[]
    {
        // left
        new Vector2Int(-1,0),
        // right
        new Vector2Int(1,0),
        // down
        new Vector2Int(0,-1),
        // up
        new Vector2Int(0,1)
    };

    //向下移动
    public bool MoveDown(int amount = 1)
    {
        transform.position += (Vector3Int)m_direction[2] * amount;
        if (!ValidChild())
        {
            transform.position -= (Vector3Int)m_direction[2] * amount;
            return false;
        }

        return true;
    }

    // 向左移动
    public bool MoveLeft(int amount = 1)
    {
        transform.position += (Vector3Int)m_direction[0] * amount;
        if (!ValidChild())
        {
            transform.position -= (Vector3Int)m_direction[0] * amount;
            return false;
        }
        return true;
    }

    // 向右移动
    public bool MoveRight(int amount = 1)
    {
        transform.position += (Vector3Int)m_direction[1] * amount;
        if (!ValidChild())
        {
            transform.position -= (Vector3Int)m_direction[1] * amount;
            return false;
        }
        return true;
    }


    // 逆时针旋转
    public virtual bool AntiClockwiseRotation()
    {

        transform.RotateAround(transform.TransformPoint(rotatePoint), axis, 90);

        var _state = state - 1 < 0 ? 3 : state - 1;

        if (!ValidChild())
        {
            if (WallKickTest(_state, out Vector2Int result))
            {
                Debug.Log(result);
                SingleMove(result.x, result.y);
                state = _state;
                return true;
            }
            else
            {
                transform.RotateAround(transform.TransformPoint(rotatePoint), axis, -90);
            }
        }

        Debug.LogFormat("{0} , {1}", state, _state);
        state = _state;

        return true;
    }
    // 顺时针旋转
    public virtual bool ClockwiseRotation()
    {
        transform.RotateAround(transform.TransformPoint(rotatePoint), axis, -90);

        var _state = state + 1 > 3 ? 0 : state + 1;

        // kick wall
        if (!ValidChild())
        {
            if (WallKickTest(_state, out Vector2Int result))
            {
                Debug.Log(result);

                SingleMove(result.x, result.y);
                state = _state;
                return true;
            }
            else
            {
                transform.RotateAround(transform.TransformPoint(rotatePoint), axis, 90);
                return false;
            }
        }

        Debug.LogFormat("{0} , {1}", state, _state);
        state = _state;
        return true;
    }
    // 检查方块
    private bool ValidChild()
    {

        foreach (Transform child in transform)
        {
            var x = Mathf.RoundToInt(child.position.x);
            var y = Mathf.RoundToInt(child.position.y);
            if (!Valid(x, y))
                return false;
        }

        return true;
    }

    private bool ValidChild(Vector2 move)
    {
        foreach (Transform child in transform)
        {
            var x = EX.Float2Int(child.position.x + move.x);
            var y = EX.Float2Int(child.position.y + move.y);
            if (!Valid(x, y)) return false;
        }
        return true;
    }

    // 检查方块
    private bool Valid(int x, int y)
    {
        // 边框检查
        if (x >= Tetris.Width || x < 0 || y >= Tetris.Height + Tetris.ExtraHeight || y < 0) return false;
        // 格子检查
        if (Tetris.Grid[x, y] != null) return false;
        return true;
    }



    private bool WallKickTest(int next, out Vector2Int result)
    {
        var data = GetWallKickData(next);
        Debug.LogFormat("Next: {0}, DATA: {1}", next, data);

        for (int i = 0; i < data.Length; i++)
        {
            Debug.LogFormat("DATA[{0}]: {1}", i, data[i]);
            if (!ValidChild(data[i]))
            {
                continue;
            }
            else
            {
                result = data[i];
                return true;
            }
        }
        result = Vector2Int.zero;
        return false;
    }

    protected virtual Vector2Int[] GetWallKickData(int next)
    {
        switch (state)
        {
            case 0:
                if (next == 1) return WallKickData.Other[0];
                if (next == 3) return WallKickData.Other[7];
                break;
            case 1:
                if (next == 0) return WallKickData.Other[1];
                if (next == 2) return WallKickData.Other[2];
                break;
            case 2:
                if (next == 1) return WallKickData.Other[3];
                if (next == 3) return WallKickData.Other[4];
                break;
            case 3:
                if (next == 2) return WallKickData.Other[5];
                if (next == 0) return WallKickData.Other[6];
                break;
            default:
                break;
        }

        return null;
    }


    public void SingleMove(int x, int y)
    {
        transform.position += new Vector3(x, y);
    }

    public void SignleUp(int amount = 1)
    {
        transform.position += (Vector3Int)m_direction[3] * amount;
    }

    public void ResetState()
    {
        state = 0;
    }

}
