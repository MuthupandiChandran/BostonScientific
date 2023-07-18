using Context;
using Entity;
using X.PagedList;

namespace BostonScientificAVS.Services
{
    public class ItemService
    {
        private readonly DataContext _context;
        public ItemService(DataContext context)
        {
            _context = context;
        }

        public List<ItemMaster> getItems()
        {
            return _context.ItemMaster.ToList();
        }

        public void saveItem(string gtin, string column, string value)
        {
            var itemToUpdate = _context.ItemMaster.FirstOrDefault(x => x.GTIN == gtin);
            //if (itemToUpdate != null)
            //{
            //    foreach(var col in itemToUpdate)
            //    {
            //        if (col = column)
            //    }
            //}

        }
      

    }
}
