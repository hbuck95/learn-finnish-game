using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Xml;
using System;

public class Scenario2 : MonoBehaviour {
    //Private variables
    private List<char> _selectedTiles = new List<char>();
    private Dictionary<string, int> _dictionary = new Dictionary<string, int>();
    private List<GameObject> _spawnedTiles = new List<GameObject>();
    private string _currentWord, _currentTranslation;
    private KeyValuePair<string, int> _current;
    private char[] _currentWordChars, _selectedChars;
    private WordSet _currentSet;
    private List<WordSet> _unusedSets = new List<WordSet>();
    private Vector2 _tileSpawnLoc = new Vector2(-8.6f, -1f);
    private int _lettersFound = 0;//, _currentWordIndex;
    private bool _waitingForKeyPress;
    private AudioSource _audioSource;
    private float _time;

    //Public variables
    public Text selected, currentWordText_FI, currentWordText_EN, questionMark, instructionText, keyPressText, currentWordText_Found, endSet_timeTaken, currentset_text;
    public GameObject tilePrefab, introPanel, newWordButton, lettersFound, endSetPanel, rulesPanel;
    public Sprite[] fruitSprites = new Sprite[16];
    public Sprite[] animalSprites = new Sprite[16];
    public Color32[] colours = new Color32[10];
    public Image currentWordImage, currentWordImageDupe, colourComplete, animalComplete, fruitComplete;
    [HideInInspector]
    public GameState gameState;
    public AudioClip[] fruitPronounciationsFI = new AudioClip[10];
    public AudioClip[] fruitPronounciationsEN = new AudioClip[10];
    public AudioClip[] colourPronounciationsFI = new AudioClip[10];
    public AudioClip[] colourPronounciationsEN = new AudioClip[10];
    public AudioClip[] animalPronounciationsFI = new AudioClip[10];
    public AudioClip[] animalPronounciationsEN = new AudioClip[10];
    public Button englishAudio;
    public AudioClip correctSfx, clickSfx, wrongSfx;


    public enum WordSet {
        Fruit = 1,
        Colours = 2,
        Animals = 3
    }

    public enum GameState {
        Active = 1,
        Paused = 2,
        Over = 3,
        WaitingToStart = 4
    }

    public enum Language    {
        Finnish = 1,
        English = 2
    }

    private void Start() {
      // UserInfo.CompletePart(2);
        _unusedSets.Add(WordSet.Fruit);
        _unusedSets.Add(WordSet.Animals);
        _unusedSets.Add(WordSet.Colours);
        _audioSource = GetComponent<AudioSource>();     
        StartCoroutine(_GameInstructions());
    }

    public void PauseGame() {
        gameState = GameState.Paused;
        Time.timeScale = 0;
    }

    public void ResumeGame() {
        gameState = GameState.Active;
        Time.timeScale = 1;
    }

    private void Update() {
        //if (Input.GetKeyDown(KeyCode.H)) {
        //    gameState = gameState == GameState.Active ? GameState.Paused : GameState.Active;
        //    Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        //}

        if (Input.GetKeyDown(KeyCode.Space)) {
            if(_waitingForKeyPress && gameState == GameState.WaitingToStart) {
                introPanel.SetActive(false);
                _waitingForKeyPress = false;
                StartGame();
            } else if (_waitingForKeyPress) {
                _waitingForKeyPress = false;
                keyPressText.enabled = false;
                Debug.Log("Key pressed!");
            }
        }

        if(gameState == GameState.Active) {
            _time += Time.deltaTime;
        }

    }

    private void _EndSet() {
        gameState = GameState.Over;
        endSetPanel.SetActive(true);
        TimeSpan time = TimeSpan.FromSeconds(_time);
        endSet_timeTaken.text = string.Format("{0}:{1}.{2}", time.Minutes.ToString("00"), time.Seconds.ToString("00"), time.Milliseconds.ToString("00"));
        
        foreach (WordSet set in (WordSet[])Enum.GetValues(typeof(WordSet))) {
            if (!_unusedSets.Contains(set)) {
                switch (set) {
                    case WordSet.Animals:
                        animalComplete.enabled = true;
                        break;
                    case WordSet.Colours:
                        colourComplete.enabled = true;
                        break;
                    case WordSet.Fruit:
                        fruitComplete.enabled = true;
                        break;
                }
            }
        }

    }

