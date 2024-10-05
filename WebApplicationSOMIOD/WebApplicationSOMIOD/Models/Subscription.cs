using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationSOMIOD.Models
{
    public class Subscription
    {
        public int Id {  get; set; }
        public string name { get; set; }
        public string creation_dt { get; set; }
        public int parent_id { get; set; }
        public string @event { get; set; }
        public string endpoint { get; set; }
    }
}