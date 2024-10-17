using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

public class Flare : MonoBehaviour
{

    //logic for an individual flare - this is entirely for the visual FX

    Rigidbody rb;
    Vector3 resetPosition;
    Quaternion resetRotation;
    bool isLaunched;
    public VisualEffect effect;
    void Start()
    {
        //    GetComponent<ParticleSystem>().Stop();
        effect = GetComponentInChildren<VisualEffect>();
        GetComponentInChildren<ParticleSystem>().Stop();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        resetPosition = transform.localPosition;
        resetRotation = transform.localRotation;
        GetComponent<HDAdditionalLightData>().intensity = 0f;
    }

    public void Launch()
    {
        isLaunched = true;
        effect.SetInt("Emit", 1);
//        GetComponent<ParticleSystem>().Play();
        GetComponentInChildren<ParticleSystem>().Play();
        rb.isKinematic = false;
        GetComponent<HDAdditionalLightData>().intensity = 3000f;
        rb.velocity = GetComponentInParent<Rigidbody>().velocity;
        rb.AddForce(transform.forward * (60f + Random.Range(0f, 20f)), ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(0f, 40f), Random.Range(0f, 40f), Random.Range(0f, 40f)));
        
        StartCoroutine(Reset());
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(4f);
        effect.SetInt("Emit", 0);
        yield return new WaitForSeconds(6f);

        GetComponent<HDAdditionalLightData>().intensity = 0f;
        GetComponentInChildren<ParticleSystem>().Stop();
        rb.isKinematic = true;
        yield return new WaitForFixedUpdate();
  //      GetComponent<ParticleSystem>().Stop();
        transform.localPosition = resetPosition;
        transform.localRotation = resetRotation;
        isLaunched = false;
       
    }

    private void LateUpdate()
    {
        if(!isLaunched)
        {
            transform.localPosition = resetPosition;
            transform.localRotation = resetRotation;
        }
    }


}
