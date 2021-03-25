using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling.Experimental;
using UnityEngine.UI;
using Random = System.Random;
public static class Utils
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

    public static IEnumerator MoveToTarget(Transform obj, Vector3 target, float duration)
    {
        Vector3 startPosition = obj.position;
        float t = 0;

        float animationDuration = duration;
        while (t < 1)
        {
            obj.position = Vector3.Lerp(startPosition, target, t);
            t += Time.deltaTime / animationDuration;
            yield return null;
        }
    }
}

interface IVisible
{
    public IEnumerator AppearAt(float x, float y);
    public IEnumerator Disappear();
    public Vector2 position { get;}

}

public class VisibleObject : IVisible
{
    public GameObject GameObject;

    public IEnumerator AppearAt(float x, float y)
    {
        this.GameObject.transform.position = new Vector2(x, y);
        this.GameObject.SetActive(true);
        yield return null;
    }

    public IEnumerator Disappear()
    {
        this.GameObject.SetActive(false);
        yield return null;
    }
    public Vector2 position { get;}
    
}
public class GameManager : MonoBehaviour
{
    public class Item
    {
    private bool _isSelected = false;
    public Button Button;
    public int Code;
    private GameObject _markSign = null;
    public Item(Button btn, int cd)
    {
        Button = btn;
        Code = cd;
    }

