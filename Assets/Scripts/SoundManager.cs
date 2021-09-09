using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip audioShoot;
    public AudioClip audioWalk;
    public AudioClip audioRun;
    public AudioClip audioImpactBody;
    public AudioClip audioImpactGround;
    public AudioClip audioDryfire;
    public AudioClip audioGetAmmo;
    public AudioClip audioExplosion;
    public AudioClip audioThrowGrenade;

    public static SoundManager Instance;
    private List<AudioSource> audioSources = new List<AudioSource>(); // 각각의 효과음 재생을 담당할 오디오 소스 리스트 생성

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < audioSources.Count; i++) // 효과음 재생이 종료된 오디오 소스가 있는지 확인 후 제거
        {
            if (!audioSources[i].isPlaying)
            {
                Destroy(audioSources[i]);
                audioSources.RemoveAt(i);
            }
        }
    }

    public AudioSource PlaySFX(string name) // 재생 요청 들어올 경우 오디오 소스 리턴해준 다음 관리 리스트에 추가
    {
        var audioSource = gameObject.AddComponent<AudioSource>();        

        switch (name) {
            case "Shoot": audioSource.clip = audioShoot; break;
            case "Walk": audioSource.clip = audioWalk; break;
            case "Run": audioSource.clip = audioRun; break;
            case "ImpactBody": audioSource.clip = audioImpactBody; break;
            case "ImpactGround": audioSource.clip = audioImpactGround; break;
            case "Dryfire": audioSource.clip = audioDryfire; break;
            case "GetAmmo": audioSource.clip = audioGetAmmo; break;
            case "Explosion":  audioSource.clip = audioExplosion; break;
            case "ThrowGrenade": audioSource.clip = audioThrowGrenade; break;    
}
        
        audioSource.Play();
        audioSources.Add(audioSource);        

        return audioSource;
    }


}
