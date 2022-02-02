using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Models.Output;

public class VerificationGroup : Group, IVerifiable {
	public string VerificationCode { get; set; }
}