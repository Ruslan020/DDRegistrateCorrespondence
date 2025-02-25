using DocsVision.BackOffice.ObjectModel;
using DocsVision.Platform.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Services
{
    public class GetAdminsSecretaryIDService : IGetAdminsSecretaryIDService
    {
        private readonly IServiceProvider serviceProvider;

        public GetAdminsSecretaryIDService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Метод возвращающий идентификатор секретаря администрации
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        public string ReturnAdminsSecretaryID(SessionContext sessionContext)
        {
            Guid secritarGroupID = new Guid("D781F6D9-FD0A-4DB9-BE32-C3060AE30611");
            StaffGroup secritarGroup = sessionContext.ObjectContext.GetObject<StaffGroup>(secritarGroupID);

            StaffEmployee adminsSecretary = secritarGroup.Employees.Where(empl => empl.PositionName == "Секретарь администрации").FirstOrDefault();

            return adminsSecretary.GetObjectId().ToString();
        }
    }
}