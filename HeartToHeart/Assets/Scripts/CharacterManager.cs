using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Character
{
    private string name;
    private float txtSpd;
    private Color txtClr;

    private int expIndex;
    private int poseIndex;
    private int nametagIndex;

    public Character(string name, float speed, Color color)
    {
        this.name = name;
        txtSpd = speed;
        txtClr = color;
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
    List<Character> characters;

    // nametag object
    public RawImage nametag;

    // Start is called before the first frame update
    void Start()
    {
        characters = new List<Character>();

        // define characters, type speed, and color type
        characters.Add(new("Aria", 0.1f, new Color(0.264f, 0.078f, 0.078f, 1)));
        characters.Add(new("Wesley", 0.08f, new Color(0.176f, 0.095f, 0.302f, 1)));
        characters.Add(new("Carrie", 0.12f, new Color(0.179f, 0.253f, 0.255f, 1)));
        characters.Add(new("Boss", 0.1f, Color.black)); // (because it's a little silhouetto of a man)

        // add nametag indices, too
        characters[0].setNametageInd(0);
        characters[1].setNametageInd(1);
        characters[2].setNametageInd(2);
        characters[3].setNametageInd(3);

        // set default character to aria
        activeCharacter = characters[0];
    }

    // Update is called once per frame
    void Update() {}

    void ResetActive()
    {
        // changes all current active character to their mouth closed components
        if (activeCharacter == null)
            return;

        // get the index of the current expression in the expression list for that character
        // increment 1 for the closed expression (orgnization purposely made for this)
        int newExp = activeCharacter.getExpIndex() + 1;

        // set image to the appropriate character
        switch(activeCharacter.getName())
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
            case "Boss":
                // doesn't need anything since there's no difference
                break;
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
                case "Boss":
                    // doesn't need anything since there's no difference
                    break;
            }
        }
    }

    public void ParseCharacter (string cmd)
    {
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
            if (letter.getName() == input)
            {
                SetActive(letter);
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
                break;
            case "Wesley":
                npc_expr.texture = wExpr[newExp];
                break;
            case "Carrie":
                npc_expr.texture = cExpr[newExp];
                break;
            case "Boss":
                // doesn't need anything since there's no difference
                break;
        }
    }

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
        }
    }
    void SetExpressionAria(string expr)
    {
        int newIndex = 0;

        // change aria's expression to the open variant of the expression
        switch (expr)
        {
            case "Happy":         // index 0
                newIndex = 0;
                break;
            case "Silly":         // index 2
                newIndex = 2;
                break;
            case "Worried":       // index 4
                newIndex = 4;
                break;
            case "Confused":      // index 6
                newIndex = 6;
                break;
            case "Excited":       // index 8
                newIndex = 8;
                break;
            default:
                print("error, no expression found");
                break;
        }

        characters[0].setExpIndex(newIndex);
        aria_expr.texture = aExpr[newIndex];
    }
    void SetExpressionWesley(string expr)
    {
        int newIndex = 0;

        // change npc's expression to the open variant of wesley's expression
        switch (expr)
        {
            case "Frown":         // index 0
                newIndex = 0;
                break;
            case "Smile":         // index 2
                newIndex = 2;
                break;
            case "Scared":       // index 4
                newIndex = 4;
                break;
            case "Worried":      // index 6
                newIndex = 6;
                break;
            default:
                print("error, no expression found");
                break;
        }

        characters[1].setExpIndex(newIndex);
        npc_expr.texture = wExpr[newIndex];
    }
    void SetExpressionCarrie(string expr)
    {
        // carrie only has one expression
        switch (expr)
        {
            default:
            case "Neutral":
                npc_expr.texture = cExpr[0];
                characters[2].setExpIndex(0);
                break;
        }
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
        }
    }
    void SetPoseAria(string pose)
    {
        int newIndex = 0;

        switch (pose)
        {
            case "Open":
                newIndex = 0;
                break;
            case "Closed":
                newIndex = 1;
                break;
            case "Down":
                newIndex = 2;
                break;
        }

        characters[0].setPoseIndex(newIndex);
        aria_body.texture = aPose[newIndex];
    }
    void SetPoseWesley(string pose)
    {
        int newIndex = 0;

        switch (pose)
        {
            case "Open":
                newIndex = 0;
                break;
            case "Closed":
                newIndex = 1;
                break;
            case "Hold":
                newIndex = 2;
                break;
        }

        characters[1].setPoseIndex(newIndex);
        npc_body.texture = wPose[newIndex];
    }
    void SetPoseCarrie(string pose)
    {
        switch (pose)
        {
            default:
            case "Down":
                npc_body.texture = cPose[0];
                characters[2].setPoseIndex(0);
                break;
        }
    }
}
