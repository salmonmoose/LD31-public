using UnityEngine;
using System.Collections;

public class Critter : MonoBehaviour {

    protected Animator animator;

    public bool left = false;
    public bool falling = true;
    public float terminalVelocity = 10.0f;
    public float fallHeight;
    public float stepheight = 0.5f;
    public bool alive = true;

    public GameObject critter_splat;

    void Start () {
        animator = GetComponent<Animator>();
        fallHeight = transform.position.y;
    }

    void FixedUpdate() {
        if(alive) {
            if(!falling) {
                if(
                    rigidbody2D.velocity.y < 0
                    && Mathf.Abs(rigidbody2D.velocity.y) > Mathf.Abs(rigidbody2D.velocity.x)
                    ) {
                    animator.SetBool("falling", true);
                    falling = true;
                    fallHeight = transform.position.y;
                } else {
                    SetVelocity();
                }
            }

            if(rigidbody2D.velocity.x > 0.1) {
                left = false;
            } else if (rigidbody2D.velocity.x < -0.1) {
                left = true;
            }
        }
    }
    
    void Update () {
    
    }

    void OnCollisionStay2D(Collision2D collision) {
        foreach (ContactPoint2D contact in collision.contacts) {

        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        foreach (ContactPoint2D contact in collision.contacts) {

            if(Mathf.Abs(transform.position.y - contact.point.y) > stepheight) {
                if (contact.normal.x < 0) {
                    left = true;
                } else if (contact.normal.x > 0) {
                    left = false;
                }

                Debug.DrawRay(
                    new Vector3(contact.point.x, contact.point.y, 0),
                    new Vector3(contact.point.x, contact.point.y + 1.0f, 0),
                    Color.red
                );
            }

            if(alive) {
                falling = false;
                if (Mathf.Abs(transform.position.y - fallHeight) > terminalVelocity) {
                    alive = false;
                    
                    animator.SetTrigger("splat");
                    Destroy(rigidbody2D);
                    
                    GameObject clone = Instantiate(
                        critter_splat,
                        transform.position,
                        transform.rotation
                    ) as GameObject;
                } else {
                    animator.SetBool("falling", false);
                }
            }

            SetVelocity();              

            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
            
        if(collision.collider.tag == "Target") {

        }
    }

    void SetVelocity() {
        if(alive && !falling) {
            animator.SetBool("left", left );
            if (left) {
                rigidbody2D.velocity = new Vector2(-1, rigidbody2D.velocity.y);
            } else {
                rigidbody2D.velocity = new Vector2( 1, rigidbody2D.velocity.y);
            }
        }
    }

    void StepUp() {
        if(alive) {
            if(left) {
                rigidbody2D.velocity = new Vector2(-1, 1);
            } else {
                rigidbody2D.velocity = new Vector2( 1, 1);
            }
        }
    }
}
