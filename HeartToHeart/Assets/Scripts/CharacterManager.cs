using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Character
{
    private string name;
    private float txtSpd;
    private Color txtClr;
    private float txtPitch;

    private int expIndex;
    private int poseIndex;
    private int nametagIndex;

    public Character(string name, float speed, Color color)
    {
        this.name = name;
        txtSpd = speed;
        txtClr = color;
        txtPitch = 0.9f;
    }
    public string getName()
    {
        return name;
    }
    public float txtSpeed()
    {
        return txtSpd;
    }
    public Color txtCol()
    {
        return txtClr;
    }
    public float getTxtPitch() { return txtPitch; }
    public int getExpIndex()
    {
        return expIndex;
    }
    public int getPoseInex()
    {
        return poseIndex;
    }
    public int getNameTagIndex()
    {
        return nametagIndex;
    }
    public void setTxtPitch(float pitch) { txtPitch = pitch; }
    public void setExpIndex(int index)
    {
        expIndex = index;
    }
    public void setPoseIndex(int index)
    {
        poseIndex = index;
    }
    public void setNametageInd(int index)
    {
        nametagIndex = index;
    }
}
public class CharacterManager : MonoBehaviour
{
    // list of expressions for each character
    public List<Texture> nametagList;
    public List<Texture> aExpr;
    public List<Texture> aPose;
    public List<Texture> wExpr;
    public List<Texture> wPose;
    public List<Texture> cExpr;
    public List<Texture> cPose;
    public List<Texture> mPose;
    public List<Texture> mExpr;

    // misc sprites for minor characters and events
    public List<Texture> misc;

    // currently active character
    public Character activeCharacter;

    // references to the game images used for the scene
    public RawImage aria_body;
    public RawImage aria_expr;
    public RawImage npc_body;
    public RawImage npc_expr;

    // list of character objects
    List<Character> characters = null;

    // nametag object
    public RawImage nametag;

    // Start is called before the first frame update
    void Start()
    {
        InitCharacters();
    }

    void InitCharacters()
    {
        characters = new List<Character>();

        // define characters, type speed, and color type
        characters.Add(new("Aria", 0.1f, new Color(0.264f, 0.078f, 0.078f, 1)));
        characters.Add(new("Wesley", 0.08f, new Color(0.176f, 0.095f, 0.302f, 1)));
        characters.Add(new("Carrie", 0.12f, new Color(0.179f, 0.253f, 0.255f, 1)));
        characters.Add(new("Marlow", 0.11f, new Color(0.632f, 0.389f, 0.057f, 1)));
        characters.Add(new("Boss", 0.1f, Color.grey)); // (because it's a little silhouetto of a man)

        // add nametag indices, too
        characters[0].setNametageInd(0);
        characters[1].setNametageInd(1);
        characters[2].setNametageInd(2);
        characters[3].setNametageInd(3);
        characters[4].setNametageInd(4);

        // and text pitch
        characters[0].setTxtPitch(0.9f);
        characters[1].setTxtPitch(0.8f);
        characters[2].setTxtPitch(0.95f);
        characters[3].setTxtPitch(0.65f);
        characters[4].setTxtPitch(0.75f);

        // set default character to aria
        activeCharacter = characters[0];
    }

    void ResetActive()
    {
        // changes all current active character to their mouth closed components
        if (activeCharacter == null)
            return;

        // get the index of the current expression in the expression list for that character
        // increment 1 for the closed expression (orgnization purposely made for this)
        int newExp = activeCharacter.getExpIndex() + 1;

        // only change expression if not a multiple of 2
        if (activeCharacter.getExpIndex() % 2 == 0)
        {
            // set image to the appropriate character
            switch (activeCharacter.getName())
            {
                case "Aria":
                    aria_expr.texture = aExpr[newExp];
                    break;
                case "Wesley":
                    npc_expr.texture = wExpr[newExp];
                    break;
                case "Carrie":
                    npc_expr.texture = cExpr[newExp];
                    break;
                case "Marlow":
                    npc_expr.texture = mExpr[newExp];
                    break;
                case "Boss":
                    // doesn't need anything since there's no difference
                    break;
            }

            // set val
            activeCharacter.setExpIndex(newExp);
        }

        // sets active char to null
        activeCharacter = null;
    }