    private Sprite GetWordSprite(int index) {
        return (int)_currentSet == 1 ? fruitSprites[index] : animalSprites[index];
    }

    private void StartGame(){
        //_currentSet = WordSet.Fruit;
        NewSet();
        LoadWords();
        NewWord();
        gameState = GameState.Active;
    }

    private void NewSet() {
        //_currentSet = WordSet.Animals;
        _currentSet = RandomWordSet();
        _unusedSets.Remove(_currentSet);
        Debug.Log(string.Format("The current set is {0}", _currentSet));
        _currentWord = "";
        currentset_text.text = string.Format("Current Set: {0}", _currentSet);
    }

    private void NewWord() {
        englishAudio.gameObject.SetActive(false);
        newWordButton.SetActive(false);
        GetWord();
        questionMark.text = "?";
        currentWordText_EN.text = "";
        currentWordImage.sprite = null;
        currentWordImageDupe.sprite = null;
        currentWordImage.gameObject.SetActive(false);
        currentWordImageDupe.gameObject.SetActive(false);
        currentWordText_FI.text = FinnishAlphabet.Niceify(_currentWord);
        _currentWordChars = _currentWord.ToCharArray();
        _selectedChars = new char[_currentWord.Length];
        selected.text = "";
        _lettersFound = 0;

        for (int i = 0; i < _selectedChars.Length; i++) {
            _selectedChars[i] = '_';
            selected.text += "_";
        }

        lettersFound.SetActive(true);

        for (int i = 0; i < _currentTranslation.Length; i++) {
            currentWordText_EN.text += "_ ";
        }


        List<char> tileChars = new List<char>();
        for (int i = 0; i <= 25; i++) {
            tileChars.Add(i < _currentWordChars.Length ? _currentWordChars[i] : FinnishAlphabet.RandomCharacter());
        }

        //Shuffle the letters so the first tile is not always the correct answer!
        var rng = new System.Random();
        tileChars.Sort((x, y) => rng.Next(0, 1));       

        Vector2 spawnRef = _tileSpawnLoc;      
        foreach (char c in tileChars) {  
            CreateTile(c, spawnRef);
            spawnRef.x += 0.75f;
        }

        Debug.Log(string.Format("The current word is {0}. ({1})", _currentWord, _currentTranslation));

    }

    private WordSet RandomWordSet() {
        return _unusedSets[new System.Random().Next(0, _unusedSets.Count)];
    }

    private void GetWord() {
      /*  if (_dictionary.Count == 0) {
            Debug.Log("new set!");
            NewSet();
            LoadWords();
        }*/

        int seed = new System.Random().Next(1, _dictionary.Count) - 1;
        //Finnish words are found on the even index in the xml document.
        //Their English translations are found on the Finnish-1 even index.

        if (seed % 2 != 0) {
            seed--;
        }

        string word = _dictionary.ElementAt(seed).Key;
        _current = _dictionary.ElementAt(seed);
        _currentWord = word;
        _currentTranslation = _dictionary.ElementAt(seed + 1).Key;
        _dictionary.Remove(_currentWord);
        _dictionary.Remove(_currentTranslation);


    }

