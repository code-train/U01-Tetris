using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 俄罗斯方块游戏类
/// </summary>
public class Tetris
{

    public int level = 1;
    public int score = 0;
    public int line = 0;

    public const float DeltaNormal = 1f;
    public const float DeltaSoft = .03f;

    public const int Height = 20;
    public const int Width = 10;
    public const int ExtraHeight = 2;
    public static Transform[,] Grid;
    private Block currentBlock;
    private bool gameOver = false; // 游戏是否结束

    private bool landed = false;
    private bool isLanding = false;

    private float lastWaitTime;

    private float waitLandTime = .5f;

    private float lastFallTime;

    public float time;

    public Block m_preview;

    private bool landedWithRotate = false;
    private bool isTSpin = false;
    private bool isMini = false;
    private bool lastClearIsSpecial = false;

    private bool holdedThisTurn;
    private Block holded;

    private Vector3 originSize;
    private float holdedViewPosX = -2.4f;
    private float holdedViewPosY = 16;
    private float holdedViewPosOffset = .3f;
    private float sizeScale = .6f;

    private enum MoveDelda
    {
        Normal,
        SoftDrop,
        HardDrop
    }

    private MoveDelda moveDelta = MoveDelda.Normal;

    public float FallDeltaTime
    {
        get
        {
            switch (moveDelta)
            {
                case MoveDelda.Normal:
                    return DeltaNormal / level;
                case MoveDelda.SoftDrop:
                    return DeltaSoft / level;
            }

            return 0f;
        }
    }
    private BlockSpawner m_spawner;

    public Tetris(BlockSpawner spawner)
    {
        Grid = new Transform[Width, Height + ExtraHeight];
        m_spawner = spawner;
        m_spawner.InitNextChainSlot();
    }

    // 自动降落
    public void Fall(float deltaTime)
    {
        if (gameOver || currentBlock == null) return;

        time += deltaTime;

        if (!landed)
        {

            if (!isLanding)
            {
                if (lastFallTime >= FallDeltaTime)
                {
                    isLanding = !currentBlock.MoveDown();
                    lastFallTime = 0;
                }
                else
                {
                    lastFallTime += deltaTime;
                }
            }
            else
            {
                if (lastWaitTime >= waitLandTime)
                {
                    landed = true;
                    lastWaitTime = 0;
                }
                else
                {
                    isLanding = !currentBlock.MoveDown();
                    lastWaitTime += Time.deltaTime;
                }
            }
        }
        else
        {
            DestroyPreview();
            AddToGrid();

            if (gameOver) return;

            CheckForLines();

            NextBlock();

            moveDelta = MoveDelda.Normal;
            isLanding = false;
            landed = false;
        }

    }

    private void AddToGrid()
    {
        bool res = true;
        foreach (Transform child in currentBlock.transform)
        {
            var x = EX.Float2Int(child.transform.position.x);
            var y = EX.Float2Int(child.transform.position.y);

            if (y < Height) res = false;

            Grid[x, y] = child;
        }

        if (res)
        {
            gameOver = true;
            currentBlock = null;
        }

    }

