using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AssetReference[] assetReference;

    public List<AudioSource> BGM;
    public AssetReference[] assetReference_SFX;
    public List<AudioSource> SFX;

    GameObject BackgroundMusic;
    AudioSource backmusic;

    void LoadAndInstatiatePrefab()
    {

        for (int i = 0; i < assetReference.Length; i++)
        {
            Debug.Log("출력");
            AsyncOperationHandle<GameObject> handle = assetReference[i].LoadAssetAsync<GameObject>();
            handle.Completed += OnLoadCompleted;
        }

        for (int i = 0; i < assetReference_SFX.Length; i++)
        {
            AsyncOperationHandle<GameObject> handle = assetReference_SFX[i].LoadAssetAsync<GameObject>();
            handle.Completed += OnLoadSFXCompleted;
        }
    }

    void OnLoadCompleted(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            BGM.Add(handle.Result.GetComponent<AudioSource>());
        }
    }

    void OnLoadSFXCompleted(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            SFX.Add(handle.Result.GetComponent<AudioSource>());
        }
    }

    void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAndInstatiatePrefab();
        }
        else
        {
            Destroy(gameObject);
        }
        
        BackgroundMusic = GameObject.Find("HomeBGM");
        backmusic = BackgroundMusic.GetComponent<AudioSource>();
        if (backmusic.isPlaying) return; 
        else
        {
            backmusic.Play();
            DontDestroyOnLoad(BackgroundMusic); 
        }

        
    }

    private void Start()
    {

        for (int i = 0; i < BGM.Count; i++)
        {
            BGM[i].dopplerLevel = 0.0f;
        }
        for (int i = 0; i < SFX.Count; i++)
        {
            SFX[i].dopplerLevel = 0.0f;
        }
    }

    private void Update()
    {
        if(PlayerPrefs.GetInt("sound") == 1)
        {
            for (int i = 0; i < BGM.Count; i++)
            {
                BGM[i].volume = 0.5f;
                //backmusic.Play();
            }
        }
        else if (PlayerPrefs.GetInt("sound") == 0)
        {
            for (int i = 0; i < BGM.Count; i++)
            {
                BGM[i].volume = 0.0f;
            }
        }

        if (PlayerPrefs.GetInt("sfx") == 1)
        {
            for (int i = 0; i < SFX.Count; i++)
            {
                SFX[i].volume = 0.5f;
            }
        }
        else if (PlayerPrefs.GetInt("sfx") == 0)
        {
            for (int i = 0; i < SFX.Count; i++)
            {
                SFX[i].volume = 0.0f;
            }
        }


        // 홈 사운드 0 1 11 12
        for (int i = 2; i < 11; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                backmusic.mute = true;
                backmusic.time = 0f;
            }
        }
        for (int i = 13; i < 18; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                backmusic.mute = true;
                backmusic.time = 0f;
            }
        }
        for (int i = 0; i < 2; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                if(PlayerPrefs.GetInt("sound") == 1)
                {
                    backmusic.mute = false;
                    backmusic.volume = 0.5f;
                }
            }
        }
        for (int i = 11; i < 13; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                if (PlayerPrefs.GetInt("sound") == 1)
                {
                    backmusic.mute = false;
                    backmusic.volume = 0.5f;
                }
            }
        }



    }
    public void HomeBGM()
    {
        PlayerPrefs.SetInt("sound",1);
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            backmusic.mute = false;
            backmusic.volume = 0.5f;
            backmusic.time = 0f;
        }
       
    }
    public void HomeBGMOff()
    {
        backmusic.mute = true;
    }

    private void OnDestroy()
    {
        if (assetReference != null)
        {
            for (int i = 0; i < assetReference.Length; i++)
            {
                assetReference[i].ReleaseAsset();
            }
        }

        if (assetReference_SFX != null)
        {
            for (int i = 0; i < assetReference_SFX.Length; i++)
            {
                assetReference_SFX[i].ReleaseAsset();
            }
        }
        

    }

}