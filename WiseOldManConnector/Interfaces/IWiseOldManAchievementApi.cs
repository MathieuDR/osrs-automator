using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.WiseOldMan;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManAchievementApi {
        #region Achievements
        Task<ConnectorCollectionResponse<Achievement>> View(int playerId);
        Task<ConnectorCollectionResponse<Achievement>> View(int playerId, bool includeMissing);
        #endregion
    }
}