    void SetActive(Character toActivate)
    {
        // set inactive the current character
        ResetActive();

        // set new active character
        activeCharacter = toActivate;

        // set nametag
        nametag.texture = nametagList[toActivate.getNameTagIndex()];

        // with the current character, open their mouth if it is currently closed
        if (activeCharacter.getExpIndex() % 2 == 1)
        {
            int newExp = activeCharacter.getExpIndex() - 1;

            // set image to the appropriate character
            switch (activeCharacter.getName())
            {
                case "Aria":
                    aria_expr.texture = aExpr[newExp];
                    break;
                case "Wesley":
                    npc_expr.texture = wExpr[newExp];
                    break;
                case "Carrie":
                    npc_expr.texture = cExpr[newExp];
                    break;
                case "Marlow":
                    npc_expr.texture = mExpr[newExp];
                    break;
                case "Boss":
                    // doesn't need anything since there's no difference
                    break;
            }

            // set the next expressin to the index for the active character
            activeCharacter.setExpIndex(newExp);
        }
    }

    public void SetNameTag(bool on)
    {
        if (on)
            nametag.color = new Color(1, 1, 1, 1);
        else
            nametag.color = new Color(1, 1, 1, 0);
    }

    public void ParseCharacter (string cmd)
    {
        // check to see if the character list is loaded. If not, do that.
        if (characters == null)
            InitCharacters();

        // FIRST check to see if a character is just needing to have their mouth closed
        if (cmd.Contains("[OFF]"))
        {
            CloseExpression();
            return;
        }

        // set the character to be active
        string input = cmd;

        if (cmd.IndexOf(" ") > 0)
            input = cmd.Substring(0, cmd.IndexOf(" "));
        cmd = cmd.Substring(cmd.IndexOf(" ") + 1);

        // find the character asked for. then, set it to be active
        foreach (Character letter in characters)
            if (letter.getName().Contains(input) || input.Contains(letter.getName()))
            {
                SetActive(letter);
                ChangeNPC(letter.getName());
                break;
            }

        // set the expression of the character
        if (cmd.Contains("[EXPR]"))
        {
            cmd = cmd.Substring(cmd.IndexOf(" ") + 1);
            
            input = cmd;

            if (input.IndexOf(" ") > 0)
                input = input.Substring(0, input.IndexOf(" "));

            // set expression in helper function
            SetCharacterExpr(input);
        }
        
        // set the pose of the character
        if (cmd.Contains("[POSE]"))
        {
            cmd = cmd.Substring(cmd.IndexOf("]") + 2);

            SetCharacterPose(cmd);
        }
    }

    // -------------- CHARACTER EXPRESSION HANDLERS -------------- //
    void CloseExpression()
    {
        // increment 1 for the closed expression (orgnization purposely made for this)
        int newExp = activeCharacter.getExpIndex() + 1;

        // set image to the appropriate character
        switch (activeCharacter.getName())
        {
            case "Aria":
                aria_expr.texture = aExpr[newExp];
                characters[0].setExpIndex(newExp);
                break;
            case "Wesley":
                npc_expr.texture = wExpr[newExp];
                characters[1].setExpIndex(newExp);
                break;
            case "Carrie":
                npc_expr.texture = cExpr[newExp];
                characters[2].setExpIndex(newExp);
                break;
            case "Marlow":
                npc_expr.texture = mExpr[newExp];
                characters[3].setExpIndex(newExp);
                break;
            case "Boss":
                // doesn't need anything since there's no difference
                break;
        }

        // change index
        activeCharacter.setExpIndex(newExp);
    }

