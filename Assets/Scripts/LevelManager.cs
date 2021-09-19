using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance = null;
    public static LevelManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<LevelManager>();
            }
            return instance;
        }
    }

    [SerializeField] private int maxLives = 3;
    [SerializeField] private int totalEnemy = 15;

    [SerializeField] private GameObject panel;
    [SerializeField] private Text statusInfo;
    [SerializeField] private Text livesInfo;
    [SerializeField] private Text totalEnemyInfo;

    [SerializeField] private Transform towerUI_Parent;
    [SerializeField] private GameObject towerUI_Prefab;
    [SerializeField] private Tower[] towerPrefabs;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform[] enemyPaths;
    [SerializeField] private float spawnDelay = 5f;

    private List<Tower> spawnedTowers = new List<Tower>();
    private List<Enemy> spawnedEnemies = new List<Enemy>();
    private List<Bullet> spawnedBullets = new List<Bullet>();

    private int currentLives;
    private int enemyCounter;
    private float runningSpawnDelay;
    public bool IsOver { get; private set; }

    private void Start()
    {
        InstantiateAllTowerUI();
        SetCurrentLives(maxLives);
        SetTotalEnemy(totalEnemy);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(IsOver)
        {
            return;
        }
        runningSpawnDelay -= Time.unscaledDeltaTime;
        if(runningSpawnDelay <= 0)
        {
            SpawnEnemy();
            runningSpawnDelay = spawnDelay;
        }
        foreach(Tower tower in spawnedTowers)
        {
            tower.CheckNearestEnemy(spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }
        foreach(Enemy enemy in spawnedEnemies)
        {
            if(!enemy.gameObject.activeSelf)
            {
                continue;
            }
            if(Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                if(enemy.CurrentPathIndex < enemyPaths.Length)
                {
                    enemy.SetTargetPosition(enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    enemy.gameObject.SetActive(false);
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }
    }

    private void InstantiateAllTowerUI()
    {
        foreach(Tower tower in towerPrefabs)
        {
            GameObject newTowerUI_Obj = Instantiate(towerUI_Prefab.gameObject, towerUI_Parent);
            TowerUI newTowerUI = newTowerUI_Obj.GetComponent<TowerUI>();
            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    public void RegisterSpawnedTower(Tower tower)
    {
        spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        SetTotalEnemy(--enemyCounter);
        if(enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
            if(isAllEnemyDestroyed)
            {
                SetGameOver(true);
            }
            return;
        }
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();
        GameObject newEnemyObj = spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        if(newEnemyObj == null)
        {
            newEnemyObj = Instantiate(enemyPrefabs[randomIndex].gameObject);
        }
        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
        if(!spawnedEnemies.Contains(newEnemy))
        {
            spawnedEnemies.Add(newEnemy);
        }
        newEnemy.transform.position = enemyPaths[0].position;
        newEnemy.SetTargetPosition(enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }

    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject newBulletObj = spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;
        if(newBulletObj == null)
        {
            newBulletObj = Instantiate(prefab.gameObject);
        }
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if(!spawnedBullets.Contains(newBullet))
        {
            spawnedBullets.Add(newBullet);
        }
        return newBullet;
    }

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach(Enemy enemy in spawnedEnemies)
        {
            if(enemy.gameObject.activeSelf)
            {
                if(Vector2.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        SetCurrentLives(currentLives - value);
        if(currentLives <= 0)
        {
            SetGameOver(false);
        }
    }

    public void SetCurrentLives(int currentLives)
    {
        currentLives = Mathf.Max(currentLives, 0);
        livesInfo.text = $"Lives: {currentLives}";
    }

    public void SetTotalEnemy(int totalEnemy)
    {
        enemyCounter = totalEnemy;
        totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(enemyCounter, 0)}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;

        statusInfo.text = isWin ? "You Win!" : "You Lose!";
        panel.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < enemyPaths.Length -1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(enemyPaths[i].position, enemyPaths[i + 1].position);
        }
    }
}
