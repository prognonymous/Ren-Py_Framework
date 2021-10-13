using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
internal class SpeechArea
{
    [Tooltip("The asset which houses the speaker's name.")]
    [SerializeField]
    private TextMeshProUGUI speakerAsset;
    [Tooltip("The asset which houses the text.")]
    [SerializeField]
    private TextMeshProUGUI speechAsset;
    [Tooltip("The text to show at the appropriate scene section.")]
    [SerializeField]
    private Text[] textActual;

    public void Initialize()
    {
        if (!this.IsInUse()) throw new System.NullReferenceException();
        
        foreach(Text t in this.textActual)
        {
            if (string.IsNullOrEmpty(t.GetSpeaker())) t.ChangeName("test:");
            if (string.IsNullOrEmpty(t.GetSpeech())) t.ChangeSpeechText("test");
        }
    }

    public bool IsInUse()
    { return (this.speakerAsset != null && this.speechAsset != null && this.textActual != null && this.textActual.Length > 0); }

    public int Length() { return this.textActual.Length; }

    public TextMeshProUGUI GetSpeechAsset() { return this.speechAsset; }
    public TextMeshProUGUI GetSpeaker() { return this.speakerAsset; }

    private Text GetTextElementAt(int position)
    {
        if (this.textActual.Length >= position)
        { return this.textActual[position]; }
        return null;
    }

    private string GetSpeakerText(Text currentTextElement) 
    {
        if (currentTextElement != null) return currentTextElement.GetSpeaker();
        return null;
    }
    private string GetSpeechText(Text currentTextElement)
    {
        if (currentTextElement != null) return currentTextElement.GetSpeech();
        return null;
    }

    public (string speak, string speech) GetTextStringsAt(int position)
    {
        Text currentTextElement = this.GetTextElementAt(position);
        return (this.GetSpeakerText(currentTextElement), this.GetSpeechText(currentTextElement));
    }

    public void SetSpeakerText(string text)
    {
        TextMeshProUGUI asset = this.GetSpeaker();
        if (asset != null)
        {
            if (text != asset.text)
            {
                asset.SetText(text);
            }
        }
    }

    public void SetSpeech(string text)
    {
        TextMeshProUGUI asset = this.GetSpeechAsset();
        if (asset != null)
        {
            if (text != asset.text)
            {
                asset.SetText(text);
            }
        }
    }

    [System.Serializable]
    private class Text
    {
        [Tooltip("The name of the speaker at the appropriate scene section number.")]
        [SerializeField]
        private string speakerText;
        [Tooltip("The speech that appears for the speaker at the same point.")]
        [TextArea]
        [SerializeField]
        private string speechText;

        public string GetSpeaker() { return this.speakerText; }
        public void ChangeName(string name) { this.speakerText = name; }

        public string GetSpeech() { return this.speechText; }
        public void ChangeSpeechText(string speech) { this.speechText = speech;  }
    }
}

[System.Serializable]
internal class CharacterPrefab
{
    [Tooltip("The image to change.")]
    [SerializeField]
    private Image Character;
    [Header("Optional Parameters")]
    [Tooltip("The sprite to use per scene section.")]
    [SerializeField]
    private Sprite[] Expressions;

    public void Initialize()
    {
        if (!this.IsUnUse()) Debug.LogWarning("Warning: a character prefab was not in use but attempted" +
            " to initialize anyway. This is probably due to the character image being valid but having no expression" +
            " elements. Don't do this.");
        else if (this.Expressions[0] == null) this.Expressions[0] = this.Character.sprite;
    }

    public bool IsUnUse()
    { return (this.Character != null && this.Expressions != null && this.Expressions.Length > 0); }

    public void ChangeCharacterSprite(Sprite s) { this.Character.sprite = s; }

    private Sprite GetExpression(int position) { return this.Length() >= position ? this.Expressions[position] : null; }

    public int Length() { return this.Expressions.Length; }

    private Sprite GetPreviousValidSprite(int position)
    {
        //Set static variables to cut down on CPU usage.
        Sprite oldSprite = this.Character.sprite;

        //If the position isn't out of bounds of the array
        if (this.IsUnUse() && this.Length() >= position)
        {
            //Find the nearest valid sprite in the array backwards.
            while (this.GetExpression(position) == null && position > 0) position--;

            //Set static sprite to cut down on CPU usage.
            Sprite staticNewSprite = this.GetExpression(position);

            //If the new sprite exists and isn't the same as the current sprite, return the found sprite.
            //Otherwise exit without changing anything.
            if (staticNewSprite != null && staticNewSprite != oldSprite) return staticNewSprite;
        }

        //Default fallthrough.
        return oldSprite;
    }
    public Sprite ReturnValidSprite(int position) { return this.GetPreviousValidSprite(position); }
}