    // -------------- CHARACTER MOVEMENT HANDLERS -------------- //
    void ChangeNPC(string name)
    {
        return;
    }
    public void ShakeCharacter(float strength)
    {
        if (activeCharacter.getNameTagIndex() == 0)
            StartCoroutine(ShakeChar(strength, aria_body.gameObject, aria_expr.gameObject));
        else
            StartCoroutine(ShakeChar(strength, npc_body.gameObject, npc_expr.gameObject));
    }
    IEnumerator ShakeChar(float strength, GameObject body, GameObject face)
    {
        Vector3 startPos = body.transform.position;
        float curTime = 0f;

        while (curTime < 0.2f)
        {
            curTime += Time.deltaTime;
            float curStr = strength * 0.01f;
            Vector3 dist = Random.insideUnitCircle * curStr;

            body.transform.position = startPos + dist;
            face.transform.position = startPos + dist;

            yield return null;
        }
        body.transform.position = startPos;
        face.transform.position = startPos;
    }
    public void FadeCharacterInOut(bool which)
    {
        // if 'which' is true, fade aria's GameObject
        if (which)
        {
            // if the opacity if greater than 0.5 (opaque), fade out
            // if not, fade in
            if (aria_body.color.a > 0.5)
                StartCoroutine(FadeChar(false, aria_body, aria_expr));
            else
                StartCoroutine(FadeChar(true, aria_body, aria_expr));
        }
        // if false, fade npc's GameObject
        else
        {
            if (npc_body.color.a > 0.5)
                StartCoroutine(FadeChar(false, npc_body, npc_expr));
            else
                StartCoroutine(FadeChar(true, npc_body, npc_expr));
        }
    }
    IEnumerator FadeChar(bool dir, RawImage body, RawImage face)
    {
        // dir = true, fades in, dir = false fades out
        float alpha;
        float curTime = 0f;

        while (curTime < 0.05f)
        {
            curTime += Time.deltaTime;

            // begin fading
            alpha = (dir) ? (curTime / 0.1f) : 1 - (curTime / 0.1f);
            body.color = new Color(1, 1, 1, alpha); 
            face.color = new Color(1, 1, 1, alpha);

            yield return null;
        }

        // then set to min/max to make sure we have no floating point issues
        alpha = (dir) ? 1 : 0;
        body.color = new Color(1, 1, 1, alpha);
        face.color = new Color(1, 1, 1, alpha);
    }


