using DocsVision.BackOffice.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Models
{
    public class CardContentModel
    {
        public PartnersCompany senderCompany {get; set;}
        public Incommong_SPbContentModel [] content { get; set; }
        public BaseCardNumber originalPartnerNumber { get; set; }
    }
}