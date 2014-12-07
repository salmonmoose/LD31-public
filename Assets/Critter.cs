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
    public bool stuck = false;

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
        bool step = true;
        float height;
        float maxHeight = 0f;

        foreach (ContactPoint2D contact in collision.contacts) {
            height = Mathf.Abs(transform.position.y - contact.point.y);

            if (height > maxHeight) { maxHeight = height; }

            if (height > stepheight) {
                step = false;
            }
        }

        if(maxHeight < 0.01) {
            step = false;
        }

        Debug.DrawLine(
            new Vector3(transform.position.x - 0.2f, transform.position.y + maxHeight, 0),
            new Vector3(transform.position.x + 0.2f, transform.position.y + maxHeight, 0),
            Color.yellow
        );

        if(Mathf.Abs(rigidbody2D.velocity.x) < 0.1f) {
            if(stuck) {
                TurnAround();
            } else {
                stuck=true;
            }
            Debug.DrawLine(
                new Vector3(transform.position.x, transform.position.y, transform.position.z),
                new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z),
                Color.magenta
            );
        }

        if (step) {
            falling = false;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + (maxHeight * 1.1f),
                0
            );
        }
    }

    void TurnAround() {
        left = !left;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        foreach (ContactPoint2D contact in collision.contacts) {

            if(Mathf.Abs(transform.position.y - contact.point.y) > stepheight) {
                TurnAround();

                Debug.DrawLine(
                    new Vector3(contact.point.x, transform.position.y, 0),
                    new Vector3(contact.point.x, transform.position.y + 1.0f, 0),
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

                    clone.transform.parent = transform;
                } else {
                    animator.SetBool("falling", false);
                }
            }

            SetVelocity();              

            Debug.DrawRay(contact.point, contact.normal, Color.white);
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

    void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Target") {
            Escape();
        }
    }

    void Escape() {
        if(alive) {
            alive = false;
            animator.SetTrigger("escape");
            Destroy(rigidbody2D);
        }
    }
}
