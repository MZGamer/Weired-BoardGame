using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;



public class UI_Manager : MonoBehaviour
{
    const int MAXCARD = 100;

    [Header("Hobby")]
    public GameObject lobby;
    public List<GameObject> hobbyPlayerNameDisplay;
    public List<TextMeshProUGUI> hobbyPlayerNameText;
    public TextMeshProUGUI IPInput;
    public TextMeshProUGUI PortInput;
    public TextMeshProUGUI PlayerNameInput;
    public GameObject gameStartButton;

    [Header("轉換動畫")]
    public Animator GameStartAni;
    public Animator newTurnAnimator;
    public TextMeshProUGUI billInd;
    public Animator nextPlayerAnimator;

    [Header("遊玩資料")]
    public int turn = -1;
    public static Queue<Package> animationQueue = new Queue<Package>();
    bool isBuyCard;

    [Header("玩家資料")]
    public List<PlayerStatus> players;
    public GameObject handCardSlot;
    public int playerID = 0;

    [Header("Player Slot")]
    public List<GameObject> playerSlot;
    public List<TextMeshProUGUI> playerName;
    public List<TextMeshProUGUI> playerMoney;
    public List<TextMeshProUGUI> playerHandCard;
    public Animator playerIndecatorAni;
    public PlayerIndecatorMotion playerIndecatorMot;
    public GameObject otherPlayerRoll;
    public TextMeshProUGUI otherPlayerRollPoint;
    public Animator otherPlayerRollAni;
    public bool anyOneIsRolling;

    [Header("NextPlayer Button")]
    public GameObject nextPlayerButon;

    [Header("Filed")]
    public List<UI_Filed> Filed;
    public GameObject FiledSelectInd;
    public bool FiledSelecting;

    [Header("CardWarning")]
    public UI_Card cardWarnig;
    public Animator cardWarnigControl;
    public List<GameObject> targetIndecater;
    public Animator targetIndecaterAni;

    [Header("Card_Description")]
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public TextMeshProUGUI cardDescription;
    public RectTransform cardDescriptionSize;

    [Header("Card_List")]
    public CardList cardList;

    [Header("FateRoll")]
    public Animator rollIndecatroAni;
    public TextMeshProUGUI rollResult;
    public Package cardIsRolling;
    public int thisTurnResult;

    [Header("UI_Status")]
    public bool viewCardDis;
    public bool selectingTarget;
    public bool harvestAsking;
    public bool selectCounter;
    public bool selectPlayer;
    public bool rolling;
    public bool waitForNewTurn;
    public bool closeSelectCounter;
    public GameObject CardisAsking;
    public GameObject PlayerSelectNotice;
    public int viewPlayerID;
    public int buttonState = 0;
    public static bool Animationing; 
    Coroutine networkStandbyFun;
    Coroutine TargetSelectFun;

    [Header("MultiTargetSelect")]
    public List<Button> hasSelectedButton;
    public bool isMultipleSelecting;

    [Header("CardSelecting")]
    public bool selectingCardToUse;
    public bool selectEventCard;
    public GameObject cardSelect;
    public Animator cardSelectAni;

    [Header("TopBar")]
    public GameObject EventNoti;
    public TextMeshProUGUI EventNotiText;
    public GameObject FateNoti;
    public TextMeshProUGUI FateNotiText;
    public Button buyCardButton;
    public TextMeshProUGUI turnText;

    

    [Header("Talk")]
    public TMP_InputField messageInput;
    public TextMeshProUGUI message;
    public static string talkMessage;
    public RectTransform messagebox;

    [Header("GameOver")]
    public GameObject GameOver;
    public TextMeshProUGUI WinText;

    [Header("Test")]
    public GameObject cardTemp;


    public int target;
    public List<int> targetGroup = new List<int>();


    void TestCase() {
        //players[playerID].farm[0].grow();
        
        FiledUpdate();
        GUIInit();
        playerSlotUpdate();
        /*
        getNewActionCard(100);
        getNewActionCard(101);

        cardWarningUI(100);
        */
        Package pkg = new Package(0, ACTION.ROLL_POINT, 300,new List<int>{0});
        animationQueue.Enqueue(pkg);
    }


