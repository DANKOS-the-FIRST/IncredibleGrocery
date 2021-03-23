using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        public int Select()
        {
            _isSelected = !_isSelected;
            MakeTranslucent(Button, _isSelected);
            return _isSelected ? 1 : -1;
        }
    }


    private List<Item> _allItems = new List<Item>();
    private int _selecteditems = 0;
    private List<int> _orderList;
    private int _currentOrderSize;
    public GameObject storageCanvas;
    public Button sellButton;
    
    
    void Start()
    {
        for (int i = 0; i < storageCanvas.transform.childCount - 1; i++ )
        {
            int code = i + 1;
            _allItems.Add(new Item(storageCanvas.transform.GetChild(code).GetComponent<Button>(), (code)));
            _allItems[i].Button.AddEventListener(code, ItemClicked);
        }
        MakeTranslucent(sellButton, true);
        sellButton.interactable = false;
        NewCustomer();
    }

    public void NewCustomer()
    {
        _currentOrderSize = new Random().Next(1, 3);
        _orderList = new List<int>();
        for (int i = 0; i < _currentOrderSize; i++)
        {
            _orderList.Add(new Random().Next(1, _allItems.Count));
        }
    }
    
    void ItemClicked (int itemNumber)
    {
        _selecteditems += _allItems[itemNumber - 1].Select();
        if (_selecteditems >= _currentOrderSize)
        {
            MakeTranslucent(sellButton, false);
            sellButton.interactable = false;
        }
        else
        {
            MakeTranslucent(sellButton, true);
            sellButton.interactable = true;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
