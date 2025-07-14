using Monitoring_Support_Server.Models.ViewModels;

namespace Monitoring_Support_Server.Services.Interfaces
{
    public interface IMessagingService
    {
            /// <summary>
            /// Sends a text message to a recipient in Bale.
            /// </summary>
            /// <param name="message">The text of the message to be sent.</param>
            /// <param name="recipient">The unique identifier for the target chat or username of the target channel.</param>
            /// <returns>True if the message is sent successfully, otherwise false.</returns>
            Task<bool> SendMessageAsync(string message, string recipient);

            /// <summary>
            /// Checks if a user exists in Bale.
            /// </summary>
            /// <param name="userId">The unique identifier for the target user.</param>
            /// <returns>True if the user exists, otherwise false.</returns>
            Task<bool> CheckUserExistsAsync(string userId);


        /// <summary>
        /// Gets basic information about the bot and validates the token.
        /// </summary>
        /// <returns>A BaleUser object if the token is valid, otherwise null.</returns>
        Task<BaleUser> GetBotInfoAsync();

    }
}