    public void ConnectButtonClick() {
        NetworkManager.ip = IPInput.text.Substring(0, IPInput.text.Length - 1);
        string port = PortInput.text;

        port = port.Substring(0, port.Length - 1);
        int.TryParse(port, out NetworkManager.port);
        NetworkManager.connectSettingComplete = true;

    }

    // Start is called before the first frame update
    void Start()
    {
        turn = -1;
        Animationing = false;
        talkMessage = "";
        viewPlayerID = playerID;
        animationQueue.Clear();
        //TestCase();

    }
    public void BacktoMenuClick() {
        NetworkManager.gameOver = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)) {
            if(messageInput.text != "") {
                NetworkManager.messageToSend = string.Format("[{0}] : {1}\n", players[playerID].name, messageInput.text) ;
                messageInput.text = "";
            }
        }
        if(talkMessage != "") {
            message.text += talkMessage;
            double newHeight = 24 * message.GetTextInfo(message.text).lineCount;
            messagebox.sizeDelta = new Vector2(cardDescription.transform.localScale.x, (float)newHeight);
            talkMessage = "";
        }


        if (selectCounter && Input.GetKeyDown(KeyCode.Mouse1)) {
            closeSelectCounter = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
            closeSelecting();
        packageFilter();
        if(rolling) {
            if(Input.GetKeyDown(KeyCode.Mouse0)) {
                roll();
            }
        }

        if (turn % players.Count != playerID || players[playerID].money < 2 || players[playerID].handCard.Count >= 5) {
            buyCardButton.interactable = false;
        } else if (!isBuyCard) {
            buyCardButton.interactable = true;
        }
    }
    //GUI Update Function
    void GUIInit() {
        for(int i=0;i<4;i++) {
            playerSlot[i].SetActive(false);
        }
    }
    void playerSlotUpdate() {
        for(int i=0;i< players.Count; i++) {
            playerSlot[i].SetActive(true);
            playerName[i].text = players[i].name;
            playerMoney[i].text = "$: " + players[i].money;
            playerHandCard[i].text = "Card: " + players[i].handCard.Count;
        }
    }
    public void FiledUpdate() {
        for(int i=0;i<4;i++) {
            if (players[viewPlayerID].farm[i].ID == 0) {
                Filed[i].plantSprite.gameObject.SetActive(false);
                Filed[i].turnCounter.gameObject.SetActive(false);
                Filed[i].moneyIndecater.gameObject.SetActive(false);
            } else {
                corpCard corp = players[viewPlayerID].farm[i];
                Sprite corpImg = cardList.corpCardsList[players[viewPlayerID].farm[i].ID % MAXCARD].card.cardImg;

                Filed[i].plantSprite.sprite = corpImg;
                Filed[i].turnCounter.text = string.Format("Turn : {0}", corp.getTurn());
                Filed[i].moneyIndecater.text = string.Format("$ : {0}", corp.getReward());

                Filed[i].plantSprite.gameObject.SetActive(true);
                Filed[i].turnCounter.gameObject.SetActive(true);
                Filed[i].moneyIndecater.gameObject.SetActive(true);
            }
        }
    }

    void roll() {
        if (Animationing)
            return;
        rolling = false;
        int result = Random.Range(1, 6);
        cardIsRolling.index = result;
        cardIsRolling.src = playerID;
        NetworkManager.sendingQueue.Enqueue(cardIsRolling);
        cardIsRolling = null;
        rollResult.text = result.ToString();
        rollIndecatroAni.Play("RollIndecatorEnd");
        AniSim(1);
        
    }

    //First Step Button Click Event

    public void gameStartClick() {
        Package pkg = new Package(playerID, ACTION.GAMESTART, 0, 0);
        NetworkManager.sendingQueue.Enqueue(pkg);
    }

    public void filedClick(int filedID) {
        target = filedID;
        int corpCardID = players[viewPlayerID].farm[filedID].ID;
        if (Animationing)
            return;
        if(FiledSelecting) {
            targetGroup.Add(viewPlayerID);
            targetGroup.Add(filedID);
            selectingTarget = false;
            return;
        }
        if(buttonState>0) {
            closeSelecting();
        }
        if (corpCardID == 0) {
            if(playerID == viewPlayerID && playerID == turn % players.Count) {
                List<Card> corpCardList = new List<Card>();
            
                for (int i=0;i<cardList.corpCardsList.Count;i++) {
                    corpCardList.Add(cardList.corpCardsList[i].card);
                }
                generateCardSelectList(corpCardList);
            }

        } else {
            if (playerID == viewPlayerID && players[playerID].farm[filedID].getTurn() > 0 && playerID == turn % players.Count && players[playerID].effect[(int)EFFECT_ID.CLOSE_SHOP] != 1) {
                //closeSelecting();
                
                Filed[filedID].HarvestButton.SetActive(true);
                CardisAsking = EventSystem.current.currentSelectedGameObject;
                harvestAsking = true;
                buttonState = 1;
            }
            
            showCardDescription(corpCardID);
        }
    }

    public void cardClick() {
        var button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int cardID = int.Parse(button.name);
        if (!selectCounter && cardID >= 100 && cardID < 200 && cardList.actionCardsList[cardID % MAXCARD].card.Action == CARD_ACTION.DEFEND /*|| cardList.actionCardsList[cardID % MAXCARD].card.effect == EFFECT_ID.DEFEND*/) {
            showCardDescription(cardID);
            return;
        }

        if(isMultipleSelecting) {
            button.interactable = false;
            hasSelectedButton.Add(button);
            targetGroup.Add(cardID);
            selectingTarget = false;
            showCardDescription(cardID);
        }
        if (CardisAsking != button.gameObject) {
            if(!(buttonState == 0 && selectingCardToUse))
                closeSelecting();

            if (cardID < 300 && playerID == turn % players.Count || selectCounter) {
                buttonState = 1;
                CardisAsking = button.gameObject;
                if(CardisAsking.GetComponent<UI_Card>().UserAction.Count > 0)
                    CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(true);
            }
            showCardDescription(cardID);


        }
    }

    public void playerSlotClick(int slotID) {
        if(selectingTarget && selectPlayer) {
            var button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            targetGroup.Add(slotID);
            selectingTarget = false;
            if(isMultipleSelecting) {
                button.interactable = false;
                hasSelectedButton.Add(button);
            }
        } else {
            viewPlayerID = slotID;
            closeSelecting();
            if (selectingCardToUse) {
                closeSelecting();
            }
            FiledUpdate();
        }
    }

    public void eventOrFateCheckClick() {
        var button = EventSystem.current.currentSelectedGameObject;
        int cardID = int.Parse(button.name);
        if (cardID == 0)
            return;
        showCardDescription(cardID);
    }

    public void nextPlayerButtonClick() {
        var button = EventSystem.current.currentSelectedGameObject;
        button.gameObject.SetActive(false);
        StartCoroutine(editPackage(ACTION.NEXT_PLAYER));
    }

    public void buyCardClick() {
        if (turn % players.Count != playerID) {
            return;
        }
        isBuyCard = true;
        buyCardButton.interactable = false;
        StartCoroutine(editPackage(ACTION.GET_NEW_CARD));
    }

    //Card Selecting
    void generateCardSelectList(List<Card> card) {
        if (!selectingCardToUse) {
            selectingCardToUse = true;
            buttonState = 0;
            foreach (Transform child in cardSelect.transform) {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < card.Count; i++) {
                GameObject newCard = Instantiate(cardTemp, Vector3.zero, Quaternion.identity, cardSelect.transform);
                UI_Card CardInf = newCard.GetComponent<UI_Card>();
                newCard.GetComponent<Button>().onClick.AddListener(delegate { cardClick(); });

                newCard.name = card[i].ID.ToString();
                CardInf.CardName.text = card[i].Name;
                CardInf.CardImage.sprite = card[i].cardImg;
                int cardID = card[i].ID;
                CardInf.UserAction[0].GetComponent<Button>().onClick.AddListener(delegate { useCard(cardID); });
                switch (card[i].ID / MAXCARD) {
                    case 1:
                        CardInf.CardDescription.text = card[i].description;
                        break;
                    case 2:
                        CardInf.CardDescription.text = corpCardDesGenerater(card[i].ID);
                        break;
                }
                
            }
            cardSelect.SetActive(true);
            cardSelectAni.Play("CardSelectOn");
        }
    }

    void closeSelecting() {
        if(isMultipleSelecting) {
            if(targetGroup.Count>0) {
                targetGroup.RemoveAt(targetGroup.Count-1);
                hasSelectedButton[hasSelectedButton.Count - 1].interactable = true;
                hasSelectedButton.RemoveAt(hasSelectedButton.Count - 1);
            }
            return;
        }
        if (selectingCardToUse && buttonState == 0) {
            if(selectCounter && !closeSelectCounter) {
                return;
            }
            selectingCardToUse = false;
            cardSelectAni.Play("CardSelectOff");
            if (selectCounter) {
                selectCounter = false;
                closeSelectCounter = false;
                StartCoroutine(editPackage(ACTION.NULL));
            }
            
        }
        if(buttonState > 0) {
            if (TargetSelectFun != null) {
                if(networkStandbyFun != null)
                    StopCoroutine(networkStandbyFun);
                StopCoroutine(TargetSelectFun);
                PlayerSelectNotice.SetActive(false);
                FiledSelectInd.SetActive(false);
                TargetSelectFun = null;
                networkStandbyFun = null;
            }
            selectingTarget = false;
            if(harvestAsking) {
                CardisAsking.GetComponent<UI_Filed>().HarvestButton.SetActive(false);
                harvestAsking = false;
            } else {
                CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(false);
            }
            buttonState = 0;
            CardisAsking = null;
        }
    }

    //2 Stage Button Click Event
    public void useCard(int cardID) {
        if (selectCounter)
            selectCounter = false;
        networkStandbyFun = StartCoroutine(editPackage(ACTION.CARD_ACTIVE, cardID));
        EventSystem.current.currentSelectedGameObject.SetActive(false);

    }

    public void harvestClick(int filedID) {
        networkStandbyFun = StartCoroutine(editPackage(ACTION.HARVEST, filedID));
        EventSystem.current.currentSelectedGameObject.SetActive(false);
        harvestAsking = false;
    }

    IEnumerator editPackage(ACTION action, int index = -1) {
        int target = 0;
        targetGroup.Clear();
        switch (action) {
            case ACTION.CARD_ACTIVE:
                int cardID = index;
                switch (cardID / MAXCARD) {
                    case 1:
                        target = cardList.actionCardsList[cardID % MAXCARD].card.target;
                        actionCard card = cardList.actionCardsList[cardID % MAXCARD].card;
                        if (card.selectType.targetCount > 1) {
                            isMultipleSelecting = true;
                            hasSelectedButton.Clear();
                        }

                        if (target == -1 && card.selectType.type == SELECT_TYPE.PLAYER) {
                            PlayerSelectNotice.SetActive(true);
                            selectPlayer = true;
                            for (int i = 0; i < card.selectType.targetCount; i++) {
                                buttonState = 2;
                                selectingTarget = true;
                                TargetSelectFun = StartCoroutine(selectTarget());
                                yield return TargetSelectFun;
                                if (targetGroup.Count != i + 1) {
                                    i--;
                                }
                                buttonState = 1;
                            }
                            PlayerSelectNotice.SetActive(false);
                            selectPlayer = false;
                            for (int i = 0; i < hasSelectedButton.Count; i++) {
                                hasSelectedButton[i].interactable = true;
                            }

                        }else if (target == -1 && card.selectType.type == SELECT_TYPE.CARD && selectEventCard) {
                            for (int i = 0; i < card.selectType.targetCount; i++) {
                                buttonState = 0;
                                selectingTarget = true;
                                TargetSelectFun = StartCoroutine(selectTarget());
                                yield return TargetSelectFun;
                                if (targetGroup.Count != i + 1) {
                                    i--;
                                }
                            }
                            for (int i = 0; i < hasSelectedButton.Count; i++) {
                                hasSelectedButton[i].interactable = true;
                            }

                        } else if (target == -1 && card.selectType.type == SELECT_TYPE.FARM) {
                            FiledSelectInd.SetActive(true);
                            FiledSelecting = true;

                            buttonState = 1;
                            selectingTarget = true;
                            TargetSelectFun = StartCoroutine(selectTarget());
                            yield return TargetSelectFun;

                            FiledSelecting = false;
                            FiledSelectInd.SetActive(false);

                        } else if (target == -2)
                                targetGroup.Add(playerID);
                        break;
                    case 2:
                        targetGroup.Add(this.target);
                        break;
                }
                break;

            case ACTION.HARVEST:
                targetGroup.Add(index);
                index = players[playerID].farm[target].ID;
                break;
                
        }
        isMultipleSelecting = false;
        selectEventCard = false;
        Package send = new Package(playerID, action, index, targetGroup);
        NetworkManager.sendingQueue.Enqueue(send);
        CardisAsking = null;
        buttonState = 0;
        closeSelecting();
        //Debug.Log(JsonUtility.ToJson(NetworkManager.sendingQueue.Dequeue()));
        Debug.Log(string.Format("Sending : {0}",JsonUtility.ToJson(send)));
        networkStandbyFun = null;

    }

    IEnumerator selectTarget() {
        yield return new WaitUntil(() => { return !selectingTarget; });
    }

    //Card Description
    string corpCardDesGenerater(int cardID) {
        corpCard corp = cardList.corpCardsList[cardID % MAXCARD].card;
        string result = corp.description;
        result += string.Format( "\n\n [收成情報(回合數/收成)]" +
                                 "\nTurn 0 : {0}" +
                                 "\nTurn 1 : {1}" +
                                 "\nTurn 2 : {2}" +
                                 "\nTurn 3 : {3}" +
                                 "\nTurn 4 : {4}", corp.reward[0], corp.reward[1], corp.reward[2], corp.reward[3], corp.reward[4]);
        return result;
    }
    public void showCardDescription(int cardID) {
        Card card =  searchCard(cardID);


        cardName.text = card.Name;
        cardImage.sprite = card.cardImg;
        switch (cardID / MAXCARD) {
            case 2:
                cardDescription.text = corpCardDesGenerater(cardID);
                break;
            case 3:
                cardDescription.text = "";
                if (thisTurnResult != 0)
                    cardDescription.text = "[骰點結果] : " + thisTurnResult.ToString() + "\n";
                cardDescription.text += card.description;
                break;
            default:
                cardDescription.text = card.description;
                break;

        }
        
        double newHeight = 21.5 * cardDescription.GetTextInfo(cardDescription.text).lineCount;
        cardDescriptionSize.sizeDelta = new Vector2( cardDescription.transform.localScale.x , (float)newHeight ) ;
    }

    //Network Package Filter
    void packageFilter() {
        if (Animationing || GameOver.active)
            return;
        if (animationQueue.Count != 0) {
            Package act = animationQueue.Dequeue();
            switch (act.ACTION){
                case ACTION.PLAYER_DISCONNECTED:
                    if(turn == -1) {
                        players = new List<PlayerStatus>(act.playerStatuses);
                        HobbyGUIUpdate();
                    } else {
                        players = new List<PlayerStatus>(act.playerStatuses);
                        playerSlotUpdate();
                        FiledUpdate();
                        if (act.index == turn % players.Count) {
                            otherPlayerRollPoint.text = thisTurnResult.ToString();
                            otherPlayerRollAni.Play("Idle");
                        }
                    }
                    break;
                case ACTION.PLAYER_JOIN:
                    players = new List<PlayerStatus>(act.playerStatuses);
                    //TestCase();
                    //playerSlotUpdate();
                    HobbyGUIUpdate();
                    break;
                case ACTION.GAMESTART:
                    gameStartAni();
                    break;
                case ACTION.NEW_TURN:
                    players = new List<PlayerStatus>(act.playerStatuses);
                    newTurnAni(act.target[playerID]);
                    break;
                case ACTION.NEXT_PLAYER:
                    nextPlayerAni();
                    break;
                case ACTION.CARD_ACTIVE:
                    players = new List<PlayerStatus>(act.playerStatuses);
                    switch (act.index/MAXCARD) {
                        case 1:
                            actionCardAni(act.index, act.src, act.target,act.askCounter);
                            break;
                        case 2:
                            players = act.playerStatuses;
                            playerSlotUpdate();
                            FiledUpdate();
                            break;
                        case 3:
                            thisTurnResult = act.target[1];
                            if (act.target[0] != playerID) {
                                otherPlayerRollPoint.text = thisTurnResult.ToString();
                                otherPlayerRollAni.Play("OtherPlayerRollEnd");
                            }
                            fateCardAni(act.index);
                            playerSlotUpdate();
                            break;
                        case 4:
                            eventCardAnimate(act.index);
                            break;
                    }
                    break;
                case ACTION.ROLL_POINT:
                    showCardDescription(act.target[0]);
                    FateNoti.name = act.target[0].ToString();
                    FateNotiText.text = string.Format("Fate:{0}", cardList.fateCardsList[act.target[0] % MAXCARD].card.Name);
                    cardWarningUI(act.target[0]);
                    if (act.index != playerID) {
                        Vector3 pos  = otherPlayerRoll.transform.localPosition;
                        pos.y = -40 - 102 * (turn % players.Count) +50;
                        otherPlayerRoll.transform.localPosition = pos;
                        otherPlayerRollAni.Play("OtherPlayerRollStart");

                        anyOneIsRolling = true;
                    } else {

                        cardIsRolling = act;
                        //anyOneIsRolling = true;
                        rollPointAni(act.target[0]);

                    }
                        
                    break;
                case ACTION.ASSIGN_PLAYER_ID:
                    playerID = act.index;
                    PlayerStatus player = new PlayerStatus();
                    player.name = PlayerNameInput.text;

                    List<PlayerStatus> sendName = new List<PlayerStatus>();
                    sendName.Add(player);
                    Package pkg = new Package(playerID, ACTION.PLAYER_JOIN, 0, 0, false, sendName);
                    NetworkManager.sendingQueue.Enqueue(pkg);
                    break;
                case ACTION.GET_NEW_CARD:
                    players = new List<PlayerStatus>(act.playerStatuses);
                    playerSlotUpdate();
                    if (act.target[0] == playerID)
                        getNewActionCard(act.index);
                    break;
                case ACTION.HARVEST:
                    players = new List<PlayerStatus>(act.playerStatuses);
                    FiledUpdate();
                    playerSlotUpdate();
                    break;
                case ACTION.GAMEOVER:
                    PlayerStatus temp = new PlayerStatus();
                    temp.money = 0;
                    for(int i=0;i<players.Count;i++) {
                        if (players[i].money > temp.money) {
                            temp = players[i];
                        }
                    }
                    NetworkManager.gameResult = true;
                    WinText.text = temp.name + " WIN!!!";
                    GameOver.SetActive(true);
                    break;
            }
        }
    }

    //Lobby
    void HobbyGUIUpdate() {
        for(int i=0;i< players.Count;i++) {
            hobbyPlayerNameText[i].text = players[i].name;
            hobbyPlayerNameDisplay[i].SetActive(true);
        }
        for(int i = 0; i < players.Count; i++) {
            if (players[i].name != "Disconnected") {
                if (i == playerID)
                    gameStartButton.SetActive(true);
                break;
            }
        }
    }

    void gameStartAni() {
        turn = 0;
        viewPlayerID = playerID;
        GUIInit();
        FiledUpdate();
        playerSlotUpdate();
        lobby.SetActive(false);
        GameStartAni.Play("GameStart");
        StartCoroutine(AniSim(GameStartAni.runtimeAnimatorController.animationClips[0].length));
        playerIndecatorMot.maxPlayer = players.Count;
        playerIndecatorMot.goTo = 0;
        playerIndecatorAni.Play("move");
        playerIndecatorMot.move = true;
        turnText.text = "Turn : " + (turn / players.Count + 1).ToString();
        if (turn == playerID)
            nextPlayerButon.SetActive(true);

    }

    //NEW_TURN
    void newTurnAni(int bill) {
        
        FiledUpdate();
        playerSlotUpdate();
        closeSelecting();

        billInd.text = string.Format("生活費 {0} 元已扣除", bill);
        playerIndecatorMot.goTo = -1;
        playerIndecatorAni.Play("move");
        playerIndecatorMot.move = true;
        newTurnAnimator.Play("NewTurn");
        StartCoroutine(AniSim(1.5f));



    }

    //NEXT_PLAYER
    void nextPlayerAni() {
        turn++;
        turnText.text = "Turn : " + (turn / players.Count + 1).ToString();
        thisTurnResult = 0;
        if(turn % players.Count == playerID) {
            nextPlayerButon.SetActive(true);
            buyCardButton.interactable = true;
            isBuyCard = false;
        } else {
            nextPlayerButon.SetActive(false);
            buyCardButton.interactable = true;
        }
        playerIndecatorMot.goTo = turn % players.Count;
        
        nextPlayerAnimator.Play("NextPlayerAni");
        playerIndecatorAni.Play("move");
        playerIndecatorMot.move = true;


    }

    //CARD_ACTIVE
    void cardWarningUI(int cardID) {
        Card card = searchCard(cardID);
        //cardWarnigControl.Play("Idle");
        cardWarnig.CardName.text = card.Name;
        cardWarnig.CardImage.sprite = card.cardImg;
        cardWarnig.CardDescription.text = card.description;

        cardWarnig.gameObject.name = cardID.ToString();
        cardWarnigControl.Play("CardWarning",0,0);
        //cardWarnig.gameObject.SetActive(true);
    }

    //ACTION_CARD
    void actionCardAni(int cardID, int src = 0,List<int> target = null, bool askForCounter = false) {
        cardWarningUI(cardID);
        actionCard card = cardList.actionCardsList[cardID % MAXCARD].card;

        if (src == playerID) {
            foreach (Transform child in handCardSlot.transform) {
                if (child.name == cardID.ToString()) {
                    Destroy(child.gameObject);
                    break;
                }

            }
        }
        switch (card.target) {
            case -1:
                if(askForCounter) {
                    if (target[0] == playerID)
                        generateCardSelectList(askCounter());
                }
                int st = 0;
                if (card.selectType.type != SELECT_TYPE.CARD) {
                    for(int i=0;i<card.selectType.targetCount;i++) {
                        if (target[i] == src)
                            continue;
                        TrailMotion t = targetIndecater[st].GetComponent<TrailMotion>();
                        t.start.y = 10 - 102 * src;
                        t.end.y = 10 - 102 * target[i];
                        t.mid.y = (t.start.y + t.end.y) / 2;
                        t.playing = true;
                        targetIndecater[st].SetActive(true);
                        
                        if (!Animationing)
                            StartCoroutine(AniSim(t.animator.runtimeAnimatorController.animationClips[0].length)); 
                        st++;
                    }
                } else {
                    if (src != playerID)
                        return;
                    selectEventCard = true;
                    List<Card> cards = new List<Card>();
                    for (int i=0;i<target.Count;i++) {
                        cards.Add(cardList.eventCardList[target[i] % MAXCARD].card);
                    }
                    StartCoroutine(editPackage(ACTION.CARD_ACTIVE, cardID));
                    generateCardSelectList(cards);
                }
                break;

            case 5:
                st = 0;
                for(int i=0;i<players.Count;i++) {
                    if (i == src)
                        continue;
                    TrailMotion tAni = targetIndecater[st].GetComponent<TrailMotion>();
                    tAni.start.y = 10 - 102 * src;
                    tAni.end.y = 10 - 102 * i;
                    tAni.mid.y = (tAni.start.y + tAni.end.y) / 2;
                    tAni.playing = true;
                    targetIndecater[st].SetActive(true);
                    if (!Animationing)
                        StartCoroutine(AniSim(tAni.animator.runtimeAnimatorController.animationClips[0].length));

                    st++;
                    
                }
                break;


        }
        if(!askForCounter) {
            playerSlotUpdate();
            FiledUpdate();
        }

    }

    //FATE_CARD
    void fateCardAni(int cardID) {
        
        cardWarningUI(cardID);
        FateNoti.name = cardID.ToString();
        FateNotiText.text = string.Format("Fate:{0}", cardList.fateCardsList[cardID % MAXCARD].card.Name);
        StartCoroutine(AniSim(1.5f));
    }

    //EVENT_CARD
    void eventCardAnimate(int cardID) {
        cardWarningUI(cardID);
        EventNoti.name = cardID.ToString();
        EventNotiText.text = string.Format("Event:{0}", cardList.eventCardList[cardID % MAXCARD].card.Name);
        StartCoroutine(AniSim(newTurnAnimator.runtimeAnimatorController.animationClips[0].length - 1.5f));
    }

    //ROLL_POINT
    void rollPointAni(int cardID) {
        AniSim(1.5f);
        cardWarningUI(cardID);
        FateNoti.name = cardID.ToString();
        FateNotiText.text = string.Format("Fate:{0}", cardList.fateCardsList[cardID % MAXCARD].card.Name);
        rolling = true;
        rollIndecatroAni.Play("RollIndecatorStart");
    }

    //GET_NEW_CARD
    void getNewActionCard(int cardID) {
        if (cardID < 100 || cardID >= 500)
            return;
        GameObject newCard = Instantiate(cardTemp, Vector3.zero, Quaternion.identity, handCardSlot.transform);
        UI_Card CardInf = newCard.GetComponent<UI_Card>();
        newCard.GetComponent<Button>().onClick.AddListener(delegate { cardClick(); });
        Card card = searchCard(cardID);
        if (card == null) {
            return;
        }
        newCard.name = cardID.ToString();
        CardInf.CardName.text = card.Name;
        CardInf.CardImage.sprite = card.cardImg;

        CardInf.UserAction[0].GetComponent<Button>().onClick.AddListener(delegate { useCard(cardID); });
        //buyCardButton.interactable = true;
    }

    //tool function
    Card searchCard(int cardID) {
        int c = cardID / MAXCARD;
        switch (c) {
            case 1:
                return cardList.actionCardsList[cardID % MAXCARD].card;
            case 2:
                return cardList.corpCardsList[cardID % MAXCARD].card;
            case 3:
                return cardList.fateCardsList[cardID % MAXCARD].card;
            case 4:
                return cardList.eventCardList[cardID % MAXCARD].card;
        }
        return null;
    }

    List<Card> askCounter() {
        List<Card> cards = new List<Card>();
            List<int> cardChk = players[playerID].handCard;
            for (int i = 0; i < cardChk.Count; i++) {
                if (cardList.actionCardsList[cardChk[i] % MAXCARD].card.Action == CARD_ACTION.DEFEND || cardList.actionCardsList[cardChk[i] % MAXCARD].card.effect == EFFECT_ID.GUARD) {
                    cards.Add(cardList.actionCardsList[cardChk[i] % MAXCARD].card);
                    selectCounter = true;
                }
            }
        return cards;

    }

    IEnumerator AniSim(float sec) {
        Animationing = true;
        yield return new WaitForSeconds(sec);
        Animationing = false;
    }


}
