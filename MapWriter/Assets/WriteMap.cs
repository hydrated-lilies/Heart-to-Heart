using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
public class WriteMap : MonoBehaviour
{
    public float timeOffset;
    public enum NOTE_TYPE { U, D, L, R, UL, UR, DL, DR };

    // play the song
    public AudioSource source;
    public AudioClip clip;

    // ring segments
    public RingControl ringL;
    public RingControl ringR;
    public RingControl ringU;
    public RingControl ringD;
    float heldTime = 0f;
    float heldTime2 = 0f;
    float heldTime3 = 0f;
    float heldTime4 = 0f;
    float maxHeldTime = 0.35f;
    // Start is called before the first frame update
    void Start()
    {
        string filePath = Application.dataPath + "/map.txt";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.Write("");
        }
        filePath = Application.dataPath + "/holdMap.txt";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.Write("");
        }

        source.Play();
    }

    void playDelayed()
    {
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (source == null || (!source.isPlaying))
            return;

        tapTracking();
    }

    void tapTracking()
    {
        // flash rings on tap
        if (Input.GetKeyDown("w"))
        {
            ringU.flashRing();
            // Write some text to the file
            WriteToFile("U " + (Time.time-0.1f).ToString());
            heldTime = Time.time;
            StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.U));
        }
        else if (Input.GetKeyUp("w"))
        {
            heldTime = Time.time - heldTime;
            float temp = Time.time - heldTime;
            WriteToHoldFile("HU " + (temp - 0.1f) + " " + (heldTime - 0.1f));
            if (heldTime < maxHeldTime)
            {
                RemoveLastHoldLine();
            }
            else
            {
                RemoveLastLine();
            }

        }

        if (Input.GetKeyDown("a"))
        {
            ringL.flashRing();
            WriteToFile("L " + (Time.time-0.1f).ToString());
            heldTime2 = Time.time;
            StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.L));
        }
        else if (Input.GetKeyUp("a"))
        {
            heldTime2 = Time.time - heldTime2;
            float temp = Time.time - heldTime2;
            WriteToHoldFile("HL " + (temp - 0.1f) + " " + (heldTime2 - 0.1f));
            if (heldTime2 < maxHeldTime)
            {
                RemoveLastHoldLine();
            }
            else
            {
                RemoveLastLine();
            }
        }

        if (Input.GetKeyDown("s"))
        {
            ringD.flashRing();
            WriteToFile("D " + (Time.time-0.1f).ToString());
            heldTime3 = Time.time;
            StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.D));
        }
        else if (Input.GetKeyUp("s"))
        {
            heldTime3 = Time.time - heldTime3;
            float temp = Time.time - heldTime3;
            WriteToHoldFile("HD " + (temp - 0.1f) + " " + (heldTime3 - 0.1f));
            if (heldTime3 < maxHeldTime)
            {
                RemoveLastHoldLine();
         
            }
            else
            {
                RemoveLastLine();
            }
        }

        if (Input.GetKeyDown("d"))
        {
            ringR.flashRing();
            WriteToFile("R " + (Time.time-0.1f).ToString());
            heldTime4 = Time.time;
            StartCoroutine(diagonalTracking(0.1f, NOTE_TYPE.R));
        }
        else if (Input.GetKeyUp("d"))
        {
            heldTime4 = Time.time - heldTime4;
            float temp = Time.time - heldTime4;
            WriteToHoldFile("HR " + (temp - 0.1f) + " " + (heldTime4 - 0.1f));
            if (heldTime4 < maxHeldTime)
            {
                RemoveLastHoldLine();

            }
            else
            {
                RemoveLastLine();
            }
        }
    }

    // helper IEnumerator that determines in a note was a diagonal tap
    IEnumerator diagonalTracking(float duration, NOTE_TYPE type)
    {
        // duration = buffer between note taps
        // NOTE_TYPE to determine the first input note and notes to check

        while (duration > 0)
        {
            // each input note has its own possible diagonals
            // check if note was hit, then flash track
            if (type == NOTE_TYPE.U)
            {
                // check L or R
                // LEFT
                if (Input.GetKeyDown("a"))
                {

                    RemoveLastLine();
                    WriteToFile("UL " + (Time.time-0.1f).ToString());
                    break;
                }
                // RIGHT
                if (Input.GetKeyDown("d"))
                {
                    RemoveLastLine();
                    WriteToFile("UR " + (Time.time-0.1f).ToString());
                    break; 
                }
            }
            else if (type == NOTE_TYPE.D)
            {
                // check L or R
                // LEFT
                if (Input.GetKeyDown("a"))
                {
                    RemoveLastLine();
                    WriteToFile("DL " + (Time.time-0.1f).ToString());
                    break;
                }
                // RIGHT
                if (Input.GetKeyDown("d"))
                {
                    RemoveLastLine();
                    WriteToFile("DR " + (Time.time-0.1f).ToString());
                    break;
                }
            }
            else if (type == NOTE_TYPE.L)
            {
                // check U or D
                // UP
                if (Input.GetKeyDown("w"))
                {
                    RemoveLastLine();
                    WriteToFile("UL " + (Time.time-0.1f).ToString());
                    break;
                }
                // DOWN
                if (Input.GetKeyDown("s"))
                {
                    RemoveLastLine();
                    WriteToFile("DL " + (Time.time-0.1f).ToString());
                    break;
                }
            }
            else if (type == NOTE_TYPE.R)
            {
                // check U or D
                // UP
                if (Input.GetKeyDown("w"))
                {
                    RemoveLastLine();
                    WriteToFile("UR " + (Time.time-0.1f).ToString());
                    break;
                }
                // DOWN
                if (Input.GetKeyDown("s"))
                {
                    RemoveLastLine();
                    WriteToFile("DR " + (Time.time-0.1f).ToString());
                    break;
                }
            }

            duration -= Time.deltaTime;

            yield return null;
        }
    }
    private void WriteToFile(string text)
    {
        string filePath = Application.dataPath + "/map.txt";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(text);
        }
    }
    private void WriteToHoldFile(string text)
    {
        string filePath = Application.dataPath + "/holdMap.txt";
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(text);
        }
    }
    private void RemoveLastLine()
    {
        string filePath = Application.dataPath + "/map.txt";
        string[] lines = File.ReadAllLines(filePath);
        Array.Resize(ref lines, lines.Length - 1);
        File.WriteAllLines(filePath, lines);
    }
    private void RemoveLastHoldLine()
    {
        string filePath = Application.dataPath + "/holdMap.txt";
        string[] lines = File.ReadAllLines(filePath);
        Array.Resize(ref lines, lines.Length - 1);
        File.WriteAllLines(filePath, lines);
    }


    void OnDestroy()
    {
        string filePath = Application.dataPath + "/map.txt";
        string[] lines = File.ReadAllLines(filePath);
        string[] newArray = lines.Distinct().ToArray();
        File.WriteAllLines(filePath, newArray);
    }
}
