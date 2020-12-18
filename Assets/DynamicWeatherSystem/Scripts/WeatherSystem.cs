using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;


[RequireComponent(typeof(AudioSource))]


public class WeatherSystem : MonoBehaviour
{
    //private Transform _player;
    //private Transform _weather;
    //public float _weatherHeight = 10.0f;


    // Particle Systems use for each weather state
    [Header("Particla System")]
    public ParticleSystem _sunnyParticleSystem;
    public ParticleSystem _overcastParticleSystem;
    public ParticleSystem _rainParticleSystem;
    public ParticleSystem _thunderStormParticleSystem;   
    public ParticleSystem _snowParticleSystem;


    // Timer use for randomize new state
    [Header("Timer")]
    public float _weatherTimer = 0.0f; //
    [SerializeField, Range(10.0f, 30.0f)]
    public float _resetWeatherTimer = 20.0f; // 


    // Audio Clips use for each weather state
    [Header("Sound Effects")]    
    public float _audioFadeTime = 0.5f;
    public AudioClip _sunnyAudio;
    public AudioClip _overcastAudio;
    public AudioClip _rainyAudio;
    public AudioClip _thunderStormAudio;   
    public AudioClip _snowyAudio;
    private AudioSource _soundSource;


    // Setting light through each state
    [Header("Light Setting")]   
    public float _lightDimTime          = 0.1f;    
    public float _sunnyIntensity        = 1.4f;
    public float _overcastIntensity     = 0.5f;
    public float _rainyIntensity        = 0.25f;
    public float _thunderStormIntensity = 0.15f;
    public float _snowyIntensity        = 0.75f;
    private Light2D _globalLight;


    // States use to set up new weather state
    [Header("Weather State")]
    public WeatherStates _currentWeatherState;
    private WeatherStates _previousWeatherState;



    // Start is called before the first frame update
    void Start()
    {
        _globalLight = GetComponent<Light2D>();
        _soundSource = GetComponent<AudioSource>();

        //GameObject _playergameObject = GameObject.FindGameObjectWithTag("Player");
        //_player = _playergameObject.transform;

        //GameObject _weathergameObject = GameObject.FindGameObjectWithTag("Weather");
        //_weather = _weathergameObject.transform;

        _previousWeatherState = _currentWeatherState;

        StartCoroutine(WeatherFSM());
    }



    // Update is called once per frame
    void Update()
    {
        SwitchWeatherTimer();

        //_weather.transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z + _weatherHeight);
    }



    // Counting time until changing to new Weather
    void SwitchWeatherTimer()
    {
        if (_weatherTimer > 0)
        {
            _weatherTimer -= Time.deltaTime;
        }       

        if(_weatherTimer <= 0)
        {
            _previousWeatherState = _currentWeatherState;
            _currentWeatherState = WeatherStates.PICKING;
            _weatherTimer = _resetWeatherTimer;
        }       
    }



    // Stop Particle System if is playing
    void StopParticleSystem(ParticleSystem pS)
    {
        if (pS.isPlaying)
        {
            pS.Stop();
        }
    }



    // Play Particle System if is stopped
    void PlayParticleSystem(ParticleSystem pS)
    {
        if (pS.isStopped)
        {
            pS.Play();
        }
    }



    // Adjusting Light by changing the Intensity
    void LightAdjustment(float currentWeatherIntnesity)
    {
        // Decrease intensity by lightDimTime
        if (_globalLight.intensity > currentWeatherIntnesity)
        {
            _globalLight.intensity -= Time.deltaTime * _lightDimTime;
        }

        // Increase intensity by lightDimTime
        else if (_globalLight.intensity < currentWeatherIntnesity)
        {
            _globalLight.intensity += Time.deltaTime * _lightDimTime;
        }
    }



