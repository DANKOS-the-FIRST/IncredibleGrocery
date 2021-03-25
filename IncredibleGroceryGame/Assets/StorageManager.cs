using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Profiling.Experimental;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Random = System.Random;

public static class ButtonExtension
{
    public static void AddEventListener<T> (this Button button, T param, Action<T> onClick)
    {
        button.onClick.AddListener (delegate() {
            onClick (param);
        });
    }
    public static void MakeTranslucent(Image img, bool make)
    {
        var tempColor = img.color;
        tempColor.a = make ? 0.5f : 1f;
        img.color = tempColor;
    }
}
public class StorageManager : MonoBehaviour
{
    public class Item
    {
        private bool _isSelected = false;
        public int Code;
        public Button Button;
        private GameObject _markSign = null;
        public Item(Button btn, int cd)
        {
            Code = cd;
            Button = btn;
        }

        public bool IsSelected() => _isSelected;
        public int Select(GameObject marksign)
        {
            _isSelected = !_isSelected;
            ButtonExtension.MakeTranslucent(Button.image, _isSelected);
            if (_markSign == null)
            {
                _markSign = Instantiate(marksign, this.Button.transform, true);
                _markSign.transform.position = this.Button.transform.position;
                _markSign.GetComponent<Image>().enabled = true;
            }
            else
            {
                Destroy(_markSign);
                _markSign = null;
            }
            return _isSelected ? 1 : -1;
        }

        public Image GetImage() => this.Button.image;
    }
    public class Cloud
    {
        protected GameObject _cloudObject;
        protected List<Image> _images = new List<Image>();

        public Cloud(GameObject cloudObject, GameObject canvas)
        {
            _cloudObject = Instantiate(cloudObject, canvas.transform, true);
            for (int i = 0; i < 3; i++)
            {
                Image cloudImage = _cloudObject.transform.GetChild(i).GetComponent<Image>();
                cloudImage.enabled = false;
                _images.Add(cloudImage);
            }
            _cloudObject.SetActive(false);
        }
        private void SetPosition(Vector2 vect)
        {
            _cloudObject.transform.position = vect;
        }
        public void Appear(Vector2 vect, AudioSource appearSound)
        {
            SetPosition(vect);
            _cloudObject.SetActive(true);
            appearSound.Play();
        }

        public void Disappear(AudioSource disappearSound)
        {
            _cloudObject.SetActive(false);
            disappearSound.Play();
        }
        ~Cloud()
        {
            Destroy(_cloudObject);
        }
    }
    public class EmotionCloud : Cloud
    {
        public EmotionCloud(GameObject obj, GameObject canvas) : base(obj, canvas)
        {
            
        }
        public void SetEmotion(Image img)
        {
            _images[1].sprite = img.sprite;
            _images[1].enabled = true;
        }
    }
    public class ItemsCloud : Cloud
    {
        protected List<int> _codes = new List<int>();
        public ItemsCloud(GameObject obj, GameObject canvas) : base(obj, canvas)
        {
            
        }
        public void AddItemToCloud(Image img, int code)
        {
            if (_codes.Exists(x => x == code))
            {
                _codes.Remove(code);
            }
            else
            {
                _codes.Add(code);
                _images[_codes.Count - 1].sprite = img.sprite;
                _images[_codes.Count - 1].enabled = true;
            }
        }
    }
    public class AnswersCloud : ItemsCloud
    {
        protected List<GameObject> _signs = new List<GameObject>();
        public AnswersCloud(GameObject obj, GameObject canvas) : base(obj, canvas)
        {
            
        }
        public int CheckCorrectAnswers(List<int> correctAnswers, GameObject correctMark, GameObject wrongMark)
        {
            int ret = 0;
            for (int i = 0; i < _codes.Count; i++)
            {
                ButtonExtension.MakeTranslucent(_images[i], true);

                if (correctAnswers.Exists(x => x == _codes[i]))
                {
                    _signs.Add(Instantiate(correctMark));
                    ret++;
                }
                else
                    _signs.Add(Instantiate(wrongMark));

                int lastIndex = _signs.Count - 1;
                _signs[lastIndex].transform.SetParent(this._images[i].transform);
                _signs[lastIndex].transform.position = this._images[i].transform.position;
                _signs[lastIndex].GetComponent<Image>().enabled = true;
            }
            return ret;
        }
    }
    class Buyer
    {
        [SerializeField]
        private GameObject _buyerGameObject;
        [SerializeField]
        private ItemsCloud _orderCloud;
        [SerializeField]
        private EmotionCloud _emotionCloud;
        public Buyer(GameObject buyerGameObject)
        {
            _buyerGameObject = buyerGameObject;
            _buyerGameObject.SetActive(false);
        }
        public void Appear(Vector2 vec)
        {
            _buyerGameObject.transform.position = vec;
            _buyerGameObject.SetActive(true);
        }
        public void Disappear()
        {
            _buyerGameObject.SetActive(false);
        }
        public void WalkTo(Vector2 vec)
        {
            //Animation
            _buyerGameObject.transform.position = vec;
        }
    }
    class Cashier
    {
        private GameObject _cashierGameObject;
        private GameObject _cashierTableGameObject;
        private AnswersCloud _answersCloud;
        public Cashier(GameObject cashierGameObject, GameObject cashierTableGameObject, AnswersCloud answersCloud)
        {
            _cashierGameObject = cashierGameObject;
            _cashierTableGameObject = cashierTableGameObject;
            _answersCloud = answersCloud;
        }
    }
    public class InventoryRepository
    {
        private List<Item> _allItems = new List<Item>();
        public int Size { get; private set; }
        private GameObject _selectionSign;
        public InventoryRepository(GameObject storageCanvas, GameObject selectionSign, Action<int> itmClicked)
        {
            _selectionSign = selectionSign;
            Size = storageCanvas.transform.childCount - 1;
            for (int i = 0; i < Size; i++ )
            {
                int code = i + 1;
                _allItems.Add(new Item(storageCanvas.transform.GetChild(code).GetComponent<Button>(), (code)));
                _allItems[i].Button.AddEventListener(code, itmClicked);
            }
        }
        public Image GetItemImage(int index)
            => _allItems[index].GetImage();
        public int SelectItem(GameObject markSign, int itemIndex)
            => _allItems[itemIndex].Select(markSign);
        public bool ItemIsSelected(int itemIndex)
            => _allItems[itemIndex].IsSelected();
    }
    private int _selecteditems = 0;
    private List<int> _orderList;
    private List<int> _answersList;
    private int _currentOrderSize;

