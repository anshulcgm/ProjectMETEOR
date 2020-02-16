
using System;
using UnityEngine;

public class EnemySpinner : Enemy
{
    //the object instantiated when the rammer dies
    public GameObject explosion;
    //the direction we're charging in
    Vector3 chargeDir;
    new void Start()
    {
        statsMenu = GameObject.FindGameObjectWithTag("Menu");
        spinnerSound = Camera.main.GetComponent<Sound_Background>();
        base.Start();
    }

    new void Update()
    {
        base.Update();
    }


    //attack
    Vector3 lastStable;
    Node attackNode = null;
    public GameObject spinnerBlade;
    public float chargeSpeed;
    public bool isPreparingToCharge = false;
    public bool isCharging = false;
    public DateTime startPreparation = DateTime.MaxValue;
    public float timePreparing = 1.5f;
    public float bladeRotSpeedFactor = 2.5f;
    public float bladeRotSpeed = 360;
    public float botDamage = 100;
    public float chargeTilt = 35f;

    public GameObject statsMenu;

    public Sound_Background spinnerSound;
    public override void attack(object tObj)
    {
        Transform t = (Transform)tObj;
        if (isPreparingToCharge)
        {
            GetComponent<AudioSource>().Play();
            follower.disabled = true;
            spinnerBlade.transform.Rotate(0, 0, (bladeRotSpeed + bladeRotSpeed * bladeRotSpeedFactor * (float)(DateTime.Now - startPreparation).TotalSeconds) * Time.deltaTime);
            transform.Rotate(chargeTilt / timePreparing * Time.deltaTime, 0, 0);
            if ((DateTime.Now - startPreparation).TotalSeconds > timePreparing)
            {
                isPreparingToCharge = false;
                isCharging = true;
            }
            return;
        }

        if (isCharging)
        {
            
            spinnerBlade.transform.Rotate(0, 0, (bladeRotSpeed + bladeRotSpeed * bladeRotSpeedFactor * (float)(DateTime.Now - startPreparation).TotalSeconds) * Time.deltaTime);
            r.velocity = chargeDir;
            return;
        }

        spinnerBlade.transform.Rotate(0, 0, bladeRotSpeed * Time.deltaTime);

        //if the attack node is null or invalid
        if (attackNode == null || !isValidAttackNode(attackNode, t))
        {
            //try to get a new one
            attackNode = navScript.TryGetNode(t.position, 5, 20, isValidAttackNode, t);
        }
        //otherwise, set the follower destination to the new attackNode.
        else
        {
            follower.destination = attackNode.location;
        }

        if (Vector3.SqrMagnitude(follower.destination - transform.position) == 0)
        {
            r.velocity = Vector3.zero;
            chargeDir = (t.position - transform.position).normalized * chargeSpeed;
            isPreparingToCharge = true;
            startPreparation = DateTime.Now;
        }

    }

    public bool isValidAttackNode(Node n, object c)
    {
        if (n == null || c == null || !isValidPositionNode(n, c) || (DateTime.Now - n.lastCostTime).TotalSeconds < 5)
        {
            return false;
        }
        float distSqr = Vector3.SqrMagnitude(n.location - ((Transform)c).position);
        RaycastHit hit;
        Physics.Raycast(n.location, (n.location - ((Transform)c).position).normalized, out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return !Physics.Linecast(n.location, ((Transform)c).position) && distSqr > 1.5 * 1.5;
        }
        return !Physics.Linecast(n.location, ((Transform)c).position) && hit.collider.gameObject.layer != 12 && hit.collider.gameObject.layer != 13 && distSqr > 1.5 * 1.5;
    }

    public override void onColl(Collision c)
    {
        //take away 200 health here
        if(c.gameObject.Equals(player))
        {
            statsMenu.GetComponent<UI>().player.damagePlayer(200);
        }
        base.onColl(c);
        if (isCharging)
        {
            kill();
        }
    }

    public override void kill()
    {
        //blow up
        statsMenu.GetComponent<UI>().player.killSpinner();
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        //kill yourself
        base.kill();
    }


}

