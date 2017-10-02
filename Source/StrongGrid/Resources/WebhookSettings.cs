﻿using Newtonsoft.Json.Linq;
using Pathoschild.Http.Client;
using StrongGrid.Models;
using StrongGrid.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace StrongGrid.Resources
{
	/// <summary>
	/// Allows you to set and check webhook settings.
	/// SendGrid’s Event Webhook will notify a URL of your choice via HTTP POST with information about events that occur as SendGrid processes your email.
	/// Common uses of this data are to remove unsubscribes, react to spam reports, determine unengaged recipients, identify bounced email addresses, or create advanced analytics of your email program.
	/// </summary>
	/// <seealso cref="StrongGrid.Resources.IWebhookSettings" />
	/// <remarks>
	/// See https://sendgrid.api-docs.io/v3.0/webhooks
	/// </remarks>
	public class WebhookSettings : IWebhookSettings
	{
		private const string _eventWebhookEndpoint = "user/webhooks/event";
		private const string _inboundParseWebhookEndpoint = "user/webhooks/parse";
		private readonly Pathoschild.Http.Client.IClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="WebhookSettings" /> class.
		/// </summary>
		/// <param name="client">The HTTP client</param>
		internal WebhookSettings(Pathoschild.Http.Client.IClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Get the current Event Webhook settings.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="EventWebhookSettings" />.
		/// </returns>
		public Task<EventWebhookSettings> GetEventWebhookSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return _client
				.GetAsync($"{_eventWebhookEndpoint}/settings")
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<EventWebhookSettings>();
		}

		/// <summary>
		/// Change the Event Webhook settings
		/// </summary>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		/// <param name="url">the webhook endpoint url</param>
		/// <param name="bounce">if set to <c>true</c> [bounce].</param>
		/// <param name="click">if set to <c>true</c> [click].</param>
		/// <param name="deferred">if set to <c>true</c> [deferred].</param>
		/// <param name="delivered">if set to <c>true</c> [delivered].</param>
		/// <param name="dropped">if set to <c>true</c> [dropped].</param>
		/// <param name="groupResubscribe">if set to <c>true</c> [groupResubscribe].</param>
		/// <param name="groupUnsubscribe">if set to <c>true</c> [groupUnsubscribe].</param>
		/// <param name="open">if set to <c>true</c> [open].</param>
		/// <param name="processed">if set to <c>true</c> [processed].</param>
		/// <param name="spamReport">if set to <c>true</c> [spamReport].</param>
		/// <param name="unsubscribe">if set to <c>true</c> [unsubscribe].</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="EventWebhookSettings" />.
		/// </returns>
		public Task<EventWebhookSettings> UpdateEventWebhookSettingsAsync(
			bool enabled,
			string url,
			bool bounce = default(bool),
			bool click = default(bool),
			bool deferred = default(bool),
			bool delivered = default(bool),
			bool dropped = default(bool),
			bool groupResubscribe = default(bool),
			bool groupUnsubscribe = default(bool),
			bool open = default(bool),
			bool processed = default(bool),
			bool spamReport = default(bool),
			bool unsubscribe = default(bool),
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var eventWebhookSettings = new EventWebhookSettings
			{
				Enabled = enabled,
				Url = url,
				Bounce = bounce,
				Click = click,
				Deferred = deferred,
				Delivered = delivered,
				Dropped = dropped,
				GroupResubscribe = groupResubscribe,
				GroupUnsubscribe = groupUnsubscribe,
				Open = open,
				Processed = processed,
				SpamReport = spamReport,
				Unsubscribe = unsubscribe
			};
			var data = JObject.FromObject(eventWebhookSettings);
			return _client
				.PatchAsync($"{_eventWebhookEndpoint}/settings")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<EventWebhookSettings>();
		}

		/// <summary>
		/// Sends a fake event notification post to the provided URL.
		/// </summary>
		/// <param name="url">the event notification endpoint url</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task SendEventTestAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
		{
			var data = new JObject
			{
				{ "url", url }
			};

			return _client
				.PostAsync($"{_eventWebhookEndpoint}/test")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		/// <summary>
		/// Create inbound parse settings for a hostname
		/// </summary>
		/// <param name="hostname">A specific and unique domain or subdomain that you have created to use exclusively to parse your incoming email. For example, parse.yourdomain.com.</param>
		/// <param name="url">The public URL where you would like SendGrid to POST the data parsed from your email. Any emails sent with the given hostname provided (whose MX records have been updated to point to SendGrid) will be parsed and POSTed to this URL.</param>
		/// <param name="spamCheck">Indicates if you would like SendGrid to check the content parsed from your emails for spam before POSTing them to your domain.</param>
		/// <param name="sendRaw">Indicates if you would like SendGrid to post the original MIME-type content of your parsed email. When this parameter is set to "false", SendGrid will send a JSON payload of the content of your email.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="InboundParseWebhookSettings" />.
		/// </returns>
		public Task<InboundParseWebhookSettings> CreateInboundParseWebhookSettings(string hostname, string url, bool spamCheck = false, bool sendRaw = false, CancellationToken cancellationToken = default(CancellationToken))
		{
			var data = CreateJObject(hostname, url, spamCheck, sendRaw);
			return _client
				.PatchAsync($"{_inboundParseWebhookEndpoint}/settings")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<InboundParseWebhookSettings>();
		}

		/// <summary>
		/// Get all the inbound parse webhook settings.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="InboundParseWebhookSettings" />.
		/// </returns>
		public Task<InboundParseWebhookSettings[]> GetAllInboundParseWebhookSettings(CancellationToken cancellationToken = default(CancellationToken))
		{
			return _client
				.GetAsync($"{_inboundParseWebhookEndpoint}/settings")
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<InboundParseWebhookSettings[]>("result");
		}

		/// <summary>
		/// Get the inbound parse webhook settings for a specific hostname.
		/// </summary>
		/// <param name="hostname">The hostname associated with the inbound parse setting that you would like to retrieve.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="InboundParseWebhookSettings" />.
		/// </returns>
		public Task<InboundParseWebhookSettings> GetInboundParseWebhookSettings(string hostname, CancellationToken cancellationToken = default(CancellationToken))
		{
			return _client
				.GetAsync($"{_inboundParseWebhookEndpoint}/settings/{hostname}")
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<InboundParseWebhookSettings>();
		}

		/// <summary>
		/// Update the inbound parse settings for a specific hostname.
		/// </summary>
		/// <param name="hostname">A specific and unique domain or subdomain that you have created to use exclusively to parse your incoming email. For example, parse.yourdomain.com.</param>
		/// <param name="url">The public URL where you would like SendGrid to POST the data parsed from your email. Any emails sent with the given hostname provided (whose MX records have been updated to point to SendGrid) will be parsed and POSTed to this URL.</param>
		/// <param name="spamCheck">Indicates if you would like SendGrid to check the content parsed from your emails for spam before POSTing them to your domain.</param>
		/// <param name="sendRaw">Indicates if you would like SendGrid to post the original MIME-type content of your parsed email. When this parameter is set to "false", SendGrid will send a JSON payload of the content of your email.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The <see cref="InboundParseWebhookSettings" />.
		/// </returns>
		public Task UpdateInboundParseWebhookSettings(string hostname, Parameter<string> url = default(Parameter<string>), Parameter<bool> spamCheck = default(Parameter<bool>), Parameter<bool> sendRaw = default(Parameter<bool>), CancellationToken cancellationToken = default(CancellationToken))
		{
			var data = CreateJObject(hostname, url, spamCheck, sendRaw);
			return _client
				.PatchAsync($"{_inboundParseWebhookEndpoint}/settings/{hostname}")
				.WithJsonBody(data)
				.WithCancellationToken(cancellationToken)
				.AsSendGridObject<InboundParseWebhookSettings>();
		}

		/// <summary>
		/// Delete the inbound parse webhook settings for a specvific hostname.
		/// </summary>
		/// <param name="hostname">The hostname associated with the inbound parse setting that you want to delete.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The async task.
		/// </returns>
		public Task DeleteInboundParseWebhookSettings(string hostname, CancellationToken cancellationToken = default(CancellationToken))
		{
			return _client
				.DeleteAsync($"{_inboundParseWebhookEndpoint}/settings/{hostname}")
				.WithCancellationToken(cancellationToken)
				.AsMessage();
		}

		private static JObject CreateJObject(string hostname, Parameter<string> url, Parameter<bool> spamCheck, Parameter<bool> sendRaw)
		{
			var result = new JObject();
			result.Add("hostname", hostname);
			if (url.HasValue) result.Add("url", url.Value);
			if (spamCheck.HasValue) result.Add("spam_check", spamCheck.Value);
			if (sendRaw.HasValue) result.Add("send_raw", sendRaw.Value);
			return result;
		}
	}
}