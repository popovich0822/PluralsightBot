﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Models;
using PluralsightBot.Services;

namespace PluralsightBot.Dialogs {
    public class GreetingDialog : ComponentDialog {
        private readonly StateService _stateService;

        public GreetingDialog(string dialogId, StateService stateService) : base(dialogId) {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() {
            var waterfallSteps = new WaterfallStep[] {
                step1, step2
            };

            AddDialog(new WaterfallDialog("gd.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt("gd.name"));

            InitialDialogId = "gd.mainFlow";
        }

        private async Task<DialogTurnResult> step1(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name)) {
                var dialogId     = "gd.name";
                var prompt       = MessageFactory.Text("What is your name?");
                var promptOptions = new PromptOptions() { Prompt = prompt };

                return await stepContext.PromptAsync(dialogId, promptOptions, cancellationToken);
            }
            else {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> step2(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name)) {
                userProfile.Name = (string)stepContext.Result;
                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}. How can I help you today?"), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
