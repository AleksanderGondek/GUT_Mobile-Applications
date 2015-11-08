using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using greengrocer_gutService.DataObjects;
using greengrocer_gutService.Models;

namespace greengrocer_gutService.Controllers
{
    public class GroceriesController : TableController<Groceries>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            GreenGrocerGutContext context = new GreenGrocerGutContext();
            DomainManager = new EntityDomainManager<Groceries>(context, Request, Services);
        }

        // GET tables/Groceries
        public IQueryable<Groceries> GetAllGroceries()
        {
            return Query();
        }

        // GET tables/Groceries/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Groceries> GetGrocery(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Groceries/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Groceries> PatchGrocery(string id, Delta<Groceries> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Groceries
        public async Task<IHttpActionResult> PostGrocery(Groceries item)
        {
            Groceries current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Groceries/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteGrocery(string id)
        {
            return DeleteAsync(id);
        }
    }
}