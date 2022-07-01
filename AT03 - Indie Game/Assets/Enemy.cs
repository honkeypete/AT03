using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : FiniteStateMachine, IInteractable
{
    public Bounds bounds;
    public float viewRadius = 5f;
    public float stunCooldown = 3f;
    public Transform player;
    public EnemyIdleState idleState;
    public EnemyWanderState wanderState;
    public EnemyChaseState chaseState;
    public StunState stunState;

    [SerializeField]
    private float cooldownTimer = -1;

    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public AudioSource AudioSource{ get; private set; }
    public bool ForceChasePlayer { get; private set; }
    public StunState Stun { get { return stunState; } }
    public float DistanceToPlayer 
    {
        get
        {
            if(player != null)
            {
                return Vector3.Distance(transform.position, player.transform.position);
            }
            else
            {
                return -1;
            }
        }
    }

    protected override void Awake()
    {
        idleState = new EnemyIdleState(this, idleState);
        wanderState = new EnemyWanderState(this, wanderState);
        chaseState = new EnemyChaseState(this, chaseState);
        entryState = idleState;
        stunState = new StunState(this, stunState);
        ObjectiveItem.ObjectiveActivatedEvent += TriggerForceChasePlayer;
        EndTrigger.VictoryEvent += delegate { SetState(new GameOverState(this)); };
        if (TryGetComponent(out NavMeshAgent agent) == true)
        {
            Agent = agent;
        }
        if (transform.GetChild(0).TryGetComponent(out Animator anim) == true)
        {  
            Anim = anim;
        }
        if (transform.TryGetComponent(out AudioSource audio) == true)
        {
            AudioSource = audio;
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // here we can write custom code to be executed after the original Start definition is run
    }

    // Update is called once per frame

    protected override void Update()
    {
        base.Update();

        if (DistanceToPlayer <= viewRadius)
        {
            if(currentState.GetType() != typeof(EnemyChaseState) && currentState.GetType() != typeof(GameOverState) && currentState.GetType() != typeof(StunState))
            {
                SetState(chaseState);
            }
        }

        if(cooldownTimer >= 0)
        {
            cooldownTimer += Time.deltaTime;
            if(cooldownTimer >= stunCooldown)
            {
                cooldownTimer = -1;
            }
        }
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    private void TriggerForceChasePlayer()
    {
        if(ForceChasePlayer == false)
        {
            Debug.Log("Start chasing the player");
            ForceChasePlayer = true;
            SetState(chaseState);
        }
    }

    public void Activate()
    {
        Debug.Log("Activate stun");
        if(cooldownTimer < 0 && currentState.GetType() != typeof(StunState))
        {
            StartCoroutine(TriggerStun());
        }
    }

    private IEnumerator TriggerStun()
    {
        SetState(Stun);
        yield return new WaitForSeconds(Stun.StunTime);
        cooldownTimer = 0;
    }
}

public abstract class EnemyBehaviourState : IState
{
    protected Enemy Instance { get; private set; }
   

    public EnemyBehaviourState(Enemy instance)
    {
        Instance = instance;

    }

    public abstract void OnStateEnter();

    public virtual void OnStateExit() { }

    public abstract void OnStateUpdate();

    public virtual void DrawStateGizmos() { }
}
[System.Serializable]
public class EnemyIdleState : EnemyBehaviourState
{
    [SerializeField]
    private Vector2 idleTimeRange = new Vector2(3, 10);
    private float timer = -1;
    private float idleTime = 0;
    [SerializeField]
    private AudioClip idleClip;

    public EnemyIdleState(Enemy instance, EnemyIdleState idle) : base(instance)
    {
        idleClip = idle.idleClip;
        idleTimeRange = idle.idleTimeRange;
    }

    public override void OnStateEnter()
    {
        Instance.AudioSource.PlayOneShot(idleClip);
        Instance.Agent.isStopped = true;
        idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        timer = 0;
        //Debug.Log("Idle state entered, waiting for " + idleTime + " seconds. ");
        Instance.Anim.SetBool("isMoving", false);
    }

    public override void OnStateExit()
    {
        timer = -1;
        idleTime = 0;
    }

    public override void OnStateUpdate()
    {   if (Vector3.Distance(Instance.transform.position, Instance.player.position) <= Instance.viewRadius)
        {
            Instance.SetState(Instance.wanderState);
        }
        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer >= idleTime)
            {
                //Debug.Log("Exiting Idle State after " + idleTime + " seconds. ");
                Instance.SetState(Instance.wanderState);
            }
        }
    }
}
[System.Serializable]
public class EnemyWanderState : EnemyBehaviourState {

    [SerializeField]
    private Vector3 targetPosition;
    [SerializeField]
    private float wanderSpeed = 3.5f;
    [SerializeField]
    private AudioClip wanderClip;

