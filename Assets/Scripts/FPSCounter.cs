using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{

    public float updateInterval = 0.5f; // Kuinka usein p‰ivitet‰‰n. Nyt 0.5 sekunnin v‰lein.

    float accum = 0.0f;
    int frames = 0;
    float timeleft;
    float fps;

    GUIStyle textStyle = new GUIStyle();

    // Alustus
    void Start()
    {
        timeleft = updateInterval;

        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.white;
    }
    // Tehd‰‰n pari kovaa kikkaa
    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // 
        if (timeleft <= 0.0)
        {
            // n‰ytet‰‰n kahden desimaalin tarkkuudella
            fps = (accum / frames);
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        // N‰yt‰ FPS 0:n desimaalin tarkkuudella
        GUI.Label(new Rect(5, 5, 100, 25), fps.ToString("F0") + " FPS", textStyle);
    }
}