    private void CheckForLines()
    {

        // 检查是否是 T-Spin
        if (currentBlock is BlockT blockT)
        {
            isTSpin = blockT.IsTSpin(out isMini) && landedWithRotate;
            Debug.LogFormat("roateted {0}, special {1}", landedWithRotate, isTSpin);
        }

        // lock block move
        currentBlock = null;

        for (int i = Height + ExtraHeight - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
            }
        }
    }

    private bool HasLine(int i)
    {
        for (int j = 0; j < Width; j++)
        {
            if (Grid[j, i] == null)
            {
                return false;
            }
        }
        return true;
    }

    private void DeleteLine(int i)
    {
        for (int j = 0; j < Width; j++)
        {
            GameObject.Destroy(Grid[j, i].gameObject);
            Grid[j, i] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < Height; y++)
        {
            for (int j = 0; j < Width; j++)
            {
                if (Grid[j, y] != null)
                {
                    Grid[j, y - 1] = Grid[j, y];
                    Grid[j, y] = null;
                    Grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }


    public void NextBlock()
    {
        if (gameOver) return;

        currentBlock = m_spawner.NextBlock();
        if (!currentBlock.MoveDown())
        {
            foreach (Transform child in currentBlock.transform)
            {
                var x = EX.Float2Int(child.transform.position.x);
                var y = EX.Float2Int(child.transform.position.y);

                if (Grid[x, y] != null)
                {
                    gameOver = true;
                    currentBlock = null;
                    return;
                }
            }
        }
        InitPreview();
        UpdatePreview();
        m_spawner.UpdateNextChainSlot();
        holdedThisTurn = false;
        landedWithRotate = false;
        isTSpin = false;

    }

    public void HardDrop()
    {
        if (currentBlock == null || landed) return;
        landedWithRotate = false;

        while (currentBlock.MoveDown())
        {

        }
        landed = true;

    }
    // 软降
    public void SoftDrop()
    {
        if (currentBlock == null || landed) return;
        moveDelta = MoveDelda.SoftDrop;
    }
    // 正常降落
    public void NormalDrop()
    {
        if (currentBlock == null || landed) return;
        moveDelta = MoveDelda.Normal;
    }

    public void MoveLeft()
    {
        if (currentBlock == null || landed) return;
        if (currentBlock.MoveLeft())
        {
            landedWithRotate = false;
            lastWaitTime = 0;
            UpdatePreview();
        }
    }

    public void MoveRight()
    {
        if (currentBlock == null || landed) return;
        if (currentBlock.MoveRight())
        {
            landedWithRotate = false;
            lastWaitTime = 0;
            UpdatePreview();
        }
    }


    /// <summary>
    /// 逆时针旋转
    /// <summary>
    public void AntiClockwiseRotation()
    {
        if (currentBlock == null || landed) return;
        if (currentBlock.AntiClockwiseRotation())
        {
            landedWithRotate = true;
            lastWaitTime = 0;
            UpdatePreview();
        }
    }

    /// <summary>
    /// 顺时针旋转
    /// </summary>
    public void ClockwiseRotation()
    {
        if (currentBlock == null || landed) return;
        if (currentBlock.ClockwiseRotation())
        {
            landedWithRotate = true;
            lastWaitTime = 0;
            UpdatePreview();
        }
    }

    /// <summary>
    /// 暂存方块
    /// </summary>
    public void HoldBlock()
    {
        if (currentBlock == null) return;

        if (holdedThisTurn) return;

        originSize = currentBlock.transform.localScale;

        if (holded != null)
        {

            //交换当前方块与已暂存方块
            var tmp = currentBlock;
            currentBlock = holded;
            holded = tmp;

            currentBlock.transform.position = new Vector3(Tetris.Width / 2, Tetris.Height);
            currentBlock.transform.localScale = originSize;

            // 重新生成预览下落位置
            DestroyPreview();
            InitPreview();
            UpdatePreview();
        }
        else
        {
            DestroyPreview();
            holded = currentBlock;
            NextBlock();
        }

        moveDelta = MoveDelda.Normal;

        holdedThisTurn = true;
        if (holded is BlockO || holded is BlockI)
        {
            holded.transform.position = new Vector3(holdedViewPosX - holdedViewPosOffset, holdedViewPosY, 0);
        }
        else
        {
            holded.transform.position = new Vector3(holdedViewPosX, holdedViewPosY, 0);
        }
        holded.transform.localScale = new Vector3(sizeScale, sizeScale);
        holded.transform.rotation = Quaternion.Euler(0, 0, 0);
        holded.ResetState();

    }



    #region Block Preview
    private void InitPreview()
    {
        m_preview = GameObject.Instantiate(currentBlock);
        foreach (Transform child in m_preview.transform)
        {
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sr.color = new Color(1, 1, 1, .3f);
                sr.sortingOrder = 15;
            }
        }
    }

    private void UpdatePreview()
    {
        if (!m_preview) return;
        m_preview.transform.position = currentBlock.transform.position;
        m_preview.transform.rotation = currentBlock.transform.rotation;
        LandingPoint(m_preview);
    }

    private void DestroyPreview()
    {
        if (m_preview != null)
        {
            GameObject.Destroy(m_preview.gameObject);
        }
    }

    private void LandingPoint(Block block)
    {
        while (block.MoveDown())
        {

        }


    }

    #endregion

}
