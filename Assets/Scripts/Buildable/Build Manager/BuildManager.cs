﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour {
   
    private int index;
    public float buildRate = 0.1f;
    public float repairRate = 0.5f;
    private bool requestToBuild = false;    
    [SerializeField] private bool canRequest = true;
    public bool canRepair = false;
    public bool canRemove = false;
    public List<Buildable> buildList;  // array of buildables. What the player can currently build  
    private Buildable currentBuilding;
    private PlayerPickup playerResource;
    public GameObject damageNumber;
    [SerializeField] private Buildable blockingObject;
    [SerializeField] private Collider2D blockingCollider;
    public Transform spawnPoint;            // Transform of gameobject in front of player
    public Grid grid;
    public GunUI gunDisplay;
    public CostUI costUI;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.instance;
    }


    // Start function
    private void Awake() {
        //damageNumber = GameObject.FindGameObjectWithTag("DamageNumber");
        playerResource = GetComponent<PlayerPickup>();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        index = 0;        
    }

    // Update function
    private void Update() {
        // place turret sprite in build spawn sprite renderer            
        blockingCollider = spawnPoint.GetComponent<DetectingBuildable>().blockingObject;
        canRemove = spawnPoint.GetComponent<DetectingBuildable>().canRemove;
        //canRequest = spawnPoint.GetComponent<DetectingBuildable>().canBuild;
        currentBuilding = buildList[index];

        if (Input.GetButton("Fire1") && canRequest)
        {
                              // Player builds with 'C' key
            requestToBuild = true;
            //gameObject.GetComponent<BuildManager>().enabled = false;
            //gameObject.GetComponent<PlayerController>().mode = PlayerController.Mode.SHOOTING_MODE;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f) {                             // Player choose what they build with the scroll wheel 
            index++;

            if (index >= buildList.Count) {
                index = 0;
            }
        }

        if (Input.GetKey(KeyCode.E) && canRemove) {
            RepairBuildable();
        } //else if (Input.GetKey(KeyCode.C) && canRequest) {                        // Player builds with 'C' key
        //requestToBuild = true; }
         else {

        }           
        
        if (Input.GetButton("Fire2") && canRemove) {                                 
            blockingObject = spawnPoint.GetComponent<DetectingBuildable>().blockingObject.GetComponent<Buildable>();
            playerResource.DisplayNumber(blockingObject.buildCost / 2, Color.green);
            blockingObject.Remove();
            playerResource.IncrementResource(blockingObject.buildCost / 2);        // Destroying a building only returns half the cost                          
        }

        costUI.UpdateCosts(buildList[index].buildCost, buildList[index].GetComponent<TurretAI>().turretName);
        gunDisplay.UpdateGunDisplay(buildList[index].GetComponent<SpriteRenderer>().sprite);
    }

    // TODO: Display prompt saying player can't build
    // FixedUpdate function
    private void FixedUpdate() {
        if (requestToBuild)                                                         // Player requested to build
        {                                                           
            if (currentBuilding.isBuildable(spawnPoint))                            // Determine if player is able to build   
            {                                          
                if (playerResource.GetResourceCount() >= currentBuilding.buildCost) // Determine if player has enough resource
                {
                    canRequest = false;
                    audioManager.PlaySound("Deploy");
                    currentBuilding.Build(spawnPoint, grid);
                    playerResource.DisplayNumber(-currentBuilding.buildCost, Color.red);
                    playerResource.DecrementResource(currentBuilding.buildCost);
                    StartCoroutine(Wait(buildRate));
                }
                else 
                {
                    Debug.Log("Not enough resource!");
                }
            }
            else 
            {
                Debug.Log("Cannot build right now");
            }

            requestToBuild = false;
        }              
    }

    public void RepairBuildable() {
        
        // cost to repairing turrets is decided by how damaged turret is
        // curHP >= 75% { 25% cost }
        // curHP < 75% && curHP >= 50% { 50% cost }
        // curHP <= 50%  { 75% cost }

        blockingObject = spawnPoint.GetComponent<DetectingBuildable>().blockingObject.GetComponent<Buildable>();

        if (blockingObject.canRepair) {
            int repairCost;
            float curHpPercent = blockingObject.CurHP / blockingObject.MaxHP;

            if (curHpPercent >= 0.75) {
                repairCost = (int)(blockingObject.buildCost * 0.25);
            } else if (curHpPercent < 0.75 && curHpPercent >= 0.50) {
                repairCost = (int)(blockingObject.buildCost * 0.50);
            } else {
                repairCost = (int)(blockingObject.buildCost * 0.75);
            }

            if (playerResource.GetResourceCount() >= repairCost) {
                playerResource.DecrementResource(repairCost);
                playerResource.DisplayNumber(-repairCost, Color.red);
                blockingObject.Repair();
            }
            else {
                Debug.Log("You do not have enough resource to repair!");
            }
        }        
    }

    public IEnumerator Wait(float rate) {
        Debug.Log("Waiting");      
        yield return new WaitForSeconds(rate);

        if (rate == buildRate) {
            canRequest = true;
        } else {
            canRepair = true;
        }
    }
}
