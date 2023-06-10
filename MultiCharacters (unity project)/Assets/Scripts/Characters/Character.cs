using UnityEngine;
using Unity.Netcode;

public class Character
{
    public const string IDLE = "idle_stance";
    public const string WALK_START = "walk_start";
    public const string WALK = "walk_civi";
    public const string RUN = "run_civi";
    public const string SPRINT = "sprint_panic";
    public const string RUNSTOP = "run_stop";

    public readonly Transform transform;
    public readonly Rigidbody rigidbody;
    public readonly NetworkObject networkObject;

    public Material Material { get; private set; }
    public CharacterStates CharacterState { get; set; }

    public enum CharacterStates
    {
        Idle,
        Walking,
        Running,
        Sprinting
    }

    public Character(GameObject gameObject, Material selectedMaterial)
    {
        Material = selectedMaterial;
        transform = gameObject.transform;
        rigidbody = gameObject.GetComponent<Rigidbody>();
        networkObject = gameObject.GetComponent<NetworkObject>();
        gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = selectedMaterial;
    }

    public void SetCharacterState(CharacterStates state)
    {
        if (CharacterState != state)
            CharacterState = state;
    }
}
