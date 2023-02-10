using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum ParticleList
{
    healUse = 0,
    healTick,
    specialCharge,
    specialParrying,
    particleCount,
}

public enum statType
{
    charaterName = 0,
    maxHP,
    attackPower,
    defencePower,
    moveSpeed,
    statCount,
}


public class DataManager : Singleton<DataManager>
{
    private readonly string path = "/Datas/";
    private readonly string skillDataCSV = "SkillData.csv";
    private readonly string characterDataCSV = "CharacterData.csv";
    private readonly string particleResourcePath = "Particles/";
    private readonly string noData = "NA";
    private readonly char multipleSplitChar = '#';

    private CoolTimeManager coolTimeUIManager;

    #region Properties #########################################################
    public CoolTimeData[] coolTimeDatas { get; private set; }
    public List<GameObject> particles { get; private set; }
    public Dictionary<Skills, Dictionary<SkillStats, object>> skillDictionary { get; private set; }
    public Dictionary<Weapons, Dictionary<statType, object>> statDictionary { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        InitializeCharacterDatas();
        InitializeSkillDatas();
        InitializeParticles();

        //// Cool Time Data Initialization --------------------------------------
        coolTimeUIManager = FindObjectOfType<CoolTimeManager>();
        coolTimeUIManager.InitializeUIs();

        coolTimeDatas = new CoolTimeData[(int)Skills.SkillCount];
        for (int i = 0; i < (int)Skills.SkillCount; i++)
        {
            coolTimeDatas[i] = new CoolTimeData((float)skillDictionary[(Skills)i][SkillStats.coolTime]);
            coolTimeDatas[i].onCoolTimeChange += coolTimeUIManager[i].RefreshUI;
        }
    }

    private void InitializeParticles()
    {
        particles = new List<GameObject>();
        for(int i = 0; i < (int)Skills.SkillCount; i++)
        {
            if (!skillDictionary[(Skills)i].ContainsKey(SkillStats.particles)) continue;

            string particleString = skillDictionary[(Skills)i][SkillStats.particles].ToString();
            string[] particles = particleString.Split(multipleSplitChar);
            foreach(var particle in particles)
            {
                var obj = (GameObject)Resources.Load(particleResourcePath + particle);
                if(obj == null)
                {
                    Debug.LogWarning($"{particle} does not exist in resources folder");
                    continue;
                }
                this.particles.Add(obj);
            }
        }
    }

    private void Update()
    {
        foreach (var data in coolTimeDatas)
        {
            data.CurrentCoolTime -= Time.deltaTime;
        }
    }

    private void InitializeSkillDatas()
    {
        StreamReader reader = new StreamReader(Application.dataPath + path + skillDataCSV);
        bool isEndofFile = false;
        int cursor = 0;
        skillDictionary = new();
        while (!isEndofFile)
        {
            string data = reader.ReadLine();
            if (cursor == 0) { cursor++; continue; }

            var statDic = new Dictionary<SkillStats, object>();
            if(data == null)
            {
                isEndofFile = true;
                break;
            }
            string[] splitData = data.Split(',');
            skillDictionary[(Skills)cursor - 1] = statDic;
            for (int j = 0; j < splitData.Length; j++)
            {
                if (splitData[j] == noData) continue;
                if (j == 0 || j == (int)SkillStats.particles)
                    statDic.Add((SkillStats)j, splitData[j]);
                else
                    statDic.Add((SkillStats)j, float.Parse(splitData[j]));
            }
            cursor++;
        }
    }

    private void InitializeCharacterDatas()
    {
        StreamReader reader = new StreamReader(Application.dataPath + path + characterDataCSV);
        bool isEndofFile = false;
        int cursor = 0;
        statDictionary = new();
        while (!isEndofFile)
        {
            string data = reader.ReadLine();
            if (cursor == 0) { cursor++; continue; }

            var statDic = new Dictionary<statType, object>();
            if (data == null)
            {
                isEndofFile = true;
                break;
            }
            string[] splitData = data.Split(',');
            statDictionary[(Weapons)cursor - 1] = statDic;
            for (int j = 0; j < splitData.Length; j++)
            {
                if (splitData[j] == noData) continue;
                if(j == (int)statType.charaterName)
                    statDic.Add((statType)j, splitData[j]);
                else
                    statDic.Add((statType)j, float.Parse(splitData[j]));
            }
            cursor++;
        }
    }
}
