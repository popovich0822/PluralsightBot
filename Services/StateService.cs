using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using PluralsightBot.Models;

namespace PluralsightBot.Services {
    public class StateService {
        public ConversationState ConversationState { get; }
        public UserState UserState { get; }        
        public static string UserProfileId { get; } = $"{nameof(StateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(StateService)}.ConversationData";
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        public StateService(UserState userState, ConversationState conversationState) {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            InitializeAccessors();
        }

        private void InitializeAccessors() {
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
        }
    }
}
