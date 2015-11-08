using System.Collections.Generic;
using Microsoft.WindowsAzure.Mobile.Service;

namespace greengrocer_gutService.DataObjects
{
    public class Groceries : EntityData
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Tags { get; set; } 
        public string OwnerUserId { get; set; }
    }
}