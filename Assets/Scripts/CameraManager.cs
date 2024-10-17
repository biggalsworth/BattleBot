using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
  
    /* This class manages all the cameras in the game.
     * 
     * There's only one active camera; since HDRP has heavy overheads for instantiating or using multiple.
     * 
     * Every Camera is a subclass of ArenaCam, tailored for it's purpose (arena wall, onboard bot, etc.)
     * 
     * This class maintains a list of all these cameras and uses them in three modes. The 'AI' mode is currently very bare-bones
     * 
     * We'll look more at this AI mode in the fuzzy logic lecture. It's not the most obvious application, but camera control is a key AI task in many games, particularly 3rd person ones.
     */

    public enum Mode { Disabled, Random, AI, Manual, Cinematic }

    public List<ArenaCam> arenaCams;



    [SerializeField] public Mode mode;

    bool allowKeyInput = true;

    public ArenaCam currentActiveCamera;

    KeyCode[] keyCodes;

    public static CameraManager instance;

    [Header("Fuzzification Values")]
    [SerializeField] public AnimationCurve botDistanceFuzzifier;
    [SerializeField] public AnimationCurve botHealthFuzzifier;
    [SerializeField] public AnimationCurve botsVisibleFuzzifier;
    [SerializeField] public AnimationCurve botsSpeedFuzzifier;
    [SerializeField] public AnimationCurve timeOnSameCameraFuzzifier;
    [SerializeField] public AnimationCurve botShootingFuzzifier;

    float targetFOV = 60f;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            keyCodes = new KeyCode[]
            {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            };

            arenaCams = new List<ArenaCam>();
            //arenaCams.AddRange(GameObject.FindObjectsOfType<ArenaCam>());
          
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartRoutine(mode);
    }
    void StartRoutine(Mode mode)
    {
        if (mode == Mode.Random) StartCoroutine(RandomCameras());
        if (mode == Mode.AI) StartCoroutine(AICameras());
        if (mode == Mode.Disabled) StartCoroutine(Disabled());
    
    }

    private void Update()
    {
        if (allowKeyInput)
        {
            for (int n = 0; n < keyCodes.Length; n++)
            {
                if (Input.GetKeyDown(keyCodes[n]))
                {
                    
                    if (BattleBot.allBots.Count > n)
                    {
                        CameraSwitch(BattleBot.allBots[n].GetComponentInChildren<BotCam>());
                        mode = Mode.Disabled;
                    }
                }
            }
        }

        if (Camera.main.GetComponentInParent<WallCam>()||Camera.main.GetComponentInParent<DroneCam>())
        {
            targetFOV = Mathf.Lerp(targetFOV, Camera.main.GetComponentInParent<ArenaCam>().recommendedFoV, Time.deltaTime);
            Matrix4x4 projMat = Camera.main.projectionMatrix;
            float aspectRatio = (float)Screen.width / Screen.height;
            float fovRad = targetFOV * Mathf.Deg2Rad;
            float top = Mathf.Tan(0.5f * fovRad);
            float bottom = -top;
            float right = top * aspectRatio;
            float left = -right;

            projMat.m00 = 2 / (right - left);
            projMat.m11 = 2 / (top - bottom);
            projMat.m02 = (right + left) / (right - left);
            projMat.m12 = (top + bottom) / (top - bottom);

            Camera.main.projectionMatrix = projMat;
            //  Camera.main.fieldOfView = targetFOV;

        }
        else
        {
            Camera.main.ResetProjectionMatrix();
        //    Camera.main.fieldOfView = 60f;
        }

        if (mode == Mode.Cinematic)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cinematicTarget, Time.deltaTime);
            Camera.main.transform.LookAt(cinematicLookTarget);
        }
       
    }

    IEnumerator AICameras()
    {
        // We'll look at and develop this a bit more when we examine fuzzy logic! 
        yield return new WaitForFixedUpdate();
        float currentPriority;

        while (mode == Mode.AI)
        {
            if (currentActiveCamera) currentPriority = currentActiveCamera.fuzzyPriority;
            else currentPriority = 0f;

            for(int n=0; n<arenaCams.Count; n++)
            {
                if (arenaCams[n] == null) arenaCams.RemoveAt(n);
            }


            foreach(ArenaCam c in arenaCams)
            {
                if (c.GetPriority() > currentPriority)
                {
                    currentPriority = c.GetPriority();
                    CameraSwitch(c);
                }
            }
            yield return new WaitForSeconds(5f); 
        }

        StartRoutine(mode);
    }

   
    IEnumerator Disabled()
    {
        //do not switch between cameras at all

        yield return new WaitUntil(() => mode != Mode.Disabled);
        StartRoutine(mode);
    }


    IEnumerator RandomCameras()
    {

        //switch between all available cameras randomly

        while (mode == Mode.Random)
        {
            ArenaCam randomCam = arenaCams[Random.Range(0, arenaCams.Count)];
            CameraSwitch(randomCam);
            yield return new WaitForSeconds(Random.Range(10f, 15f));
        }
        yield return new WaitUntil(()=> mode == Mode.Random);
        StartRoutine(mode);
    }

    //allows for switching to a botcam by passing the bot, rather than the camera, for convenience
    public void CameraSwitch(BattleBot bot)
    {
        mode = Mode.Disabled;
        CameraSwitch(bot.GetComponentInChildren<BotCam>());
    }

    //revert to a random camera
    public void CameraSwitch()
    {
        ArenaCam randomCam = arenaCams[Random.Range(0, arenaCams.Count)];
        CameraSwitch(randomCam);
    }

    public void PerformBotCinematic(BattleBot bot)
    {
        StartCoroutine(DoBotCinematic(bot));
    }

    Vector3 cinematicTarget;
    Vector3 cinematicLookTarget;
    IEnumerator DoBotCinematic(BattleBot bot)
    {
        Mode previousMode = mode;
        mode = Mode.Cinematic;
        cinematicLookTarget = bot.GetHeadTransform().position;
        
        cinematicTarget = bot.GetHeadTransform().position + (Random.insideUnitSphere*Random.Range(3f,5.5f));
        Camera.main.transform.position = bot.GetHeadTransform().position + (cinematicTarget - bot.GetHeadTransform().position).normalized * Random.Range(3f, 5f) + Random.insideUnitSphere;
        yield return new WaitForSeconds(3f);
        Camera.main.transform.position = bot.GetHeadTransform().position + (cinematicTarget - bot.GetHeadTransform().position).normalized * Random.Range(3f, 5f) + Random.insideUnitSphere;
        cinematicTarget = bot.GetHeadTransform().position + (Random.insideUnitSphere * Random.Range(3f, 5.5f));
        yield return new WaitForSeconds(4f);
        Camera.main.transform.position = bot.GetHeadTransform().position + (cinematicTarget - bot.GetHeadTransform().position).normalized * Random.Range(3f, 5f) + Random.insideUnitSphere;
        cinematicTarget = bot.GetHeadTransform().position + (Random.insideUnitSphere * Random.Range(3f, 5.5f));
        yield return new WaitForSeconds(3f);
        mode = previousMode;


    }
    //perform a camera switch
    void CameraSwitch(ArenaCam targetCamera)
    {
      
       
            currentActiveCamera = targetCamera;
            //parent the main camera to the anchor of the specified arena cam and reset its transform
            Camera.main.transform.parent = targetCamera.GetAnchor();
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            targetFOV = targetCamera.recommendedFoV;
            //enable postprocessing style by camera type
           
        }
    
  
}
