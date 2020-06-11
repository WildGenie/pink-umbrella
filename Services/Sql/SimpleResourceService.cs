using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services.Sql
{
    public class SimpleResourceService : ISimpleResourceService
    {
        public SimpleResourceModel CreateResource(SimpleResourceModel initial)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteResource(int id, int by_user_id)
        {
            throw new System.NotImplementedException();
        }

        public SimpleResourceModel ForkResource(int id, int inventoryId)
        {
            throw new System.NotImplementedException();
        }

        public List<SimpleResourceModel> GetAllForUser(int id)
        {
            throw new System.NotImplementedException();
        }

        public SimpleResourceModel GetResource(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<SimpleResourceModel> QueryAll(string text, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public List<SimpleResourceModel> QueryInventory(int id, string text, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateAmount(int id, double newAmount)
        {
            throw new System.NotImplementedException();
        }
    }
}