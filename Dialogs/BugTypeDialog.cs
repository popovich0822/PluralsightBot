using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Helpers;
using PluralsightBot.Services;

namespace PluralsightBot.Dialogs {
    public class BugTypeDialog : ComponentDialog {
        private readonly StateService _stateService;
        private readonly BotServices _botServices;

        public BugTypeDialog(string dialogId, StateService botStateService, BotServices botServices) : base(dialogId) {
            _stateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() {
            var waterfallsSteps = new WaterfallStep[] {
                    InitialStepAsync,
                    FinalStepAsync
                };

            AddDialog(new WaterfallDialog($"{nameof(BugTypeDialog)}.mainFlow", waterfallsSteps));
            InitialDialogId = $"{nameof(BugTypeDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            if (result.Entities != null && result.Entities.Count > 0 && result.Entities.First.ToString() != "\"$instance\": {}") {
                var token = result.Entities.FindTokens("BugType").First();
                var rgx = new Regex("[^a-zA-Z0-9 - ]");
                var value = rgx.Replace(token.ToString(), string.Empty).Trim();

                if (Common.BugTypes.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase))) {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes! {value} is a Bug Type."), cancellationToken);
                }
                else {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No! {value} is not a Bug Type."), cancellationToken);
                }
            }
            else {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I can't understand what you are talking, maybe you are quering bug type?"), cancellationToken);
            }
            

            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
