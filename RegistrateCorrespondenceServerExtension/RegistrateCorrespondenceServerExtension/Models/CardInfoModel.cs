using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Models
{
    public class CardInfoModel
    {
        private string ID;
        private string Name;

        public CardInfoModel(string id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}