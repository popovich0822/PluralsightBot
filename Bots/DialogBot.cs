using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PluralsightBot.Helpers;
using PluralsightBot.Services;

namespace PluralsightBot.Bots {
    public class DialogBot<T> : ActivityHandler where T: Dialog {
        protected readonly Dialog _dialog;
        protected readonly StateService _stateService;
        protected readonly ILogger _logger;

        public DialogBot(StateService botStateService, T dialog, ILogger<DialogBot<T>> logger) {
            _stateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(dialog));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default) {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _stateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken) {
            _logger.LogInformation("Running dialog with Message Activity.");

            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }
    }
}
