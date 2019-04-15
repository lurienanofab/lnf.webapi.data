using LNF.Models.Data;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class RoomController : ApiController
    {
        [Route("room/active")]
        public IEnumerable<IRoom> GetActiveRooms(bool? parent = null)
        {
            return ServiceProvider.Current.Data.Room.GetActiveRooms(parent);
        }
    }
}