    private void LoadWords() {
        _dictionary.Clear();

        XmlDocument _doc = new XmlDocument();
        TextAsset asset = (TextAsset)Resources.Load("XML/Wordsets", typeof(TextAsset));
        _doc.LoadXml(asset.text);

        XmlNodeList nodes = _doc.GetElementsByTagName(_currentSet.ToString());

        for (int i = 0; i < nodes.Count; i++) {
            var childNodes = nodes[i].ChildNodes;
            int x = 0;
            for (int j = 0; j < childNodes.Count;  j+=2) {
                _dictionary.Add(childNodes[j].InnerText, x);
                _dictionary.Add(childNodes[j + 1].InnerText, x);
                x++;
            }

        }
    /*   foreach (XmlNode node in _doc.GetElementsByTagName(_currentSet.ToString())) {
            foreach (XmlNode child in node.ChildNodes) {
                _dictionary.Add(child.InnerText.ToUpper(), false);
            }
        }*/
    }

    public void CreateTile(char letter, Vector2 spawn) {
        GameObject tile = Instantiate(tilePrefab, spawn, Quaternion.identity) as GameObject;
        Rigidbody2D rb = tile.GetComponent<Rigidbody2D>();
        tile.GetComponent<TextMesh>().text = letter.ToString();
        tile.name = letter.ToString();
        rb.isKinematic = false;
        rb.AddRelativeForce(UnityEngine.Random.onUnitSphere * 0.05f);
        _spawnedTiles.Add(tile);
    }

    private void FoundWord() {
        englishAudio.gameObject.SetActive(true);
        questionMark.text = "";
        currentWordImage.gameObject.SetActive(true);
        if (_currentSet != WordSet.Colours) {
            currentWordImageDupe.gameObject.SetActive(true);
            currentWordImage.sprite = GetWordSprite(_current.Value);
            currentWordImageDupe.sprite = GetWordSprite(_current.Value);
        } else {
            currentWordImage.GetComponent<Image>().color = colours[_current.Value];
        }
        Debug.Log(_dictionary.Count);
        //currentWordText_EN.text = FinnishAlphabet.Niceify(_currentTranslation);

        foreach (GameObject g in _spawnedTiles) {
            Destroy(g);         
        }

        _spawnedTiles.Clear();
        StartCoroutine(_DisplayTranslation());    
    }

    public void ClickTile(GameObject g) {
        char c = char.Parse(g.name);
        char next = _currentWordChars.First(x => x != '#');

        if(c != next) {
            Debug.Log(string.Format("{0} is not {1}!", c, next));
            _audioSource.PlayOneShot(wrongSfx, 0.15f);
            return;
        }

        if (!_currentWordChars.Contains(c)) {
            Debug.Log(string.Format("{0} is not a character in {1}", c, _currentWord));
            _audioSource.PlayOneShot(wrongSfx, 0.15f);
            return;
        }

        int i = Array.FindIndex(_currentWordChars, x => x == c);//Find the index position of the selected character in the current word.
        _currentWordChars[i] = '#';//set it to a char that is never used to avoid null references.
        _selectedChars[i] = c;
        selected.text = "";
        _lettersFound++;
        _audioSource.PlayOneShot(correctSfx, 0.15f);

        StringBuilder sb = new StringBuilder();
        foreach (var letter in _selectedChars) {
            //We want to use "__" rather than "_" to increase the space taken up.
            sb.Append(string.Format("{0}", letter == '_' ? "_" : letter.ToString()));
        }

        selected.text = FinnishAlphabet.Niceify(sb.ToString());

        Destroy(g);

        if(_currentWordChars.Count() == _lettersFound) {
            selected.text = "You found the word!";
            // Time.timeScale = 0;
            FoundWord();
        }

        return;
    }

