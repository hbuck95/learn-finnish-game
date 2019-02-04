using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml;

public class Scenario1 : MonoBehaviour {

    //Public Variables
    public GameObject allCharacterTiles, endGameScreen, characterInfo, introduction, helpScreen, introPanel, pregameButton;
    public GameObject[] characterTiles = new GameObject[10];
    public AudioClip[] alphabetSounds = new AudioClip[29];
    public Text timerValue, questionNo, correctAnswers, streakAmount, bestStreakAmount; //Text components used in game panel.
    public Text questionAns, correctAns, timeTaken, percentageOfAns; //Text components used on end game panel.
    public Text instructionText, keypressText, alphabetPosition, pronounciation, selectedChar;
    public Slider timerSlider;
    public Image sliderFill;
    public AudioClip correctSfx, clickSfx, wrongSfx;

    //Private Variables
    private bool _gamePaused, _gameOver, _gameStarted, _waitingForKeyPress;
    private float _timeLeft, _initialTime = 30f, _timeStarted;
    private int _correctAnswers, _incorrectAnswers, _streak, _question, _bestStreak;
    private List<char> _currentLetters = new List<char>();
    private List<string> _pronounciations = new List<string>();
    private char _letterToGuess, _lastLetter;
    private AudioSource _audioSource;

	// Use this for initialization
	private void Start () {
       // UserInfo.CompletePart(1);


        _audioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        keypressText.enabled = false;
        XmlDocument _doc = new XmlDocument();
        TextAsset asset = (TextAsset)Resources.Load("XML/Pronounciations", typeof(TextAsset));
        _doc.LoadXml(asset.text);

        foreach (XmlNode node in _doc.GetElementsByTagName("alphabet")) {
            foreach (XmlNode child in node.ChildNodes) {
                _pronounciations.Add(child.InnerText);
            }
        }

        StartCoroutine(GameInstructions());

    }

    // Update is called once per frame
    private void Update () {

        if (_waitingForKeyPress && Input.GetKeyDown(KeyCode.Space)) {
                _waitingForKeyPress = false;
                keypressText.enabled = false;
            Debug.Log("Key pressed!");     
        }

        if (_gamePaused || !_gameStarted)
            return;

        if (!_gameOver) {            
            if (_timeLeft > 0) {
                timerValue.text = _timeLeft.ToString("00.00");
                _timeLeft -= Time.deltaTime;
                timerSlider.value = _timeLeft;
                sliderFill.color = Color.Lerp(Color.red, Color.green, _timeLeft / _initialTime);
            } else {
                GameOver();
                Debug.Log("Game over!");
                timerValue.text = "00.00";
            }
        }

	}

    private IEnumerator GameInstructions() {
        List<string> instruction = new List<string>();
        var buttons = introduction.GetComponentsInChildren<Button>();

        foreach (var button in buttons) {
            button.enabled = false;
        }

        XmlDocument _doc = new XmlDocument();
        TextAsset asset = (TextAsset)Resources.Load("XML/Instructions", typeof(TextAsset));
        _doc.LoadXml(asset.text);

        foreach (XmlNode node in _doc.GetElementsByTagName(SceneManager.GetActiveScene().name)) {
            foreach (XmlNode child in node.ChildNodes) {
                instruction.Add(child.InnerText);
            }
        }

        yield return new WaitForSeconds(1);
        introPanel.SetActive(true);

        if (!UserInfo.CheckPartCompletion(1)) {
            for (int x = 0; x < instruction.Count; x++) {
                Debug.Log(instruction[x]);
                instructionText.text = instruction[x];
                yield return new WaitForSeconds(0.75f);
                Debug.Log("Waiting for key press...");
                _waitingForKeyPress = true;
                keypressText.enabled = true;
                yield return new WaitWhile(() => _waitingForKeyPress);
            }
        }

        instructionText.enabled = false;
        keypressText.enabled = false;
        _waitingForKeyPress = false;
        characterInfo.SetActive(true);
        introPanel.SetActive(false);
        pregameButton.SetActive(true);

        foreach (var button in buttons) {
            button.enabled = true; ;
        }
    }
 
