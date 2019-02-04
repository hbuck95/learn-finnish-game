using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class Scenario3 : MonoBehaviour {
    public GameObject imageCardPrefab, textCardPrefab, introPanel, endGame, rulesPanel;
    public Sprite[] fruit, animals = new Sprite[10];
    public Sprite square;
    public List<Transform> spawnLocs = new List<Transform>();
    public Text timerText, matches, instructionText, keyPressText, scoreText, prevTimeText, timeText, finalScoreText, currentTime, streakText, bestTime, message;
    private int _matchesMade, _setId, _streak, _score;
    private Dictionary<string, string> _dictionary = new Dictionary<string, string>();
    private List<GameObject> spawnedItem = new List<GameObject>();
    private GameState _state;
    private float _timer;
    private bool _waitingForKeyPress;
    private float _prevTime, _bestTime;
    private AudioSource _audiosource;
    public AudioClip correctSfx, wrongSfx;

	private void Start () {
        _audiosource = GetComponent<AudioSource>();
        _state = GameState.InActive;
        StartCoroutine(_GameInstructions());
        _prevTime = PlayerPrefs.HasKey("PreviousTime") ? PlayerPrefs.GetFloat("PreviousTime") : -1f;
        _bestTime = PlayerPrefs.HasKey("BestTime") ? PlayerPrefs.GetFloat("BestTime") : -1f;
        TimeSpan t = TimeSpan.FromSeconds(_bestTime);
        bestTime.text = _bestTime == -1f ? "<b>Best Time:</b>  --:--" : string.Format("<b>Best Time:</b>  {0}:{1}.{2}", t.Minutes.ToString("00"), t.Seconds.ToString("00"), t.Milliseconds.ToString("0"));
    }

    private void StartGame() {
        _setId = 0;
        LoadWords();
        CreateCards();
        _state = GameState.Active;
    }

    private void PlaySound(AudioClip sfx) {
        _audiosource.Stop();
        _audiosource.clip = null;
        _audiosource.PlayOneShot(sfx, 0.15f);
    }
	
	private void Update () {
        if (_state == GameState.Paused)
            return;

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (_waitingForKeyPress && _state == GameState.WaitingToStart) {
                introPanel.SetActive(false);
                _waitingForKeyPress = false;
                StartGame();
            } else if (_waitingForKeyPress) {
                _waitingForKeyPress = false;
                keyPressText.enabled = false;
                Debug.Log("Key pressed!");
            }
        }

        if (_state == GameState.Active) {
            _timer += Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(_timer);
            timerText.text = string.Format("{0}:{1}.{2}", t.Minutes.ToString("00"), t.Seconds.ToString("00"), t.Milliseconds.ToString("0"));
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            foreach (var item in spawnedItem) {
                Destroy(item);
            }
            _setId++;
            _dictionary.Clear();
            LoadWords();
            CreateCards();
        }
	}

    private void NextSet() {
        if (_setId == 2) {
            EndGame();
            PlayerPrefs.SetFloat("PreviousTime", _timer);           
            return;
        }
        _setId++;
        CreateCards();
    }

    private enum GameState {
        Paused = 0,
        Active = 1,
        InActive = 2,
        WaitingToStart = 4
    }

    private void EndGame() {
        _state = GameState.InActive;
        endGame.SetActive(true);
        TimeSpan timerTime = TimeSpan.FromSeconds(_timer);
        TimeSpan previousTime = TimeSpan.FromSeconds(Convert.ToDouble(_prevTime));
        TimeSpan bestTimeTime = TimeSpan.FromSeconds(_bestTime);
        prevTimeText.text = "--:--";    
        currentTime.text = string.Format("{0}:{1}.{2}", timerTime.Minutes.ToString("00"), timerTime.Seconds.ToString("00"), timerTime.Milliseconds.ToString("0"));
        streakText.text = _streak.ToString();
        finalScoreText.text = _score.ToString();
        if(_prevTime != -1) {
            prevTimeText.text = string.Format("{0}:{1}.{2}", previousTime.Minutes.ToString("00"), previousTime.Seconds.ToString("00"), previousTime.Milliseconds.ToString("0"));
        }

        if (_timer > _bestTime) {
            Debug.Log("New best time!");
            PlayerPrefs.SetFloat("BestTime", _timer);
            _bestTime = _timer;
            bestTime.text = string.Format("<b>Best Time:</b>  {0}:{1}.{2}", timerTime.Minutes.ToString("00"), timerTime.Seconds.ToString("00"), timerTime.Milliseconds.ToString("0"));
            message.text = "Congratulations! You set a new best time!";
        } else if (_timer < _prevTime){
            string s = string.Format("You managed to beat your previous time! You were {0} seconds faster!", bestTimeTime - timerTime);
            Debug.Log(s);
            message.text = s;
        } else if (_timer > _prevTime) {
            TimeSpan dif = timerTime.Subtract(previousTime);
            string difVal = string.Format("{0}{1}", dif.Minutes == 0 ? "" : string.Format("{0} minutes ", dif.Minutes.ToString()), string.Format("{0}.{1} seconds", dif.Seconds.ToString(), dif.Milliseconds.ToString("00")));
            string s = string.Format("You didn't beat your time! You were {0} slower", difVal);
            Debug.Log(s);
            message.text = s;
        }

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
            _state = GameState.WaitingToStart;
            keyPressText.text = "Press [Space] to start the game!";
            _waitingForKeyPress = true;
        } else {
            StartGame();
        }
    }

    private List<Transform> ShuffleList(List<Transform> list) {
        var _random = new System.Random();
        Transform trans;
    
        int n = list.Count;
        for (int i = 0; i < n; i++) {
            int r = i + (int)(_random.NextDouble() * (n - i));
            trans = list[r];
            list[r] = list[i];
            list[i] = trans;
        }

        return list;
    }

    private Sprite GetSprite(string name) {
        switch (name) {
            case "orange-fruit"://Had ti rename this due to the ambiguity between the colour 'orange' and the fruit 'orange'.
                return GetFruitSprite("orange");
            case "apple":      
            case "pear":
            case "pineapple":
            case "blueberry":
            case "peach":
            case "strawberry":
            case "cherry":
            case "coconut":
            case "grapes":
                return GetFruitSprite(name);
            case "cat":
            case "dog":
            case "cow":
            case "rabbit":
            case "horse":
            case "fish":
            case "snail":
            case "snake":
            case "spider":
            case "duck":
                return GetAnimalSprite(name);
            default:
                return null;
        }
    }

    private Sprite GetAnimalSprite(string animalName) {
        switch (animalName) {
            case "cat":
                return animals[0];
            case "dog":
                return animals[1];
            case "spider":
                return animals[2];
            case "fish":
                return animals[3];
            case "snake":
                return animals[4];
            case "snail":
                return animals[5];
            case "horse":
                return animals[6];
            case "cow":
                return animals[7];
            case "rabbit":
                return animals[8];
            case "duck":
                return animals[9];
            default:
                return null;
        }
    }

    private Sprite GetFruitSprite(string fruitName) {
        switch (fruitName) {
            case "apple":
                return fruit[9];
            case "orange":
                return fruit[3];
            case "pear":
                return fruit[4];
            case "pineapple":
                return fruit[5];
            case "blueberry":
                return fruit[7];
            case "peach":
                return fruit[1];
            case "strawberry":
                return fruit[2];
            case "cherry":
                return fruit[8];
            case "coconut":
                return fruit[0];
            case "grapes":
                return fruit[6];
            default:
                return null;
        }
    }


    private Color32 GetColour(string colour) {
        switch (colour) {
            case "red":
                return new Color32(255, 0, 0, 255);
            case "blue":
                return new Color32(0, 0, 255, 255);
            case "green":
                return new Color32(0, 255, 0, 255);
            case "pink":
                return new Color32(255, 137, 255, 255);
            case "brown":
                return new Color32(159, 116, 59, 255);
            case "white":
                return new Color32(255, 255, 255, 255);
            case "black":
                return new Color32(0, 0, 0, 255);
            case "purple":
                return new Color32(148, 75, 148, 255);
            case "orange":
                return new Color32(255, 164, 0, 255);
            case "yellow":
                return new Color32(255,255,0,255);
            default:
                return new Color32(255, 255, 255, 255);
        }
    }

    private void CreateCards() {
        var rng = new System.Random();
        spawnLocs = ShuffleList(spawnLocs);
        spawnLocs.Sort((x, y) => rng.Next(0, 1));
        int index = 0;
        for (int i = 0; i < 20; i+=2) {
            GameObject imageCard = Instantiate(imageCardPrefab, spawnLocs[i].position, Quaternion.identity) as GameObject;
            GameObject textCard = Instantiate(textCardPrefab, spawnLocs[i + 1].position, Quaternion.identity) as GameObject;
            KeyValuePair<string, string> item = _dictionary.ElementAt(index);
            Sprite s = GetSprite(item.Value.ToLower());
            SpriteRenderer rend = imageCard.GetComponentInChildren<SpriteRenderer>();
            var c = s == null ? GetColour(item.Value.ToLower()) : new Color32(255,255,255,255);

            //Make changes to the cards.
            textCard.GetComponent<TextMesh>().text = FinnishAlphabet.Niceify(item.Key);
            imageCard.GetComponentInChildren<TextMesh>().text = item.Value == "ORANGE-FRUIT" ? "Orange" : FinnishAlphabet.Niceify(item.Value);
            rend.sprite = s == null ? square : s;
            rend.color = c;

            if(s == null)
                rend.transform.localScale = new Vector3(1, 1);
  
            textCard.name = item.Value.ToLower();
            imageCard.name = item.Value.ToLower();
            index++;
            spawnedItem.Add(imageCard);
            spawnedItem.Add(textCard);
        }

        for (int i = 0; i < 10; i++) {
            _dictionary.Remove(_dictionary.ElementAt(0).Key);
        }
        Debug.Log(_dictionary.Count);

    }

    private void LoadWords() {
        _dictionary.Clear();

        XmlDocument _doc = new XmlDocument();
        TextAsset asset = (TextAsset)Resources.Load("XML/Part3Sets", typeof(TextAsset));
        _doc.LoadXml(asset.text);

        XmlNodeList[] listOfNodes = new XmlNodeList[3];

        for (int i = 0; i < 3; i++) {
            listOfNodes[i] = _doc.GetElementsByTagName(string.Format("Set_{0}", i));
        }

        //XmlNodeList nodes = _doc.GetElementsByTagName(string.Format("Set_{0}",_setId));

        foreach (var nodes in listOfNodes){
            for (int i = 0; i < nodes.Count; i++) {
                var childNodes = nodes[i].ChildNodes;
                for (int j = 0; j < childNodes.Count; j += 2) {
                    _dictionary.Add(childNodes[j].InnerText, childNodes[j+1].InnerText);
                }
            }
        }

        var rng = new System.Random();
        _dictionary = _dictionary.OrderBy(x => rng.Next()).ToDictionary(x => x.Key, y => y.Value);
        Debug.Log(_dictionary.Count);

    }

    private void _IncorrectMatch() {
        _streak = 0;
        Debug.Log("Incorrect match made! Resetting your streak.");
        PlaySound(wrongSfx);
    }

    private void _MatchMade(GameObject[] match) {
        _matchesMade++;
        _streak++;
        var a = 100 + (_streak * 10);
        _score += a;

        if (_streak % 5 == 0) {
            _score += 100;
            Debug.Log(string.Format("Streakin' Bonus!\nStreak: {0}\n+100 Points", _streak));
        }

        Debug.Log("Score: " + _score);
        spawnedItem.Remove(match[0]);
        spawnedItem.Remove(match[1]);
        Destroy(match[0]);
        Destroy(match[1]);
        Debug.Log(string.Format("You made a match!\nMatches made: {0}\nStreak: {1}", _matchesMade, _streak));
        matches.text = string.Format("{0} Matches", _matchesMade);
        scoreText.text = string.Format("Score: {0}", _score);
        PlaySound(correctSfx);
        
        if(spawnedItem.Count == 0)
            NextSet();
     
    }

    public void MainMenu_Button() {
        SceneManager.LoadSceneAsync(0);
    }

    public void Replay_Button() {
        Debug.Log("click!");
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame() {
        _state = GameState.Paused;
        Time.timeScale = 0;
    }

    public void ResumeGame() {
        _state = GameState.Active;
        Time.timeScale = 1;
    }

    public void GotIt_Button() {
        rulesPanel.SetActive(false);
        ResumeGame();
    }

    public void ToggleHelp_Button() {
        rulesPanel.SetActive(!rulesPanel.activeSelf);
        if (_state == GameState.Paused) {
            ResumeGame();
        } else if (_state == GameState.Active) {
            PauseGame();
        }
    }
}
