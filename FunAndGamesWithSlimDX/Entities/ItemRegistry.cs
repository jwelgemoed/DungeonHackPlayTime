using System.Collections.Generic;

namespace DungeonHack.Entities
{
    public class ItemRegistry
    {
        private IList<Item> _items;

        public ItemRegistry()
        {
            _items = new List<Item>();
        }

        public void AddItem(Item item) => _items.Add(item);

        public IEnumerable<Item> GetItems()
        {
            return _items;
        }
    }
}