    private ItemsCloud _orderCloud;
    private AnswersCloud _answersCloud;
    private EmotionCloud _emotionCloud;
    private int _cash;
    private InventoryRepository _storage;
    
    public Text cashCounterGameObject;
    public GameObject mainCanvas;
    public GameObject storageCanvas;
    public Button sellButton;
    public GameObject cloudGameObject;
    public GameObject markSignCorrect;
    public GameObject markSignWrong;
    
    public Image[] emotions;
    // Emorions :
    // 0 - Good
    // 1 - Bad
    
    public AudioSource[] sounds;
    // Sounds :
    // 0 - Money added
    // 1 - Bubble Appeared
    // 2 - Money Disappeared
    void Start()
    {
        _storage = new InventoryRepository(storageCanvas, markSignCorrect, ItemClicked);
        
        cashCounterGameObject.text = (PlayerPrefs.HasKey("MoneyCash") ? PlayerPrefs.GetInt("MoneyCash") : 0) + " $";
        
        _orderCloud = new ItemsCloud(cloudGameObject, mainCanvas);
        _orderCloud.Appear( new Vector2(100, 100), sounds[1]);
        
        _answersCloud = new AnswersCloud(cloudGameObject, mainCanvas);
        _emotionCloud = new EmotionCloud(cloudGameObject, mainCanvas);

        ButtonExtension.MakeTranslucent(sellButton.image, true);
        sellButton.interactable = false;
        NewCustomerOrder();
    }

    public void NewCustomerOrder()
    {
        _currentOrderSize = new Random().Next(1, 4);
        _orderList = new List<int>();
        _answersList = new List<int>();
        //Debug.Log("Order size = " + _currentOrderSize);
        //Debug.Log("Numbers of order items:");
        Random rand = new Random();
        int newNumber = 0;
        for (int i = 0; i < _currentOrderSize; i++)
        {
            do
            {
                newNumber = rand.Next(1, _storage.Size);
            } while (_orderList.Exists(x => x == newNumber));
            _orderList.Add(newNumber);
            //Debug.Log("Item number = " + _orderList[i]);
        }

        FillCloudWithItems(_orderCloud, _orderList);
    }

    private void FillCloudWithItems(ItemsCloud cloud, List<int> itemsNumbers)
    {
        for (int i = 0; i < itemsNumbers.Count; i++)
        {
            cloud.AddItemToCloud(_storage.GetItemImage(itemsNumbers[i] - 1), itemsNumbers[i]);
        }
    }

    public void SellClicked()
    {
        _orderCloud.Disappear(sounds[2]);
        FillCloudWithItems(_answersCloud, _answersList);
        _answersCloud.Appear( new Vector2(300, 100), sounds[1]);
        int correctAns = _answersCloud.CheckCorrectAnswers(_orderList, markSignCorrect, markSignWrong);
        Debug.Log("correctAns: " + correctAns);
        Debug.Log("orderList.Count: " + _orderList.Count);
        AddMoney(correctAns * (correctAns == _orderList.Count ? 20 : 10));
        

        _emotionCloud.SetEmotion(correctAns == _orderList.Count ? emotions[0] : emotions[1]);
        _emotionCloud.Appear( new Vector2(200, 200), sounds[1]);
        
        //AnswersCloudGameObject.transform;
    }

    public void AddMoney(int sum)
    {
        if (sum != 0)
        {
            sounds[0].Play();
            int cash = int.Parse(cashCounterGameObject.text.Split(' ')[0]) + sum;
            Debug.Log("cashStr: " + cash);
            Debug.Log("sum: " + sum);
            cashCounterGameObject.text = cash + " $";
            PlayerPrefs.SetInt("MoneyCash", cash);
        }
    }
    void ItemClicked (int itemNumber)
    {
        // Make item selected/deselected
        // Add/delete item number from _answersList after selection/deselction
        if (_selecteditems < _currentOrderSize)
        {
            _selecteditems += _storage.SelectItem(markSignCorrect, itemNumber - 1);
           _answersList.Add(itemNumber);
        }
        else if (_storage.ItemIsSelected(itemNumber - 1))
        {
            _selecteditems += _storage.SelectItem(markSignCorrect, itemNumber - 1);
            _answersList.Remove(itemNumber);
        }
        
        // Activate/deactivate Sell button according to selected elements
        if (_selecteditems == _currentOrderSize)
        {
            ButtonExtension.MakeTranslucent(sellButton.image, false);
            sellButton.interactable = true;
        }
        else
        {
            ButtonExtension.MakeTranslucent(sellButton.image, true);
            sellButton.interactable = false;
        }
    }
}
