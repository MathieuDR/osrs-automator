using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Common.Models.Data;

public enum ClanFundEventType {
	// fund event types
	[Display(Name = "Deposit")]
	Deposit,
	
	[Display(Name = "Withdrawal")]
	Withdraw,
	
	[Display(Name = "Donation")]
	Donation,
	
	[Display(Name = "Donation Refund")]
	Refund,
	
	[Display(Name = "Other")]
	Other,
	
	[Display(Name = "System event")]
	System
}