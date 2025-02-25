using DocsVision.Platform.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Services
{
    public interface INotifyService
    {
        Boolean HaveAdminsStaff(String[] reciversIDs, SessionContext sessionContext);
        String ReturnOriginalSameComplaintDate(SessionContext sessionContext, String cardID);
        String NotifyPersonsAndGroups(SessionContext sessionContext, String cardID);
    }
}