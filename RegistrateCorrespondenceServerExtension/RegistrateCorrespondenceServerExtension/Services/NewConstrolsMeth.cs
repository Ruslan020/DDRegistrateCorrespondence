using DocsVision.BackOffice.ObjectModel;
using DocsVision.BackOffice.ObjectModel.Services;
using DocsVision.Platform.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrateCorrespondenceServerExtension.Services
{
    public class NewConstrolsMeth: INewConstrolsMeth
    {
		private readonly IServiceProvider serviceProvider;
        //ID узла виды документов на ТЕСТОВОМ сервере
        //private readonly string DOCUMENT_TYPE_UNIVERSAL_TYPE = "{C580D786-152C-4700-8260-7D3774BCB899}";
        //ID узла виды документов на Оригинальном сервере
        //private readonly string DOCUMENT_TYPE_UNIVERSAL_TYPE = "{ACCE8E9D-62AA-4811-937E-5D3928DA4479}";

        public NewConstrolsMeth(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Метод возвращающий список всех строк справочника
		/// </summary>
		/// <param name="sessionContext"></param>
		/// <param name="baseUniversalItemTypeId">идентификатор узла</param>
		/// <returns></returns>
		public Dictionary<Guid, string> GetAllRowsOfDirectory(SessionContext sessionContext, Guid baseUniversalItemTypeId)
		{
			IBaseUniversalService baseUniversalService = sessionContext.ObjectContext.GetService<IBaseUniversalService>();
			Dictionary<Guid, string> rowsNames = new Dictionary<Guid, string>();

			BaseUniversalItemType documentTypeUniversal = sessionContext.ObjectContext.GetObject<BaseUniversalItemType>(baseUniversalItemTypeId);

			foreach(BaseUniversalItem item in documentTypeUniversal.Items)
            {
				rowsNames.Add(item.GetObjectId(), item.Name);
			}

			return rowsNames;
		}
		/// <summary>
		/// Метод создающий новый вид документа
		/// </summary>
		/// <param name="sessionContext"></param>
		/// <param name="cardID"></param>
		/// <param name="newDocTypeName"></param>
		public void AddNewDocumentType(SessionContext sessionContext, String cardID, String newDocTypeName)
		{
			//ID узла виды документов на ТЕСТОВОМ сервере
			const string DOCUMENT_TYPE_UNIVERSAL_TYPE = "{ACCE8E9D-62AA-4811-937E-5D3928DA4479}";
			//ID узла виды документов на Оригинальном сервере
			//const string DOCUMENT_TYPE_UNIVERSAL_TYPE = "{ACCE8E9D-62AA-4811-937E-5D3928DA4479}";

			IBaseUniversalService baseUniversalService = sessionContext.ObjectContext.GetService<IBaseUniversalService>();

			BaseUniversalItemType documentTypeUniversal = sessionContext.ObjectContext.GetObject<BaseUniversalItemType>(new Guid(DOCUMENT_TYPE_UNIVERSAL_TYPE));
			BaseUniversalItem newDocType = baseUniversalService.AddNewItem(documentTypeUniversal);


			newDocType.Name = newDocTypeName;
			sessionContext.ObjectContext.SaveObject<BaseUniversalItem>(newDocType);
		}
	}
}