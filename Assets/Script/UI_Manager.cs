using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class UI_Manager : MonoBehaviour
{
    const int MAXCARD = 100;

    [Header("遊玩資料")]
    public int turn;
    public static Queue<Package> animationQueue = new Queue<Package>();

    [Header("玩家資料")]
    public List<PlayerStatus> players;
    public GameObject handCardSlot;
    public int playerID = 0;

    [Header("Player Slot")]
    public List<GameObject> slot;
    public List<TextMeshProUGUI> playerName;
    public List<TextMeshProUGUI> playerMoney;
    public List<TextMeshProUGUI> playerHandCard;

    [Header("Filed")]
    public List<UI_Filed> Filed;

    [Header("Card_Description")]
    public UI_Card cardWarnig;
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public TextMeshProUGUI cardDescription;
    public RectTransform cardDescriptionSize;

    [Header("Card_List")]
    public CardList cardList;

    [Header("UI_Status")]
    public bool selectingCardToUse;
    public bool viewCardDis;
    public bool selectingTarget;
    public bool harvestAsking;
    public GameObject CardisAsking;
    public GameObject PlayerSelectNotice;
    public int viewPlayerID;
    Coroutine CardStandbyFun;
    Coroutine TargetSelectFun;

    [Header("TopBar")]
    public GameObject EventNoti;
    public TextMeshProUGUI EventNotiText;

    [Header("Test")]
    public GameObject cardTemp;
    public TextMeshProUGUI message;
    public GameObject cardSelect;
    public static string talkMessage;
    public int target = -1;



    // Start is called before the first frame update
    void Start()
    {
        talkMessage = "";
        viewPlayerID = playerID;
        animationQueue = new Queue<Package>();
        players[playerID].farm[0].grow();
        FiledUpdate();
        GUIInit();
        playerSlotUpdate();
        getNewActionCard(100);
        getNewActionCard(101);

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
            closeSelecting();
        GUIUpdate();
    }
    void GUIInit() {
        for(int i=0;i<4;i++) {
            slot[i].SetActive(false);
        }
    }
    void playerSlotUpdate() {
        for(int i=0;i< players.Count; i++) {
            slot[i].SetActive(true);
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
                Filed[i].plantSprite.gameObject.SetActive(true);
                Filed[i].turnCounter.gameObject.SetActive(true);
                Filed[i].moneyIndecater.gameObject.SetActive(true);

                Filed[i].plantSprite.sprite = corp.cardImg;
                Filed[i].turnCounter.text = string.Format("Turn : {0}", corp.getTurn());
                Filed[i].moneyIndecater.text = string.Format("$ : {0}", corp.getReward());
            }
        }
    }

    public void filedClick(int filedID) {
        target = filedID;
        int corpCardID = players[viewPlayerID].farm[filedID].ID;
        closeSelecting();
        if(selectingCardToUse) {
            closeSelecting();
        }
        if (corpCardID == 0) {
            if(playerID == viewPlayerID) {
                List<Card> corpCardList = new List<Card>();
            
                for (int i=0;i<cardList.corpCardsList.Count;i++) {
                    corpCardList.Add(cardList.corpCardsList[i].card);
                }

                generateCardSelectList(corpCardList);
            }

        } else {
            if (playerID == viewPlayerID && players[playerID].farm[filedID].getTurn() > 0) {
                //closeSelecting();
                Filed[filedID].HarvestButton.SetActive(true);

                harvestAsking = true;
            }
            CardisAsking = EventSystem.current.currentSelectedGameObject;
            showCardDescription(corpCardID);
        }
    }

    public void cardClick() {
        var button = EventSystem.current.currentSelectedGameObject;
        int cardID = int.Parse(button.name);
        if (CardisAsking != button.gameObject) {
            if(!(CardisAsking == null && selectingCardToUse))
                closeSelecting();

            if (cardID < 300) {
                CardisAsking = button.gameObject;
                if(CardisAsking.GetComponent<UI_Card>().UserAction.Count > 0)
                    CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(true);
            }
            showCardDescription(cardID);


        }
    }

    public void playerSlotClick(int slotID) {
        if(selectingTarget) {
            target = slotID;
            selectingTarget = false;
        } else {
            viewPlayerID = slotID;
            closeSelecting();
            if (selectingCardToUse) {
                closeSelecting();
            }
            FiledUpdate();
        }
    }

    public void eventCheckClick() {
        var button = EventSystem.current.currentSelectedGameObject;
        int cardID = int.Parse(button.name);
        showCardDescription(cardID);
    }

    void generateCardSelectList(List<Card> card) {
        if (!selectingCardToUse) {
            selectingCardToUse = true;
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
        }
    }

    void closeSelecting() {
        if(CardisAsking)
            Debug.Log(CardisAsking.name);
        if(selectingCardToUse && CardisAsking == null) {
            selectingCardToUse = false;
            cardSelect.SetActive(false);
            foreach(Transform child in cardSelect.transform) {
                Destroy(child.gameObject);
            }
        }
        if(CardisAsking != null) {
            if (TargetSelectFun != null) {
                if(CardStandbyFun != null)
                    StopCoroutine(CardStandbyFun);
                StopCoroutine(TargetSelectFun);
                PlayerSelectNotice.SetActive(false);
                TargetSelectFun = null;
                CardStandbyFun = null;
            }
            selectingTarget = false;
            if(harvestAsking) {
                CardisAsking.GetComponent<UI_Filed>().HarvestButton.SetActive(false);
                harvestAsking = false;
            } else {
                CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(false);
            }
            
            CardisAsking = null;
        }
    }


    public void useCard(int cardID) {
        CardStandbyFun = StartCoroutine(editPackage(ACTION.CARD_ACTIVE, cardID));
        EventSystem.current.currentSelectedGameObject.SetActive(false);

    }

    public void harvestClick(int filedID) {
        CardStandbyFun = StartCoroutine(editPackage(ACTION.HARVEST, filedID));
        EventSystem.current.currentSelectedGameObject.SetActive(false);
        harvestAsking = false;
    }

    IEnumerator editPackage(ACTION action, int index = -1) {
        int target = -2;
        switch (action) {
            case ACTION.CARD_ACTIVE:
                int cardID = index;
                switch (cardID / MAXCARD) {
                    case 1:
                        target = cardList.actionCardsList[cardID % MAXCARD].card.target;
                        if (target == -1) {
                            selectingTarget = true;
                            PlayerSelectNotice.SetActive(true);
                            TargetSelectFun = StartCoroutine(selectTarget());
                            yield return TargetSelectFun;
                            PlayerSelectNotice.SetActive(false);
                            target = this.target;
                        }
                        break;
                    case 2:
                        target = this.target;
                        break;
                }
                break;

            case ACTION.HARVEST:
                target = index;
                index = players[playerID].farm[target].ID;
                 break;
        }

        Package send = new Package(playerID, action, index, target);
        NetworkManager.sendingQueue.Enqueue(send);
        CardisAsking = null;
        closeSelecting();
        Debug.Log(JsonUtility.ToJson(NetworkManager.sendingQueue.Dequeue()));

    }

    IEnumerator selectTarget() {
        yield return new WaitUntil(() => { return !selectingTarget; });
    }

    string corpCardDesGenerater(int cardID) {
        corpCard corp = cardList.corpCardsList[cardID % MAXCARD].card;
        string result = corp.description;
        result += string.Format( "\n\n [收成情報(回合數/收成)]" +
                                 "\nTurn 1 : {0}" +
                                 "\nTurn 2 : {1}" +
                                 "\nTurn 3 : {2}" +
                                 "\nTurn 4 : {3}", corp.reward[0], corp.reward[1], corp.reward[2], corp.reward[3]);
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
            default:
                cardDescription.text = card.description;
                break;

        }
        
        double newHeight = 21.5 * cardDescription.GetTextInfo(cardDescription.text).lineCount;
        cardDescriptionSize.sizeDelta = new Vector2( cardDescription.transform.localScale.x , (float)newHeight ) ;
    }

    void GUIUpdate() {
        while (animationQueue.Count != 0) {
            Package act = animationQueue.Dequeue();
            switch (act.ACTION){
                case ACTION.NEW_TURN:
                    break;
                case ACTION.NEXT_PLAYER:
                    break;
                case ACTION.CARD_ACTIVE:
                    switch(act.index/MAXCARD) {
                        case 1:
                            actionCardAnimate(act.index);
                            playerSlotUpdate();
                            break;
                        case 2:
                            playerSlotUpdate();
                            FiledUpdate();
                            break;
                        case 3:
                            FateCardAni(act.index);
                            playerSlotUpdate();
                            break;
                        case 4:
                            eventCardAnimate(act.index);
                            break;
                    }
                    break;
                case ACTION.ASSIGN_PLAYER_ID:
                    playerID = act.index;
                    break;
                case ACTION.GET_NEW_CARD:
                    getNewActionCard(act.index);
                    break;
                case ACTION.HARVEST:
                    FiledUpdate();
                    playerSlotUpdate();
                    break;
            }
        }
    }
    void actionCardAnimate(int cardID) {
        cardWarningUI(cardID);
    }

    void cardWarningUI(int cardID) {
        Card card = searchCard(cardID);
        cardWarnig.CardName.text = card.Name;
        cardWarnig.CardImage.sprite = card.cardImg;
        cardWarnig.CardDescription.text = card.description;

        cardWarnig.gameObject.name = cardID.ToString();
        cardWarnig.gameObject.SetActive(true);
    }

    void FateCardAni(int cardID) {
        cardWarningUI(cardID);
    }

    void eventCardAnimate(int cardID) {
        EventNoti.name = cardID.ToString();
        EventNotiText.text = string.Format("Event:{0}", cardList.eventCardList[cardID % MAXCARD].card.Name);
    }

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

    void getNewActionCard(int cardID) {
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
        players[playerID].handCard.Add(cardID);
    }

}
