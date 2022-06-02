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

    [Header("Card_Description")]
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
    public GameObject CardisAsking;

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
        animationQueue = new Queue<Package>();
        GUIInit();
        getNewActionCard(100);
    }

    // Update is called once per frame
    void Update()
    {
        playerStatusDisplay();
        closeSelecting();
        GUIUpdate();
    }
    void GUIInit() {
        for(int i=0;i<4;i++) {
            slot[i].SetActive(false);
        }
    }
    void playerStatusDisplay() {
        for(int i=0;i<4;i++) {
            if(players.Count-1 >i) {
                slot[i].SetActive(true);
                playerName[i].text = players[i].name;
                playerMoney[i].text = "$: " + players[i].money;
                playerHandCard[i].text = "Card: " + players[i].handCard.Count;
            }
        }
    }

    public void filedClick(int filedID) {
        target = filedID;
        int corpCardID = players[playerID].farm[filedID].ID;
        if (corpCardID == 0) {
            List<Card> corpCardList = new List<Card>();
            for(int i=0;i<cardList.corpCardsList.Count;i++) {
                corpCardList.Add(cardList.corpCardsList[i].card);
            }

            generateCardSelectList(corpCardList);
        } else {
            showCardDescription(corpCardID);
        }
    }
    void generateCardSelectList(List<Card> card) {
        if (!selectingCardToUse) {
            selectingCardToUse = true;
            for (int i = 0; i < card.Count; i++) {
                GameObject newCard = Instantiate(cardTemp, Vector3.zero, Quaternion.identity, cardSelect.transform);
                UI_Card CardInf = newCard.GetComponent<UI_Card>();
                newCard.GetComponent<Button>().onClick.AddListener(delegate { showCardDescription(); });

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
        if(selectingCardToUse && Input.GetKeyDown(KeyCode.Mouse1) && CardisAsking == null) {
            selectingCardToUse = false;
            cardSelect.SetActive(false);
            foreach(Transform child in cardSelect.transform) {
                Destroy(child.gameObject);
            }
        }
        if(CardisAsking != null && Input.GetKeyDown(KeyCode.Mouse1)) {
            CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(false);
            CardisAsking = null;
        }
    }

    void getNewActionCard(int cardID) {
        GameObject newCard = Instantiate(cardTemp,Vector3.zero,Quaternion.identity, handCardSlot.transform);
        UI_Card CardInf = newCard.GetComponent<UI_Card>();
        newCard.GetComponent<Button>().onClick.AddListener(delegate { showCardDescription(); });
        Card card = searchCard(cardID);
        if(card == null) {
            return;
        }
        newCard.name = cardID.ToString();
        CardInf.CardName.text = card.Name;
        CardInf.CardImage.sprite = card.cardImg;
        
        CardInf.UserAction[0].GetComponent<Button>().onClick.AddListener(delegate { useCard(cardID); });
        players[playerID].handCard.Add(cardID);
    }

    public void useCard(int cardID) {
        Debug.Log("OWO");
        StartCoroutine(editPackage(cardID));
    }

    IEnumerator editPackage(int cardID) {
        int cardTarget = -2;
        switch (cardID / MAXCARD) {
            case 1:
                cardTarget = cardList.actionCardsList[cardID % MAXCARD].card.target;
                if (cardTarget == -1) {
                    selectingTarget = true;
                    var b = StartCoroutine(selectTarget());
                    yield return b;
                    cardTarget = target;
                }
                break;
            case 2:
                cardTarget = target;
                break;
        }
        Package send = new Package(playerID, ACTION.CARD_ACTIVE, cardID, cardTarget);
        NetworkManager.sendingQueue.Enqueue(send);
        Debug.Log(JsonUtility.ToJson(NetworkManager.sendingQueue.Dequeue()));

    }

    public IEnumerator selectTarget() {
        yield return new WaitUntil(() => { return !selectingTarget; });
    }

    string corpCardDesGenerater(int cardID) {
        corpCard corp = cardList.corpCardsList[cardID % MAXCARD].card;
        string result = corp.description;
        result += string.Format( "\n\n [收成情報(生長回合數/收成)]" +
                                 "\nTurn 1 : {0}" +
                                 "\nTurn 2 : {1}" +
                                 "\nTurn 3 : {2}" +
                                 "\nTurn 4 : {3}", corp.reward[0], corp.reward[1], corp.reward[2], corp.reward[3]);
        return result;
    }
    public void showCardDescription(int ID = 0) {
        Card card;
        int cardID = ID;
        if (ID == 0) {
            var button = EventSystem.current.currentSelectedGameObject;
            cardID = int.Parse(button.name);
            card = searchCard(cardID);
            if (CardisAsking != button.gameObject) {

                if (CardisAsking != null) 
                    CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(false);

                CardisAsking = button.gameObject;
                CardisAsking.GetComponent<UI_Card>().UserAction[0].SetActive(true);

            }
            
        } else {
            card = searchCard(ID);
        }


        cardName.text = card.Name;
        cardImage.sprite = card.cardImg;
        switch (cardID / MAXCARD) {
            case 1:
                cardDescription.text = card.description;
                break;
            case 2:
                cardDescription.text = corpCardDesGenerater(cardID);
                break;
        }
        
        double newHeight = 17.5 * cardDescription.GetTextInfo(cardDescription.text).lineCount;
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
                    break;
                case ACTION.ASSIGN_PLAYER_ID:
                    playerID = act.index;
                    break;
                case ACTION.GET_NEW_CARD:
                    getNewActionCard(act.index);
                    break;
                case ACTION.HARVEST:
                    break;
            }
        }
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

}
