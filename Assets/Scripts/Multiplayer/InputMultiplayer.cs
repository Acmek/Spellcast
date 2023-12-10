using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System.IO;

using Photon.Pun;
using Photon.Realtime;

public class InputMultiplayer : MonoBehaviourPunCallbacks
{
    [Header("Balls")]
    [SerializeField] private GameObject[] ballPrefabs;
    [SerializeField] private GameObject[] powerupPrefabs;
    [SerializeField] private List<GameObject> activePowerups;

    [Header("Effects")]
    [SerializeField] private GameObject particle;
    [SerializeField] private GameObject multiplayerParticle;
    [SerializeField] private ErrorCode error;
    [SerializeField] private ComboCode combo;

    [Header("Player")]
    [SerializeField] private PlayerCode player;
    [SerializeField] private float attackLevel;
    [SerializeField] private TextAsset dict;

    [Header("Sound")]
    [SerializeField] private AudioClip inputError;
    [SerializeField] private GameObject spellSpawn;

    [Header("Multiplayer")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private LineRenderer otherLine;
    [SerializeField] private EnemyMultiplayer enemyMultiplayerCode;
    [SerializeField] private float enemyAttackLevel;

    private TMP_Text text;
    private LineRenderer line;
    private string input = "";

    private string[] words;
    private List<string> used;

    private bool mouseDown;

    private List<GameObject> balls;
    private List<GameObject> otherBalls;

    private bool draw;
    private bool canSelect;

    private Coroutine destroyingBalls;
    private bool isDestroyingBalls;
    private bool frozen;

    private List<GameObject> exploded;

    private string longestWord = "";
    private int lettersUsed;
    private int wordsSpelled;

    private bool gameEnd;

    private AudioSource audioSource;

    private PhotonView view;

    void Start() {
        view = GetComponent<PhotonView>();
        text = GetComponent<TMP_Text>();
        line = GetComponent<LineRenderer>();
        used = new List<string>();
        balls = new List<GameObject>();
        otherBalls = new List<GameObject>();
        exploded = new List<GameObject>();
        audioSource = GetComponent<AudioSource>();

        // Add Words to Dictionary
        words = dict.text.Split(System.Environment.NewLine);

        // Add Bought Spells to "activePowerups," a List Referenced to Spawn Powerups
        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("fireball") > 0)
            activePowerups.Add(powerupPrefabs[0]);
        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("snowball") > 0)
            activePowerups.Add(powerupPrefabs[1]);
        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("rose") > 0)
            activePowerups.Add(powerupPrefabs[2]);
        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("lightning") > 0)
            activePowerups.Add(powerupPrefabs[3]);
        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetSpellLevel("bomb") > 0)
            activePowerups.Add(powerupPrefabs[4]);

        //Display Costumes
        if(PhotonNetwork.CurrentRoom.PlayerCount < 2) {
            playerObj.GetComponent<SpriteRenderer>().sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeArr()[GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeIndex()];
            OnPlayerLeftRoom(null);
        }
        else {
            view.RPC("SendPlayerData", RpcTarget.All, PlayerPrefs.GetString("username"), GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeIndex(), attackLevel + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetDamageLevel(), PhotonNetwork.IsMasterClient);
        }
    }

    [PunRPC]
    private void SendPlayerData(string username, int costumeIndex, float levelNum, bool isMaster) {
        if(isMaster == PhotonNetwork.IsMasterClient) {
            playerObj.GetComponent<SpriteRenderer>().sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeArr()[costumeIndex];
            UpdateCollider(playerObj, playerObj.GetComponent<SpriteRenderer>().sprite);
        }
        else {
            enemyObj.GetComponent<SpriteRenderer>().sprite = GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetCostumeArr()[costumeIndex];
            enemyAttackLevel = levelNum;
            UpdateCollider(enemyObj, enemyObj.GetComponent<SpriteRenderer>().sprite);
            player.SetEnemyName(username);
        }
    }

    void UpdateCollider(GameObject obj, Sprite objSprite) { 
        // update count
        PolygonCollider2D objCol = obj.GetComponent<PolygonCollider2D>();
        objCol.pathCount = objSprite.GetPhysicsShapeCount();
                
        // new paths variable
        List<Vector2> path = new List<Vector2>();

        // loop path count
        for(int i = 0; i < objCol.pathCount; i++) {
            // get shape
            objSprite.GetPhysicsShape(i, path);
            // set path
            objCol.SetPath(i, path.ToArray());
        }
    }

    //  Update is called once per frame
    void Update()
    {   
        // Draw Line Across Selected Letters
        DrawOtherLine();
        if(draw)
            DrawLine();

        // Check if Letters Are Within Range of Each Other
        if(canSelect)
            CheckDis();

        // Check if Left Mouse is Pressed
        if(Input.GetMouseButtonDown(0))
            mouseDown = true;

        if(Input.GetMouseButtonUp(0)) {
            if(canSelect) {
                if(input.Length > 2) { // If Word > 2 Letters
                    if(!used.Contains(input)) { // If Word is Not Used
                        if(System.Array.IndexOf(words, input) != -1) { // If Dictionary Contains Words
                            draw = false; // Stop Drawing Line
                            canSelect = false; // Cancels Selection As Animation Plays
                            line.positionCount = 0; // Resets Line Drawn
                            used.Add(input); // Mark Word as Used
                            
                            // Scoreboard Stats
                            if(input.Length > longestWord.Length)
                                longestWord = input;
                            wordsSpelled++;
                            
                            isDestroyingBalls = true;
                            destroyingBalls = StartCoroutine(DestroyBalls());
                        }
                        else {
                            error.Wrong();
                            audioSource.clip = inputError;
                            audioSource.volume = 0.1f;
                            audioSource.Play();
                            canSelect = false;
                        }
                    }
                    else {
                        error.Used();
                        audioSource.clip = inputError;
                        audioSource.volume = 0.1f;
                        audioSource.Play();
                        canSelect = false;
                    }
                }
                else if(input.Length > 0) { // If Player Attempts to Spell
                    error.Wrong();
                    audioSource.clip = inputError;
                    audioSource.volume = 0.1f;
                    audioSource.Play();
                    canSelect = false;
                }
            }

            mouseDown = false;
        }
    }

    IEnumerator DestroyBalls() {
        combo.AddCombo();

        //Freeze Balls Position
        FreezeBall();

        // Letters Cleared Animation
        for(int i = 0; i < balls.Count; i++) {
            while(frozen)
                yield return null;

            if(balls[i] != null) {
                // Disappear
                view.RPC("Disappear", RpcTarget.All, balls[i].GetComponent<PhotonView>().ViewID, PhotonNetwork.IsMasterClient, false);

                GameObject spellSpawned = Instantiate(spellSpawn, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);
                spellSpawned.GetComponent<AudioSource>().pitch = 1 + (0.25f * i);

                // Reset Combo Timer
                combo.DelayCombo();

                //Spawn Spell
                GameObject spell = Instantiate(balls[i].GetComponent<BallCode>().GetSpell(), new Vector3(-6.78f, -2.58f, -0.1f), new Quaternion(0, 0, 0, 1));

                // If Ball is Not A Bomb, Spawn Spell Normally
                if(balls[i].GetComponent<BallCode>().GetBombAmount() == 0) {
                    spell.GetComponent<SpellCode>().SetDamage(combo.GetCombo() + (attackLevel + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetDamageLevel()));
                }
                else { // If Ball is Bomb Start Return Recursion for Spell Values
                    float[] bombStats = SetBomb(balls[i], balls[i].GetComponent<BallCode>().GetBombAmount());

                    // Spawn Spell and Set Spell Values
                    spell.GetComponent<SpellCode>().SetDamage(combo.GetCombo() + (bombStats[0] + attackLevel)); // [damage, burn, freeze, shock, healing]
                    spell.GetComponent<SpellCode>().SetBurnDuration(bombStats[1]);
                    spell.GetComponent<SpellCode>().SetFreezeDuration(bombStats[2]);
                    spell.GetComponent<SpellCode>().SetShockChance(bombStats[3]);
                    spell.GetComponent<SpellCode>().SetHealingFactor(bombStats[4]);
                }

                //Spawn Spell For Multiplayer
                view.RPC("SpawnSpell", RpcTarget.Others, balls[i].GetComponent<BallCode>().GetSpell().name);
                lettersUsed++;

                //Turn Ball Into Powerup
                if(activePowerups.Count > 0) {
                    if(i > 2) {
                        GameObject[] arr = GameObject.FindGameObjectsWithTag("Ball");
                        GameObject replacement = arr[Random.Range(0, arr.Length)];

                        while(balls.Contains(replacement) || exploded.Contains(replacement) || replacement.GetComponent<BallCode>().IsPowerup() || replacement.GetComponent<BallCode>().IsDestroying())
                            replacement = arr[Random.Range(0, arr.Length)];
                        
                        GameObject replacementPowerup = Instantiate(activePowerups[Random.Range(0, activePowerups.Count)], replacement.transform.position, replacement.transform.rotation, GameObject.Find("BallHolder").transform);
                        replacement.name = replacementPowerup.name;
                        int bombNum = 0;
                        if(replacementPowerup.GetComponent<BallCode>().GetBombAmount() > 0)
                            bombNum = 1;
                        replacement.GetComponent<BallCode>().TurnPowerup(replacementPowerup.GetComponent<BallCode>().GetSpell(), bombNum);
                        replacement.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = replacementPowerup.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite;
                        replacement.transform.GetChild(2).GetComponent<SpriteRenderer>().color = replacementPowerup.transform.GetChild(2).GetComponent<SpriteRenderer>().color;
                        replacementPowerup.transform.GetChild(3).SetParent(replacement.transform);

                        Destroy(replacementPowerup);

                        ParticleSystem ps = Instantiate(particle, replacement.transform.position, new Quaternion(0, 0, 0, 1)).GetComponent<ParticleSystem>();
                        ps.startColor = replacement.transform.GetChild(3).GetComponent<ParticleSystem>().startColor;
                        var col = ps.colorOverLifetime;
                        col.color = replacement.transform.GetChild(3).GetComponent<ParticleSystem>().colorOverLifetime.color;
                    }
                }

                // Wait
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Spawn New Letters and Destory Old Ones
        while(balls.Count > 0) {
            if(balls[0] != null) {
                if(PhotonNetwork.IsMasterClient) {
                    PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count + balls.Count, 0), new Quaternion(0, 0, 0, 1));
                    PhotonNetwork.Destroy(balls[0]);
                }
                else {
                    view.RPC("InstantiateRPC", RpcTarget.Others, exploded.Count, balls.Count);
                    view.RPC("DestroyRPC", RpcTarget.Others, balls[0].GetComponent<PhotonView>().ViewID);
                }
                balls.RemoveAt(0);
            }
        }

        // Remove Letters Destroyed by the Bomb (to avoid spawning extra powerups)
        while(exploded.Count > 0) {
            if(exploded[0] != null) {
                if(PhotonNetwork.IsMasterClient) {
                    PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count, 0), new Quaternion(0, 0, 0, 1));
                    PhotonNetwork.Destroy(exploded[0]);
                }
                else {
                    view.RPC("InstantiateRPC", RpcTarget.Others, exploded.Count, balls.Count);
                    view.RPC("DestroyRPC", RpcTarget.Others, exploded[0].GetComponent<PhotonView>().ViewID);
                }
                exploded.RemoveAt(0);
            }
        }

        ChangeOtherBall();

        // Reset and Enable Input
        input = "";
        text.text = input;
        draw = true;
        canSelect = true;
        isDestroyingBalls = false;
    }

    [PunRPC]
    private void SpawnSpell(string spellName) {
        Instantiate(Resources.Load("Spells/" + spellName) as GameObject, new Vector3(-2.74f, -2.58f, -0.1f), new Quaternion(0, 0, 0, 1)).GetComponent<SpellCode>().SetMultiplayerSpell();
    }

    public void SpellHit(float damage, float burnTime, float freezeTime, bool shocked, float healingFactor) {
        view.RPC("SpellHitRPC", RpcTarget.Others, damage, burnTime, freezeTime, shocked, healingFactor);
    }

    [PunRPC]
    private void SpellHitRPC(float damage, float burnTime, float freezeTime, bool shocked, float healingFactor) {
        player.Damage(damage, burnTime, freezeTime, shocked, healingFactor);
    }

    public void StopBurning() {
        view.RPC("StopBurningRPC", RpcTarget.Others);
    }

    [PunRPC]
    private void StopBurningRPC() {
        enemyMultiplayerCode.StopBurning();
    }

    public void HealEnemy(float num) {
        view.RPC("HealEnemyRPC", RpcTarget.Others, num);
    }

    [PunRPC]
    private void HealEnemyRPC(float num) {
        player.Heal(num);
    }

    public void Freeze() {
        frozen = true;
        if(!isDestroyingBalls) {
            if(balls.Count > 0)
                if(balls[0] != null)
                    ClearStarting(balls[0]);
            ChangeOtherBall();

            draw = false;
            line.positionCount = 0;
            canSelect = false;
        }
    }

    public void Unfreeze() {
        if(frozen) {
            frozen = false;
            if(!isDestroyingBalls) {
                draw = true;
                canSelect = true;
            }
            view.RPC("UnfreezeEnemy", RpcTarget.Others);
        }
    }

    [PunRPC]
    private void UnfreezeEnemy() {
        enemyMultiplayerCode.Unfreeze();
    }

    public void Shock() {
        if(isDestroyingBalls) {
            StopCoroutine(destroyingBalls);

            string ballsStr = "";
            foreach(GameObject ballObj in balls)
                if(ballObj != null)
                    ballsStr += ballObj.GetComponent<PhotonView>().ViewID + " ";
        
            view.RPC("UnfreezeBallRPC", RpcTarget.All, ballsStr, PhotonNetwork.IsMasterClient);

            draw = true;
            canSelect = true;
        }

        if(balls.Count > 0)
            if(balls[0] != null)
                ClearStarting(balls[0]);
    }

    [PunRPC]
    private void UnfreezeBallRPC(string param, bool isMaster) {
        string[] arr = param.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

        foreach(string viewStr in arr) {
            PhotonView frozenBallView = PhotonView.Find(int.Parse(viewStr));

            if(frozenBallView != null) {
                GameObject frozenBall = frozenBallView.gameObject;

                if(frozenBall.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite == null) {
                    if(isMaster == PhotonNetwork.IsMasterClient) {
                        balls.Remove(frozenBall);

                        // Spawn New Letters and Destory Old Ones
                        if(PhotonNetwork.IsMasterClient) {
                            PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count + balls.Count, 0), new Quaternion(0, 0, 0, 1));
                            PhotonNetwork.Destroy(frozenBall);
                        }
                        else {
                            view.RPC("InstantiateRPC", RpcTarget.Others, exploded.Count, balls.Count);
                            view.RPC("DestroyRPC", RpcTarget.Others, frozenBall.GetComponent<PhotonView>().ViewID);
                        }
                    }
                }
                else {
                    frozenBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(frozenBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.r, frozenBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.g, frozenBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.b, 1f);
                    frozenBall.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                    frozenBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
                    frozenBall.GetComponent<BallCode>().BallDestroying(false);
                }
            }
        }

        // Remove Letters Destroyed by the Bomb (to avoid spawning extra powerups)
        if(isMaster == PhotonNetwork.IsMasterClient) {
            while(exploded.Count > 0) {
                if(exploded[0] != null) {
                    if(PhotonNetwork.IsMasterClient) {
                        PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count, 0), new Quaternion(0, 0, 0, 1));
                        PhotonNetwork.Destroy(exploded[0]);
                    }
                    else {
                        view.RPC("InstantiateRPC", RpcTarget.Others, exploded.Count, balls.Count);
                        view.RPC("DestroyRPC", RpcTarget.Others, exploded[0].GetComponent<PhotonView>().ViewID);
                    }
                    exploded.RemoveAt(0);
                }
            }
        }
    }

    public void UpdateHealthBar(float num) {
        view.RPC("UpdateHealthBarRPC", RpcTarget.Others, num);
    }

    [PunRPC]
    private void UpdateHealthBarRPC(float num) {
        enemyMultiplayerCode.UpdateHealthBar(num);
    }

    private void FreezeBall() {
        string ballsStr = "";
        foreach(GameObject ballObj in balls)
            ballsStr += ballObj.GetComponent<PhotonView>().ViewID + " ";

        view.RPC("FreezeBallRPC", RpcTarget.All, ballsStr, PhotonNetwork.IsMasterClient);
    }

    [PunRPC]
    private void FreezeBallRPC(string param, bool isMaster) {
        if(isMaster != PhotonNetwork.IsMasterClient)
            otherBalls.Clear();

        string[] arr = param.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

        foreach(string viewStr in arr) {
            PhotonView freezingBallView = PhotonView.Find(int.Parse(viewStr));

            if(freezingBallView != null) {
                GameObject freezingBall = freezingBallView.gameObject;

                freezingBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(freezingBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.r, freezingBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.g, freezingBall.transform.GetChild(2).GetComponent<SpriteRenderer>().color.b, 0.5f);
                freezingBall.transform.GetChild(1).gameObject.SetActive(false);
                freezingBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
                freezingBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
                freezingBall.GetComponent<BallCode>().BallDestroying(true);

                if(isMaster != PhotonNetwork.IsMasterClient)
                    if(balls.Contains(freezingBall))
                        ClearStarting(freezingBall);
            }
        }
    }

    [PunRPC]
    private void Disappear(int viewId, bool isMaster, bool isExploding) {
        PhotonView rpcBallView = PhotonView.Find(viewId);

        if(rpcBallView != null) {
            GameObject rpcBall = rpcBallView.gameObject;

            rpcBall.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;
            if(rpcBall.GetComponent<BallCode>().IsPowerup()) // If Powerup Remove Particle Effect
                rpcBall.transform.GetChild(3).gameObject.SetActive(false);
            rpcBall.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);

            if(isExploding) {
                if(isMaster != PhotonNetwork.IsMasterClient) {
                    if(balls.Contains(rpcBall))
                        ClearStarting(rpcBall);
                }
            }

            //Pop Animation
            if(isMaster == PhotonNetwork.IsMasterClient) {
                ParticleSystem ps = Instantiate(particle, rpcBall.transform.position, new Quaternion(0, 0, 0, 1)).GetComponent<ParticleSystem>();

                if(!rpcBall.GetComponent<BallCode>().GetSpell().tag.Equals("Normal")) {
                    ps.startColor = rpcBall.transform.GetChild(3).GetComponent<ParticleSystem>().startColor;
                    var col = ps.colorOverLifetime;
                    col.color = rpcBall.transform.GetChild(3).GetComponent<ParticleSystem>().colorOverLifetime.color;
                }
            }
            else {
                Instantiate(multiplayerParticle, rpcBall.transform.position, new Quaternion(0, 0, 0, 1));
            }
        }
    }

    [PunRPC]
    private void InstantiateRPC(int explodedNum, int ballsNum) {
        PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + explodedNum + ballsNum, 0), new Quaternion(0, 0, 0, 1));
    }

    [PunRPC]
    private void DestroyRPC(int viewId) {
        PhotonView viewIdBall = PhotonView.Find(viewId);

        if(viewIdBall != null)
            PhotonNetwork.Destroy(viewIdBall.gameObject);
    }

    float[] SetBomb(GameObject bomb, int bombAmount) {
        // Prevent Infinite Loop By Marking Bomb as Set
        bomb.GetComponent<BallCode>().SetBombBall();

        // Sort All Letters by Distance
        GameObject[] arr = QuickSort(bomb, GameObject.FindGameObjectsWithTag("Ball"), 0, GameObject.FindGameObjectsWithTag("Ball").Length - 1);
        float[] stats = {0, 0, 0, 0, 0}; // [damage, burn, freeze, shock, healing]
        
        // Counter to Track Amount of Letters Allowed to Explode
        int j = 0;

        for(int i = 1; i < arr.Length; i++) { // Run Through Array of Sorted Letters
            if(arr[i] != null) {
                if(j < bombAmount) {
                    if(!exploded.Contains(arr[i]) && !balls.Contains(arr[i]) && !arr[i].GetComponent<BallCode>().IsSet()) { // Check if Not Exploded && if Not A Selected Letter && If it is a Unset Bomb
                        // Make Letter Disappear
                        view.RPC("Disappear", RpcTarget.All, arr[i].GetComponent<PhotonView>().ViewID, PhotonNetwork.IsMasterClient, true);
                
                        // Increase Damage for Every Letter
                        stats[0] += attackLevel;

                        // If It is a Powerup Add its Properties to the Bomb (using the "stats" array)
                        if(arr[i].GetComponent<BallCode>().IsPowerup()) {
                            // If Statements Check if Property is Not Set, then Sets Each Property to the Appropiate "stats" index
                            if(stats[1] == 0)
                                stats[1] = arr[i].GetComponent<BallCode>().GetSpell().GetComponent<SpellCode>().GetBurnDuration();
                            if(stats[2] == 0)
                                stats[2] = arr[i].GetComponent<BallCode>().GetSpell().GetComponent<SpellCode>().GetFreezeDuration();
                            if(stats[3] == 0)
                                stats[3] = arr[i].GetComponent<BallCode>().GetSpell().GetComponent<SpellCode>().GetShockChance();
                            if(stats[4] == 0)
                                stats[4] = arr[i].GetComponent<BallCode>().GetSpell().GetComponent<SpellCode>().GetHealingFactor();

                            // If Exploded Ball is Another Bomb
                            if(arr[i].GetComponent<BallCode>().GetBombAmount() > 0) {
                                // Recall Set Bomb Method for Another "stats" Array
                                float[] bomb2 = SetBomb(arr[i], arr[i].GetComponent<BallCode>().GetBombAmount());

                                // Combine Both Values
                                stats[0] += bomb2[0];
                                if(bomb2[1] > 0)
                                    stats[1] = bomb2[1];
                                if(bomb2[2] > 0)
                                    stats[2] = bomb2[2];
                                if(bomb2[3] > 0)
                                    stats[3] = bomb2[3];
                                if(bomb2[4] > 0)
                                    stats[4] = bomb2[4];
                            }
                        }

                        // Mark As Exploded
                        exploded.Add(arr[i]);
                        j++;
                    }
                }
            }
        }

        return stats;
    }

    int Partition(GameObject bomb, GameObject[] arr, int low, int high)
    {
        if(arr[high] != null) {
            float pivot = Vector3.Distance(bomb.transform.position, arr[high].transform.position);

            int i = (low - 1); // Index of Smaller Element
            for (int j = low; j < high; j++)
            {
                // If Current Element is Smaller Than or Equal to Pivot
                if(arr[j] != null) {
                    if (Vector3.Distance(bomb.transform.position, arr[j].transform.position) <= pivot)
                    {
                        i++;

                        // Swap arr[i] and arr[j]
                        GameObject temp = arr[i];

                        if(arr[i] != null) {
                            arr[i] = arr[j];
                            arr[j] = temp;
                        }
                    }
                }
            }
    
            // Swap arr[i+1] and arr[high] (or Pivot)
            GameObject temp2 = arr[i + 1];
            if(temp2 != null) {
                arr[i + 1] = arr[high];
                arr[high] = temp2;
            }
    
            return i + 1;
        }
        return low - 1;
    }

    GameObject[] QuickSort(GameObject bomb, GameObject[] arr, int low, int high)
    {
        if(bomb != null) {
            if (low < high)
            {
                // Pi is Partitioning Index, arr[p] is Now at Right Place
                int pi = Partition(bomb, arr, low, high);

                QuickSort(bomb, arr, low, pi - 1); // Before Pi
                QuickSort(bomb, arr, pi + 1, high); //After Pi
            }
        }

        return arr;
    }

    public bool AddLetter(GameObject ball) { // Returns True if Selected Letter Can Be Added to Input
        if(ball != null) {
            if(mouseDown) {
                if(canSelect) {
                    if(balls.Count - 1 >= 0) {
                        if(Vector3.Distance(ball.transform.position, balls[balls.Count - 1].transform.position) < 1.5f) {
                            if(balls.Count < 15) {
                                balls.Add(ball);
                                SetInput();
                                ChangeOtherBall();
                                return true;
                            }
                        }
                    }
                    if(balls.Count == 0) {
                        if(balls.Count < 15) {
                            balls.Add(ball);
                            SetInput();
                            ChangeOtherBall();
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }

    void SetInput() { // Scans Selected Letter List and Combines Them Into a String to be Displayed
        input = "";
        for(int i = 0; i < balls.Count; i++)
            if(balls[i] != null)
                input += balls[i].transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text;

        text.text = input;
    }

    public void ClearInput() { // Only Removes Letters From Selected Letter List, Doesn't Destroy
        while(balls.Count > 0) {
            if(balls[0] != null) {
                balls[0].transform.GetChild(1).gameObject.SetActive(false);
                balls.RemoveAt(0);
            }
        }
        ChangeOtherBall();
            
        input = "";
        text.text = input;

        canSelect = true;
    }

    void DrawLine() { // Draw Line Across Selected Letter List
        line.positionCount = balls.Count;

        for(int i = 0; i < balls.Count; i++) {
            if(balls[i] != null) {
                line.SetPosition(i, new Vector3(balls[i].transform.position.x, balls[i].transform.position.y, -0.2f));
                balls[i].transform.GetChild(1).gameObject.SetActive(true);
                balls[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0, 1, 1, 1);
            }
        }
    }

    private void ChangeOtherBall() {
        string otherBallsStr = "";
        foreach(GameObject otherBall in balls)
            if(otherBall != null)
                otherBallsStr += otherBall.GetComponent<PhotonView>().ViewID + " ";

        view.RPC("ChangeOtherBallRPC", RpcTarget.Others, otherBallsStr);
    }

    [PunRPC]
    private void ChangeOtherBallRPC(string otherBallsStr) {
        string[] arr = otherBallsStr.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

        foreach(GameObject otherBall in otherBalls)
            if(otherBall != null)
                otherBall.transform.GetChild(1).gameObject.SetActive(false);
        otherBalls.Clear();

        foreach(string viewStr in arr) {
            PhotonView viewIdBall = PhotonView.Find(int.Parse(viewStr));

            if(viewIdBall != null)
                otherBalls.Add(viewIdBall.gameObject);
        }
    }

    void DrawOtherLine() {
        otherLine.positionCount = otherBalls.Count;

        for(int i = 0; i < otherBalls.Count; i++) {
            if(otherBalls[i] != null) {
                otherLine.SetPosition(i, new Vector3(otherBalls[i].transform.position.x, otherBalls[i].transform.position.y, -0.2f));
                otherBalls[i].transform.GetChild(1).gameObject.SetActive(true);
                otherBalls[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1, 0, 1, 1);
            }
        }
    }

    void CheckDis() { // Checks if Letters are Close Enough to be Selected
        if(balls.Count > 1) {
            for(int i = 0; i < balls.Count - 1; i++) {
                if(balls[i] != null && balls [i + 1] != null) {
                    if(Vector3.Distance(balls[i].transform.position, balls[i + 1].transform.position) >= 1.5f) { // Checks Distance
                        // Runs Through Selected Letter List and Removes Each Ball After
                        for(int j = i + 1; j < balls.Count; j++)
                            if(balls[j] != null)
                                balls[j].GetComponent<BallCode>().Deselect();
                        balls.RemoveRange(i + 1, balls.Count - i - 1);
                        ChangeOtherBall();

                        // Resets Input Display
                        SetInput();
                        return;
                    }
                }
            }
        }
    }

    public void ClearBall(GameObject ballObj) { // Clear Letters Past Selected Ball
        int ballIndex = balls.IndexOf(ballObj);
        for(int i = ballIndex + 1; i < balls.Count; i++)
            if(balls[i] != null)
                balls[i].GetComponent<BallCode>().Deselect();
        balls.RemoveRange(ballIndex + 1, balls.Count - ballIndex - 1);
        ChangeOtherBall();
        SetInput();
    }

    void ClearStarting(GameObject ballObj) {
        int ballIndex = balls.IndexOf(ballObj);
        for(int i = ballIndex; i < balls.Count; i++)
            if(balls[i] != null)
                balls[i].GetComponent<BallCode>().Deselect();
        balls.RemoveRange(ballIndex, balls.Count - ballIndex);
        ChangeOtherBall();
        SetInput();
    }

    public float GetAttackLevel() { // Returns Player Attack
        return attackLevel + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetDamageLevel();
    }

    public float GetEnemyAttackLevel() { // Returns Enemy Attack
        return enemyAttackLevel;
    }

    public void MultiplayerEndGame() {
        view.RPC("EndGameRPC", RpcTarget.All);
    }

    [PunRPC]
    private void EndGameRPC() {
        EndGame();
    }

    public void EndGame() {
        if(!gameEnd) {
            gameEnd = true;

            // Deactivate UI Elements
            GameObject.Find("Error").SetActive(false);
            GameObject.Find("Combo").SetActive(false);
            GameObject.Find("PlayerHealthBar").SetActive(false);

            //Destroy All Balls
            GameObject[] endBalls = GameObject.FindGameObjectsWithTag("Ball");
            for(int i = 0; i < endBalls.Length; i++) {
                if(endBalls[i] != null) {
                    ParticleSystem ps = Instantiate(particle, endBalls[i].transform.position, new Quaternion(0, 0, 0, 1)).GetComponent<ParticleSystem>();
                    if(!endBalls[i].GetComponent<BallCode>().GetSpell().tag.Equals("Normal")) {
                        ps.startColor = endBalls[i].transform.GetChild(3).GetComponent<ParticleSystem>().startColor;
                        var col = ps.colorOverLifetime;
                        col.color = endBalls[i].transform.GetChild(3).GetComponent<ParticleSystem>().colorOverLifetime.color;
                    }
                }
                if(PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(endBalls[i]);
            }
            otherBalls.Clear();
            otherLine.positionCount = 0;

            //Set Scoreboard Values
            player.SetLettersUsed(lettersUsed);
            player.SetWordsSpelled(wordsSpelled);
            player.SetLongestWord(longestWord);
            player.EndGame();
            gameObject.SetActive(false);
        }
    }

    public void EnableGame() {
        view.RPC("EnableGameRPC", RpcTarget.All);
    }

    [PunRPC]
    private void EnableGameRPC() {
        // Spawn Letters
        if(PhotonNetwork.IsMasterClient) {
            for(int i = 0; i < 60; i++) {
                PhotonNetwork.Instantiate("Multiplayer/Balls/" + ballPrefabs[Random.Range(0, 5)].name, new Vector3(Random.Range(0.69f, 7.61f), 4.5f + i, 0), Quaternion.identity);
            }
        }
        
        canSelect = true;
        draw = true;
    }

    public override void OnPlayerLeftRoom(Player other) {
        GameObject.Find("Ready").SetActive(false);
        player.HasLeft();
        EndGame();
    }
}