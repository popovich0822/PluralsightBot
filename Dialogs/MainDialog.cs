using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Models;
using PluralsightBot.Services;

namespace PluralsightBot.Dialogs {
    public class MainDialog : ComponentDialog {
        private readonly StateService _stateService;
        private readonly BotServices _botServices;

        public MainDialog(StateService stateService, BotServices botServices) : base(nameof(MainDialog)) {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() {
            var waterfallSteps = new WaterfallStep[] {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _stateService));
            AddDialog(new BugTypeDialog($"{nameof(MainDialog)}.bugType", _stateService, _botServices));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult.GetTopScoringIntent();

            switch (topIntent.intent) {
                case "GreetingIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);

                case "NewBugReportIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);

                case "QueryBugTypeIntent":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugType", null, cancellationToken);

                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I'm sorry I don't know what you mean."), cancellationToken);
                    break;                
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
