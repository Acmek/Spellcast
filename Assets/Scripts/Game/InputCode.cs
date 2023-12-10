using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System.IO;

public class InputCode : MonoBehaviour
{
    [Header("Enemy List")]
    [SerializeField] private string[] enemies;
    [SerializeField] private GameObject[] enemyList;

    [Header("Balls")]
    [SerializeField] private GameObject[] ballPrefabs;
    [SerializeField] private GameObject[] powerupPrefabs;
    [SerializeField] private List<GameObject> activePowerups;

    [Header("Effects")]
    [SerializeField] private GameObject particle;
    [SerializeField] private ErrorCode error;
    [SerializeField] private ComboCode combo;

    [Header("Player")]
    [SerializeField] private PlayerCode player;
    [SerializeField] private float attackLevel;
    [SerializeField] private TextAsset dict;

    [Header("Background")]
    [SerializeField] private Sprite middleLevel;
    [SerializeField] private Sprite topLevel;
    [SerializeField] private SpriteRenderer[] backgrounds;

    [Header("Sound")]
    [SerializeField] private AudioClip inputError;
    [SerializeField] private GameObject spellSpawn;

    private TMP_Text text;
    private LineRenderer line;
    private string input = "";

    private string[] words;
    private List<string> used;

    private bool mouseDown;

    private List<GameObject> balls;

    private bool draw;
    private bool canSelect;

    private int enemy;

    private List<GameObject> exploded;

    private string longestWord = "";
    private int wordsSpelled;

    private bool gameEnd;

    private AudioSource audioSource;

    void Start() {
        text = GetComponent<TMP_Text>();
        line = GetComponent<LineRenderer>();
        used = new List<string>();
        balls = new List<GameObject>();
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

        if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() >= 6 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() <= 10)
            foreach(SpriteRenderer background in backgrounds)
                background.sprite = middleLevel;
        else if(GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() >= 11 && GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() <= 15)
            foreach(SpriteRenderer background in backgrounds)
                background.sprite = topLevel;

        draw = true;
        canSelect = true;

        // Spawn Letters
        for(int i = 0; i < 60; i++)
            Instantiate(ballPrefabs[Random.Range(0, 5)], new Vector3(Random.Range(0.69f, 7.61f), 4.5f + i, 0), Quaternion.identity, GameObject.Find("BallHolder").transform);

