using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services
{
    public interface ISimpleResourceService
    {
        List<SimpleResourceModel> GetAllForUser(int id);

        List<SimpleResourceModel> QueryInventory(int id, string text, PaginationModel pagination);

        List<SimpleResourceModel> QueryAll(string text, PaginationModel pagination);

        SimpleResourceModel GetResource(int id);

        SimpleResourceModel CreateResource(SimpleResourceModel initial);

        SimpleResourceModel ForkResource(int id, int inventoryId);

        void UpdateAmount(int id, double newAmount);

        void DeleteResource(int id, int by_user_id);
    }
}