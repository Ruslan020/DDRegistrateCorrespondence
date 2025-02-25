using DocsVision.BackOffice.CardLib.CardDefs;
using DocsVision.BackOffice.ObjectModel;
using DocsVision.BackOffice.ObjectModel.Services;
using DocsVision.Platform.ObjectManager;
using DocsVision.Platform.ObjectManager.Metadata;
using DocsVision.Platform.ObjectManager.SearchModel;
using DocsVision.Platform.ObjectManager.SystemCards;
using DocsVision.Platform.ObjectModel;
using DocsVision.Platform.WebClient;
using RegistrateCorrespondenceServerExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows;

namespace RegistrateCorrespondenceServerExtension.Services
{
    public class SystemService : ISystemService
    {
		private readonly IServiceProvider serviceProvider;
		//private readonly string LINKS_SECTION_ID = "{568CE0A6-7096-43CC-9800-E0B268B14CC4}";

		public SystemService(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}
		/// <summary>
		/// Метод для сохранения карточки в нужную папку
		/// </summary>
		/// <param name="sessionContext"></param>
		/// <param name="cardID"></param>
		public void SetCardFolder(SessionContext sessionContext, String cardID)
        {
			try
			{
				bool isIncomming = true;
				string regFolderName = "";
				//ID видов карточек в тестовом сервере
				/*const String INCOMMING_SPB_CORRESP_KIND_ID = "{84A65191-AA2D-4E50-9333-B1E7E5DC551E}";
				const String OUTCOMMING_SPB_CORRESP_KIND_ID = "{8B4F0B65-1654-4011-A149-164D85B18C10}";*/

				//ID видов карточек в оригинальном сервере ddsm-dvnew
				//const String INCOMMING_SPB_CORRESP_KIND_ID = "{6F25FD25-DAD9-40EB-97EB-409750832D7D}";
				const String OUTCOMMING_SPB_CORRESP_KIND_ID = "{D2C71F23-939B-494D-A5D3-222E8F4271D3}";

				CardData out_incomming_SPbCardData = sessionContext.Session.CardManager.GetCardData(new Guid(cardID));
				RowData mainInfoRow = out_incomming_SPbCardData.Sections[CardDocument.MainInfo.ID].FirstRow;
				RowData systemRow = out_incomming_SPbCardData.Sections[CardDocument.System.ID].FirstRow;

				if ((Guid)systemRow["Kind"] == new Guid(OUTCOMMING_SPB_CORRESP_KIND_ID))
				{
					isIncomming = false;
				}

				if(isIncomming)
                {
					regFolderName = "Входящие";
				}
				else
				{
					regFolderName = "Исходящие";
				}
				DateTime regDate = (DateTime)mainInfoRow["RegDate"];
				String regDateYear = regDate.Year.ToString();
				
				Guid folderCardID = new Guid("{DA86FABF-4DD7-4A86-B6FF-C58C24D12DE2}");
				FolderCard folderCard = (FolderCard)sessionContext.Session.CardManager.GetCard(folderCardID);

				Guid rootFolderID = new Guid("{052aa82c-f658-4482-99c5-7d5e10b1ba46}");
				Folder rootFolder = folderCard.GetFolder(rootFolderID);

				Folder regYearFolder = rootFolder.Folders.FirstOrDefault(f => f.Name == regDateYear);
				if (regYearFolder == null)
				{
					regYearFolder = rootFolder.Folders.AddNew(regDateYear);
				}

				Folder regFolder = regYearFolder.Folders.FirstOrDefault(f => f.Name == regFolderName);
				if (regFolder == null)
				{
					regFolder = regYearFolder.Folders.AddNew(regFolderName);
				}
				

				List<Guid> cartsShortcuts = regFolder.Shortcuts.Select(x => x.CardId).ToList();
				if(!cartsShortcuts.Contains(new Guid(cardID)))
                {
					regFolder.Shortcuts.AddNew(new Guid(cardID), false);
				}
			}
			catch (Exception ex) 
			{
				throw ex; 
			}
		}
		
		/// <summary>
		/// Метод создющий ответную ссылку на созданную только что карточку (только для типа ссылок "в ответ на")
		/// </summary>
		/// <param name="sessionContext"></param>
		/// <param name="cardID"></param>
		public void CreateReplyLink(SessionContext sessionContext, String cardID)
        {
			const string IN_RESPONCE_TO_REFERENCE_TYPE_ID = "{502F7FE3-477F-492F-9F43-ED2AA7CB32D9}";

			IReferenceListService referenceListService = sessionContext.ObjectContext.GetService<IReferenceListService>();
			ILinkService linkService = sessionContext.ObjectContext.GetService<ILinkService>();

			ReferenceList referenceList;
			bool notNullRefList = referenceListService.TryGetReferenceListFromCard(new Guid(cardID), false, out referenceList);

            if (notNullRefList)
            {
				foreach(ReferenceListReference reference in referenceList.References)
                {
					LinksLinkType inResponceToLinkType = sessionContext.ObjectContext.GetObject<LinksLinkType>(new Guid(IN_RESPONCE_TO_REFERENCE_TYPE_ID));

					string cardIDFromURL = "";

					if (reference.Type == inResponceToLinkType)
                    {
						bool haveSameLink = false;
						LinksLinkType responceLinkType = linkService.FindOppositeLink(inResponceToLinkType);
						ReferenceList responceReferenceList;
						if(reference.Card != null && reference.Card != Guid.Empty /*&& reference.URL != null*/)
                        {
							referenceListService.TryGetReferenceListFromCard(reference.Card, true, out responceReferenceList);
						}
                        else
                        {
							cardIDFromURL = reference.URL.Substring(reference.URL.LastIndexOf("/") + 1);
							Guid responceCardID = new Guid(cardIDFromURL);

							reference.Card = responceCardID;
							sessionContext.ObjectContext.SaveObject<ReferenceListReference>(reference);

							referenceListService.TryGetReferenceListFromCard(responceCardID, true, out responceReferenceList);
						}

						foreach(ReferenceListReference referenceOfOldCard in responceReferenceList.References)
                        {
							if (referenceOfOldCard.Card == new Guid(cardID) && referenceOfOldCard.Type == responceLinkType)
                            {
								haveSameLink = true;
								break;
							}
                        }

                        if (!haveSameLink)
                        {
							ReferenceListReference newReference = referenceListService.CreateReference(responceReferenceList, responceLinkType, new Guid(cardID), CardDocument.ID, false);

							if (cardIDFromURL != "")
							{
								newReference.Card = new Guid(cardID);
								/*CardData responceReferenceListCardData = sessionContext.Session.CardManager.GetCardData(responceReferenceList.GetObjectId());
								RowData linksRow = responceReferenceListCardData.Sections[new Guid(LINKS_SECTION_ID)].GetRow(newReference.GetObjectId());
								linksRow["Card"] = new Guid(cardIDFromURL);*/
							}
							else
                            {
								newReference.Card = new Guid(cardID);
								/*CardData responceReferenceListCardData = sessionContext.Session.CardManager.GetCardData(responceReferenceList.GetObjectId());
								RowData linksRow = responceReferenceListCardData.Sections[new Guid(LINKS_SECTION_ID)].GetRow(newReference.GetObjectId());
								linksRow["Card"] = null;*/
							}

							sessionContext.ObjectContext.SaveObject<ReferenceListReference>(newReference);
							sessionContext.ObjectContext.SaveObject<ReferenceList>(responceReferenceList);
						}
					}
                }
            }
		}

	}
}