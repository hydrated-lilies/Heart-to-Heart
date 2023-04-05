using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class VN_Control : MonoBehaviour
{
    // information for the textbox & visuals
    public TextMeshProUGUI txt;
    public float txtSpd;
    public GameObject textbox;
    bool crawling = false;

    // can players click?
    bool interactable = false;

    // information for the data being read from the file
    private List<string> VNScript;
    private int index;

    // object to fade between scenes
    public Image bg_fader;
    public Image scene_fader;

    // 0 - APARTMENT
    // 1 - SUBWAY
    // 2 - OFFICE
    // 3 - BLACK SCREEN (for extended fades)
    // 4 - WHITE SCREEN
    public List<Texture> backgrounds;
    public RawImage currBg;

    // manager for characters, and their graphics
    public CharacterManager cManager;
    public GameObject ariaVisual;
    public GameObject npcVisual;

    // sounds for text crawling and clicking noises
    public AudioSource txtBeep;

    // container for game prefab
    public GameObject gamePrefab;
    GameObject rhythmComponent;

    // Start is called before the first frame update
    void Start()
    {
        // txtSpd = 1; // based on speed

        ReadScript(1);

        StartCoroutine(FadeScene(true, 1f));

        index = -1;
        ReadNextLine();
    }

    void ReadScript(int sceneNumber)
    {
        // read lines using the stupid resources operation becaue of course this shit is packaged away in a place where I listerally can't get to it
        string rawScript = Resources.Load<TextAsset>("VN_Files/scene" + sceneNumber).text;

        // split it by newline delimeter because fuck it i guess why not ahaahaha
        // THANK YOU TO THE KIND FOLKS WHO PROGRAMMED STRING ARRAY TO GIVE FUNCTONALITY TO CONVERT TO LISTS
        VNScript = rawScript.Split("\n").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);

        if (Input.GetKeyDown("l"))
            CompleteSong();

        // check to see if button was pressed
        if(interactable && 
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            // if true set text to be finished immediately
            if (crawling)
                crawling = false;
            else
                ReadNextLine();
        }
    }

    void ReadNextLine()
    {
        index++;

        // at end of script, deactivate textbox and characters
        if (index > VNScript.Count - 1)
        {
            SceneManager.LoadScene(0);

            textbox.SetActive(false);
            //cManager.FadeCharacterInOut(false);
            //cManager.FadeCharacterInOut(true);
            return;
        }

        // check for commands that don't affect the screen
        while (VNScript[index].Contains("[Character]") || VNScript[index].Contains("[Toggle]") || VNScript[index] == "")
        {
            if (VNScript[index].Contains("[Character]"))
                cManager.ParseCharacter(VNScript[index].Substring(12));
            else if (VNScript[index].Contains("[Toggle]"))
            {
                string toToggle = VNScript[index].Substring(9);

                if (toToggle.Contains("npc"))
                {
                    cManager.FadeCharacterInOut(false);
                }
                else if (toToggle.Contains("Aria"))
                {
                    cManager.FadeCharacterInOut(true);
                }
            }
            index++;
        }

        // if [Wait] is called, set interactable to false for the duration of time
        if (VNScript[index].Contains("[Wait]"))
        {
            string line = VNScript[index].Substring(7);
            StartCoroutine(Wait(float.Parse(line)));
        }

        // read the next line!
        if (VNScript[index].Contains("[Line]"))
        {
            // clear current text
            txt.text = "";

            string line = VNScript[index].Substring(7);
            StartCoroutine(Type(line, cManager.activeCharacter.txtCol(), cManager.activeCharacter.txtSpeed(), cManager.activeCharacter.getTxtPitch()));

            // shake char
            cManager.ShakeCharacter(2.0f);
        }
        else if (VNScript[index].Contains("[Background]"))
        {
            // do we need to fade?
            if (VNScript[index].Contains("[F]"))
            {
                // change bg!
                string bgName = VNScript[index].Substring(17);
                ChangeBG(bgName, true);
            }
            else
            {
                // change bg!
                string bgName = VNScript[index].Substring(13);
                ChangeBG(bgName, false);
            }
        }
        else if (VNScript[index].Contains("[Load Game]"))
        {
            string song = VNScript[index].Substring(12);

            // load in prefab
            rhythmComponent = Instantiate(gamePrefab);
            
            // feed it the song it needs
            if (song.Contains("tutorial"))
                rhythmComponent.GetComponentInChildren<RhythmManager>().setMusicTrack(0, gameObject);
            // placeholder for now
            else
                rhythmComponent.GetComponentInChildren<RhythmManager>().setMusicTrack(0, gameObject);

            // disable VN interactability
            txt.gameObject.GetComponentInParent<Image>().gameObject.SetActive(false);
            interactable = false;

            // dimbg, clear text
            StartCoroutine(DimBG(0.35f, 1f));
            txt.text = "";

            // start song
            rhythmComponent.GetComponentInChildren<RhythmManager>().startSong();
        }
    }

    public void CompleteSong()
    {
        Destroy(rhythmComponent);

        // let the VN be usable again
        txt.gameObject.GetComponentInParent<Image>(true).gameObject.SetActive(true);
        interactable = true;

        // undim bg
        StartCoroutine(DimBG(1f, 1f));

        ReadNextLine();
    }

    IEnumerator Type(string line, Color col, float speed, float pitch)
    {
        // set text color
        txt.color = col;

        // set the current text pitch
        txtBeep.pitch = pitch;

        crawling = true;

        foreach (char c in line.ToCharArray())
        {
            // at any point, if 'crawling' is turned off, stop early
            if (!crawling)
            {
                txt.text = line;
                break;
            }

            // if not, remain crawlin!
            txt.text += c;

            // play a sound
            txtBeep.Play();

            yield return new WaitForSeconds(speed / 2.0f);
        }

        crawling = false;
    }
    void ChangeBG(string bg, bool fade)
    {
        float length = 0f;

        if (fade)
            length = 0.5f;

        if (bg.Contains("Apartment"))
            StartCoroutine(ChangeBG(backgrounds[0], length));
        else if (bg.Contains("Subway"))
            StartCoroutine(ChangeBG(backgrounds[1], length));
        else if (bg.Contains("Office"))
            StartCoroutine(ChangeBG(backgrounds[2], length));
        else if (bg.Contains("Black"))
            StartCoroutine(ChangeBG(backgrounds[3], length));
        else if (bg.Contains("White"))
            StartCoroutine(ChangeBG(backgrounds[4], length));
        else
            StartCoroutine(ChangeBG(backgrounds[3], length));
    }
    IEnumerator ChangeBG(Texture bgChange, float duration)
    {
        interactable = false;
        float time = 0;

        // fade to black
        while (time < duration)
        {
            bg_fader.color = new Color(0, 0, 0, (time / duration));
            time += Time.deltaTime;

            yield return null;
        }


        bg_fader.color = new Color(0, 0, 0, 1);

        // clear current text
        txt.text = "";
        // change bg texture
        currBg.texture = bgChange;

        // fade to transparent
        time = 0;
        while (time < duration)
        {
            bg_fader.color = new Color(0, 0, 0, 1f - (time / duration));
            time += Time.deltaTime;

            yield return null;
        }
        bg_fader.color = new Color(0, 0, 0, 0);
        interactable = true;
    }
    IEnumerator FadeScene(bool dir, float duration)
    {
        // wait for a brief moment
        yield return new WaitForSeconds(duration);

        // dir = true, fades in, dir = false fades out
        float alpha;
        float curTime = 0f;

        while (curTime < duration)
        {
            curTime += Time.deltaTime;

            // begin fading
            alpha = (dir) ? 1 - (curTime / duration) : (curTime / duration);
            scene_fader.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        // then set to min/max to make sure we have no floating point issues
        alpha = (dir) ? 0 : 1;
        scene_fader.color = new Color(0, 0, 0, alpha);

        interactable = true;
    }
    IEnumerator Wait(float duration)
    {
        interactable = false;

        yield return new WaitForSeconds(duration);

        interactable = true;
    }
    IEnumerator DimBG(float level, float duration)
    {
        // wait for a brief moment
        yield return new WaitForSeconds(duration);

        float curLevel = currBg.color.a;
        float step = (level - curLevel) / duration;

        float curTime = 0f;

        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            currBg.color = new Color(1, 1, 1, curLevel + (step * curTime));

            yield return null;
        }

        currBg.color = new Color(1, 1, 1, level);

        // then set to min/max to make sure we have no floating point issues
    }
}