    private IEnumerator _DisplayTranslation() {
        char[] letters = FinnishAlphabet.Niceify(_currentTranslation).ToCharArray();
        char[] finLetters = FinnishAlphabet.Niceify(_currentWord).ToCharArray();
        StringBuilder sb = new StringBuilder();

        currentWordText_FI.color = Color.green;

        PlayWord(1);
        for (int i = finLetters.Length; i > 0; i--) {
            currentWordText_FI.text = "";
            for (int j = finLetters.Length - i; j < (finLetters.Length - i) + 1; j++)
            {
                Debug.Log(j);
                sb.Append(finLetters[j]);
            }
            currentWordText_FI.text = sb.ToString();
            yield return new WaitForSeconds(0.2f);
        }

        currentWordText_FI.color = Color.white;
        sb.Length = 0;
        yield return new WaitForSeconds(0.5f);
        currentWordText_EN.color = Color.green;
        PlayWord(0);
        for (int i = letters.Length; i > 0; i--) {
            currentWordText_EN.text = "";
            for (int j = letters.Length - i; j < (letters.Length - i) + 1; j++) {
                Debug.Log(j);
                sb.Append(letters[j]);
            }
            currentWordText_EN.text = sb.ToString();
            yield return new WaitForSeconds(0.2f);
        }
        currentWordText_EN.color = Color.white;
        
        yield return new WaitWhile(() => _audioSource.isPlaying);

        if (_dictionary.Count == 0) {
            _EndSet();
            yield break;
        }
        newWordButton.SetActive(true);
    }

    /// <summary>
    /// Play the pronounciation of the word according to the input parameter. 1 = Finnish, 2 = English
    /// </summary>
    /// <param name="language"></param>
    public void PlayWord(int language) {
        AudioClip[] audio = null;
        switch (_currentSet) {
            case WordSet.Animals:
                audio = language == (int)Language.Finnish ? animalPronounciationsFI : animalPronounciationsEN;
                break;
            case WordSet.Colours:
                audio = language == (int)Language.Finnish ? colourPronounciationsFI : colourPronounciationsEN;
                break;
            case WordSet.Fruit:
                audio = language == (int)Language.Finnish ? fruitPronounciationsFI : fruitPronounciationsEN;
                break;
        }

        _audioSource.Stop();
        _audioSource.PlayOneShot(audio[_current.Value]);
        Debug.Log(string.Format("Playing {1} audio at index {0}", _current.Value, (Language)language));
    }

    private IEnumerator _GameInstructions() {
        List<string> instruction = new List<string>();
        XmlDocument _doc = new XmlDocument();
        TextAsset asset = (TextAsset)Resources.Load("XML/Instructions", typeof(TextAsset));
        _doc.LoadXml(asset.text);

        if (!UserInfo.CheckPartCompletion(2)) {
            foreach (XmlNode node in _doc.GetElementsByTagName(SceneManager.GetActiveScene().name)) {
                foreach (XmlNode child in node.ChildNodes) {
                    instruction.Add(child.InnerText);
                }
            }
   
            yield return new WaitForSeconds(1);
            introPanel.SetActive(true);

            for (int x = 0; x < instruction.Count; x++) {
                Debug.Log(instruction[x]);
                instructionText.text = instruction[x];
                yield return new WaitForSeconds(0.75f);
                Debug.Log("Waiting for key press...");
                _waitingForKeyPress = true;
                keyPressText.enabled = true;
                yield return new WaitWhile(() => _waitingForKeyPress);
            }
            instructionText.text = "Press space to start the game!";
            gameState = GameState.WaitingToStart;
            keyPressText.text = "Press [Space] to start the game!";
            _waitingForKeyPress = true;
        } else {
            StartGame();
        }
    }

    public void NewWord_Button() {
        NewWord();
    }

    public void NextSet_Button() {
        gameState = GameState.Active;
        endSetPanel.SetActive(false);
        NewSet();
        LoadWords();
        _time = 0f;
        NewWord();
    }

    public void GotIt_Button() {
        rulesPanel.SetActive(false);
        ResumeGame();
    }

    public void ToggleHelp_Button() {
        rulesPanel.SetActive(!rulesPanel.activeSelf);
        if(gameState == GameState.Paused) {
            ResumeGame();
        } else if (gameState == GameState.Active) {
            PauseGame();
        }
    }

    public void Back_Button() {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void Replay_Button()
    {
        Debug.Log("click!");
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel_Button() {
        SceneManager.LoadSceneAsync("Part_3");
    }

}