[System.Serializable]
internal class RawImagePrefab
{
    [Tooltip("An image element. This can be any raw image.")]
    [SerializeField]
    private RawImage Image;
    [Tooltip("The values associated with this image to change during scene sections if applicable.")]
    [SerializeField]
    private Vars[] Values;

    public void Initalize()
    {
        //Make sure the prefab doesn't attempt to initialize if it's not currently valid.
        if (!this.IsInUse())
        { Debug.LogWarning("Warning: a raw image prefab was not in use but attempted to initialize anyway."); }
        else
        {
            Vars staticValues = this.Values[0];
            if (!staticValues.UseColor())
            {
                staticValues.SetValue(this.Image.color);
                staticValues.SetUseColor(false);
            }
            if (staticValues.GetTexture() == null) staticValues.SetValue(this.Image.texture);
        }
    }

    public bool IsInUse()
    { return (this.Image != null && this.Values != null && this.Values.Length > 0); }

    public RawImage GetRawImage() { return this.Image; }

    private (Texture, bool) GetPreviousValidTexture(int position)
    {
        //Check if the array is initialized or if null textures are allowed.
        if (this.IsInUse() && this.Values.Length >= position)
        {
            //Set static variables to cut down on CPU usage.
            //The position chosen is not outside the bounds of the array.
            Vars staticValues = this.Values[position];

            if (!staticValues.NullTexture())
            {
                //Find the nearest valid value in the array backwards.
                while (staticValues.GetTexture() == null && position > 0)
                {
                    position--;
                    staticValues = this.Values[position];
                }

                //Return the found texture.
                return (staticValues.GetTexture(), staticValues.NullTexture());
            } else return (null, true);
        }

        //Default fallthrough value.
        return (this.Image.texture, false);
    }

    public (Texture, bool) ReturnValidTexture(int position)
    { return this.GetPreviousValidTexture(position); }
    
    public bool SameColor(Color32 lhs, Color32 rhs)
    { return (lhs.a == rhs.a && lhs.b == rhs.b && lhs.g == rhs.g && lhs.r == rhs.r); }

    private (Color32, bool) GetPreviousValidColor(int position)
    {
        //Set static variables to cut down on CPU usage.
        Color32 oldColor = this.Image.color;

        //Check if the array is initialized, the prefab is in use and the position is within the bounds of the array.
        if (this.IsInUse() && this.Values != null && this.Values.Length >= position)
        {
            Vars staticValues = this.Values[position];

            //Find the nearest valid value in the array backwards.
            while (!staticValues.UseColor() && position > 0)
            {
                position--;
                staticValues = this.Values[position];
            }

            //Return found value.
            return (staticValues.GetColor(), staticValues.UseColor());
        }

        //Default fallthrough value.
        return (oldColor, true);
    }

    public (Color32 color, bool use) ReturnValidColor(int position)
    { return this.GetPreviousValidColor(position); }

    [System.Serializable]
    private class Vars
    {
        [Tooltip("The texture for the specified scene section.")]
        [SerializeField]
        private Texture TextureVariable;
        [Tooltip("Empty the current texture. Does not display the texture selected if true.")]
        [SerializeField]
        private bool EmptyCurrentTexture;
        [Tooltip("The color for the specified scene section.")]
        [SerializeField]
        private Color32 Color;
        [Tooltip("Whether or not to change the scene color to the picked color at the specified scene section. So you don't have to set multiples of the same value.")]
        [SerializeField]
        private bool NoColor;

        public Texture GetTexture() { return this.TextureVariable; }

        public bool NullTexture() { return this.EmptyCurrentTexture;  }

        public void SetNoTexture(bool value) { this.EmptyCurrentTexture = value; }

        public void SetValue(Texture value) { this.TextureVariable = value; }

        public Color32 GetColor() { return this.Color; }

        public void SetValue(Color32 value) { this.Color = value; }

        public bool UseColor() { return !this.NoColor; }

        public void SetUseColor(bool value) { this.NoColor = value; }
    }
}

public class SceneManager : MonoBehaviour
{
    [Header("Array Starting Position")]
    [Tooltip("Current position of the array in this scene manager. Set here to change it from the start. Save to keep the history at the point when the user saved the game.")]
    [SerializeField]
    private int CurrentPosition;

