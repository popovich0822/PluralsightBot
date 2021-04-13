using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;

namespace PluralsightBot.Services {
    public class BotServices {
        public BotServices(IConfiguration configuration) {
            string appId = configuration["LuisAppId"],
                   apiKey = configuration["LuisAPIKey"],
                   endpoint = $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com";

            var lusApplication = new LuisApplication(appId, apiKey, endpoint);
            
            var recognitionOptions = new LuisRecognizerOptionsV3(lusApplication) {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions() {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };

            Dispatch = new LuisRecognizer(recognitionOptions);
        }

        public LuisRecognizer Dispatch { get; private set; }
    }
}