    public bool IsSelected() => _isSelected;
    public int Select(GameObject marksign)
    {
        _isSelected = !_isSelected;
        Utils.MakeTranslucent(Button.image, _isSelected);
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
    public class Cloud : IVisible
    {
        protected GameObject _cloudObject; 
        protected List<Image> _images = new List<Image>();
        private AudioSource _appearSound;
        private AudioSource _disappearSound;

        public Vector2 position => _cloudObject.transform.position;

        public Cloud(GameObject cloudObject, GameObject canvas, AudioSource appearSound, AudioSource disappearSound)
        {
            _appearSound = appearSound;
            _disappearSound = disappearSound;
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
        public IEnumerator AppearAt(float x, float y)
        {
            SetPosition(new Vector2(x, y));
            _cloudObject.SetActive(true);
            _appearSound.Play();
            yield return null;
        }

        public IEnumerator Disappear()
        {
            _cloudObject.SetActive(false);
            _disappearSound.Play();
            yield return null;
        }
        ~Cloud()
        {
            Destroy(_cloudObject);
        }
    }
    public class ItemsCloud : Cloud
    {
        protected List<int> _codes = new List<int>();
        public ItemsCloud(GameObject obj, GameObject canvas, AudioSource appearSound, AudioSource disappearSound)
            : base(obj, canvas, appearSound, disappearSound)
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

        public void AddItemsToCloud(Dictionary<int, Image> dict)
        {
            foreach( KeyValuePair<int, Image> item in dict )
                AddItemToCloud(item.Value, item.Key);
        }
        
    }
    public class AnswersCloud : ItemsCloud
    {
        protected List<GameObject> _signs = new List<GameObject>();
        private GameObject _markSignCorrect;
        private GameObject _markSignWrong;
        public int numberOfCorrectAnswers { get; private set; }

        public AnswersCloud(GameObject obj, GameObject canvas, AudioSource appearSound, AudioSource disappearSound,
            GameObject markSignCorrect, GameObject markSignWrong)
            : base(obj, canvas, appearSound, disappearSound)
        {
            _markSignCorrect = markSignCorrect;
            _markSignWrong = markSignWrong;
        }
        public IEnumerator CheckCorrectAnswers(List<int> correctAnswers)
        {
            int ret = 0;
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < _codes.Count; i++)
            {
                yield return new WaitForSeconds(0.5f);
                Utils.MakeTranslucent(_images[i], true);
                if (correctAnswers.Exists(x => x == _codes[i]))
                {
                    _signs.Add(Instantiate(_markSignCorrect));
                    ret++;
                }
                else
                    _signs.Add(Instantiate(_markSignWrong));
    
                int lastIndex = _signs.Count - 1;
                _signs[lastIndex].transform.SetParent(this._images[i].transform);
                _signs[lastIndex].transform.position = this._images[i].transform.position;
                _signs[lastIndex].GetComponent<Image>().enabled = true;
            }
            numberOfCorrectAnswers = ret;
            yield return null;
        }
    }
    public class EmotionCloud : Cloud
    {
        public EmotionCloud(GameObject obj, GameObject canvas, AudioSource appearSound, AudioSource disappearSound)
            : base(obj, canvas, appearSound, disappearSound)
        {
        
        }
        public void SetEmotion(Image img)
        {
            _images[1].sprite = img.sprite;
            _images[1].enabled = true;
        }
    }
    private class Cashier
    {
        private GameObject _cashierGameObject;
    
        private GameObject _cashierTableGameObject;
        
        public Cashier(GameObject cashierPrefab, GameObject cashierTablePrefab, GameObject canvas)
        {
            _cashierTableGameObject = Instantiate(cashierTablePrefab, canvas.transform, true);
            _cashierGameObject = Instantiate(cashierPrefab, canvas.transform, true);
        }

        public void AppearAt(float x, float y)
        {
            _cashierTableGameObject.transform.position = new Vector2(x, y - 40);
            _cashierTableGameObject.SetActive(true);
            
            _cashierGameObject.transform.position = new Vector2(x,y);
            _cashierGameObject.SetActive(true);
        }
    }
    public class Buyer : IVisible
    {
        private GameObject _buyerGameObject;
        private List<int> _myOrderList;
        public int storageSize;
        public Buyer(GameObject buyerPrefab, GameObject canvas)
        {

            _buyerGameObject = Instantiate(buyerPrefab, canvas.transform, true);
            _buyerGameObject.SetActive(false);
        }
        public IEnumerator AppearAt(float x, float y)
        {
            _buyerGameObject.transform.position = new Vector2(x, y);
            _buyerGameObject.SetActive(true);
            yield return null;
        }

        public IEnumerator Disappear()
        {
            _buyerGameObject.SetActive(false);
            yield return null;
        }
        public IEnumerator Turn()
        {
            _buyerGameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            yield return null;
        }
        public Vector2 position => _buyerGameObject.transform.position;

        public IEnumerator WalkTo(Vector3 vec)
        {
            //Animation
            return Utils.MoveToTarget(_buyerGameObject.transform, vec, 3);
        }
        public List<int> NewBuyerOrder()
        {
            Debug.Log("Start to creating new buyers order");
            int orderSize = new Random().Next(1, 4);
            _myOrderList = new List<int>();
            Random rand = new Random();
            int newNumber = 0;
            for (int i = 0; i < orderSize; i++)
            {
                do
                {
                    newNumber = rand.Next(1, storageSize + 1);
                } while (_myOrderList.Exists(x => x == newNumber));
                _myOrderList.Add(newNumber);
            }
            return _myOrderList;
        }
    }
    public class InventoryRepository : IVisible
    {
        private List<Item> _allItems = new List<Item>();
        public int Size;

        private List<int> _selectedItems;
        public int _orderSize;
        private GameObject _storageWrap;
        private GameObject _itemsBox;
        public bool SellClicked { get; private set; }
        private Button _sellButton;
        private GameObject _markSign;
        //, Action<int> itmClicked
        public void SelectItem(int itemIndex)
        {
            Debug.Log("ClickedOn item code : " + (itemIndex + 1));
            if (_selectedItems.Count < _orderSize)
            {
                if (_allItems[itemIndex].Select(_markSign) == 1)
                    _selectedItems.Add(itemIndex + 1);
                else
                    _selectedItems.Remove(itemIndex + 1);
            }
            else if (_selectedItems.Exists(x => x == (itemIndex + 1)))
            {
                _allItems[itemIndex].Select(_markSign);
                _selectedItems.Remove(itemIndex + 1);
            }
            ActivateSellButton(_selectedItems.Count == _orderSize);
        }
        public InventoryRepository(GameObject storageWrap, GameObject markSign)
        {
            _markSign = markSign;
            _storageWrap = storageWrap;
            
            _selectedItems = new List<int>();
            
            _itemsBox = _storageWrap.transform.GetChild(0).gameObject;
            _sellButton = _storageWrap.transform.GetChild(1).GetComponent<Button>();
            
            Size = _itemsBox.transform.childCount - 1;
            for (int i = 0; i < Size; i++)
            {
                Debug.Log("i = " + i);
                int code = i + 1;
                _allItems.Add(new Item(_itemsBox.transform.GetChild(code).GetComponent<Button>(), (code)));
                _allItems[i].Button.AddEventListener(code - 1, SelectItem);
            }
            
            _storageWrap.SetActive(false);
            ActivateSellButton(false);
            _sellButton.AddEventListener(Size, SellButtonClicked);
            
        }
        
        public Image GetItemImage(int index)
            => _allItems[index].GetImage();
        public List<Image> GetItemsImages(List<int> codes)
        {
            List<Image> ret = new List<Image>();
            foreach (var code in codes)
                ret.Add(GetItemImage(code-1));
            return ret;
        }
        

        private void SellButtonClicked(int a) => SellClicked = true;

        public List<int> GETSelectedItems()
        {
            if (SellClicked)
                return _selectedItems;
            else
                throw new ArgumentException("There are no selected items!");
        }
        
        private void ActivateSellButton(bool want)
        {
            Utils.MakeTranslucent(_sellButton.image, !want);
            _sellButton.interactable = want;
        }
        public bool ItemIsSelected(int itemIndex)
            => _allItems[itemIndex].IsSelected();

        public IEnumerator AppearAt(float x, float y)
        {
            _storageWrap.transform.position = new Vector3(x, y);
            _storageWrap.SetActive(true);
            SellClicked = false;
            yield return null;
        }
        public IEnumerator MoveTo(Vector3 vec)
        {
            return Utils.MoveToTarget(_storageWrap.transform, vec, 3);
        }
        public IEnumerator Disappear()
        {
            _storageWrap.SetActive(false);
            yield return null;
        }

        public Vector2 position => _storageWrap.transform.position;
    }

    private ItemsCloud _orderCloud;
    private AnswersCloud _answersCloud;
    private EmotionCloud _emotionCloud;
    private int _cash;
    private InventoryRepository _storage;
    private Cashier _cashier;
    private Buyer _buyer;
    
    // Positions : 
    private Vector3 cashierPos;
    private Vector3 InventoryInPos;
    private Vector3 InventoryOutPos;
    private Vector3 DoorPos;
    private Vector3 NearCashierPos;
    private float _screenHeight;
    private float _screenWidth;

    
    
    public Text cashCounterGameObject;
    public GameObject mainCanvas;
    public GameObject storageCanvas;
    //public Button sellButton;
    public GameObject cloudGameObject;
    public GameObject markSignCorrect;
    public GameObject markSignWrong;

    
    public GameObject[] gamePrefabs;
    // gamePrefabs:
    // 0 - buyer
    // 1 - cashier
    // 2 - cashier table
    
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
        cashCounterGameObject.text = (PlayerPrefs.HasKey("MoneyCash") ? PlayerPrefs.GetInt("MoneyCash") : 0) + " $";
        
        _storage = new InventoryRepository(storageCanvas, markSignCorrect);
        _buyer = new Buyer(gamePrefabs[0], mainCanvas);
        _cashier = new Cashier(gamePrefabs[1], gamePrefabs[2], mainCanvas);
        _orderCloud = new ItemsCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2]);
        _answersCloud = new AnswersCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2],
            markSignCorrect, markSignWrong);
        _emotionCloud = new EmotionCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2]);
        _buyer.storageSize = _storage.Size;
        
        // Positioning : 
        cashierPos = new Vector3(450, 250);
        InventoryInPos = new Vector3(540, 213);
        InventoryOutPos = new Vector3(798, 213);
        DoorPos = new Vector3(100, 300);
        NearCashierPos = new Vector3(380, 250);
        
        _cashier.AppearAt(cashierPos.x, cashierPos.y);
        StartCoroutine(NewBuyerArrived());
    }

    public IEnumerator NewBuyerArrived()
    {
        yield return StartCoroutine(
            _buyer.AppearAt(DoorPos.x, DoorPos.y));
        
        yield return StartCoroutine(
            _buyer.WalkTo(NearCashierPos));
        
        List<int> orderList = _buyer.NewBuyerOrder();
        List<Image> OrderItemsImages = _storage.GetItemsImages(orderList);
        
        _storage._orderSize = orderList.Count;
        _orderCloud.AddItemsToCloud(orderList
            .ToDictionary(x => x, x => OrderItemsImages[orderList.IndexOf(x)]));
        
        yield return StartCoroutine(
            _orderCloud.AppearAt(_buyer.position.x, _buyer.position.y));
        
        yield return new WaitForSeconds(5);
        
        yield return StartCoroutine(
            _storage.AppearAt(InventoryOutPos.x ,InventoryOutPos.y));
        
        yield return StartCoroutine(
            _storage.MoveTo(InventoryInPos));
        
        yield return StartCoroutine(
            _orderCloud.Disappear());
        
        yield return new WaitUntil(
            () => _storage.SellClicked);
        
        yield return StartCoroutine(
            _storage.MoveTo(InventoryOutPos));
        
        yield return StartCoroutine(
            _storage.Disappear());
        
        List<int> selectedItems = _storage.GETSelectedItems();
        List<Image> ItemsImages = _storage.GetItemsImages(selectedItems);
        
        _answersCloud.AddItemsToCloud(selectedItems
            .ToDictionary(x => x, x => ItemsImages[selectedItems.IndexOf(x)]));
        
        yield return StartCoroutine(
            _answersCloud.AppearAt(cashierPos.x, cashierPos.y));
        
        yield return StartCoroutine(
            _answersCloud.CheckCorrectAnswers(orderList));
        
        AddMoney(_answersCloud.numberOfCorrectAnswers * 
                 (_answersCloud.numberOfCorrectAnswers == orderList.Count ? 20 : 10));
        

        
        yield return new WaitForSeconds(1);
        
        yield return StartCoroutine(
            _answersCloud.Disappear());
        
        _emotionCloud.SetEmotion(_answersCloud.numberOfCorrectAnswers == orderList.Count ? emotions[0] : emotions[1]);
        
        yield return StartCoroutine(
            _emotionCloud.AppearAt(_buyer.position.x, _buyer.position.y));

        yield return StartCoroutine(_buyer.Turn());
        
        yield return StartCoroutine(_emotionCloud.Disappear());
        
        yield return StartCoroutine(_buyer.WalkTo(DoorPos));
        
        yield return StartCoroutine(_buyer.Disappear());
        

        
        yield return StartCoroutine(_buyer.Turn());
    }
    public void AddMoney(int sum)
    {
        if (sum != 0)
        {
            sounds[0].Play();
            int cash = int.Parse(cashCounterGameObject.text.Split(' ')[0]) + sum;
            cashCounterGameObject.text = cash + " $";
            PlayerPrefs.SetInt("MoneyCash", cash);
        }
    }
    
}