    /// <summary>
    /// Called by the "Got it!" button on the help screen which initialises the game.
    /// </summary>
    public void GotIt() {
        helpScreen.SetActive(false);

        if (_gamePaused) {
            _gamePaused = false;
        } else {
            StartGame();
        }
    }

    /// <summary>
    /// Handles the end game scenario.
    /// - Disables the tile buttons and the replay sound button.
    /// </summary>
    private void GameOver() {
        _gameOver = true;
        GameObject.Find("PlaySoundButton").GetComponent<Button>().enabled = false;
        foreach (var tile in characterTiles) {
            tile.GetComponent<Button>().enabled = false;
        }

        //Debug.Log(string.Format("Answers correct: {0}\nPercent: {1}", _correctAnswers, Mathf.Round(_question/_correctAnswers)));
        //UserInfo.CompletePart(1);
        endGameScreen.SetActive(true);
        timeTaken.text = string.Format("Total Time Taken: {0} secs", (Time.time - _timeStarted).ToString("00.00"));
        questionAns.text = string.Format("Questions Answered: {0}", _question);
        correctAnswers.text = string.Format("Correct Answers: {0}", _correctAnswers);
        percentageOfAns.text = string.Format("{0}% of Questions Answered Correctly!", Mathf.Round(_question / _correctAnswers));// Mathf.Round(_correctAnswers/_question) * 100);
    }

    public void Prestart() {
        introduction.SetActive(false);
        helpScreen.SetActive(true);
    }

    /// <summary>
    /// Start the initial game.
    /// </summary>
    public void StartGame() {
        _timeLeft = _initialTime;
        allCharacterTiles.SetActive(true);
        timerSlider.interactable = false;
        timerSlider.maxValue = 30f;
        timerSlider.minValue = 0f;
        _gamePaused = false;
        _gameOver = false;
        _gameStarted = true;
        _timeStarted = Time.time;
        CreateTiles();
    }

    /// <summary>
    /// Convert a Char value to a integer with the same value as the alphabetSounds index location of the letter.
    /// </summary>
    /// <param name="c">The Char to convert.</param>
    /// <returns>Integer value of the character that corresponds to the characters audioclip index within alphabetSounds</returns>
    private int CharToInt(char c) {
        switch (c) {
            case 'Å':
                return 26;
            case 'Ä':
                return 27;
            case 'Ö':
                return 28;
            default:
                return char.ToLower(c) - 'a';
        }
    }

    /// <summary>
    /// Triggered via the character tile buttons this method is used to evaluate a players answer.
    /// </summary>
    /// <param name="tileNumber">The list index of the tile which is the same as the list index of the character shown on the tile.</param>
    public void Answer(int tileNumber) {
        bool correct = false;

        if(_currentLetters[tileNumber] == _letterToGuess) {
            Debug.Log("You guessed correctly!");
            _lastLetter = _letterToGuess;
            _correctAnswers++;
            _streak++;
            _timeLeft += 2f;
            correct = true;
            _audioSource.PlayOneShot(correctSfx, .15f);
        } else {
            Debug.Log(string.Format("Wrong answer! The letter to guess was '{0}' and you guessed '{1}'!", _letterToGuess, _currentLetters[tileNumber]));
            _incorrectAnswers++;
            _streak = 0;
            _timeLeft -= 1;
            _audioSource.PlayOneShot(wrongSfx, .15f);
        }

        if (_streak > _bestStreak) {
            _bestStreak = _streak;
            bestStreakAmount.text = string.Format("Highest Streak: {0}", _bestStreak);
        }

        streakAmount.text = string.Format("Current Streak: {0}", _streak);
        correctAnswers.text = string.Format("Correct Answers: {0}", _correctAnswers);
        StartCoroutine(_ShowAnswer(correct, characterTiles[tileNumber]));
    }


