using System;
using System.Collections.Generic;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace LyncManager
{
    public class LyncManager
    {
        private LyncClient _lyncClient;
        private string _myParticipantUri;
        private Conversation _conversation;

        #region Properties
        public string User { get; private set; }
        public string Message { get; private set; }
        #endregion

        public LyncManager(string _user, string _message)
        {
            _lyncClient = LyncClient.GetClient();
            User = _user;
            Message = _message;
        }

        /// <summary>
        /// Starts a conversation with another client.
        /// </summary>
        /// <param name="participantUri">Complete uri of participant, format protocol:full@name.</param>
        public void StartConversation(string participantUri)
        {
            _lyncClient.ConversationManager.ConversationAdded += new EventHandler<ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
            _myParticipantUri = participantUri;
            _conversation = _lyncClient.ConversationManager.AddConversation();
        }

        /// <summary>
        /// Send a message to a client that have been started a conversation.
        /// </summary>
        /// <param name="messageToSend">A message that will be sent.</param>
        public void SendMessage(string messageToSend)
        {
            try
            {
                IDictionary<InstantMessageContentType, string> textMessage = new Dictionary<InstantMessageContentType, string>();
                textMessage.Add(InstantMessageContentType.PlainText, messageToSend);

                ((InstantMessageModality)_conversation.Modalities[ModalityTypes.InstantMessage]).BeginSendMessage(
                    textMessage
                    , SendMessageCallback
                    , textMessage);
            }
            catch (LyncClientException e)
            {
                throw new Exception("Client PLatform Exception" + e.Message);
            }

        }

        #region EventHandler
        private void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            if (!e.Participant.IsSelf)
            {
                if (((Conversation)sender).Modalities.ContainsKey(ModalityTypes.InstantMessage))
                {
                    ((InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage])
                        .InstantMessageReceived += myInstantMessageModality_MessageReceived;
                }
            }
        }

        private void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            e.Conversation.ParticipantAdded += new EventHandler<ParticipantCollectionChangedEventArgs>(Conversation_ParticipantAdded);

            if (_lyncClient.ContactManager.GetContactByUri(_myParticipantUri) != null)
                e.Conversation.AddParticipant(_lyncClient.ContactManager.GetContactByUri(_myParticipantUri));
        }

        private void SendMessageCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted == true)
            {
                try
                {
                    ((InstantMessageModality)_conversation.Modalities[ModalityTypes.InstantMessage]).EndSendMessage(ar);
                }
                catch (LyncClientException lce)
                {
                    throw new Exception("Lync Client Exception on EndSendMessage " + lce.Message);
                }

            }
        }

        private void myInstantMessageModality_MessageReceived(object sender, MessageSentEventArgs e)
        {
            IDictionary<InstantMessageContentType, string> messageFormatProperty = e.Contents;
            if (messageFormatProperty.ContainsKey(InstantMessageContentType.PlainText))
            {
                string outVal = string.Empty;
                string Sender =
                    (string)
                        ((InstantMessageModality)sender).Participant.Contact.GetContactInformation(
                            ContactInformationType.DisplayName);
            }
        }
        #endregion
    }
}
