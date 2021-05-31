using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{


    [SerializeField] LineRenderer linePrefab;
    [SerializeField] private Block[] blocks;

    private Tetris tetris;

    private const float offset = .5f;

    private float previousTime;
    public float fallTime = 0.8f;

    private bool left_key_pressed = false;
    private bool right_key_pressed = false;
    private bool up_key_pressed = false;
    private bool down_key_pressed = false;

    private float lastInputTime;
    private float inputDeltaTime = .2f;

    // Start is called before the first frame update
    void Start()
    {

        Init();

        StartGame();

    }

    // Update is called once per frame
    void Update()
    {

        InitPlayerInput();
        //自动降落
        tetris.Fall(Time.deltaTime);
    }

    public void StartGame()
    {
        tetris.NextBlock();
    }

    public void Init()
    {
        tetris = new Tetris(new BlockSpawner(blocks));
        // draw row line
        for (int i = 0; i <= Tetris.Height + Tetris.ExtraHeight - 1; i++)
        {
            var row = Instantiate(linePrefab, this.transform);
            row.positionCount = 2;
            row.SetPosition(0, new Vector3(-offset, i - offset));
            row.SetPosition(1, new Vector3(Tetris.Width - offset, i - offset));
        }

        // draw col line
        for (int i = 0; i <= Tetris.Width; i++)
        {
            var col = Instantiate(linePrefab, this.transform);
            col.positionCount = 2;
            col.SetPosition(0, new Vector3(i - offset, -offset));
            col.SetPosition(1, new Vector3(i - offset, Tetris.Height + Tetris.ExtraHeight - 1 - offset));
        }
    }


    // 玩家输入控制
    private void InitPlayerInput()
    {

        MoveBlockDown();
        MoveBlockLeft();
        MoveBlockRight();
        SpinBlock();
        HoldBlock();
    }


    void HoldBlock()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            tetris.HoldBlock();
        }

    }
    void SpinBlock()
    {
        // rotate
        if (Input.GetKeyDown(KeyCode.Z))
        {   // 逆时针旋转
            tetris.AntiClockwiseRotation();
        }
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
        {  // 顺时针旋转
            tetris.ClockwiseRotation();
        }

    }

    // 向下按键
    void MoveBlockDown()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        { // Hard Drop
            tetris.HardDrop();
        }
        else if (!down_key_pressed && Input.GetKey(KeyCode.DownArrow))
        { // Soft Drop
            tetris.SoftDrop();
            down_key_pressed = true;
        }
        else if (down_key_pressed && Input.GetKeyUp(KeyCode.DownArrow))
        { // 松开按键时恢复正常降落速度
            tetris.NormalDrop();
            down_key_pressed = false;
        }
    }

    // 向右按键
    void MoveBlockRight()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { // 按下右键向右移动， 并设置向右状态为 true
            tetris.MoveRight();
            right_key_pressed = true;
        }
        if (right_key_pressed && Input.GetKey(KeyCode.RightArrow))
        {
            if (lastInputTime >= inputDeltaTime)
            { // 当持续按键时间大于可移动时间时，进行移动，并重置持续按键时间
                tetris.MoveRight();
                lastInputTime = 0;
            }
            else
            {
                lastInputTime += Time.deltaTime;
            }
        }
        else if (right_key_pressed && Input.GetKeyUp(KeyCode.RightArrow))
        { // 当向右状态为 true, 并且松开 右键时
            // 改变向右方向为 true 并重置输入时间
            right_key_pressed = false;
            lastInputTime = 0;
        }
    }

    // 向左按键
    void MoveBlockLeft()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        { // 按下左键向左移动， 并设置向左状态为 true
            tetris.MoveLeft();
            left_key_pressed = true;
        }

        if (left_key_pressed && Input.GetKey(KeyCode.LeftArrow))
        { // 当向左状态为 true 并且持续按住 左键时

            if (lastInputTime >= inputDeltaTime)
            { // 当持续按键时间大于可移动时间时，进行移动，并重置持续按键时间
                tetris.MoveLeft();
                lastInputTime = 0;
            }
            else
            {
                lastInputTime += Time.deltaTime;
            }
        }
        else if (left_key_pressed && Input.GetKeyUp(KeyCode.LeftArrow))
        { // 当向左状态为 true, 并且松开 左键时
            // 改变向左状态 为 true 置持续按键输入时间
            left_key_pressed = false;
            lastInputTime = 0;
        }
    }

}
