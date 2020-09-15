﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private Animator anima;
    private bool isHurt;
    private bool isGround;
    private int extraJump;

    public Collider2D coll;
    public Collider2D disColl;

    public LayerMask ground;    //地面
    public Text cherryNum;
    public AudioSource jumpAudio,hurtAudio,cherryAudio,deathAudio;
    public Transform top,buttom;

    public float speed;
    public float jumpForce;
    public int cherryCount;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        anima = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()  //FixedUpdate保证不同设备相同的效果
    {
        if(!isHurt)Movement();
        SwitchAnim();
        isGround = Physics2D.OverlapCircle(buttom.position, 0.2f, ground);
        Jump();
    }

    void Movement() //移动
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float facedirection = Input.GetAxisRaw("Horizontal");

        if(horizontalMove != 0) //左右移动
        {
            rigidBody.velocity = new Vector2(horizontalMove * speed * Time.fixedDeltaTime, rigidBody.velocity.y);
            //* Time.deltaTimene能得到一个平滑、不跳帧的运动方式
            anima.SetFloat("running",Mathf.Abs(facedirection));
        }
        else
        {
            rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
        }

        if (facedirection != 0) //随着左右移动改变人物方向
        {
            transform.localScale = new Vector3(facedirection, 1, 1);
        }

        //if (Input.GetButtonDown("Jump")) //按空格跳跃
        //{
        //    if(coll.IsTouchingLayers(ground)) //如果人物接触到了地面
        //    {
        //        jumpAudio.Play(); //播放跳跃音效
        //        rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.fixedDeltaTime); //施加向上跳的速度
        //        anima.SetBool("falling", false);
        //        anima.SetBool("jumping", true); //动画切换
        //    }
        //}
        Crouch();
    }

    void Jump()
    {
        if (isGround)
        {
            extraJump = 2;
        }
        if (Input.GetButtonDown("Jump") && extraJump > 0)
        {
            jumpAudio.Play(); //播放跳跃音效
            extraJump--;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce * Time.fixedDeltaTime); //施加向上跳的速度
            anima.SetBool("falling", false);
            anima.SetBool("jumping", true); //动画切换
            Debug.Log("正常跳！");
            Debug.Log(extraJump);
        }
        //if (Input.GetButtonDown("Jump") && extraJump == 0 && isGround)
        //{
        //    jumpAudio.Play(); //播放跳跃音效
        //    rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce * Time.fixedDeltaTime); //施加向上跳的速度
        //    anima.SetBool("falling", false);
        //    anima.SetBool("jumping", true); //动画切换
        //    Debug.Log("莫名其妙的跳！");
        //}
    }
    void SwitchAnim()//动画控制
    {
        if (rigidBody.velocity.y < 0.1 && !coll.IsTouchingLayers(ground))
        {
            anima.SetBool("jumping", false);
            anima.SetBool("falling", true);
        }
        else if (isHurt)
        {
            if (Mathf.Abs(rigidBody.velocity.x) < 0.01f)
            {
                anima.SetBool("hurt",false);
                isHurt = false;
            }
        }
        else if (coll.IsTouchingLayers(ground)){    //如果与ground有碰撞
            anima.SetBool("falling", false);
        }
    }

    void Crouch()
    {
        if (Input.GetButton("Crouch")&&coll.IsTouchingLayers(ground))
        {
            if (!anima.GetBool("crouch"))
            {
                anima.SetBool("crouch", true);
                disColl.enabled = false;
            }
        }
        else if (!Physics2D.OverlapCircle(top.position,0.2f,ground))
        {
            anima.SetBool("crouch", false);
            disColl.enabled = true;
        }
    }

    void ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collections")
        {
            collision.GetComponent<Animator>().Play("Collected");
            cherryAudio.Play();
        }
        else if(collision.tag == "DeadLine")
        {
            GetComponent<AudioSource>().enabled = false;
            rigidBody.isKinematic = false;
            deathAudio.Play();
            Invoke("ReStart", 2f);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if(anima.GetBool("falling"))
            {
                enemy.JumpOn();
                Debug.Log("踩到啦！");
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce * Time.deltaTime);
                anima.SetBool("falling", false);
                anima.SetBool("jumping", true);
            }
            else if (transform.position.x < collision.gameObject.transform.position.x)
            {
                rigidBody.velocity = new Vector2(-3, rigidBody.velocity.y);
                hurtAudio.Play();
                anima.SetBool("hurt", true);
                isHurt = true;
            }
            else if (transform.position.x > collision.gameObject.transform.position.x)
            {
                rigidBody.velocity = new Vector2(3, rigidBody.velocity.y);
                hurtAudio.Play();
                anima.SetBool("hurt", true);
                isHurt = true;
            }
        }
    }

    public void CherryCount()
    {
        cherryNum.text = (++cherryCount).ToString();
    }
}