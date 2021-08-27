using Discord.Commands;

namespace Discord.Addons.Interactive.Results {
    public class OkResult : RuntimeResult {
        public OkResult(string reason = null) : base(null, reason) { }
    }
}