    private IEnumerator _ShowAnswer(bool correct, GameObject g) {
        Image cTile = characterTiles.Single(x => char.Parse(x.GetComponentInChildren<Text>().text) == _letterToGuess).GetComponent<Image>();
        Image wTile = correct == true ? null : g.GetComponent<Image>();
        Color def = cTile.color;
        int j = correct == true ? 1 : 2;

        if (!correct)
            wTile.color = Color.red;

        for (int i = 0; i < j; i++) {
            cTile.color = Color.green;
            yield return new WaitForSeconds(.2f);
            cTile.color = def;
            if(!correct)
                yield return new WaitForSeconds(.2f);
        }

        if (!correct)
            wTile.color = def;

       // yield return new WaitWhile(()=>_audioSource.isPlaying);
        CreateTiles();
    }

    /// <summary>
    /// Setup the letter tiles for the turn.
    /// </summary>
    private void CreateTiles() {
        _currentLetters.Clear();

        for (int i = 0; i < characterTiles.Length; i++) {
            char c = FinnishAlphabet.RandomCharacter();
            if (_currentLetters.Contains(c) || _lastLetter == c) {
              //  Debug.Log(string.Format("{0} is either the current letter or is currently in the tile list. Trying again.", c));
                i--;
            } else {
                _currentLetters.Add(c);
            }
        }

        //Shuffle the letters so the first tile is not always the correct answer!
        var rng = new System.Random();
        _currentLetters.Sort((x, y) => rng.Next(0, 1));

        _letterToGuess = _currentLetters[UnityEngine.Random.Range(0, 10)];
        Debug.Log(string.Format("The letter to guess this turn is '{0}'.", _letterToGuess));
        PlaySound();

        //Update the tile characters.
        for (int i = 0; i < _currentLetters.Count; i++) {
            characterTiles[i].GetComponentInChildren<Text>().text = _currentLetters[i].ToString();
        }

        _question++;
        questionNo.text = _question.ToString();
    }

    /// <summary>
    /// Select a random uppercase character from the Finnish alphabet (not counting letters used within loan words).
    /// Use a random integer between 0 and 28 where 0 - 25 are used to select a random english alphabet character using char integer values.
    /// 26, 27, & 28 are then switched to their Finnish varients.
    /// </summary>
    /// <returns>A random Finnish letter based on the seed.</returns>
    //private char RandomCharacter()
    //{
    //    int seed = Random.Range(0, 29);
    //    switch (seed)
    //    {
    //        case 26:
    //            return 'Å';
    //        case 27:
    //            return 'Ä';
    //        case 28:
    //            return 'Ö';
    //        default:
    //            return char.ToUpper((char)('a' + seed));
    //    }
    //}

    /// <summary>
    /// Toggle the help screen as well as pause the game.
    /// </summary>
    public void ToggleHelp() {
        helpScreen.SetActive(!helpScreen.activeSelf);
        Debug.Log("Toggling help screen.");
        _gamePaused = helpScreen.activeSelf ? true : false;
    }

    /// <summary>
    /// Handles the button click to return to the main menu.
    /// </summary>
    public void MainMenu() {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    /// <summary>
    /// Handles the button click scenario for playing/replaying the sound for the current letter.
    /// </summary>
    public void PlaySound() {
        _audioSource.Stop();
        _audioSource.PlayOneShot(alphabetSounds[CharToInt(_letterToGuess)]);
    }

    public void AlphabetIntro(int letter) {
        _audioSource.Stop();
        _audioSource.PlayOneShot(alphabetSounds[letter]);
        alphabetPosition.text = string.Format("The {0}{1} letter in the Finnish Alphabet", letter+1, Nth(letter+1));
        pronounciation.text = string.Format("Pronounciation: {0}", _pronounciations[letter]);
        selectedChar.text = FinnishAlphabet.GetCharacter(letter).ToString();
        Debug.Log(_pronounciations[letter]);
    }

    /// <summary>
    /// Proceed to the next level without going to the main menu.
    /// </summary>
    public void NextLevel() {
        SceneManager.LoadSceneAsync("Part_2");
    }

    private string Nth(int x) {
        int i = x.ToString().Length == 1 ? x : int.Parse(x.ToString().Last().ToString());

        if(i == 3 && x != 13) {
            return "rd";
        } else if (i == 2 && x != 12) {
            return "nd";
        } else if (i == 1 && x != 11) {
            return "st";
        } else {
            return "th";
        }
            
    }

}
