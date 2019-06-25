using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformGroupType
{
    Grass,
    Winter
}
public class PlatformSpawner : MonoBehaviour
{
    public Vector3 startSpawnPos;
    public int milestoneCount = 10;
    public float fallTime;
    public float minFallTime;
    public float multiple;
    private ManagerVars vars;
    // number of platforms (5)
    private int spawnPlatformCount;
    //platform left or right
    private bool isLeftSpawn = false;
    // location of generating new platform
    private Vector3 platformSpawnPosition;
    private Sprite selectPlatformSprite;
    // platform type: winter or grass
    private PlatformGroupType groupType;
    // for generate more platforms in spike direction
    private bool spikeSpawnLeft = false;
    private Vector3 spikeDirPlatformPos;
    private int afterSpawnSpikeSpawnCount;
    private bool isSpawnSpike;
    private void Awake()
    {
        EventCenter.AddListener(EventDefine.DecidePath, DecidePath);
        vars = ManagerVars.GetManagerVars();
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventDefine.DecidePath, DecidePath);
    }

    private void Start()
    {
        RandomPlatformTheme();
        platformSpawnPosition = startSpawnPos;
        for (int i = 0; i < 5; i++)
        {
            spawnPlatformCount = 5;
            DecidePath();
        }

        // generate character.
        GameObject go = Instantiate(vars.characterPre);
        go.transform.position = new Vector3(0, -1.8f, 0);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameStarted && GameManager.Instance.IsGameOver == false)
        {
            UpdateFallTime();
        }
    }
    private void UpdateFallTime()
    {
        if (GameManager.Instance.GetGameScore() > milestoneCount)
        {
            milestoneCount *= 2;
            fallTime *= multiple;
            if (fallTime < minFallTime)
            {
                fallTime = minFallTime;
            }

        }
    }

    private void DecidePath()
    {
        // dont need to repeated decidepath, if the platform has spike, just invoke specific func.
        if (isSpawnSpike)
        {
            AfterSpawnSpike();
            return;
        }
        if (spawnPlatformCount > 0)
        {
            spawnPlatformCount--;
            SpawnPlatform();
        }
        else
        {
            isLeftSpawn = !isLeftSpawn;
            spawnPlatformCount = Random.Range(1, 4);
            SpawnPlatform();
        }
    }

    private void SpawnPlatform()
    {
        int ranObstacleDir = Random.Range(0, 2);
        if (spawnPlatformCount >= 1)
        {
            SpawnNormalPlatform(ranObstacleDir);
        }
        else if(spawnPlatformCount == 0)
        {
            int ran = Random.Range(0, 3);
            if(ran == 0)
            {
                SpawnCommonPlatformGroup(ranObstacleDir);
            }
            else if(ran == 1)
            {
                switch (groupType)
                {
                    case PlatformGroupType.Grass:
                        SpawnGrassPlatformGroup(ranObstacleDir);
                        break;
                    case PlatformGroupType.Winter:
                        SpawnWinterPlatformGroup(ranObstacleDir);
                        break;
                    default:
                        break;
                }
            }

            // generate platform with spike
            else
            {
                int value = -1;
                if (isLeftSpawn)
                {
                    value = 0;
                }
                else
                {
                    value = 1;
                }
                SpawnSpikePlatform(value);

                isSpawnSpike = true;
                // number of extended platforms
                afterSpawnSpikeSpawnCount = 4;
                if (spikeSpawnLeft) // generate more platform in left direction
                {
                    spikeDirPlatformPos = new Vector3(platformSpawnPosition.x - 1.65f,
                        platformSpawnPosition.y + vars.nextYPos, 0);
                }
                else // generate platform in right
                {
                    spikeDirPlatformPos = new Vector3(platformSpawnPosition.x + 1.65f,
                        platformSpawnPosition.y + vars.nextYPos, 0);
                }
            }
        }

        int ranSpawnDiamond = Random.Range(0, 8);
        if (ranSpawnDiamond >= 6 && GameManager.Instance.PlayerIsMove)
        {
            GameObject go = ObjectPool.Instance.GetDiamond();
            go.transform.position = new Vector3(platformSpawnPosition.x,
                platformSpawnPosition.y + 0.5f, 0);
            go.SetActive(true);
        }
        if (isLeftSpawn)
        {
            platformSpawnPosition = new Vector3(platformSpawnPosition.x - vars.nextXPos,
                platformSpawnPosition.y + vars.nextYPos, 0);
        }
        else
        {
            platformSpawnPosition = new Vector3(platformSpawnPosition.x + vars.nextXPos,
                platformSpawnPosition.y + vars.nextYPos, 0);
        }
    }
    private void RandomPlatformTheme()
    {
        int ran = Random.Range(0, vars.platformThemeSpriteList.Count);
        selectPlatformSprite = vars.platformThemeSpriteList[ran];

        if (ran == 2)
        {
            groupType = PlatformGroupType.Winter;
        }
        else
        {
            groupType = PlatformGroupType.Grass;
        }
    }


    private void SpawnNormalPlatform(int ranObstacleDir)
    {
        GameObject go = ObjectPool.Instance.GetNormalPlatform();
        go.transform.position = platformSpawnPosition;
        go.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, ranObstacleDir);
        go.SetActive(true);
    }
    private void SpawnCommonPlatformGroup(int ranObstacleDir)
    {
        GameObject go = ObjectPool.Instance.GetCommonPlatformGroup();
        go.transform.position = platformSpawnPosition;
        go.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, ranObstacleDir);
        go.SetActive(true);
    }
    private void SpawnGrassPlatformGroup(int ranObstacleDir)
    {
        GameObject go = ObjectPool.Instance.GetGrassPlatformGroup();
        go.transform.position = platformSpawnPosition;
        go.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, ranObstacleDir);
        go.SetActive(true);
    }
    private void SpawnWinterPlatformGroup(int ranObstacleDir)
    {
        GameObject go = ObjectPool.Instance.GetWinterPlatformGroup();
        go.transform.position = platformSpawnPosition;
        go.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, ranObstacleDir);
        go.SetActive(true);
    }

    // generate spike platform input dierction
    private void SpawnSpikePlatform(int dir)
    {
        GameObject temp = null;
        if(dir == 0)
        {
            spikeSpawnLeft = false;
            temp = ObjectPool.Instance.GetRightSpikePlatform();
        }
        else
        {
            spikeSpawnLeft = true;
            temp = ObjectPool.Instance.GetLeftSpikePlatform();
        }
        temp.transform.position = platformSpawnPosition;
        temp.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, dir);
        temp.SetActive(true);
    }

    // generate platforms after spikeplatform, including both directions: spike and original
    private void AfterSpawnSpike()
    {
        if (afterSpawnSpikeSpawnCount > 0)
        {
            afterSpawnSpikeSpawnCount--;
            for (int i = 0; i < 2; i++)
            {
                GameObject temp = ObjectPool.Instance.GetNormalPlatform();
                if (i == 0) //original direction, renew next platform location
                {
                    temp.transform.position = platformSpawnPosition;
                    if (spikeSpawnLeft)
                    {
                        platformSpawnPosition = new Vector3(platformSpawnPosition.x + vars.nextXPos,
                            platformSpawnPosition.y + vars.nextYPos, 0);
                    }
                    else
                    {
                        platformSpawnPosition = new Vector3(platformSpawnPosition.x - vars.nextXPos,
                            platformSpawnPosition.y + vars.nextYPos, 0);
                    }
                }
                else// spike direction
                {
                    temp.transform.position = spikeDirPlatformPos;
                    if (spikeSpawnLeft)
                    {
                        spikeDirPlatformPos = new Vector3(spikeDirPlatformPos.x - vars.nextXPos,
                            spikeDirPlatformPos.y + vars.nextYPos, 0);
                    }
                    else
                    {
                        spikeDirPlatformPos = new Vector3(spikeDirPlatformPos.x + vars.nextXPos,
                            spikeDirPlatformPos.y + vars.nextYPos, 0);
                    }
                }

                temp.GetComponent<PlatformScript>().Init(selectPlatformSprite, fallTime, 1);
                temp.SetActive(true);
            }
        }
        else
        {
            isSpawnSpike = false;
            DecidePath();
        }
    }
}
