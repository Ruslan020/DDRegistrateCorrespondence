using DocsVision.Platform.WebClient;
using RegistrateCorrespondenceServerExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrateCorrespondenceServerExtension.Services
{
    public interface ISystemService
    {
        void SetCardFolder(SessionContext sessionContext, String cardID);
        void CreateReplyLink(SessionContext sessionContext, String cardID);
    }
}
