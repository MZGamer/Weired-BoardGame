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
    public static Queue<Package> animationQueue;


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

    [Header("Test")]
    public GameObject cardTemp;
    public TextMeshProUGUI message;
    public GameObject cardSelect;
    public static string talkMessage;



    // Start is called before the first frame update
    void Start()
    {
        talkMessage = "";
        GUIInit();
        getNewActionCard(100);
    }

    // Update is called once per frame
    void Update()
    {
        playerStatusDisplay();
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
        int corpCardID = players[playerID].farm[filedID].ID;
        if (corpCardID == 0) {
            corpCardSelect();
        } else {
            showCardDescription(corpCardID);
        }
    }
    void corpCardSelect() {
        if (!cardSelect.active) {
            GameObject newCard = Instantiate(cardTemp, Vector3.zero, Quaternion.identity, cardSelect.transform);

            for (int i = 0; i < cardList.corpCardsList.Count; i++) {
                corpCard corp = cardList.corpCardsList[i].card;

                newCard.GetComponent<Button>().onClick.AddListener(delegate { showCardDescription(); });

                newCard.name = corp.ID.ToString();
                foreach (Transform component in newCard.transform) {
                    switch (component.name) {
                        case "CardName":
                            component.GetComponent<TextMeshProUGUI>().text = corp.Name;
                            break;
                        case "CardImage":
                            component.GetComponent<Image>().sprite = corp.cardImg;
                            break;
                        case "CardDescription":
                            
                            component.GetComponent<TextMeshProUGUI>().text = corpCardDesGenerater(corp.ID);
                            break;
                    }
                }
            }
            cardSelect.SetActive(true);
        }
    }

    void getNewActionCard(int cardID) {
        GameObject newCard = Instantiate(cardTemp,Vector3.zero,Quaternion.identity, handCardSlot.transform);

        newCard.GetComponent<Button>().onClick.AddListener(delegate { showCardDescription(); });
        Card card = getCardType(cardID);
        if(card == null) {
            return;
        }
        newCard.name = cardID.ToString();
        foreach (Transform component in newCard.transform) {
            switch (component.name) {
                case "CardName":
                    component.GetComponent<TextMeshProUGUI>().text = card.Name;
                    break;
                case "CardImage":
                    component.GetComponent<Image>().sprite = card.cardImg;
                    break;
                case "CardDescription":
                    component.GetComponent<TextMeshProUGUI>().text = card.description;
                    break;
            }
        }
        players[playerID].handCard.Add(cardID);
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
            card = getCardType(cardID);
        } else {
            card = getCardType(ID);
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
        Debug.Log(newHeight);
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

    Card getCardType(int cardID) {
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