    // Handle sounds through each weather state
    void SoundAdjustment(AudioClip _currentSoundClip)
    {
        // Gradually reduce active clip's volume
        if (_soundSource.volume > 0 && _soundSource.clip != _currentSoundClip)
        {
            _soundSource.volume -= Time.deltaTime * _audioFadeTime;
        }

        // Play approriate sound for specific weather
        if (_soundSource.volume == 0)
        {
            _soundSource.Stop();
            _soundSource.clip = _currentSoundClip;
            _soundSource.loop = true;
            _soundSource.Play();
        }

        // Increase active clip's volume
        if (_soundSource.volume < 1 && _soundSource.clip == _currentSoundClip)
        {
            _soundSource.volume += Time.deltaTime * _audioFadeTime;
        }
    }



    // While the weather state machine is active
    // Switch the weather state
    IEnumerator WeatherFSM()
    {
        
        while(true)
        {
            switch (_currentWeatherState)
            {
                case WeatherStates.PICKING:
                    PickWeather();
                    break;

                case WeatherStates.SUNNY:
                    SunnyWeather();
                    break;

                case WeatherStates.OVERCAST:
                    OvercastWeather();
                    break;

                case WeatherStates.RAINY:
                    RainyWeather();
                    break;

                case WeatherStates.THUNDERSTORM:
                    ThunderStormWeather();
                    break;

                case WeatherStates.TORONTO:
                    TotorntoWeather();
                    break;
            }

            yield return null;
        }
    }



    // When time out, switch to a new Weather State
    void PickWeather()
    {
        Debug.Log("PickWeather!");

       
        // Stop all playing Particle Systems
        StopParticleSystem(_sunnyParticleSystem);
        StopParticleSystem(_overcastParticleSystem);
        StopParticleSystem(_rainParticleSystem);
        StopParticleSystem(_thunderStormParticleSystem);
        StopParticleSystem(_snowParticleSystem);


        // Randomize new weather which should satisfy Prerequisite
        WeatherStates _switchWeatherState;
        bool isPrerequisiteOK = false;

        do
        {
            _switchWeatherState = (WeatherStates)UnityEngine.Random.Range(1, 6);

            if (_switchWeatherState == WeatherStates.RAINY)
            {
               if(_previousWeatherState == WeatherStates.OVERCAST)
                {
                    isPrerequisiteOK = true;
                }
            }
            else if(_switchWeatherState == WeatherStates.THUNDERSTORM)
            {
                if (_previousWeatherState == WeatherStates.RAINY)
                {
                    isPrerequisiteOK = true;
                }
            }
            //else if(_switchWeatherState == WeatherStates.TORONTO)
            //{
            //    if (_previousWeatherState == WeatherStates.OVERCAST)
            //    {
            //        isPrerequisiteOK = true;
            //    }
            //}
            else
            {
                isPrerequisiteOK = true;
            }
        }
        while (isPrerequisiteOK != true);


        // set current weather state to a new state
        _currentWeatherState = _switchWeatherState;
    }



    // Sunny Weather with brighter light
    void SunnyWeather()
    {
        Debug.Log("SunnyWeather!");       

        PlayParticleSystem(_sunnyParticleSystem);

        LightAdjustment(_sunnyIntensity);

        SoundAdjustment(_sunnyAudio);
    }



    // Overcast Weather with darker light
    void OvercastWeather()
    {
        Debug.Log("OvercastWeather!");

        PlayParticleSystem(_overcastParticleSystem);

        LightAdjustment(_overcastIntensity);

        SoundAdjustment(_overcastAudio);
    }



    // Rainy Weather with very darker light plus rain
    void RainyWeather()
    {
        Debug.Log("RainyWeather!");

        PlayParticleSystem(_rainParticleSystem);

        LightAdjustment(_rainyIntensity);

        SoundAdjustment(_rainyAudio);
    }



    // Thunderstorm Weather with rain plus flashes of lightning
    void ThunderStormWeather()
    {
        Debug.Log("ThunderStormWeather!");

        PlayParticleSystem(_rainParticleSystem);
        PlayParticleSystem(_thunderStormParticleSystem);

        LightAdjustment(_thunderStormIntensity);

        SoundAdjustment(_thunderStormAudio);
    }



    // Nothing other than whole snowy day!
    void TotorntoWeather()
    {
        Debug.Log("TotorntoWeather!");

        PlayParticleSystem(_snowParticleSystem);

        LightAdjustment(_snowyIntensity);

        SoundAdjustment(_snowyAudio);
    }
}