    [Header("History")]
    [Tooltip("Path where the file is being held. This must be somewhere in the current working directory.")]
    [TextArea]
    [SerializeField]
    private string HistoryPath;
    [Tooltip("The name to use to save the history. This can be anything you like.")]
    [SerializeField]
    private string HistoryFileName;

    [Header("Mandatory Components")]

    [Tooltip("I've added a button element to allow onClick() functionality. Feel free to change to whatever UI element you wish to use.")]
    [SerializeField]
    private Button UIButton;

    //Strings to set if setting from UI instead of programmatically.
    //Not strictly required unless the button is being used.

    [Tooltip("The elements that change what is inside the text box.")]
    [SerializeField]
    private SpeechArea SpeechElements;

    [Header("OPTIONAL Components")]
    [Tooltip("The NPC's elements to change.")]
    [SerializeField]
    private CharacterPrefab[] NPCElements;

    [Tooltip("Element of the landscape to change.")]
    [SerializeField]
    private RawImagePrefab[] LandscapeElements;

    [Header("In-Game Settings")]
    [Tooltip("Whether the same array number has been visited by the user yet.")]
    [SerializeField]
    private bool[] Visited;

    //Get the current position.
    //You'll need the current position for any history implimentation.
    public int getCurrentPosition()
    { return this.CurrentPosition; }

    //Get the current Visited bool array.
    //Useful though not strictly required for any history implimentation.
    public bool currentSceneSectionIsVisited()
    { return this.Visited[this.CurrentPosition]; }

    private void Start()
    {
        //Initialize all variables. Ignoring those which are optional AND unset.
        this.SpeechElements.Initialize();

        if (this.NPCElements != null && this.NPCElements.Length > 0)
        { foreach (CharacterPrefab c in this.NPCElements) if (c.IsUnUse()) c.Initialize(); }

        if (this.LandscapeElements != null && this.LandscapeElements.Length > 0)
        { foreach (RawImagePrefab rip in this.LandscapeElements) if (rip.IsInUse()) rip.Initalize(); }

        //Read and parse the history file then add those values to the current manager.
        this.ReadHistory();

        //Show the first scene.
        this.ShowScene(false);
    }

    private void OnEnable()
    {
        //Register the showing of scenes with the mouse click button.
        this.UIButton.onClick.AddListener(() => this.ShowScene());
    }

    //Show the scene that the CurrentPosition+1 currently would show.
    //If you do not advance the CurrentPosition will not move forward and the currently stored scene will show.
    public void ShowScene(bool advance = true)
    {
        //As long as you want to advance and the length of the speech text array is greater than the current position + 1
        //The current position will be moved forward before showing the scene proper.
        if (this.SpeechElements.Length() > (this.CurrentPosition + 1) && advance) this.CurrentPosition++;

        //Show the scene.
        this.ChangeSceneValues(
            this.CurrentPosition,
            this.SpeechElements,
            this.NPCElements,
            this.LandscapeElements);

        //Make sure that if the current Visited bool is not yet true, to turn it true.
        if (!this.Visited[this.CurrentPosition]) this.Visited[this.CurrentPosition] = true;
    }


    //Set to the nearest sprite BACKWARDS in the array to the image provided. If one cannot be found it will set to the very first
    //sprite in the sprite array. If THAT doesn't exist won't change the sprite at all.
    //This is for backwards functionality.
    private void SetObject(CharacterPrefab npc, int position = 0)
    { if (npc.IsUnUse()) npc.ChangeCharacterSprite(npc.ReturnValidSprite(position)); }

    //This is an overload of the above SetObject.
    //Does the exact same things just with different things.
    private void SetObject(RawImagePrefab imgObj, int position = 0)
    {
        if (imgObj.IsInUse())
        {
            RawImage img = imgObj.GetRawImage();
            //If the raw image is initialized.
            if (img != null)
            {
                (Texture t, bool allowNull) newTexture = imgObj.ReturnValidTexture(position);
                //Check if the texture really is valid before changing it.
                if (newTexture.t != img.texture && (newTexture.t != null || newTexture.allowNull))
                {
                    //Change the image's texture.
                    img.texture = newTexture.t;
                }

                (Color32 color, bool use) newColor = imgObj.ReturnValidColor(position);
                //Check if the color really is valid before changing it.
                if (newColor.use && !imgObj.SameColor(newColor.color, img.color))
                {
                    //Change the image's color.
                    img.color = newColor.color;
                }
            }
        }
    }