        // Spawn First Enemy
        Instantiate(enemyList[int.Parse(enemies[GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() - 1].Split(" ")[enemy])], new Vector3(4.2f, -3.37f, 0), new Quaternion(0, 0, 0, 1));
    }

    //  Update is called once per frame
    void Update()
    {   
        // Draw Line Across Selected Letters
        if(draw) {
            DrawLine();
        }
        // Check if Letters Are Within Range of Each Other
        if(canSelect) {
            CheckDis();
        }

        // Check if Left Mouse is Pressed
        if(Input.GetMouseButtonDown(0)) {
            mouseDown = true;
        }

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
                            
                            StartCoroutine(DestroyBalls());
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

                    // Tells Enemy if Player Attacks Early to Reset Combo
                    GameObject enemySpawned = GameObject.FindWithTag("Enemy");
                    if(enemySpawned != null)
                        enemySpawned.transform.parent.gameObject.GetComponent<EnemyCode>().PlayerAttacked();
                }
                else if(input.Length > 0) { // If Player Attempts to Spell
                    error.Wrong();
                    audioSource.clip = inputError;
                    audioSource.volume = 0.1f;
                    audioSource.Play();
                    canSelect = false;

                    // Tells Enemy if Player Attacks Early to Reset Combo
                    GameObject enemySpawned = GameObject.FindWithTag("Enemy");
                    if(enemySpawned != null)
                        enemySpawned.transform.parent.gameObject.GetComponent<EnemyCode>().PlayerAttacked();
                }
            }

            mouseDown = false;
        }
    }

    IEnumerator DestroyBalls() {
        combo.AddCombo();

        // Turn Letters Transparent, Remove Dot Highlight, Freeze Position
        for(int i = 0; i < balls.Count; i++) {
            balls[i].transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(balls[i].transform.GetChild(2).GetComponent<SpriteRenderer>().color.r, balls[i].transform.GetChild(2).GetComponent<SpriteRenderer>().color.g, balls[i].transform.GetChild(2).GetComponent<SpriteRenderer>().color.b, 0.5f);
            balls[i].transform.GetChild(1).gameObject.SetActive(false);
            balls[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
            balls[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Letters Cleared Animation
        for(int i = 0; i < balls.Count; i++) {
            // Disappear
            balls[i].transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;
            if(balls[i].GetComponent<BallCode>().IsPowerup()) // If Powerup Remove Particle Effect
                balls[i].transform.GetChild(3).gameObject.SetActive(false);
            balls[i].transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = "";

            GameObject spellSpawned = Instantiate(spellSpawn, Vector3.zero, Quaternion.identity, GameObject.Find("SoundHolder").transform);
            spellSpawned.GetComponent<AudioSource>().pitch = 1 + (0.25f * i);
        
            // Pop Animation
            ParticleSystem psPop = Instantiate(particle, balls[i].transform.position, new Quaternion(0, 0, 0, 1)).GetComponent<ParticleSystem>();
            if(!balls[i].GetComponent<BallCode>().GetSpell().tag.Equals("Normal")) {
                psPop.startColor = balls[i].transform.GetChild(3).GetComponent<ParticleSystem>().startColor;
                var colPop = psPop.colorOverLifetime;
                colPop.color = balls[i].transform.GetChild(3).GetComponent<ParticleSystem>().colorOverLifetime.color;
            }

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

            //Turn Ball Into Powerup
            if(activePowerups.Count > 0) {
                if(i > 2) {
                    GameObject[] arr = GameObject.FindGameObjectsWithTag("Ball");
                    GameObject replacement = arr[Random.Range(0, arr.Length)];

                    while(balls.Contains(replacement) || exploded.Contains(replacement) || replacement.GetComponent<BallCode>().IsPowerup())
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

        // Spawn New Letters and Destory Old Ones
        while(balls.Count > 0) {
            Instantiate(ballPrefabs[Random.Range(0, 5)], new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count + balls.Count, 0), new Quaternion(0, 0, 0, 1), GameObject.Find("BallHolder").transform);
            Destroy(balls[0]);
            balls.RemoveAt(0);
        }

        // Remove Letters Destroyed by the Bomb (to avoid spawning extra powerups)
        while(exploded.Count > 0) {
            Instantiate(ballPrefabs[Random.Range(0, 5)], new Vector3(Random.Range(0.69f, 7.61f), 4.5f + exploded.Count, 0), new Quaternion(0, 0, 0, 1), GameObject.Find("BallHolder").transform);
            Destroy(exploded[0]);
            exploded.RemoveAt(0);
        }

        // Reset and Enable Input
        input = "";
        text.text = input;
        draw = true;
        canSelect = true;
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
            if(j < bombAmount) {
                if(!exploded.Contains(arr[i]) && !balls.Contains(arr[i]) && !arr[i].GetComponent<BallCode>().IsSet()) { // Check if Not Exploded && if Not A Selected Letter && If it is a Unset Bomb
                    // Make Letter Disappear
                    arr[i].transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;
                    if(arr[i].GetComponent<BallCode>().IsPowerup())
                        arr[i].transform.GetChild(3).gameObject.SetActive(false);
                    arr[i].transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = "";
                    
                    //Pop Animation
                    Instantiate(particle, arr[i].transform.position, new Quaternion(0, 0, 0, 1));
            
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

        return stats;
    }

    int Partition(GameObject bomb, GameObject[] arr, int low, int high)
    {
        float pivot = Vector3.Distance(bomb.transform.position, arr[high].transform.position);

        int i = (low - 1); // Index of Smaller Element
        for (int j = low; j < high; j++)
        {
            // If Current Element is Smaller Than or Equal to Pivot
            if (Vector3.Distance(bomb.transform.position, arr[j].transform.position) <= pivot)
            {
                i++;

                // Swap arr[i] and arr[j]
                GameObject temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
 
        // Swap arr[i+1] and arr[high] (or Pivot)
        GameObject temp2 = arr[i + 1];
        arr[i + 1] = arr[high];
        arr[high] = temp2;
 
        return i + 1;
    }

    GameObject[] QuickSort(GameObject bomb, GameObject[] arr, int low, int high)
    {
        if (low < high)
        {
            // Pi is Partitioning Index, arr[p] is Now at Right Place
            int pi = Partition(bomb, arr, low, high);

            QuickSort(bomb, arr, low, pi - 1); // Before Pi
            QuickSort(bomb, arr, pi + 1, high); //After Pi
        }

        return arr;
    }

    public bool AddLetter(GameObject ball) { // Returns True if Selected Letter Can Be Added to Input
        if(mouseDown) {
            if(canSelect) {
                if(balls.Count == 0 || Vector3.Distance(ball.transform.position, balls[balls.Count - 1].transform.position) < 1.5f) {
                    if(balls.Count < 15) {
                        balls.Add(ball);
                        SetInput();
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    void SetInput() { // Scans Selected Letter List and Combines Them Into a String to be Displayed
        input = "";
        for(int i = 0; i < balls.Count; i++)
            input += balls[i].transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text;

        text.text = input;
    }

    public void ClearInput() { // Only Removes Letters From Selected Letter List, Doesn't Destroy
        while(balls.Count > 0) {
            balls[0].transform.GetChild(1).gameObject.SetActive(false);
            balls.RemoveAt(0);
        }
            
        input = "";
        text.text = input;

        canSelect = true;
    }

    void DrawLine() { // Draw Line Across Selected Letter List
        line.positionCount = balls.Count;

        for(int i = 0; i < balls.Count; i++) {
            line.SetPosition(i, new Vector3(balls[i].transform.position.x, balls[i].transform.position.y, -0.2f));
        }
    }

    void CheckDis() { // Checks if Letters are Close Enough to be Selected
        if(balls.Count > 1) {
            for(int i = 0; i < balls.Count - 1; i++) {
                if(Vector3.Distance(balls[i].transform.position, balls[i + 1].transform.position) >= 1.5f) { // Checks Distance
                    // Runs Through Selected Letter List and Removes Each Ball After
                    for(int j = i + 1; j < balls.Count; j++)
                        balls[j].GetComponent<BallCode>().Deselect();
                    balls.RemoveRange(i + 1, balls.Count - i - 1);

                    // Resets Input Display
                    SetInput();
                    return;
                }
            }
        }
    }

    public void ClearBall(GameObject ballObj) { // Clear Letters Past Selected Ball
        int ballIndex = balls.IndexOf(ballObj);
        for(int i = ballIndex + 1; i < balls.Count; i++)
            balls[i].GetComponent<BallCode>().Deselect();
        balls.RemoveRange(ballIndex + 1, balls.Count - ballIndex - 1);
        SetInput();
    }

    public float GetAttackLevel() { // Returns Player Attack
        return attackLevel + GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetDamageLevel();
    }

    public void NextEnemy() { // Spawns Next Enemy
        if(enemy + 1 < enemies[GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() - 1].Split(" ").Length) {
            enemy++;
            Instantiate(enemyList[int.Parse(enemies[GameObject.Find("PlayerSaves").GetComponent<PlayerSave>().GetLastLevel() - 1].Split(" ")[enemy])], new Vector3(4.2f, -3.37f, 0), new Quaternion(0, 0, 0, 1));
        }
        else { //Ends Game if no Levels Left
            EndGame();
        }
    }

    public void EndGame() {
        if(!gameEnd) {
            gameEnd = true;

            // Deactivate UI Elements
            GameObject.Find("Timer").SetActive(false);
            GameObject.Find("Error").SetActive(false);
            GameObject.Find("Combo").SetActive(false);
            GameObject.Find("PauseButton").GetComponent<PauseButtonCode>().EndPause();
            GameObject.Find("PlayerHealthBar").SetActive(false);

            //Destroy All Balls
            GameObject[] endBalls = GameObject.FindGameObjectsWithTag("Ball");
            for(int i = 0; i < endBalls.Length; i++) {
                Instantiate(particle, endBalls[i].transform.position, new Quaternion(0, 0, 0, 1));
                Destroy(endBalls[i]);
            }

            //Set Scoreboard Values
            player.SetWordsSpelled(wordsSpelled);
            player.SetLongestWord(longestWord);
            player.EndGame();
            gameObject.SetActive(false);
        }
    }

    public void Paused() {
        canSelect = false;
    }

    public void Unpaused() {
        canSelect = true;
    }
}
