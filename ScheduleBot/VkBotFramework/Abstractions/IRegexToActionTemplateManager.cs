﻿using System.Collections.Generic;
using VkBotFramework.Models;
using VkNet.Model;

namespace VkBotFramework.Abstractions
{
	public interface IRegexToActionTemplateManager
	{
		IEnumerable<RegexToActionTemplate> SearchTemplatesMatchingMessage(Message message);

		//void Register(string incomingMessageRegexPattern, string responseMessage,
		//	MessageKeyboard messageKeyboard = null,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase);

		//void Register(string incomingMessageRegexPattern, List<string> responseMessages,
		//	MessageKeyboard messageKeyboard = null,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase);

		//void Register(string incomingMessageRegexPattern, Action<VkBot, Message> callback,
		//	RegexOptions incomingMessageRegexPatternOptions = RegexOptions.IgnoreCase);
		void Unregister(RegexToActionTemplate template);
		void Unregister(string incomingMessageRegexPattern, long peerId = 0);
		void Register(RegexToActionTemplate template);
	}
}