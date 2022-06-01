using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Manager : MonoBehaviour
{

    [Header("ª±®a¸ê®Æ")]
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

    [Header("Card_List")]
    public CardList cardList;

    [Header("Test")]
    public GameObject actionCardTemp;
    public TextMeshProUGUI message;
    public 



    // Start is called before the first frame update
    void Start()
    {
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

    void getNewActionCard(int cardID) {
        GameObject newCard = Instantiate(actionCardTemp,Vector3.zero,Quaternion.identity, handCardSlot.transform);

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

    public void showCardDescription() {
        var button = EventSystem.current.currentSelectedGameObject;
        int cardID = int.Parse(button.name);
        Card card = getCardType(cardID);


        this.cardName.text = card.Name;
        this.cardImage.sprite = card.cardImg;
        this.cardDescription.text = card.description;
    }

    Card getCardType(int cardID) {
        int c = cardID / 100;
        switch (c) {
            case 1:
                return cardList.actionCardsList[cardID % 100].card;
            case 2:
                return cardList.corpCardsList[cardID % 100].card;
            case 3:
                return cardList.fateCardsList[cardID % 100].card;
            case 4:
                return cardList.eventCardList[cardID % 100].card;
        }
        return null;
    }
}
