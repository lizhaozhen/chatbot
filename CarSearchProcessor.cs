using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using ChatBot.Models;
using Newtonsoft.Json;

namespace ChatBot
{
    public class CarSearchProcessor : AbstractIntentProcessor
    {
        public static readonly List<Item> Items =
            JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("search.txt"));

        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            Item item = null;
            foreach (var slot in lexEvent.CurrentIntent.Slots)
            {
                item = Search(slot.Value, Items);
                if (item == null) break;
            }

            return Close(
                lexEvent.SessionAttributes,
                "Fulfilled",
                new LexResponse.LexMessage
                {
                    ContentType = MESSAGE_CONTENT_TYPE,
                    Content = item == null
                        ? $"https://www.carsales.com.au/cars/results/"
                        : $"https://www.carsales.com.au/cars/results/?q=({item.Action})"
                }
            );
        }

        public Item Search(string text, List<Item> items)
        {
            return items?.Select(x => string.Equals(x.Value, text) ? x : Search(text, x.Subs))
                .FirstOrDefault(x => x != null);
        }
    }
}
