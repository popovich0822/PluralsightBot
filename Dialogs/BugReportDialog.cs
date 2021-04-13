using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using PluralsightBot.Models;
using PluralsightBot.Services;

namespace PluralsightBot.Dialogs {
    public class BugReportDialog : ComponentDialog {
        private readonly StateService _stateService;

        public BugReportDialog(string dialogId, StateService stateService) : base(dialogId) {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() {
            var waterfallSteps = new WaterfallStep[] {
                DescriptionStepAsync,
                CallbackTimeStepAsync,
                PhoneNumberStepAsync,
                BugStepAsync,
                SummaryStepAsync
            };
            
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var userProfile   = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());                        
            var dialogId      = $"{nameof(BugReportDialog)}.description";
            var prompt        = MessageFactory.Text("Enter a description for your report");
            var promptOptions = new PromptOptions() { Prompt = prompt };

            return await stepContext.PromptAsync(dialogId, promptOptions, cancellationToken);            
        }

        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            stepContext.Values["description"] = (string)stepContext.Result;

            var dialogId    = $"{nameof(BugReportDialog)}.callbackTime";
            var prompt      = MessageFactory.Text("Please enter in a callback time");
            var retryPrompt = MessageFactory.Text("The value entered must be between the hours of 9 am and 5 pm.");

            var promptOptions = new PromptOptions() {
                Prompt = prompt,
                RetryPrompt = retryPrompt
            };
            
            return await stepContext.PromptAsync(dialogId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            var value = ((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value;
            stepContext.Values["callbackTime"] = Convert.ToDateTime(value);

            var dialogId = $"{nameof(BugReportDialog)}.phoneNumber";
            var prompt = MessageFactory.Text("Please enter in a phone number that we can call your back at");
            var retryPrompt = MessageFactory.Text("please enter a valid phone number");

            var promptOptions = new PromptOptions() {
                Prompt = prompt,
                RetryPrompt = retryPrompt
            };

            return await stepContext.PromptAsync(dialogId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            stepContext.Values["phoneNumber"] = (string)stepContext.Result;
            
            var dialogId = $"{nameof(BugReportDialog)}.bug";
            var prompt   = MessageFactory.Text("Please enter the type of bug.");
            var choices  = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other"});
            
            var promptOptions = new PromptOptions() {
                Prompt = prompt,
                Choices = choices
            };

            return await stepContext.PromptAsync(dialogId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;

            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Description  = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber  = (string)stepContext.Values["phoneNumber"];
            userProfile.Bug          = (string)stepContext.Values["bug"];

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your bug report:"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Description: {userProfile.Description}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time: {userProfile.CallbackTime.ToString()}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number: {userProfile.PhoneNumber}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug: {userProfile.Bug}"), cancellationToken);

            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            
            return await stepContext.EndDialogAsync();
        }

        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken) {
            var valid = false;

            if (promptContext.Recognized.Succeeded) {
                var resolution   = promptContext.Recognized.Value.First();
                var selectedDate = Convert.ToDateTime(resolution.Value);
                var start        = new TimeSpan(9, 0, 0);
                var end          = new TimeSpan(17, 0, 0);

                valid = (selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end);
            }

            return Task.FromResult(valid);
        }

        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken) {
            var valid = false;

            if (promptContext.Recognized.Succeeded) {
                valid = Regex.Match(promptContext.Recognized.Value, @"^(\+\d{1, 2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$").Success;
            }

            return Task.FromResult(valid);
        }
    }
}
