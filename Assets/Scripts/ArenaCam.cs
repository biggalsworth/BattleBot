using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArenaCam : MonoBehaviour
{
    public enum CameraType {Wall, Bot, Drone};
    protected CameraType cameraType;
    protected Quaternion recommendedRotation;
    public float recommendedFoV;
    float recommendedFoVLerp;
    public GameObject target;
    public Vector3 lerpTarget;
    public int botsInForwardAngle = 0;
    public int canseeBots = 0;
    public float trackingSpeed=1f;
    float newFieldOfView;
    List<BattleBot> visibleBots;
    public LayerMask blockingLayers;
    public float fuzzyPriority = 0f;
    float timeInUse = 0f;


    public void Awake()
    {
        target = Instantiate(new GameObject(), transform.position, transform.rotation);
        lerpTarget = GetMountTransform().position + GetMountTransform().forward;
        target.name = "Camera target";
        StartCoroutine(CalculateVisible());
        StartCoroutine(CalculateRecommendedValues());
        StartCoroutine(CalculateFuzzyPriority());
      
        
    }

    public void Start()
    {
        CameraManager.instance.arenaCams.Add(this);
    }

    public virtual Transform GetAnchor()
    {
        Debug.LogWarning("An attempt was made to parent the camera to a null anchor");
        return null;
    }

    public float GetPriority()
    {
        return fuzzyPriority;
    }
    protected abstract float GetRotationAngle();

    protected abstract Transform GetMountTransform();

    private void Update()
    {
        if (CameraManager.instance.currentActiveCamera == this) timeInUse += Time.deltaTime; else timeInUse -=Time.deltaTime;
        if (timeInUse < 0) timeInUse = 0;
        target.transform.position = Vector3.Lerp(target.transform.position, lerpTarget, Time.deltaTime * trackingSpeed);
      //  Camera.main.fieldOfView = Mathf.Clamp(newFieldOfView, 40f, 120f);
        recommendedFoV = Mathf.Lerp(recommendedFoV, recommendedFoVLerp, Time.deltaTime * trackingSpeed);
    }

    public CameraType GetCameraType()
    {
        return cameraType;
    }

    IEnumerator CalculateRecommendedValues()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (visibleBots != null)
            {
                GenerateRecommendedValues(visibleBots);
            }

        }
    }

    Bounds CalculateEncapsulatingBounds(List<BattleBot> bots)
    {
        Bounds bounds = new Bounds(bots[0].GetStats().position, Vector3.zero);

        foreach (BattleBot b in bots)
        {
            bounds.Encapsulate(new Bounds(b.GetStats().position, new Vector3(10f,10f,10f)));
        }

        return bounds;
    }


    public List<BattleBot> VisibilityRaycastCull(List<BattleBot> visibleBots)
    {
        botsInForwardAngle = visibleBots.Count;
        if (visibleBots.Count > 0)
        {
          
            for (int n = 0; n < visibleBots.Count; n++)
            {
                RaycastHit hit;
           
                if (Physics.SphereCast(GetMountTransform().transform.position + GetMountTransform().transform.forward*2f, 0.5f, (visibleBots[n].GetStats().position - GetMountTransform().transform.position).normalized, out hit, 100f, blockingLayers))
                {
                    if (!hit.collider.gameObject.GetComponentInParent<BattleBot>())
                    {
                        visibleBots.RemoveAt(n);
                    }
                }
            }

          
            
        }
        canseeBots = visibleBots.Count;
        return visibleBots;
    }

    public void GenerateRecommendedValues(List<BattleBot> visibleBots)
    {

        if (visibleBots.Count > 0)
        {


            Bounds bounds = CalculateEncapsulatingBounds(visibleBots);
            float radius = bounds.extents.magnitude;
            float distance = Vector3.Distance(Camera.main.transform.position, bounds.center);
            // Calculate the field of view based on the bounding sphere's radius
            recommendedFoVLerp = Mathf.Clamp(Mathf.Atan(radius / distance) * Mathf.Rad2Deg * 2f, 10f, 90f);

            Vector3 totalPos = Vector3.zero;

            foreach (BattleBot b in visibleBots)
            {
                totalPos += b.GetStats().position;
            }

            lerpTarget = totalPos / visibleBots.Count;
        }
        else
        {
            lerpTarget = GetMountTransform().position + GetMountTransform().forward;
            recommendedFoVLerp = 70f;
        }

    }
    IEnumerator CalculateVisible()
    {
        while (true)
        {

            visibleBots = new List<BattleBot>();

            if (BattleBot.allBots != null)
            {
                foreach (BattleBot b in BattleBot.allBots)
                {
                    Vector3 vectorToBot = (b.GetStats().position - GetMountTransform().position).normalized;
                    if (Vector3.Dot(GetMountTransform().forward, vectorToBot) > GetRotationAngle())
                    {
                        visibleBots.Add(b);
                    }
                }
            }

            VisibilityRaycastCull(visibleBots);   
            yield return new WaitForSeconds(5f);  
        }
    }

   IEnumerator CalculateFuzzyPriority()
   {
        while (true)
        {
            if (visibleBots.Count > 0)
            {
                BattleBot nearestVisible = GetNearestVisibleBot();
                float distanceToNearestVisible = Vector3.Distance(nearestVisible.GetStats().position, GetMountTransform().transform.position);
                float healthOfNearestVisible = nearestVisible.GetStats().health;
                float speedOfNearestVisible = nearestVisible.GetStats().velocity.magnitude;
                float fuzzyDistance = Fuzzify(distanceToNearestVisible, CameraManager.instance.botDistanceFuzzifier);
                float fuzzyHealth = Fuzzify(healthOfNearestVisible, CameraManager.instance.botHealthFuzzifier);
                float fuzzySpeed = Fuzzify(speedOfNearestVisible, CameraManager.instance.botsSpeedFuzzifier);
                float fuzzyVisible = Fuzzify((float)visibleBots.Count, CameraManager.instance.botsVisibleFuzzifier);
                float fuzzyTime = Fuzzify(timeInUse, CameraManager.instance.timeOnSameCameraFuzzifier);
                float shootValue = 0f;
                yield return new WaitForFixedUpdate();
                if (nearestVisible.GetAutoFire(BattleBot.Mount.LEFT)) shootValue += 0.5f;
                if (nearestVisible.GetAutoFire(BattleBot.Mount.RIGHT)) shootValue += 0.5f;
                float fuzzyProjectiles = Fuzzify(shootValue, CameraManager.instance.botShootingFuzzifier);

                float average = (fuzzyDistance + fuzzyHealth + fuzzySpeed + fuzzyVisible + fuzzyProjectiles) / 5f;
                float result = Mathf.Min(average, fuzzyTime);

                fuzzyPriority = result;
            }
            else fuzzyPriority = 0f;

            yield return new WaitForSeconds(1f);
        }
   }
      
    BattleBot GetNearestVisibleBot()
    {
        BattleBot nearestVisible=visibleBots[0];
        float distance = Mathf.Infinity;
        foreach(BattleBot b in visibleBots)
        {
            float currentDistance = Vector3.Distance(b.GetStats().position, GetMountTransform().position);
            if (currentDistance < distance)
            {
                nearestVisible = b;
                distance = currentDistance;
            }
        }
        return nearestVisible;
    }
    float Fuzzify(float value, AnimationCurve mapping)
    {
        return Mathf.Clamp(mapping.Evaluate(value), 0f, 1f);
    }

}
