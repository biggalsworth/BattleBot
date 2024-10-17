using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class EventFeed : MonoBehaviour
{

    static string toDisplay = "";
    TMP_Text TMPtext;
    static float timeOfLastBroadcast;
    static Animator anim;
    static string buffer;
    void Start()
    {
        anim = GetComponentInParent<Animator>();
        TMPtext = GetComponent<TMP_Text>();
        StartCoroutine(Fade());
        StartCoroutine(UpdateFeed());
    }

    public static void Broadcast(string toBroadcast)
    {
        toDisplay += toBroadcast + "\n";
        anim.SetBool("Visible", true);
        timeOfLastBroadcast = Time.time;
    }


    IEnumerator Fade()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
     
            if(Time.time-timeOfLastBroadcast > 5f)
            {
                anim.SetBool("Visible", false);
                yield return new WaitForSeconds(2f);
                TMPtext.text = "";
                toDisplay = "";
                buffer = "";
                timeOfLastBroadcast = Time.time;
           }

        }
    }
    IEnumerator UpdateFeed()
    {
        while (true)
        {
            int index = 0;
            buffer = "";
            float timeSinceLastUpdate = 0f;
            while (toDisplay.Length>0f)
            {
                if (index < toDisplay.Length)
                {
                    buffer += toDisplay[index];
                    timeSinceLastUpdate = 0f;
                    index++;
                }
                else
                {
                    timeSinceLastUpdate += Time.fixedDeltaTime;
                }

                if (index < toDisplay.Length && buffer.Length > 0 && !TMPtext.text.Equals(toDisplay))
                {
                    TMPtext.text = buffer + "_";
                }
                else
                {
                    TMPtext.text = buffer;
                }

                buffer = RemoveFirstLineIfExceedsMaxLines(buffer, 5);
                TMPtext.text = RemoveFirstLineIfExceedsMaxLines(TMPtext.text, 5);


                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForFixedUpdate();
        }
    }
    public string RemoveFirstLineIfExceedsMaxLines(string input, int maxLines)
    {
        // Split the input string into lines
        string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Check if the number of lines exceeds the maximum allowed
        if (lines.Length > maxLines)
        {
            // Remove the first line
            input = string.Join(Environment.NewLine, lines.Skip(1));
        }

        return input;
    }
}
