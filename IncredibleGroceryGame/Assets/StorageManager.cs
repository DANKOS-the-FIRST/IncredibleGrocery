using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Random = System.Random;

public static class ButtonExtension
{
    public static void AddEventListener<T> (this Button button, T param, Action<T> OnClick)
    {
        button.onClick.AddListener (delegate() {
            OnClick (param);
        });
    }
}
public class StorageManager : MonoBehaviour
{
    public static void MakeTranslucent(Button btn, bool make)
    {
        var image = btn.image;
        var tempColor = image.color;
        tempColor.a = make ? 0.5f : 1f;
        image.color = tempColor;
    }

    public class Item
    {
        private bool _isSelected = false;
        public int Code;
        public Button Button;

        public Item(Button btn, int cd)
        {
            Code = cd;
            Button = btn;
        }

        public bool IsSelected() => _isSelected;
        public int Select()
        {
            _isSelected = !_isSelected;
            MakeTranslucent(Button, _isSelected);
            return _isSelected ? 1 : -1;
        }

        public Image GetImage() => this.Button.image;
    }
    private List<Item> _allItems = new List<Item>();
    private int _selecteditems = 0;
    private List<int> _orderList;
    private int _currentOrderSize;
    public GameObject storageCanvas;
    public Button sellButton;
    public GameObject OrderCloudGameObject;
    
    void Start()
    {
        for (int i = 0; i < storageCanvas.transform.childCount - 1; i++ )
        {
            int code = i + 1;
            _allItems.Add(new Item(storageCanvas.transform.GetChild(code).GetComponent<Button>(), (code)));
            _allItems[i].Button.AddEventListener(code, ItemClicked);
        }

        for (int i = 0; i < 3; i++)
        {
            Image cloudImage = OrderCloudGameObject.transform.GetChild(i).GetComponent<Image>();
            cloudImage.enabled = false;
        }
        MakeTranslucent(sellButton, true);
        sellButton.interactable = false;
        NewCustomerOrder();
    }

    public void NewCustomerOrder()
    {
        _currentOrderSize = new Random().Next(1, 4);
        _orderList = new List<int>();
        Debug.Log("Order size = " + _currentOrderSize);
        Debug.Log("Numbers of order items:");
        Random rand = new Random();
        int newNumber = 0;
        for (int i = 0; i < _currentOrderSize; i++)
        {
            do
            {
                newNumber = rand.Next(1, _allItems.Count + 1);
            } while (_orderList.Exists(x => x == newNumber));
            _orderList.Add(newNumber);
            Debug.Log("Item number = " + _orderList[i]);
        }
        
        // for (int i = 0; i < _orderList.Count; i++)
        // {
        //     Image cloudImage = OrderCloudGameObject.transform.GetChild(i).GetComponent<Image>();
        //     Debug.Log("BEFORE: cloudImage.enabled = " + cloudImage.enabled);
        //     cloudImage.enabled = false;
        //     Debug.Log("AFTER: cloudImage.enabled = " + cloudImage.enabled);
        // }
        for (int i = 0; i < _orderList.Count; i++)
        {
            Debug.Log("Item image number " + _orderList[i] + " added to cloud");
            Image cloudImage = OrderCloudGameObject.transform.GetChild(i).GetComponent<Image>();
            cloudImage.sprite = _allItems[_orderList[i] - 1].GetImage().sprite;
            cloudImage.enabled = true;
        }
    }
    
    void ItemClicked (int itemNumber)
    {
        if (_selecteditems < _currentOrderSize ||
            (_selecteditems == _currentOrderSize &&
             _allItems[itemNumber - 1].IsSelected()))
                _selecteditems += _allItems[itemNumber - 1].Select();

        
        if (_selecteditems == _currentOrderSize)
        {
            MakeTranslucent(sellButton, false);
            sellButton.interactable = true;
        }
        else
        {
            MakeTranslucent(sellButton, true);
            sellButton.interactable = false;
        }
    }

    public void HideCloud()
    {
        // There we need to disable visibility of all items in cloud
        for (int i = 0; i < _orderList.Count; i++)
        {
            Image cloudImage = OrderCloudGameObject.transform.GetChild(i).GetComponent<Image>();
            cloudImage.enabled = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
