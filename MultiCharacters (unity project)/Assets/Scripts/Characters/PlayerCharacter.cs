using Cinemachine;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerCharacter : Character
{
    private float moveSpeed;
    private readonly float rotationSpeed = 10;

    private readonly Transform camera;
    
    private Vector3 movement;
    private Quaternion targetRotation;

    private GameManager GameManager { get; set; }
    private CoroutineRunner CoroutineRunner { get; set; }

    public PlayerCharacter(GameManager gameManager, GameObject gameObject, Material selectedMaterial) 
        : base(gameObject, selectedMaterial)
    {
        GameManager = gameManager;
        CoroutineRunner = new CoroutineRunner(GameManager);

        if (!networkObject.IsOwner)
            return;

        SetSkin();

        camera = GameObject.FindObjectOfType<Camera>().transform;
        var cinemachine = GameObject.FindObjectOfType<CinemachineFreeLook>();

        cinemachine.LookAt = transform;
        cinemachine.Follow = transform;
    }

    public void Update()
    {
        if (!networkObject.IsOwner)
            return;

        CheckStates();
    }

    private void CheckStates()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        movement = camera.TransformDirection(new Vector3(horizontalInput, 0f, verticalInput)).normalized;
        movement.y = 0f;

        if (movement.magnitude > 0)
        {
            targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (CharacterState != CharacterStates.Sprinting && Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftAlt))
            {
                SetCharacterState(CharacterStates.Sprinting);
                GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, SPRINT);
                CoroutineRunner.Start(SmoothChangeSpeed(7));
                return;
            }
            if (CharacterState != CharacterStates.Walking && Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.Space))
            {
                SetCharacterState(CharacterStates.Walking);
                CoroutineRunner.Start(PlayAfterFinished(WALK_START, WALK));
                CoroutineRunner.Start(SmoothChangeSpeed(2));
                return;
            }
            if (CharacterState == CharacterStates.Idle)
            {
                SetCharacterState(CharacterStates.Running);
                GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, RUN);
                CoroutineRunner.Start(SmoothChangeSpeed(5));
                return;
            }

            if (Input.GetKeyUp(KeyCode.Space))
                CoroutineRunner.Start(StopSprint());
            if (Input.GetKeyUp(KeyCode.LeftAlt))
                SetCharacterState(CharacterStates.Idle);
        }
        else
        {
            if (CharacterState == CharacterStates.Sprinting)
            {
                SetCharacterState(CharacterStates.Idle);
                GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, RUNSTOP);
                CoroutineRunner.StopAll();
                moveSpeed = 0;
                return;
            }
            if (CharacterState != CharacterStates.Idle && CharacterState != CharacterStates.Sprinting)
            {
                SetCharacterState(CharacterStates.Idle);
                GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, IDLE);
                CoroutineRunner.StopAll();
                moveSpeed = 0;
            }

            SetCharacterState(CharacterStates.Idle);
        }
    }

    public void FixedUpdate() => Move();

    private void Move()
    {
        if (rigidbody != null)
            rigidbody.MovePosition(rigidbody.position + moveSpeed * Time.fixedDeltaTime * movement);
    }

    public void SetSkin()
    {
        GameManager.ChangeSkinServerRpc(networkObject.OwnerClientId, Material.ToString());
    }

    private IEnumerator SmoothChangeSpeed(float newSpeed)
    {
        while (Mathf.Abs(moveSpeed - newSpeed) > 0.1f)
        {
            yield return new WaitForSeconds(0.005f);
            if (moveSpeed < newSpeed)
                moveSpeed += 0.1f;
            else
                moveSpeed -= 0.1f;
        }
        moveSpeed = newSpeed;
    }

    private IEnumerator PlayAfterFinished(string firstAnimName, string secondAnimName)
    {
        GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, firstAnimName);
        yield return new WaitForSeconds(0.01f);
        GameManager.PlayAnimServerRpc(networkObject.OwnerClientId, secondAnimName);
    }

    private IEnumerator StopSprint()
    {
        yield return new WaitForSeconds(0.45f);
        SetCharacterState(CharacterStates.Idle);
    }
}