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
        // Return top models after Make
        // Check model

        public const string Step = "Step";
        public const string Make = "Make";
        public const string Model = "Model";

            
        public static readonly List<Item> Items =
            JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("search.txt"));

        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            lexEvent.SessionAttributes[DateTime.Now.Millisecond.ToString()] = lexEvent.InputTranscript;

            if (!lexEvent.SessionAttributes.ContainsKey(Step))
            {
                lexEvent.CurrentIntent.Slots.TryGetValue(Make, out var make);
                return string.IsNullOrEmpty(make)
                    ? Delegate(lexEvent.SessionAttributes, lexEvent.CurrentIntent.Slots)
                    : MakeResponse(lexEvent);
            }
            
            return lexEvent.SessionAttributes[Step] == Make ? ModelResponse(lexEvent) : FulfillResponse(lexEvent);
        }

        public LexResponse MakeResponse(LexEvent lexEvent)
        {
            var make = lexEvent.CurrentIntent.Slots[Make];
            var item = Items.First(x => string.Equals(make, x.Value, StringComparison.InvariantCultureIgnoreCase));
            var topModels = item.Subs.OrderByDescending(x => x.Count).Take(3);
            lexEvent.SessionAttributes[Step] = Make;
            return ElicitSlot(lexEvent.SessionAttributes, lexEvent.CurrentIntent.Name, lexEvent.CurrentIntent.Slots,
                Model, new LexResponse.LexMessage()
                {
                    ContentType = MESSAGE_CONTENT_TYPE,
                    Content =
                        $"Do you know what model you want? {string.Join(", ", topModels.Select(x => x.DisplayValue))}"
                });
        }

        public LexResponse ModelResponse(LexEvent lexEvent)
        {
            var model = lexEvent.CurrentIntent.Slots[Model];

            var make = lexEvent.CurrentIntent.Slots[Make];
            var item = Items.First(x => string.Equals(make, x.Value, StringComparison.InvariantCultureIgnoreCase));

            var valid = item.Subs?.Any(x =>
                            string.Equals(x.Value, model, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(x.DisplayValue, model, StringComparison.InvariantCultureIgnoreCase)) ?? false;

            if (valid)
            {
                lexEvent.SessionAttributes[Step] = Model;
                return Delegate(lexEvent.SessionAttributes, lexEvent.CurrentIntent.Slots);
            }
            
            lexEvent.CurrentIntent.Slots[Model] = null;
            return ElicitSlot(lexEvent.SessionAttributes, lexEvent.CurrentIntent.Name, lexEvent.CurrentIntent.Slots,
                Model, new LexResponse.LexMessage()
                {
                    ContentType = MESSAGE_CONTENT_TYPE,
                    Content = $"Can not find model {model}"
                });
        }

        public LexResponse FulfillResponse(LexEvent lexEvent)
        {
            var model = lexEvent.CurrentIntent.Slots[Model];

            var make = lexEvent.CurrentIntent.Slots[Make];
            var item = Items.First(x => string.Equals(make, x.Value, StringComparison.InvariantCultureIgnoreCase));

            var modelItem = item.Subs.FirstOrDefault(x =>
                            string.Equals(x.Value, model, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(x.DisplayValue, model, StringComparison.InvariantCultureIgnoreCase));

            return Close(
                lexEvent.SessionAttributes,
                "Fulfilled",
                new LexResponse.LexMessage
                {
                    ContentType = MESSAGE_CONTENT_TYPE,
                    Content = modelItem == null
                        ? $"https://www.carsales.com.au/cars/results/"
                        : $"https://www.carsales.com.au/cars/results/?q={modelItem.Action}"
                }
            );
        }
    }
}
