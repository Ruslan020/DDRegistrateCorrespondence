using System;
using DocsVision.Platform.WebClient.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocsVision.Platform.WebClient;
using System.Web.Mvc;
using DocsVision.Platform.WebClient.Models.Generic;
using RegistrateCorrespondenceServerExtension.Services;
using System.Web.Http;
using RegistrateCorrespondenceServerExtension.Models;
using DocsVision.Platform.WebClient.Diagnostics;

namespace RegistrateCorrespondenceServerExtension.Controllers
{
    public class RegCorController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        //private readonly ServiceHelper _serviceHelper;

        public RegCorController(IServiceProvider serviceProvider)
        {
            //_serviceHelper = new ServiceHelper(serviceProvider);
            this.serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Метод реализующий ответ сервера
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private ContentResult GetResponse<T>(CommonResponse<T> response)
        {
            return Content(JsonHelper.SerializeToJson(response), "application/json");
        }
        /// <summary>
        /// Серверный метод проверяющий наличие администраторов среди получателей
        /// </summary>
        /// <param name="recieversIDs"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult CheckAdminsInRecievers([FromBody] String[] recieversIDs)
        {
            //List<String> recieversIDs = collectionPost.collection;
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var notifyService = ServiceUtil.GetService<INotifyService>(serviceProvider);

            CommonResponse<ResultReturnModel> response = new CommonResponse<ResultReturnModel>();

            try
            {
                ResultReturnModel result = new ResultReturnModel();
                result.Initialize(notifyService.HaveAdminsStaff(recieversIDs, sessionContext));
                response.InitializeSuccess(result);
            }
            catch (Exception)
            {
                Trace.TraceError("CheckAdminsInRecievers: error");
            }
            return GetResponse(response);
        }
        /// <summary>
        /// Серверный метод возвращающий идентификатор секретаря администрации
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public ActionResult GetAdminsSecritarID()
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var getAdminsSercretarIDService = ServiceUtil.GetService<IGetAdminsSecretaryIDService>(serviceProvider);

            CommonResponse<String> response = new CommonResponse<String>();

            try
            {
                String result = getAdminsSercretarIDService.ReturnAdminsSecretaryID(sessionContext);
                response.InitializeSuccess(result);
            }
            catch (Exception)
            {
                Trace.TraceError("GetAdminsSecritarID: error");
            }

            return GetResponse(response);
        }
        /// <summary>
        /// Серверный метод получения оригинала жалобы и отправки уведомления на почтовую группу "жалобы дд" если жалоба сама оригинальна
        /// </summary>
        /// <param name="cardID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public ActionResult GetOriginalSameComplaintDate([FromUri] String cardID)
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var notifyService = ServiceUtil.GetService<INotifyService>(serviceProvider);

            CommonResponse<String> response = new CommonResponse<String>();

            try
            {
                String result = notifyService.ReturnOriginalSameComplaintDate(sessionContext, cardID);
                response.InitializeSuccess(result);
            }
            catch (Exception ex)
            {
                response.InitializeSuccess(ex.StackTrace);
                Trace.TraceError("GetOriginalSameComplaintDate: error");
            }