    private int currentIndex = 0;

    public bool Enabled { get { return Instance.enabled; } }

    public EnemyWanderState(Enemy instance, EnemyWanderState wander) : base(instance)
    {
        wanderClip = wander.wanderClip;
        wanderSpeed = wander.wanderSpeed;
    }

    public EnemyWanderState(Enemy instance) : base(instance)
    {
    }

    public override void OnStateEnter()
    {
        Instance.AudioSource.PlayOneShot(wanderClip);
        Instance.Agent.speed = wanderSpeed;
        Instance.Agent.isStopped = false;
        Vector3 randomPosInBounds = new Vector3
            (
            Random.Range(-Instance.bounds.extents.x, Instance.bounds.extents.x),
            Instance.transform.position.y,
            Random.Range(-Instance.bounds.extents.z, Instance.bounds.extents.z)
            );
        targetPosition = randomPosInBounds;
        Instance.Agent.SetDestination(targetPosition);
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", false);
    }

    public override void OnStateExit()
    {
    }

    public override void OnStateUpdate()
    {
        Vector3 t = targetPosition;
        t.y = 0;
        if (Vector3.Distance(Instance.transform.position, targetPosition) <= Instance.Agent.stoppingDistance)
        {
            Instance.SetState(Instance.idleState);
        }

        if (Vector3.Distance(Instance.transform.position, Instance.player.position) <= Instance.viewRadius)
        {
            Instance.SetState(Instance.chaseState);
        }
    }

    public override void DrawStateGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}

public class GameOverState : EnemyBehaviourState
{
    public GameOverState(Enemy instance) : base(instance)
    {
    }

    public override void OnStateEnter()
    {
        if (Instance.Agent != null)
        {
            if (Instance.DistanceToPlayer <= Instance.Agent.stoppingDistance)
            {
                HUD.Instance.ActivateEndPrompt(false);
            }
            Instance.Agent.isStopped = true;
        }
        PlayerController.canMove = false;
        MouseLook.mouseLookEnabled = false;
    }

    public override void OnStateUpdate()
    {

    }
}

[System.Serializable]
public class EnemyChaseState : EnemyBehaviourState
{
    [SerializeField]
    private float chaseSpeed = 5f;
    [SerializeField]
    private AudioClip chaseClip;
    public EnemyChaseState(Enemy instance, EnemyChaseState chase) : base(instance)
    {
        chaseClip = chase.chaseClip;
    }

    public override void OnStateEnter()
    {
        Instance.AudioSource.PlayOneShot(chaseClip);
        Instance.Agent.isStopped = false;
        Instance.Agent.speed = chaseSpeed;
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", true);
    }

    public override void OnStateExit()
    {
    }

    public override void OnStateUpdate()
    {
        if(Instance.DistanceToPlayer <= Instance.viewRadius)
        {
            if(Instance.DistanceToPlayer <= Instance.Agent.stoppingDistance)
            {
                Debug.Log(Instance.DistanceToPlayer);
                Instance.SetState(new GameOverState(Instance));
            }
            else
            {
                Instance.Agent.SetDestination(Instance.player.transform.position);
            }
        }
        else
        {
            if (Instance.ForceChasePlayer == false)
            {
                if (Instance.wanderState.Enabled == true)
                {
                    Instance.SetState(Instance.wanderState);
                }
                else
                {
                    Instance.SetState(Instance.wanderState);
                }
            }
            else
            {
                Instance.Agent.SetDestination(Instance.player.transform.position);
            }
        }
    }
}

[System.Serializable]
public class StunState : EnemyBehaviourState
{
    [SerializeField] private float stunTime;

    private float timer = -1;

    public float StunTime { get { return stunTime; } }

    [SerializeField]
    private AudioClip stunClip;

    public StunState(Enemy instance, StunState stun) : base(instance)
    {
        stunClip = stun.stunClip;
        stunTime = stun.stunTime;
    }


    public override void OnStateEnter()
    {
        Instance.AudioSource.PlayOneShot(stunClip);
        Instance.Agent.isStopped = true;
        Instance.Anim.SetTrigger("stun");
        Instance.Anim.SetBool("isMoving", false);
        Instance.Anim.SetBool("isChasing", false);
        timer = 0;
    }


    public override void OnStateUpdate()
    {
        if (timer >= 0)
        {
            timer += Time.deltaTime;
            if (timer >= stunTime)
            {
                timer = -1;
                if (Instance.ForceChasePlayer == false)
                {
                    Instance.SetState(Instance.wanderState);
                }
                else
                {
                    Instance.SetState(Instance.chaseState);
                }
            }
        }
    }
    public override void OnStateExit()
    {
        Debug.Log("Exited stun state.");
    }
}