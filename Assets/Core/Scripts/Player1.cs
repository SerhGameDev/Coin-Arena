using UnityEngine;

public class Player1 : MonoBehaviour
{
    public PlayerController Controller;

    public float Speed = 5f;
    public float JumpHeight = 3f;

    public float DashSpeed = 15f;
    public float DashTime = 0.2f;
    public float DashCooldown = 1f;
    public float PushForce = 10f;
    public KeyCode  JumpKey = KeyCode.Space;

    void Update()
    {
        Controller.SetSpeed(Speed);
        Controller.SetJumpHeight(JumpHeight);
        
        Controller.SetDashSpeed(DashSpeed);
        Controller.SetDashTime(DashTime);
        Controller.SetDashCooldown(DashCooldown);


        if (Input.GetKey(KeyCode.A))
        {
            Controller.MoveLeft();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Controller.MoveRight();
        }
        else
        {
            Controller.Stop();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Controller.Jump();
        }


        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Controller.Dash();
            PlayerInfo.AddMoney(PlayerId.Player1,1);
        }
        Controller.SetPushDistance(3f);
        Controller.ShowRays(false);
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject leftPlayer = Controller.GetObjectLeft();
            GameObject rightPlayer = Controller.GetObjectRight();

            if (leftPlayer != null)
            {
                Controller.Push(leftPlayer, PushForce);
            }

            if (rightPlayer != null)
            {
                Controller.Push(rightPlayer, PushForce);
            }
        }
    }
}