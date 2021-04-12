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

        public MainDialog(StateService stateService) : base(nameof(MainDialog)) {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() {
            var waterfallSteps = new WaterfallStep[] {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _stateService));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {            
            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success) {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            }
            else {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
