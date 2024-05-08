using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;

namespace ZachPerini_6._3A_HA.Repositories
{
    public class PubSubRepository
    {
        private string _projectId;
        private string _topicId;

        public PubSubRepository(string projectId, string topicId)
        {
            _projectId = projectId;
            _topicId = topicId;
        }

        //when this will be called, I will insert the artefact inside the topic
        //so that a process will pull the blogid and start rendering a report in pdf  containing all the posts within that blog
        public async Task<string> Publish(string artefactId, string userEmail)
        {
            TopicName topicName = TopicName.FromProjectTopic(_projectId, _topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            var pubsubMessage = new PubsubMessage
            {
                // The data is any arbitrary ByteString. Here, we're using text.
                Data = ByteString.CopyFromUtf8(artefactId),
                // The attributes provide metadata in a string-to-string dictionary.
                Attributes =
                {
                    { "user_email", userEmail } // Add the user's email as an attribute
                }
            };
            string messageId = await publisher.PublishAsync(pubsubMessage);

            return messageId; // Return the ID of the published message
        }

        public string Pull(string subscriptionId, bool acknowledge)
        {
            string artefactId = "";

            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, subscriptionId);
            SubscriberServiceApiClient subscriberClient = SubscriberServiceApiClient.Create();
            int messageCount = 0;
            try
            {
                // Pull messages from server,
                // allowing an immediate response if there are no messages.
                PullResponse response = subscriberClient.Pull(subscriptionName, maxMessages: 1);
                // Print out each received message.
                ReceivedMessage msg = response.ReceivedMessages.FirstOrDefault();
                if (msg != null)
                {
                    artefactId = msg.Message.Data.ToStringUtf8();
                    Interlocked.Increment(ref messageCount);
                }
                // If acknowledgement required, send to server.
                if (acknowledge && messageCount > 0)
                {
                    subscriberClient.Acknowledge(subscriptionName, response.ReceivedMessages.Select(msg => msg.AckId));
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
            {
                // UNAVAILABLE due to too many concurrent pull requests pending for the given subscription.
            }
            return artefactId;
        }
    }
}
