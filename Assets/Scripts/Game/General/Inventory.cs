using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace MyGame
{
    public interface IInventory
    {
        Dictionary<ItemType, List<IItem>> MyInventory { get; }

        List<IItem> GetItemsByType(ItemType type);

        IItem GetItemById(int id, ItemType type);

        List<IItem> GetItemsById(int id, ItemType type);

        bool HasItem(int id, ItemType type = ItemType.None);

        void AddItem(IItem item);

        void RemoveItem(IItem item);

        void Clear();

        void ClearCategory(ItemType type);
    }


    public class Inventory : IInventory
    {

        public Dictionary<ItemType, List<IItem>> MyInventory => itemsDict;

        private Dictionary<ItemType, List<IItem>> itemsDict = new Dictionary<ItemType, List<IItem>>();

        public IItem GetItemById(int id, ItemType type = ItemType.None)
        {
            if(type != ItemType.None)
            {
                if (itemsDict.ContainsKey(type))
                {
                    return itemsDict[type].Find(x => x.ID == id);
                }
            }
            else
            {
                foreach(var itemList in itemsDict.Values)
                {
                    if(itemList.Any(x=>x.ID == id)){
                        return itemList.Find(x => x.ID == id);
                    }
                }
            }
            return null;
        }

        public List<IItem> GetItemsById(int id, ItemType type = ItemType.None)
        {
            if (type != ItemType.None)
            {
                if (itemsDict.ContainsKey(type))
                {
                    return itemsDict[type].FindAll(x => x.ID == id);
                }
            }
            else
            {
                foreach (var itemList in itemsDict.Values)
                {
                    if (itemList.Any(x => x.ID == id))
                    {
                        return itemList.FindAll(x => x.ID == id);
                    }
                }
            }

            return new List<IItem>();
        }

        public List<IItem> GetItemsByType(ItemType type)
        {
            if (itemsDict.ContainsKey(type))
            {
                return itemsDict[type];
            }

            return new List<IItem>();
        }

        public bool HasItem(int id, ItemType type = ItemType.None)
        {
            if (type != ItemType.None)
            {
                if (itemsDict.ContainsKey(type))
                {
                    return itemsDict[type].Any(x=> x.ID == id);
                }
            }
            else
            {
                foreach (var itemList in itemsDict.Values)
                {
                    if (itemList.Any(x => x.ID == id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddItem(IItem item)
        {
            if (itemsDict.ContainsKey(item.ItemType))
            {
                itemsDict[item.ItemType].Add(item);
            }
            else
            {
                var newList = new List<IItem>();
                newList.Add(item);
                itemsDict.Add(item.ItemType, newList);
            }
        }

        public void RemoveItem(IItem item)
        {
            if (itemsDict.ContainsKey(item.ItemType))
            {
                itemsDict[item.ItemType].Remove(item);
            }
        }

        public void Clear()
        {
            itemsDict.Clear();
        }

        public void ClearCategory(ItemType type)
        {
            if (itemsDict.ContainsKey(type))
            {
                itemsDict[type].Clear();
            }
        }
    }

}