    //Change the scene. This does not use any of the local varibles, just what is passed to it.
    private void ChangeSceneValues(
        int curPos,
        SpeechArea text,
        CharacterPrefab[] chars,
        RawImagePrefab[] landscape)
    {
        (string speaker, string speech) textElements = text.Length() >= this.CurrentPosition ? text.GetTextStringsAt(curPos)
            : (text.GetSpeaker().text, "Error: You've gone outside the bounds of the speech array!");

        //Warning messages if the speech or speaker is empty. Delete these if you don't want them.
        if (string.IsNullOrEmpty(textElements.speaker))
        {
            Debug.LogWarning(string.Format("Error no element was passed for speaker text " +
            "at scene modification value ({0}). This will not crash the game, but it's a terrible idea!", curPos));
        }
        if (string.IsNullOrEmpty(textElements.speech))
        {
            Debug.Log(string.Format("Warning no element was passed for speech text " +
            "at scene modification value ({0}). Was this what you wanted?", curPos));
        }

        //Set up the objects used in the scene.
        if (chars != null && chars.Length > 0)
        { foreach (CharacterPrefab c in chars) this.SetObject(c, curPos); }

        if (landscape != null && landscape.Length > 0)
        { foreach (RawImagePrefab rip in landscape) this.SetObject(rip, curPos); }

        text.SetSpeakerText(textElements.speaker);
        text.SetSpeech(textElements.speech);

        //Save the history at this current point.
        this.SaveCurrentHistory();
    }

    private void OnDisable()
    {
        //Un-Register Events
        //Pretty sure this only happens if you disable the script programmatically?
        this.UIButton.onClick.RemoveAllListeners();
    }

    // Update is called once per frame
    // Includes mouse scroll functionality.
    private void Update()
    {
        //Capture the current mouse scroll and save it.
        //Can help prevent issues where the mouse scroll gets reset too fast so nothing changes.
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");

        //Shows the next scene if the mouse scroll is scrolled toward the user and the previous if away.
        //This is to emulate Ren'Py functionality as I understand it.
        //To swap this change the "less than" (<) and "greater than" (>) symbols on the mouse scroll checks.
        //So for example change the mouseScroll < 0 to mouseScroll > 0 to
        //show the next scene when the mouse scroll is scrolled away from the user.
        int nextSceneSection = this.CurrentPosition + 1;
        int previousSceneSection = this.CurrentPosition - 1;
        if (mouseScroll < 0 && nextSceneSection < this.Visited.Length)
        { if (this.Visited[nextSceneSection]) this.ShowScene(); }
        else if (mouseScroll > 0 && previousSceneSection >= 0)
        {
            this.CurrentPosition--;
            this.ShowScene(false);
        }
    }

    //Read and parse history.
    private void ReadHistory()
    {
        //The path to the directory.
        string directoryPath = string.Format("{0}/{1}", Application.dataPath, this.HistoryPath);

        //The path including the working directory and file name.
        string fullPath = string.Format("{0}/{1}.txt", directoryPath, this.HistoryFileName);

        //Check if history file exists and skip if not.
        if (File.Exists(fullPath) && Directory.Exists(directoryPath))
        {
            //Get all lines from the file at once. History values are new line delimited.
            string[] lines = File.ReadAllLines(fullPath);

            //Parse to the current position the first line read. If this fails log a warning,
            //as persistent history is not strictly required.
            bool parsedCorrectly = int.TryParse(lines[0], out this.CurrentPosition);
            if (!parsedCorrectly)
            {
                Debug.LogWarning("Warning: could not parse the saved position for this scene. " +
                "Could be due to corruption!");
            }

            //Do the same for the Visited array.
            for(int i = 0; i < this.Visited.Length; i++)
            {
                int curLine = i + 1;

                parsedCorrectly = bool.TryParse(lines[curLine], out this.Visited[i]);
                if (!parsedCorrectly)
                {
                    Debug.LogWarning(string.Format("Warning: could not parse the visited value at line number {0} " +
                        "for Visited bool number {1}. Could be due to corruption!", curLine, i));
                }
            }
        }
    }


    //Save history with a newline delimiter.
    private void SaveCurrentHistory()
    {
        //The path to the directory.
        string directoryPath = string.Format("{0}/{1}", Application.dataPath, this.HistoryPath);

        //The path including the working directory and file name.
        string fullPath = string.Format("{0}/{1}.txt", directoryPath, this.HistoryFileName);

        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        //Write the current position to the file with a newline delimiter. Overwrites file if it already exists.
        File.WriteAllText(fullPath, string.Format("{0}\n", this.CurrentPosition));

        //Write all Visited bools to the file with a newline delimiter. Only adds them to the file and deletes nothing.
        foreach (bool v in this.Visited) File.AppendAllText(fullPath, string.Format("{0}\n", v));
    }
}
