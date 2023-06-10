using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;

public class GameManager : NetworkBehaviour
{
    [Header("ConnectionUI")]
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private TMP_InputField _ipField;

    [Header("SelectSkinUI")]
    [SerializeField] private Button _redSkinButton;
    [SerializeField] private Button _blueSkinButton;
    [SerializeField] private Button _yellowSkinButton;
    [SerializeField] private GameObject _selectSkinUI;

    [Header("SkinMaterials")]
    [SerializeField] private Material _redMat;
    [SerializeField] private Material _blueMat;
    [SerializeField] private Material _yellowMat;

    private Material selectedMaterial;

    public List<PlayerCharacter> Players { get; private set; } = new();

    private void Awake()
    {
        SwitchConnectionButtons(false);

        _redSkinButton.onClick.AddListener(() => ChangeSelectedMaterial(_redMat));
        _blueSkinButton.onClick.AddListener(() => ChangeSelectedMaterial(_blueMat));
        _yellowSkinButton.onClick.AddListener(() => ChangeSelectedMaterial(_yellowMat));

        _hostButton.onClick.AddListener(() =>
        {
            NetworkManager.StartHost();
            SpawnPlayer(OwnerClientId);
            SwitchConnectionButtons(false);
        });
        _clientButton.onClick.AddListener(() =>
        {
            NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = _ipField.text;
            NetworkManager.StartClient();
            NetworkManager.OnClientConnectedCallback += SpawnPlayer;
            NetworkManager.OnClientConnectedCallback += UpdateSkinServerRpc;
            SwitchConnectionButtons(false);
        });
    }

    private void Update()
    {
        if (Players.Count > 0)
            foreach (var player in Players)
                player.Update();
    }

    private void FixedUpdate()
    {
        if (Players.Count > 0)
            foreach (var player in Players)
                player.FixedUpdate();
    }

    private void SpawnPlayer(ulong _)
    {
        var playerObject = NetworkManager.SpawnManager.GetLocalPlayerObject();
        var playerController = new PlayerCharacter(this, playerObject.gameObject, selectedMaterial);

        Cursor.visible = false;
        Players.Add(playerController);
    }

    private void SwitchConnectionButtons(bool value)
    {
        _hostButton.gameObject.SetActive(value);
        _ipField.gameObject.SetActive(value);
        _clientButton.gameObject.SetActive(value);
    }

    private void ChangeSelectedMaterial(Material mat)
    {
        selectedMaterial = mat;

        _selectSkinUI.SetActive(false);
        SwitchConnectionButtons(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayAnimServerRpc(ulong id, string animName)
    {
        PlayAnimClientRpc(id, animName);
    }

    [ClientRpc]
    private void PlayAnimClientRpc(ulong id, string animName)
    {
        if (!IsOwner)
            return;

        var animator = NetworkManager.SpawnManager.GetPlayerNetworkObject(id).GetComponent<Animator>();
        animator.CrossFadeInFixedTime(animName, 0.25f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateSkinServerRpc(ulong _)
    {
        foreach (var player in Players)
            player.SetSkin();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSkinServerRpc(ulong id, string mat)
    {
        var networkObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(id);
        ChangeSkinClientRpc(networkObject, mat);
    }

    [ClientRpc]
    private void ChangeSkinClientRpc(NetworkObjectReference target, string mat)
    {
        if (target.TryGet(out NetworkObject networkObject))
        {
            var skin = networkObject.GetComponentInChildren<SkinnedMeshRenderer>();

            if (mat == _redMat.ToString())
                skin.material = _redMat;
            else if (mat == _blueMat.ToString())
                skin.material = _blueMat;
            else if (mat == _yellowMat.ToString())
                skin.material = _yellowMat;
        }
    }
}