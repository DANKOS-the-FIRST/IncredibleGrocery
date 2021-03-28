using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public interface IVisibleSceneObject
{
    public IEnumerator AppearAt(float x, float y);
    public IEnumerator Disappear();
    public Vector2 Position { get; }
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
            if (_isSelected)
            {
                Button.image.MakeTranslucent();
            }
            else
            {
                Button.image.MakeOpaque();
            }

            //Utils.MakeTranslucent(Button.image, _isSelected);
            if (_markSign is null)
            {
                _markSign = Instantiate(marksign, Button.transform.position, Quaternion.identity);
                _markSign.GetComponent<SpriteRenderer>().enabled = true;
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

    public class Cloud : IVisibleSceneObject
    {
        protected GameObject _cloudObject;
        protected List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
        protected AudioSource _appearSound;
        protected AudioSource _disappearSound;

        public Vector2 Position => _cloudObject.transform.position;

        public Cloud(GameObject cloudObject, GameObject canvas, AudioSource appearSound, AudioSource disappearSound)
        {
            _appearSound = appearSound;
            _disappearSound = disappearSound;
            _cloudObject = Instantiate(cloudObject, cloudObject.transform.position, Quaternion.identity);
            for (int i = 0; i < 3; i++)
            {
                SpriteRenderer cloudSprite = _cloudObject.transform.GetChild(i).GetComponent<SpriteRenderer>();
                cloudSprite.enabled = false;
                _sprites.Add(cloudSprite);
            }

            _cloudObject.SetActive(false);
        }

        public IEnumerator AppearAt(float x, float y)
        {
            _cloudObject.transform.position = new Vector3(x, y, 0);
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

        public void ClearValues()
        {
            for (int i = 0; i < _sprites.Count; i++)
            {
                _sprites[i].enabled = false;
            }
            _codes.Clear();
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
                _sprites[_codes.Count - 1].sprite = img.sprite;
                _sprites[_codes.Count - 1].enabled = true;
            }
        }

        public void AddItemsToCloud(Dictionary<int, Image> dict)
        {
            foreach (KeyValuePair<int, Image> item in dict)
            {
                AddItemToCloud(item.Value, item.Key);
            }
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
                _sprites[i].MakeTranslucent();
                if (correctAnswers.Exists(x => x == _codes[i]))
                {
                    _signs.Add(Instantiate(_markSignCorrect, _sprites[i].transform.position, Quaternion.identity));
                    ret++;
                }
                else
                    _signs.Add(Instantiate(_markSignWrong, _sprites[i].transform.position, Quaternion.identity));

                int lastIndex = _signs.Count - 1;
                _signs[lastIndex].transform.SetParent(this._sprites[i].transform);
                _signs[lastIndex].transform.position = this._sprites[i].transform.position;
                _signs[lastIndex].GetComponent<SpriteRenderer>().enabled = true;
            }

            numberOfCorrectAnswers = ret;
            yield return null;
        }

        public IEnumerator Disappear()
        {
            _cloudObject.SetActive(false);
            foreach (var sign in _signs)
                Destroy(sign);

            _disappearSound.Play();

            yield return null;
        }
        public void ClearValues()
        {
            _signs.Clear();
            _codes.Clear();
            numberOfCorrectAnswers = 0;
            for (int i = 0; i < _sprites.Count; i++)
            {
                _sprites[i].MakeOpaque();
                _sprites[i].enabled = false;
            }
        }
    }

    public class EmotionCloud : Cloud
    {
        public EmotionCloud(GameObject obj, GameObject canvas, AudioSource appearSound, AudioSource disappearSound)
            : base(obj, canvas, appearSound, disappearSound)
        {
        }

        public void SetEmotion(SpriteRenderer sprite)
        {
            _sprites[1].sprite = sprite.sprite;
            _sprites[1].enabled = true;
        }
    }

    private class Cashier
    {
        private GameObject _cashierGameObject;

        private GameObject _cashierTableGameObject;

        public Cashier(GameObject cashierPrefab, GameObject cashierTablePrefab, GameObject canvas)
        {
            _cashierTableGameObject = Instantiate(cashierTablePrefab, cashierTablePrefab.transform.position,
                Quaternion.identity);
            _cashierGameObject = Instantiate(cashierPrefab, cashierPrefab.transform.position, Quaternion.identity);
        }

        public void AppearAt(float x, float y)
        {
            _cashierTableGameObject.transform.position = new Vector2(x - 20, y - 20);
            _cashierTableGameObject.SetActive(true);

            _cashierGameObject.transform.position = new Vector2(x, y);
            _cashierGameObject.SetActive(true);
        }
    }

    public class Buyer : IVisibleSceneObject
    {
        private GameObject _buyerGameObject;
        private List<int> _myOrderList;
        public int storageSize;

        public Buyer(GameObject buyerPrefab, GameObject canvas)
        {
            _buyerGameObject = Instantiate(buyerPrefab, buyerPrefab.transform.position, Quaternion.identity);
            _buyerGameObject.SetActive(false);
        }

        public IEnumerator AppearAt(float x, float y)
        {
            _buyerGameObject.transform.position = new Vector3(x, y, 0);
            _buyerGameObject.SetActive(true);
            yield return null;
        }

        public IEnumerator Disappear()
        {
            _buyerGameObject.SetActive(false);
            yield return null;
        }

        public IEnumerator TurnLeft()
        {
            _buyerGameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            yield return null;
        }
        public IEnumerator TurnRight()
        {
            _buyerGameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            yield return null;
        }

        public Vector2 Position => _buyerGameObject.transform.position;

        public IEnumerator WalkTo(Vector3 vec)
        {
            //Animation
            return Animations.BuyerWalkToTarget(_buyerGameObject.transform, vec, 3);
        }

        public List<int> NewBuyerOrder()
        {
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

        public void ClearValues()
        {
            _myOrderList.Clear();
        }
    }

    public class InventoryRepository : IVisibleSceneObject
    {
        private GameObject _storageWrap;
        private GameObject _itemsBox;
        private GameObject _markSign;
        private Button _sellButton;
        
        private List<Item> _allItems = new List<Item>();
        public int Size;

        private List<int> _selectedItems;
        public int _orderSize;

        public bool SellClicked { get; private set; }

        //, Action<int> itmClicked
        public void SelectItem(int itemIndex)
        {
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
                ret.Add(GetItemImage(code - 1));
            return ret;
        }


        private void SellButtonClicked(int a) => SellClicked = true;

        public List<int> GETSelectedItems()
        {
            if (SellClicked)
            {
                // deselect every selected item
                foreach (int itm in _selectedItems)
                {
                    _allItems[itm - 1].Select(_markSign);
                }

                return _selectedItems;
            }

            else
                throw new ArgumentException("There are no selected items!");
        }

        private void ActivateSellButton(bool want)
        {
            if (want)
            {
                _sellButton.image.MakeOpaque();
            }
            else
            {
                _sellButton.image.MakeTranslucent();
            }

            //Utils.MakeTranslucent(_sellButton.image, !want);
            _sellButton.interactable = want;
        }

        public bool ItemIsSelected(int itemIndex)
            => _allItems[itemIndex].IsSelected();

        public IEnumerator AppearAt(float x, float y)
        {
            _storageWrap.transform.position = new Vector3(x, y, 0);
            _storageWrap.SetActive(true);
            SellClicked = false;
            yield return null;
        }

        public IEnumerator MoveTo(Vector3 vec)
        {
            return Animations.MoveToTarget(_storageWrap.transform, vec, 1);
        }

        public IEnumerator Disappear()
        {
            _storageWrap.SetActive(false);
            yield return null;
        }
        public void ClearValues()
        {
            _selectedItems.Clear();
            SellClicked = false;
            _orderSize = 0;
        }
        public Vector2 Position => _storageWrap.transform.position;
    }

    private ItemsCloud _orderCloud;
    private AnswersCloud _answersCloud;
    private EmotionCloud _emotionCloud;
    private InventoryRepository _storage;
    private Cashier _cashier;
    private Buyer _buyer;

    // Positions : 
    private Vector3 _cashierPos;
    private Vector3 _inventoryInPos;
    private Vector3 _inventoryOutPos;
    private Vector3 _doorPos;
    private Vector3 _nearCashierPos;
    private Vector3 _answersAppearPos;
    private Vector3 _orderAppearPos;


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

    public SpriteRenderer[] emotions;
    // Emorions :
    // 0 - Good
    // 1 - Bad

    public AudioSource[] sounds;

    // Sounds :
    // 0 - Money added
    // 1 - Bubble Appeared
    // 2 - Money Disappeared
    private void Start()
    {
        cashCounterGameObject.text = ("$ " + (PlayerPrefs.HasKey("MoneyCash") ? PlayerPrefs.GetInt("MoneyCash") : 0));
        
        _storage = new InventoryRepository(storageCanvas, markSignCorrect);
        _buyer = new Buyer(gamePrefabs[0], mainCanvas);
        _cashier = new Cashier(gamePrefabs[1], gamePrefabs[2], mainCanvas);
        _orderCloud = new ItemsCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2]);
        _answersCloud = new AnswersCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2],
            markSignCorrect, markSignWrong);
        _emotionCloud = new EmotionCloud(cloudGameObject, mainCanvas, sounds[1], sounds[2]);

        _buyer.storageSize = _storage.Size;

        // Positioning : 

        // position where cashier stays
        _cashierPos = new Vector2(10, 370);

        // position where buyer stays to order smth
        _nearCashierPos = new Vector2(_cashierPos.x - 45, _cashierPos.y + 5);

        // position of Storage when we user see storage
        _inventoryInPos = new Vector2(storageCanvas.transform.position.x, storageCanvas.transform.position.y);

        // position of Storage when we user see storage
        _inventoryOutPos = new Vector2(200, _inventoryInPos.y);

        // position of buyer when he arrives to the shop (appears)
        _doorPos = new Vector2(-130, 410);

        // cashier answers cloud appearing position
        _orderAppearPos = new Vector2(4, 420);

        // buyer order cloud appearing position
        _answersAppearPos = new Vector2(45, 420);

        _cashier.AppearAt(_cashierPos.x, _cashierPos.y);

        StartCoroutine(NewBuyerArrived());
    }

    public IEnumerator NewBuyerArrived()
    {
        yield return StartCoroutine(
            _buyer.AppearAt(_doorPos.x, _doorPos.y));

        yield return StartCoroutine(
            _buyer.WalkTo(_nearCashierPos));

        List<int> orderList = _buyer.NewBuyerOrder();
        List<Image> OrderItemsImages = _storage.GetItemsImages(orderList);

        _storage._orderSize = orderList.Count;
        _orderCloud.AddItemsToCloud(orderList
            .ToDictionary(x => x, x => OrderItemsImages[orderList.IndexOf(x)]));

        yield return StartCoroutine(
            _orderCloud.AppearAt(_orderAppearPos.x, _orderAppearPos.y));

        yield return new WaitForSeconds(5);

        yield return StartCoroutine(
            _storage.AppearAt(_inventoryOutPos.x, _inventoryOutPos.y));

        yield return StartCoroutine(
            _storage.MoveTo(_inventoryInPos));

        yield return StartCoroutine(
            _orderCloud.Disappear());

        yield return new WaitUntil(
            () => _storage.SellClicked);

        List<int> selectedItems = _storage.GETSelectedItems();

        yield return StartCoroutine(
            _storage.MoveTo(_inventoryOutPos));

        yield return StartCoroutine(
            _storage.Disappear());

        List<Image> ItemsImages = _storage.GetItemsImages(selectedItems);

        _answersCloud.AddItemsToCloud(selectedItems
            .ToDictionary(x => x, x => ItemsImages[selectedItems.IndexOf(x)]));

        yield return StartCoroutine(
            _answersCloud.AppearAt(_answersAppearPos.x, _answersAppearPos.y));

        yield return StartCoroutine(
            _answersCloud.CheckCorrectAnswers(orderList));

        AddMoney(_answersCloud.numberOfCorrectAnswers *
                 (_answersCloud.numberOfCorrectAnswers == orderList.Count ? 20 : 10));


        yield return new WaitForSeconds(1);

        _emotionCloud.SetEmotion(_answersCloud.numberOfCorrectAnswers == orderList.Count ? emotions[0] : emotions[1]);

        yield return StartCoroutine(
            _answersCloud.Disappear());

        yield return StartCoroutine(
            _emotionCloud.AppearAt(_orderAppearPos.x, _orderAppearPos.y));

        yield return new WaitForSeconds(2);

        yield return StartCoroutine(_buyer.TurnLeft());

        yield return StartCoroutine(_emotionCloud.Disappear());

        yield return StartCoroutine(_buyer.WalkTo(_doorPos));

        yield return StartCoroutine(_buyer.Disappear());

        yield return StartCoroutine(_buyer.TurnRight());
        
        yield return new WaitForSeconds(1);
        
        NextBuyer();
    }
    
    public void NextBuyer()
    {
        _buyer.ClearValues();
        _storage.ClearValues();
        _answersCloud.ClearValues();
        _orderCloud.ClearValues();
        StartCoroutine(NewBuyerArrived());
    }
    public void AddMoney(int sum)
    {
        if (sum != 0)
        {
            sounds[0].Play();
            int cash = int.Parse(cashCounterGameObject.text.Split(' ')[1]) + sum;
            cashCounterGameObject.text = "$ " + cash;
            PlayerPrefs.SetInt("MoneyCash", cash);
        }
    }
}