using DocsVision.Platform.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrateCorrespondenceServerExtension.Services
{
    interface INewConstrolsMeth
    {
        Dictionary<Guid, string> GetAllRowsOfDirectory(SessionContext sessionContext, Guid baseUniversalItemTypeId);
        void AddNewDocumentType(SessionContext sessionContext, String cardID, String newDocTypeName);
    }
}