    // -------------- CHARACTER EXPRESSION HANDLERS -------------- //
    void SetCharacterExpr(string expr)
    {
        switch (activeCharacter.getName())
        {
            case "Aria":
                SetExpressionAria(expr);
                break;
            case "Wesley":
                SetExpressionWesley(expr);
                break;
            case "Carrie":
                SetExpressionCarrie(expr);
                break;
            case "Marlow":
                SetExpressionMarlow(expr);
                break;
            default:
                SetExpressionMisc(expr);
                break;
        }
    }
    void SetExpressionAria(string expr)
    {
        int newIndex = 0;

        // change aria's expression to the open variant of the expression
        if (expr.Contains("Happy"))
            newIndex = 0;
        else if (expr.Contains("Silly"))
            newIndex = 2;
        else if (expr.Contains("Worried"))
            newIndex = 4;
        else if (expr.Contains("Confused"))
            newIndex = 6;
        else if (expr.Contains("Excited"))
            newIndex = 8;
        else if (expr.Contains("Wink"))
            newIndex = 10;
        else if (expr.Contains("Surprise"))
            newIndex = 12;
        else if (expr.Contains("Regret"))
            newIndex = 14;
        else
            print("error, no expression found for: " + expr);

        characters[0].setExpIndex(newIndex);
        aria_expr.texture = aExpr[newIndex];
    }
    void SetExpressionWesley(string expr)
    {
        int newIndex = 0;

        // change npc's expression to the open variant of wesley's expression
        if (expr.Contains("Frown"))
            newIndex = 0;
        else if (expr.Contains("Smile"))
            newIndex = 2;
        else if (expr.Contains("Scared"))
            newIndex = 4;
        else if (expr.Contains("Worried"))
            newIndex = 6;
        else
            print("error, no expression found for: " + expr);

        characters[1].setExpIndex(newIndex);
        npc_expr.texture = wExpr[newIndex];
    }
    void SetExpressionCarrie(string expr)
    {
        int newIndex = 0;

        if (expr.Contains("Neutral"))
            newIndex = 0;
        else if (expr.Contains("Nervous"))
            newIndex = 2;
        else if (expr.Contains("Sheepish"))
            newIndex = 4;
        else if (expr.Contains("Blush"))
            newIndex = 6;
        else
            print("somehow managed to mistype carrie's default pos");

        characters[2].setExpIndex(newIndex);
        npc_expr.texture = cExpr[newIndex];
    }
    void SetExpressionMarlow(string expr)
    {
        // carrie only has one expression
        if (expr.Contains("Frown"))
        {
            npc_expr.texture = mExpr[0];
            characters[2].setExpIndex(0);
        }
        else
        {
            print("somehow managed to mistype marlow's default expr");
            npc_expr.texture = mExpr[0];
            characters[2].setExpIndex(0);
        }
    }
    void SetExpressionMisc(string expr)
    {
        // TODO: EXPAND FOR MORE MISC TEXTURES
        npc_expr.texture = misc[0];
    }

    // -------------- CHARACTER POSE HANDLERS -------------- //
    void SetCharacterPose(string pose)
    {
        switch (activeCharacter.getName())
        {
            case "Aria":
                SetPoseAria(pose);
                break;
            case "Wesley":
                SetPoseWesley(pose);
                break;
            case "Carrie":
                SetPoseCarrie(pose);
                break;
            case "Marlow":
                SetPoseMarlow(pose);
                break;
            default:
                SetPoseMic(pose);
                break;
        }
    }
    void SetPoseAria(string pose)
    {
        int newIndex = 0;

        if (pose.Contains("Open"))
            newIndex = 0;
        else if (pose.Contains("Closed"))
            newIndex = 1;
        else if (pose.Contains("Down"))
            newIndex = 2;
        else if (pose.Contains("Point"))
            newIndex = 3;
        else
            print("missing pose for Aria: " + pose);

        characters[0].setPoseIndex(newIndex);
        aria_body.texture = aPose[newIndex];
    }
    void SetPoseWesley(string pose)
    {
        int newIndex = 0;

        if (pose.Contains("Open"))
            newIndex = 0;
        else if (pose.Contains("Closed"))
            newIndex = 1;
        else if (pose.Contains("Hold"))
            newIndex = 2;
        else
            print("missing pose for Wesley: " + pose);

        characters[1].setPoseIndex(newIndex);
        npc_body.texture = wPose[newIndex];
    }
    void SetPoseCarrie(string pose)
    {
        if (pose.Contains("Down"))
        {
            npc_body.texture = cPose[0];
            characters[2].setPoseIndex(0);
        }
        else
        {
            npc_body.texture = cPose[0];
            characters[2].setPoseIndex(0);

            print("mistyped carrie's only pose: " + pose);
        }
    }
    void SetPoseMarlow(string pose)
    {
        if (pose.Contains("Cross"))
        {
            npc_body.texture = mPose[0];
            characters[2].setPoseIndex(0);
        }
        else
        {
            npc_body.texture = mPose[0];
            characters[2].setPoseIndex(0);

            print("mistyped marlow's only pose: " + pose);
        }
    }
    void SetPoseMic(string pose)
    {
        // TODO: EXPAND FOR MORE MISC TEXTURES
        npc_body.texture = misc[0];
    }

}
