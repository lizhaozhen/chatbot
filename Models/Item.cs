using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBot.Models
{
    public class Item
    {
        public string Value { get; set; }
        public string DisplayValue { get; set; }
        public string Action { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public List<Item> Subs { get; set; } = new List<Item>();
    }
}
