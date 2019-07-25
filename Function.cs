using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Amazon.Lambda.LexEvents;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using ChatBot.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ChatBot
{
    public class Function
    {

        public static readonly List<Item> Items =
            JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("search.txt"));

        /// <summary>
        /// Then entry point for the Lambda function that looks at the current intent and calls 
        /// the appropriate intent process.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public LexResponse FunctionHandler(LexEvent lexEvent, ILambdaContext context)
        {
            IIntentProcessor process = new CarSearchProcessor();
            return process.Process(lexEvent, context);
        }

    }
}