            return GetResponse(response);
        }
        /// <summary>
        /// Серверный метод уведомления персон и групп в соотвтествующих полях
        /// </summary>
        /// <param name="cardID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public ActionResult NotifyPersonsAndGroups([FromUri] String cardID)
        {
            try
            {
                var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
                var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
                //Вызов основного метода
                var notifyService = ServiceUtil.GetService<INotifyService>(serviceProvider);

                CommonResponse<String> response = new CommonResponse<String>();

                try
                {
                    String result = notifyService.NotifyPersonsAndGroups(sessionContext, cardID);
                    response.InitializeSuccess(result);
                }
                catch (Exception ex)
                {
                    response.InitializeSuccess("Ошибка NotifyPersonsAndGroups: " + ex.StackTrace);
                    Trace.TraceError("NotifyPersonsAndGroups: error: " + ex.StackTrace);
                }

                return GetResponse(response);
            }
            catch (Exception ex)
            {
                CommonResponse<String> response = new CommonResponse<String>(); 
                response.InitializeSuccess("Ошибка NotifyPersonsAndGroupsServerMethod: " + ex.StackTrace);
                return GetResponse(response);
            }
           
        }
        /// <summary>
        /// Серверный метод установки папки карточки (реализация изменена, теперь не требутся)
        /// </summary>
        /// <param name="cardID"></param>
        /// <returns></returns>
        public ActionResult SetCardFolder([FromUri] String cardID)
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var systemService = ServiceUtil.GetService<ISystemService>(serviceProvider);

            CommonResponse<String> response = new CommonResponse<String>();

            try
            {
                systemService.SetCardFolder(sessionContext, cardID);
                response.InitializeSuccess("Карта успешно помещена в папку");
            }
            catch (Exception ex)
            {
                response.InitializeSuccess("SetCardFolder: error: " + ex.Message);
                Trace.TraceError("SetCardFolder: error: " + ex.Message);
            }

            return GetResponse(response);
        }
       
        /// <summary>
        /// Серверный метод добавления нового вида документа в справочник
        /// </summary>
        /// <param name="cardID"></param>
        /// <param name="newDocTypeName"></param>
        /// <returns></returns>
        public ActionResult AddNewDocumentType([FromUri] String cardID, [FromUri] String newDocTypeName)
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var newControlsMethSerice = ServiceUtil.GetService<INewConstrolsMeth>(serviceProvider);

            CommonResponse<String> response = new CommonResponse<String>();

            try
            {
                newControlsMethSerice.AddNewDocumentType(sessionContext, cardID, newDocTypeName);
                var cardTimestamp = sessionContext.AdvancedCardManager.GetCardTimestampModel(new Guid(cardID), true);
                response.InitializeSuccess(cardTimestamp, "Добален новый вид документов");
            }
            catch (Exception ex)
            {
                response.InitializeSuccess("AddNewDocumentType: error: " + ex.Message);
                Trace.TraceError("AddNewDocumentType: error: " + ex.Message);
            }

            return GetResponse(response);
        }
        /// <summary>
        /// Серверный метод получения всех строк узла конструктора справочников
        /// </summary>
        /// <param name="baseUniversalItemTypeId">идентификатор узла</param>
        /// <returns></returns>
        public ActionResult GetAllDictionaryRows([FromUri] Guid baseUniversalItemTypeId)
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var newControlsMethSerice = ServiceUtil.GetService<INewConstrolsMeth>(serviceProvider);

            CommonResponse<Dictionary<Guid, string>> response = new CommonResponse<Dictionary<Guid, string>>();

            Dictionary<Guid, string> allRows = new Dictionary<Guid, string>();

            try
            {
                allRows = newControlsMethSerice.GetAllRowsOfDirectory(sessionContext, baseUniversalItemTypeId);
                response.InitializeSuccess(allRows);
            }
            catch (Exception ex)
            {
                allRows.Add(Guid.Empty, "AddNewDocumentType: error: " + ex.Message);
                response.InitializeSuccess(allRows);
                Trace.TraceError("AddNewDocumentType: error: " + ex.Message);
            }

            return GetResponse(response);
        }
        /// <summary>
        /// Серверный метод Создания обратной ссылки ответ
        /// </summary>
        /// <param name="cardID">идентификатор карты</param>
        /// <returns></returns>
        public ActionResult CreateReplyLink([FromUri] String cardID)
        {
            var currentObjectContextProvider = ServiceUtil.GetService<ICurrentObjectContextProvider>(this.serviceProvider);
            var sessionContext = currentObjectContextProvider.GetOrCreateCurrentSessionContext();
            //Вызов основного метода
            var systemService = ServiceUtil.GetService<ISystemService>(serviceProvider);

            CommonResponse<String> response = new CommonResponse<String>();

            try
            {
                systemService.CreateReplyLink(sessionContext, cardID);
                response.InitializeSuccess("Создана обратная ссылка 'ответ'");
            }
            catch (Exception ex)
            {
                response.InitializeSuccess("CreateReplyLink: error: " + ex.StackTrace);
                Trace.TraceError("CreateReplyLink: error: " + ex.Message);
                
            }

            return GetResponse(response);
        }
        
    }
}