using DocsVision.BackOffice.CardLib.CardDefs;
using DocsVision.BackOffice.ObjectModel;
using DocsVision.BackOffice.ObjectModel.Services;
using DocsVision.Platform.ObjectManager;
using DocsVision.Platform.WebClient;
using DocsVision.Platform.ObjectManager.SearchModel;
using System;
using System.Collections.Generic;
using System.Linq;
using DocsVision.Platform.ObjectModel;
using System.IO;
using DocsVision.Platform.ObjectManager.SystemCards;
using DocsVision.Workflow.Objects;
using FieldType = DocsVision.Platform.ObjectManager.Metadata.FieldType;
using System.Threading;

namespace RegistrateCorrespondenceServerExtension.Services
{
    /// <summary>
    /// Сервис связанный с уведомлениями
    /// </summary>
    public class NotifyService : INotifyService
    {
        //состояние сервера ТЕСТ?
        private readonly bool testServise = false;

        private readonly IServiceProvider serviceProvider;
        private readonly String DOCUMENT_SENDER_PARTNER_SECTION_ID = "{6E976D72-3EA7-4708-A2C2-2A1499141301}";
        private readonly String DOCUMENT_RECIEVERS_SECTION_ID = "{B6DFAEAD-BAAA-4024-908C-5DBD693D0FD3}";
        private readonly String DOCUMENT_APPROVALERS_SECTION_ID = "{281A97FF-667F-46C8-8FBE-7CFC02EDFEDB}";

        //ID секции INCOMMING_SPB_CORR_CONTENT
        private readonly String DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID;
        
        //ID шаблона бизнес процесса уведомления
        private readonly String BP_TEMPLATE_ID;
       
        //ID переменных БП
        private readonly Guid addressesStrVarId; 
        private readonly Guid filesNamesStrVarId;
        private readonly Guid filesFolderVarId;
        private readonly Guid textVarId;
        private readonly Guid topicVarId;

        //Уведомляемая на жалобы группа, + ТДО  + Секретари
        private readonly string reclamationsGroup;
        private readonly string TDODDGroup;
        private readonly string secretaryDDMscGroup;
        private readonly string secretaryDDSpbAHUGroup;


        private List<string> reclamaitionsStaffEmails = new List<string>();
        private List<string> TDODDExtraStaffEmails = new List<string>();
        private List<string> secretaryDDMscStaffEmails = new List<string>();
        private List<string> secretaryDDSpbAHUStaffEmails = new List<string>();

        //ID группы технологи ДД и жалобы ДД на серверах
        private readonly string reclamaitionsDDStaffGroupID;

        public NotifyService(IServiceProvider serviceProvider)
        {
            //Logger.Logger.InitLogger();
            //this.logger = Logger.Logger.Log;

            this.serviceProvider = serviceProvider;

            if (!this.testServise)
            {
                //ПРОД
                //ID секции INCOMMING_SPB_CORR_CONTENT на оригинальном ddsm-dvnew сервере
                this.DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID = "{561F2088-9D27-4B01-BAF1-8D70E600728B}";

                //ID шаблона бизнес процесса уведомления на оригинатьном сервере ddsm-dvnew
                this.BP_TEMPLATE_ID = "BFAE58F5-CEFB-42C7-9812-031E8A3146BD";

                //ID переменных БП на оригинатьном сервере ddsm-dvnew
                this.addressesStrVarId = new Guid("{DE5AF7E8-1A73-4F00-A81E-7603297D3BF8}");
                this.filesNamesStrVarId = new Guid("{2C5D2647-D039-47ED-B9B5-D4938417952C}");
                this.filesFolderVarId = new Guid("{F07038E6-ADE4-4295-B886-3552757FE7FA}");
                this.textVarId = new Guid("{270F36E9-0EEF-4AD5-92D6-A826DB717621}");
                this.topicVarId = new Guid("{3D9C97BE-05BB-4AD7-9E9C-A98F876B0EEC}");

                //уведомление почтовой группы жалобы дд для работы
                this.reclamaitionsDDStaffGroupID = "{09430DDA-ED21-4D6D-9946-584C841F2C12}";

                this.reclamationsGroup = "Reclamations@digdes.com";

                this.secretaryDDSpbAHUGroup = "sekretarySPB@digdes.com";

                this.secretaryDDSpbAHUStaffEmails.Add("aleynikova.a@digdes.com");
                this.secretaryDDSpbAHUStaffEmails.Add("poritskaya.o@digdes.com");
                this.secretaryDDSpbAHUStaffEmails.Add("zhilina.e@digdes.com");
                this.secretaryDDSpbAHUStaffEmails.Add("popel.n@digdes.com");

                this.secretaryDDMscGroup = "SecretaryMP@digdes.com";

                this.secretaryDDMscStaffEmails.Add("bogatova.v@digdes.com");
                this.secretaryDDMscStaffEmails.Add("mozhaeva.t@digdes.com");

                this.TDODDGroup = "TDO_DD@digdes.com";

                this.TDODDExtraStaffEmails.Add("yureva.a@digdes.com");
                this.TDODDExtraStaffEmails.Add("sytenskikh.o@digdes.com");
            }
            else
            {
               

                //ID секции INCOMMING_SPB_CORR_CONTENT на тестовом сервере
                this.DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID = "{561F2088-9D27-4B01-BAF1-8D70E600728B}";

                //ID шаблона бизнес процесса уведомления на тесте
                this.BP_TEMPLATE_ID = "{BFAE58F5-CEFB-42C7-9812-031E8A3146BD}";

                //ID переменных БП на тесте
                this.addressesStrVarId = new Guid("{DE5AF7E8-1A73-4F00-A81E-7603297D3BF8}");
                this.filesNamesStrVarId = new Guid("{2C5D2647-D039-47ED-B9B5-D4938417952C}");
                this.filesFolderVarId = new Guid("{F07038E6-ADE4-4295-B886-3552757FE7FA}");
                this.textVarId = new Guid("{270F36E9-0EEF-4AD5-92D6-A826DB717621}");
                this.topicVarId = new Guid("{3D9C97BE-05BB-4AD7-9E9C-A98F876B0EEC}");

                //Тестовая группа ДУК технологи (для жалоб)
                this.reclamaitionsDDStaffGroupID = "{DC1AD9D0-8956-44FE-8D13-E5FD940BD068}";

                //this.reclamationsGroup = "technologs@digdes.com";
                this.reclamationsGroup = "automatization@digdes.com";

                /*this.reclamaitionsStaffEmails.Add("dolzhenkova.a@digdes.com");
                this.reclamaitionsStaffEmails.Add("dyadenko.e@digdes.com");
                this.reclamaitionsStaffEmails.Add("gorohova.g@digdes.com");
                this.reclamaitionsStaffEmails.Add("mihaylova.d@digdes.com");
                this.reclamaitionsStaffEmails.Add("prokoshina.e@digdes.com");
                this.reclamaitionsStaffEmails.Add("rasskazchikova.a@digdes.com");
                this.reclamaitionsStaffEmails.Add("semenova.v@digdes.com");
                this.reclamaitionsStaffEmails.Add("sorokina.a@digdes.com");
                this.reclamaitionsStaffEmails.Add("sorokina.n@digdes.com");
                this.reclamaitionsStaffEmails.Add("sutyrina.o@digdes.com");
                this.reclamaitionsStaffEmails.Add("varlamova.k@digdes.com");
                */

                this.secretaryDDSpbAHUGroup = "semenova.v@digdes.com";

                //this.secretaryDDSpbAHUStaffEmails.Add("Chernyh.T@digdes.com");

                this.secretaryDDMscGroup = "semenova.v@digdes.com";

                //this.secretaryDDMscStaffEmails.Add("Bogatova.V@digdes.com");

                this.TDODDGroup = "semenova.v@digdes.com";
                /*
                this.TDODDExtraStaffEmails.Add("Yureva.A@digdes.com");
                this.TDODDExtraStaffEmails.Add("Sytenskikh.O@digdes.com");*/
            }
        }
        public void WriteLogFile(String errorText)
        {
            string text = "";
            string line;
             StreamReader sr = new StreamReader(@"C:\Users\Ovchinnikov.R\logs\RegCorLogs.log");
            //Read the first line of text
            line = sr.ReadLine();
            //Continue to read until you reach end of file
            while (line != null)
            {
                text += line + "\n";
                //write the line to console window
                Console.WriteLine(line);
                //Read the next line
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();

            //Pass the filepath and filename to the StreamWriter Constructor
            string filePath = @"C:\Users\Ovchinnikov.R\logs\RegCorLogs.log";

            int retries = 5;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        fs.Seek(0, SeekOrigin.End); 
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            writer.WriteLine(text + errorText); 
                            writer.Close();
                        }
                    }
                    break; 
                }
                catch (IOException ex)
                {
                    Thread.Sleep(3000);
                }
            }
            
        }









        /// <summary>
        /// Метод для уведомления сотрудников и групп в полях "Уведомить"
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="cardID"></param>
        public String NotifyPersonsAndGroups(SessionContext sessionContext, String cardID)
        {
            try
            {
                string additionalInfo = "исключаем: spbOffice: ";
                foreach(string rml in secretaryDDSpbAHUStaffEmails)
                {
                    additionalInfo += rml + "; ";
                }

                WriteLogFile(DateTime.Now.ToString() + "\t" + "Отправляется уведомление по карточке с ID = " + cardID);
                /*logger.Info("info logger в карточке с ID=" + cardID);
                logger.Error("Типо ошибка с ID=" + cardID);*/
                List<string> secretaryGroupEmployeesEmails = new List<string>();
                String secretaryGroupEmail = "";
                List<string> notifyEmoloyeesAddresses = new List<string>();

                CardData incomming_SPbCardData = sessionContext.Session.CardManager.GetCardData(new Guid(cardID));

                RowDataCollection approvalersRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_APPROVALERS_SECTION_ID)].Rows;
                RowDataCollection spbContentRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;
                RowData mainInfoRow = incomming_SPbCardData.Sections[CardDocument.MainInfo.ID].FirstRow;
                RowData senderPartnerRow = incomming_SPbCardData.Sections[new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID)].FirstRow;
                RowDataCollection recieversRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_RECIEVERS_SECTION_ID)].Rows;

                String spbOfficeProd = "4ef88076-29dd-4705-90c3-6d6aa06224e9";
                if (mainInfoRow["Office"].ToString().ToLower().Contains(spbOfficeProd))
                {
                    secretaryGroupEmployeesEmails = secretaryDDSpbAHUStaffEmails;
                    secretaryGroupEmail = secretaryDDSpbAHUGroup;
                }
                else
                {
                    secretaryGroupEmployeesEmails = secretaryDDMscStaffEmails;
                    secretaryGroupEmail = secretaryDDMscGroup;
                }

                //Перечисления адресатов ТДО ДД для исключения повторений
                List<string> TDODDGroupStaffEmails = new List<string>();
                StaffGroup TDOGroup = sessionContext.ObjectContext.GetObject<StaffGroup>(new Guid("{9A92B169-CAB3-4D4B-83FE-C40D9C753761}"));

                additionalInfo += "---- TDO:  ";
                foreach (StaffEmployee emplItem in TDOGroup.Employees)
                {
                    TDODDGroupStaffEmails.Add(emplItem.Email.ToLower());

                    additionalInfo += emplItem.Email.ToLower() + "; ";
                }

                //Добавление адресатов при отсутсвии жалобы
                if (mainInfoRow["HaveComplaint"] == null || mainInfoRow["HaveComplaint"].ToString() == "False"|| mainInfoRow["NotReportComplaint"].ToString() == "True")
                {
                    foreach (RowData rowData in approvalersRows)
                    {
                        //Добавление отдельных пользователей из поля "Уведомить" за исключением уже добавленных и сотрудников из почтовых групп секретарем МСК и СПБ АХУ
                        if (rowData["Notify"] != null)
                        {
                            if(new Guid(rowData["Notify"].ToString()) != Guid.Empty)
                            {
                                StaffEmployee employee = sessionContext.ObjectContext.GetObject<StaffEmployee>(new Guid(rowData["Notify"].ToString()));
                                //Если Адрес сотрудника не указан в группе рассылки, Группак Секретарей и группе ТДО
                                if (!notifyEmoloyeesAddresses.Contains(employee.Email.ToLower()) && !secretaryGroupEmployeesEmails.Contains(employee.Email.ToLower()) &
                                    !TDODDGroupStaffEmails.Contains(employee.Email.ToLower()))
                                {
                                    notifyEmoloyeesAddresses.Add(employee.Email.ToLower());
                                }
                            }
                        }
                        //Добавление групп пользователей из поля "Уведомить" за исключением уже добавленных и сотрудников из почтовых групп секретарем МСК и СПБ АХУ
                        if (rowData["NotifyGroup"] != null)
                        {
                            if(new Guid(rowData["NotifyGroup"].ToString()) != Guid.Empty)
                            {
                                // Добавляем сотрудников из групп в адресаты
                                StaffGroup group = sessionContext.ObjectContext.GetObject<StaffGroup>(new Guid(rowData["NotifyGroup"].ToString()));
                                foreach (StaffEmployee emplItem in group.Employees)
                                {
                                    if (!notifyEmoloyeesAddresses.Contains(emplItem.Email.ToLower()) && !secretaryGroupEmployeesEmails.Contains(emplItem.Email.ToLower()) &
                                        !TDODDGroupStaffEmails.Contains(emplItem.Email.ToLower()))
                                    {
                                        notifyEmoloyeesAddresses.Add(emplItem.Email.ToLower());
                                    }
                                }
                            }
                        }
                    }

                    // Добавление группы ТДО  и дополнительных членов из группы в справочнике
                    notifyEmoloyeesAddresses.Add(TDODDGroup);
                    foreach (String email in TDODDExtraStaffEmails)
                    {
                        if (!notifyEmoloyeesAddresses.Contains(email.ToLower()))
                        {
                            notifyEmoloyeesAddresses.Add(email.ToLower());
                        }
                    }

                }
                //Добавление адресатов при наличии жалобы
                else
                {
                    notifyEmoloyeesAddresses.Add(reclamationsGroup);

                    //Перечисления адресатов ДУК технологи для исключения повторений
                    IStaffService staffService = sessionContext.ObjectContext.GetService<IStaffService>();
                    StaffGroup technologsDDGroup = staffService.GetGroup(new Guid(reclamaitionsDDStaffGroupID));
                    foreach (StaffEmployee emplItem in technologsDDGroup.Employees)
                    {
                        this.reclamaitionsStaffEmails.Add(emplItem.Email.ToLower());
                    }

                    additionalInfo += "----- Reclam:  ";
                    foreach(string eml in this.reclamaitionsStaffEmails)
                    {
                        additionalInfo += eml + "; ";
                    }

                    foreach (RowData rowData in approvalersRows)
                    {
                        //Добавление отдельных пользователей из поля "Уведомить" за исключением уже добавленных и сотрудников из почтовых групп секретарем МСК, СПБ АХУ и жалобы ДД
                        if (rowData["Notify"] != null)
                        {
                            if(new Guid(rowData["Notify"].ToString()) != Guid.Empty)
                            {
                                StaffEmployee employee = sessionContext.ObjectContext.GetObject<StaffEmployee>(new Guid(rowData["Notify"].ToString()));

                                if (!reclamaitionsStaffEmails.Contains(employee.Email.ToLower()) && !notifyEmoloyeesAddresses.Contains(employee.Email.ToLower()) 
                                    && !secretaryGroupEmployeesEmails.Contains(employee.Email.ToLower()) &
                                        !TDODDGroupStaffEmails.Contains(employee.Email.ToLower()))
                                {
                                    notifyEmoloyeesAddresses.Add(employee.Email.ToLower());
                                }
                            }
                        }
                        //Добавление групп пользователей из поля "Уведомить" за исключением уже добавленных и сотрудников из почтовых групп жалобы ДД и секретарей МСК и СПБ АХУ
                        if (rowData["NotifyGroup"] != null)
                        {
                            if(new Guid(rowData["NotifyGroup"].ToString()) != Guid.Empty)
                            {
                                // Добавляем сотрудников из групп в адресаты
                                StaffGroup group = sessionContext.ObjectContext.GetObject<StaffGroup>(new Guid(rowData["NotifyGroup"].ToString()));
                                foreach (StaffEmployee emplItem in group.Employees)
                                {
                                    if (!notifyEmoloyeesAddresses.Contains(emplItem.Email.ToLower()))
                                    {
                                        if (!reclamaitionsStaffEmails.Contains(emplItem.Email.ToLower()) && !secretaryGroupEmployeesEmails.Contains(emplItem.Email.ToLower()) &
                                                !TDODDGroupStaffEmails.Contains(emplItem.Email.ToLower()))
                                        {
                                            notifyEmoloyeesAddresses.Add(emplItem.Email.ToLower());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Добавление группы ТДО  и дополнительных членов из группы в справочнике
                    notifyEmoloyeesAddresses.Add(TDODDGroup);
                    foreach (String email in TDODDExtraStaffEmails)
                    {
                        if (!notifyEmoloyeesAddresses.Contains(email.ToLower()) & !reclamaitionsStaffEmails.Contains(email.ToLower()))
                        {
                            notifyEmoloyeesAddresses.Add(email.ToLower());
                        }
                    }

                    //Проверка на наличие ТДО в адресатах
                    /*
                    StaffGroup TDOGroup = sessionContext.ObjectContext.GetObject<StaffGroup>(new Guid("{9A92B169-CAB3-4D4B-83FE-C40D9C753761}"));
                    foreach (StaffEmployee emplItem in TDOGroup.Employees)
                    {
                        if (!notifyEmoloyeesAddresses.Contains(emplItem.Email))
                        {
                            if (!reclamaitionsStaffEmails.Contains(emplItem.Email.ToLower()))
                            {
                                notifyEmoloyeesAddresses.Add(emplItem.Email);
                            }
                        }
                    }*/
                }

                //Добавление почтовых групп  секретарей МСК или СПБ АХУ
                notifyEmoloyeesAddresses.Add(secretaryGroupEmail);

                this.reclamaitionsStaffEmails.Clear();
                /*this.reclamaitionsStaffEmails.Clear();
                this.TDODDExtraStaffEmails.Clear();
                this.secretaryDDMscStaffEmails.Clear();
                this.secretaryDDSpbAHUStaffEmails.Clear();*/

                //Создание письма если есть кому посылать
                if (notifyEmoloyeesAddresses.Count > 0)
                {
                    string recieversFullName = "";
                    try
                    {
                        foreach(var recieversRow in recieversRows)
                        {
                            StaffEmployee reciever = sessionContext.ObjectContext.GetObject<StaffEmployee>(new Guid(recieversRow["ReceiverStaff"].ToString()));
                            recieversFullName += reciever.FullName + "; ";
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile(DateTime.Now.ToString() + "\n\tНе критическая ошибка - отсутствует получатель\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                    }
                    
                    PartnersCompany partnerSender = sessionContext.ObjectContext.GetObject<PartnersCompany>(new Guid(senderPartnerRow["SenderOrg"].ToString()));

                    String number = "";
                    RowDataCollection numbersRows = incomming_SPbCardData.Sections[CardDocument.Numbers.ID].Rows;

                    foreach (RowData numberR in numbersRows)
                    {
                        if (numberR["RowID"].ToString() == mainInfoRow["IncommingNumber"].ToString())
                        {
                            number = numberR["Number"].ToString();
                        }
                    }










                    /////////////////////////////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////////












                    String linkHtml = "http://ddsm-dvnew.digdes.com:81/DocsvisionWebClient/#/CardView/" + incomming_SPbCardData.Id.ToString();
                    String text = "<html><head><style type=\"text/css\">td{border:1px solid black;}</style></head><body>Здравствуйте!<br/><br/>В Компанию поступил документ " + number + " от организации " +
                        partnerSender.Name + ": " + "<a href=\"" + linkHtml + "\">" + linkHtml + "</a><br/>Содержание: <br/>";

                    if (spbContentRows.Count != 0)
                    {
                        text += "<table width=\"80%\" cellspacing=\"-1\">";

                        foreach (RowData contentTableRow in spbContentRows)
                        {
                            string docTypeStr = "";
                            string identityDataStr = "";
                            try
                            {
                                BaseUniversalItem docType = sessionContext.ObjectContext.GetObject<BaseUniversalItem>(new Guid(contentTableRow["DocumentType"].ToString()));
                                docTypeStr = docType.Name;
                                identityDataStr = contentTableRow["IdentityData"].ToString();
                            }
                            catch (NullReferenceException ex)
                            {
                                WriteLogFile(DateTime.Now.ToString() + "\n\tНе критическая ошибка" + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                            }

                            text += "<tr><td>" + docTypeStr + "</td><td>" + identityDataStr + "</td></tr>";
                        }
                        text += "</table>";
                    }

                    text += "<br/>Документ будет передан:" + recieversFullName + "</body></html>";

                    string topicText = "Входящая корресподенция. ";
                    topicText += partnerSender.Name + "; " + number;

                    SendMessage(notifyEmoloyeesAddresses, sessionContext, incomming_SPbCardData, topicText);
                    
                    string notifiedPersTXT = "notAdr: ";
                    foreach (string adr in notifyEmoloyeesAddresses)
                    {
                        notifiedPersTXT += adr + "; ";
                    }
                    //notifiedPersTXT += "----- " + additionalInfo;
                    return notifiedPersTXT;
                }
                else return "НЕкому посылать, ошибки нет. ";
            }
            catch(Exception ex)
            {
                this.reclamaitionsStaffEmails.Clear();
                /*
                this.reclamaitionsStaffEmails.Clear();
                this.TDODDExtraStaffEmails.Clear();
                this.secretaryDDMscStaffEmails.Clear();
                this.secretaryDDSpbAHUStaffEmails.Clear();*/


                WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                return ex.StackTrace;
            }
        }
        
        /// <summary>
        /// Метод, проверяющий наличие администраторов в графе "получатели" (необходимо для автозаполнения поля "уведомить")
        /// </summary>
        /// <param name="reciversIDs"></param>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        public Boolean HaveAdminsStaff(String[] reciversIDs, SessionContext sessionContext)
        {
            Guid directorsDDGroupID = new Guid("9F4D9D27-2433-4144-8D5F-D3572608C52F");
            StaffGroup adminsGroup = sessionContext.ObjectContext.GetObject<StaffGroup>(directorsDDGroupID);
            foreach (String emplIdStr in reciversIDs)
            {
                Guid emplId = new Guid(emplIdStr);
                if (adminsGroup.EmployeesIds.Contains(emplId)) return true;
            }

            return false;
        }
        /// <summary>
        /// Метод, ищущий похожие карточки жалобы и заполняющий поле "дополнительные сведения" при необходимости
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="cardID"></param>
        /// <returns></returns>
        public String ReturnOriginalSameComplaintDate(SessionContext sessionContext, String cardID)
        {
            try
            {
                const String DOCUMENT_TYPE_ID = "{B9F7BFD7-7429-455E-A3F1-94FFB569C794}";

                CardData incomming_SPbCardData = sessionContext.Session.CardManager.GetCardData(new Guid(cardID));

                RowData senderPartnerRow = incomming_SPbCardData.Sections[new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID)].FirstRow;
                RowData mainInfoRow = incomming_SPbCardData.Sections[CardDocument.MainInfo.ID].FirstRow;
                RowDataCollection filesRows = incomming_SPbCardData.Sections[CardDocument.Files.ID].Rows;
                RowDataCollection spbContentRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;

                if (mainInfoRow["ReportComplaintAgain"].ToString() == "False")
                {
                        return "null";
                }

                SearchQuery searchQuery = sessionContext.Session.CreateSearchQuery();

                CardTypeQuery doumentQuery = searchQuery.AttributiveSearch.CardTypeQueries.AddNew(new Guid(DOCUMENT_TYPE_ID));

                SectionQuery senderPartnerQuery = doumentQuery.SectionQueries.AddNew(new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID));
                senderPartnerQuery.ConditionGroup.Operation = ConditionGroupOperation.And;
                senderPartnerQuery.ConditionGroup.Conditions.AddNew("SenderOrg", FieldType.RefId, ConditionOperation.Equals, senderPartnerRow["SenderOrg"]);

                senderPartnerQuery.Operation = SectionQueryOperation.And;

                SectionQuery mainInfoQuery = doumentQuery.SectionQueries.AddNew(CardDocument.MainInfo.ID);
                mainInfoQuery.ConditionGroup.Operation = ConditionGroupOperation.And;
                if (mainInfoRow["OutgoingPartnerNumberString"] == null)
                {
                    mainInfoQuery.ConditionGroup.Conditions.AddNew("OutgoingPartnerNumberString", FieldType.String, ConditionOperation.IsNull, mainInfoRow["OutgoingPartnerNumberString"]);
                }
                else if (mainInfoRow["OutgoingPartnerNumberString"].ToString() == "")
                {
                    mainInfoQuery.ConditionGroup.Conditions.AddNew("OutgoingPartnerNumberString", FieldType.String, ConditionOperation.Equals, "");
                }
                else
                {
                    mainInfoQuery.ConditionGroup.Conditions.AddNew("OutgoingPartnerNumberString", FieldType.String, ConditionOperation.Equals, mainInfoRow["OutgoingPartnerNumberString"]);
                }
                mainInfoQuery.ConditionGroup.Conditions.AddNew("HaveComplaint", FieldType.Bool, ConditionOperation.Equals, true);


                //Получение текста запроса
                string query = searchQuery.GetXml();
                CardDataCollection sameCards = sessionContext.Session.CardManager.FindCards(query);

                if (sameCards.Count != 0)
                {
                    DateTime minDate = DateTime.MaxValue;
                    foreach (CardData sameCard in sameCards)
                    {
                        if (sameCard.Id.Equals(new Guid(cardID))) continue;

                        bool haveDifferentContentRows = false;
                        bool haveSameRow;
                        RowDataCollection sameCardsSpbContentRows = sameCard.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;
                        if (sameCardsSpbContentRows.Count != spbContentRows.Count)
                        {
                            continue;
                        }

                        foreach (RowData sameCardsContentRow in sameCardsSpbContentRows)
                        {
                            if (haveDifferentContentRows) break;

                            haveSameRow = false;
                            foreach (RowData contentRow in spbContentRows)
                            {
                                if(contentRow["IdentityData"]!= null && sameCardsContentRow["IdentityData"]!= null && contentRow["DocumentType"]!=null && sameCardsContentRow["DocumentType"]!= null)
                                {
                                    if (contentRow["IdentityData"].ToString() == sameCardsContentRow["IdentityData"].ToString() && contentRow["DocumentType"].ToString() == sameCardsContentRow["DocumentType"].ToString())
                                    {
                                        haveSameRow = true;
                                        break;
                                    }
                                }
                            }
                            if (!haveSameRow)
                            {
                                haveDifferentContentRows = true;
                            }
                        }

                        if (haveDifferentContentRows) continue;

                        if (sameCard.CreateDate < minDate)
                        {
                            RowDataCollection comparableFilesRows = sameCard.Sections[CardDocument.Files.ID].Rows;
                            try
                            {
                                if (comparableFilesRows.Count != filesRows.Count)
                                {
                                    continue;
                                }
                                else if (comparableFilesRows.Count > 0)
                                {
                                    if (EqualFiles(filesRows, comparableFilesRows, cardID, sameCard.Id.ToString(), sessionContext))
                                    {
                                        minDate = sameCard.CreateDate;
                                    }
                                }
                                else minDate = sameCard.CreateDate;
                            }
                            catch (Exception ex)
                            {
                                WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                                return ("Ошибка при сравнении файлов: " + ex.Message);
                            }
                        }
                    }
                    if (minDate != DateTime.MaxValue && minDate != null)
                    {
                        try
                        {
                            if (mainInfoRow["Content"] != null )
                            {
                                if(mainInfoRow["Content"].ToString() != "")
                                {
                                    mainInfoRow["Content"] = mainInfoRow["Content"].ToString() + "\nОригинал получен " + minDate.ToString();
                                }
                                else
                                {
                                    mainInfoRow["Content"] = "Оригинал получен " + minDate.ToString();
                                }
                            }
                            else
                            {
                                mainInfoRow["Content"] = "Оригинал получен " + minDate.ToString();
                            }

                            //logger.Info("Найдена похожая карточка при жалобе в карточке с ID=" + cardID);
                            return "Найдена похожая карточка";
                        }
                        catch (Exception ex)
                        {
                            WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                            return ("Ошибка при присвоении значения полю Content: " + ex.Message);
                        }
                    }
                }
                if(mainInfoRow["NotReportComplaint"] == null)
                {
                    //уведомляю топ-менеджмент при новой жалобе
                    string responce = NotifyTopManagement(sessionContext, incomming_SPbCardData);
                    if (responce != "OK")
                    {
                        return responce;
                    }

                    /*logger.Debug("Отправлена жалоба от карты с ID=" + cardID);
                    logger.Error("Отправлена жалоба от карты с ID=" + cardID);*/
                    return "Похожих карт нет. Топ-менеджмент уведомлен";
                }
                else if(mainInfoRow["NotReportComplaint"].ToString() == "False")
                {
                    string responce = NotifyTopManagement(sessionContext, incomming_SPbCardData);
                    if(responce != "OK")
                    {
                        return responce;
                    }

                    /*logger.Debug("Отправлена жалоба от карты с ID=" + cardID);
                    logger.Error("Отправлена жалоба от карты с ID=" + cardID);*/
                    return "Похожих карт нет. Топ-менеджмент уведомлен";
                }
                else
                {
                    return "Похожих карт нет. Уведомление не производится";
                }
                
            }
            catch(Exception ex)
            {
                WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t " + ex.Message);
                return ex.Message + "; " + ex.StackTrace;
            }
        }
        /// <summary>
        /// Метод, проверяющий эквивалентность всех файлов одной карточки всем файлам другой карточки
        /// </summary>
        /// <param name="files1Rows"></param>
        /// <param name="files2Rows"></param>
        /// <param name="card1ID"></param>
        /// <param name="card2ID"></param>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        private bool EqualFiles(RowDataCollection files1Rows, RowDataCollection files2Rows, String card1ID, String card2ID, SessionContext sessionContext)
        {
            IVersionedFileCardService versionedFileCardService = sessionContext.ObjectContext.GetService<IVersionedFileCardService>();
            
            //Получаем файлы для вложения 
            string directoryPath = @"C:\Файлы для вложения БП уведомления";
            string filesFolder1 = Directory.CreateDirectory(directoryPath + @"\" + card1ID).FullName;
            string filesFolder2 = Directory.CreateDirectory(directoryPath + @"\" + card2ID).FullName;
            string directoryPath1 = filesFolder1 + @"\";
            string directoryPath2 = filesFolder2 + @"\";

            string filePath1 = "";
            string filePath2 = "";
            //debugPro
            string fileN1 = "";
            string fileN2 = "";

            bool haveOneSameFile;

            foreach (RowData fаiles1RowItem in files1Rows)
            {
                VersionedFileCard vfCard1 = GetFileVersionCard(fаiles1RowItem, sessionContext);
                haveOneSameFile = false;

                foreach (RowData fаiles2RowItem in files2Rows)
                {
                    VersionedFileCard vfCard2 = GetFileVersionCard(fаiles2RowItem, sessionContext);
                    ///////////////////////
                    fileN1 = vfCard1.Name;
                    fileN2 = vfCard2.Name;
                    /////////////////////////////

                    if (vfCard2.CheckoutPath != null)
                    {
                        filePath2 = vfCard2.CheckoutPath;

                        if (vfCard1.CheckoutPath != null)
                        {
                            filePath1 = vfCard1.CheckoutPath;
                        }
                        else
                        {
                            filePath1 = versionedFileCardService.CheckOut(vfCard1);
                            versionedFileCardService.UndoCheckOut(vfCard1);
                        }
                    }
                    else
                    {
                        filePath2 = versionedFileCardService.CheckOut(vfCard2);
                        versionedFileCardService.UndoCheckOut(vfCard2);

                        if (vfCard1.CheckoutPath != null)
                        {
                            filePath1 = vfCard1.CheckoutPath;
                        }
                        else
                        {
                            filePath1 = versionedFileCardService.CheckOut(vfCard1);
                            versionedFileCardService.UndoCheckOut(vfCard1);
                        }
                    }
                    if (FileCompare(filePath1, filePath2))
                    {
                        haveOneSameFile = true;
                        break;
                    }
                }
                if(!haveOneSameFile) return false;
            }
            return true;
        }
        /// <summary>
        /// Метод возвращающий карточку файла с версиями
        /// </summary>
        /// <param name="files1Row"></param>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        private VersionedFileCard GetFileVersionCard(RowData files1Row, SessionContext sessionContext)
        {
            IVersionedFileCardService versionedFileCardService = sessionContext.ObjectContext.GetService<IVersionedFileCardService>();
            VersionedFileCard vfCard1 = versionedFileCardService.OpenCard(new Guid(files1Row["FileId"].ToString()));

            return vfCard1;
        }
        /// <summary>
        /// Метод для уведомления топ-менеджмента
        /// </summary>
        /// <param name="sessionContext"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private string NotifyTopManagement(SessionContext sessionContext, CardData card)
        {
            try
            {
                RowDataCollection approvalersRows = card.Sections[new Guid(DOCUMENT_APPROVALERS_SECTION_ID)].Rows;
                RowDataCollection spbContentRows = card.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;
                RowData mainInfoRow = card.Sections[CardDocument.MainInfo.ID].FirstRow;
                RowData senderPartnerRow = card.Sections[new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID)].FirstRow;
                RowDataCollection recieversRows = card.Sections[new Guid(DOCUMENT_RECIEVERS_SECTION_ID)].Rows;


                //переменные для уведомления почтовой группы "жалобы дд"
                List<string> notifyManagersAddresses = new List<string>();

                String number = "";
                RowDataCollection numbersRows = card.Sections[CardDocument.Numbers.ID].Rows;
                foreach (RowData numberR in numbersRows)
                {
                    if (numberR["RowID"].ToString() == mainInfoRow["IncommingNumber"].ToString())
                    {
                        number = numberR["Number"].ToString();
                    }
                }

                String recieversFullName = "";
                try
                {
                    foreach(var recieversRow in recieversRows)
                    {
                        StaffEmployee reciever = sessionContext.ObjectContext.GetObject<StaffEmployee>(new Guid(recieversRow["ReceiverStaff"].ToString()));
                        recieversFullName += reciever.FullName + "; ";
                    }
                }
                catch(Exception ex)
                {
                    WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                }

                PartnersCompany partnerSender = sessionContext.ObjectContext.GetObject<PartnersCompany>(new Guid(senderPartnerRow["SenderOrg"].ToString()));

                String linkHtml = "http://ddsm-dvnew.digdes.com:81/DocsvisionWebClient/#/CardView/" + card.Id.ToString();
                String text = "<html><head><style type=\"text/css\">td{border:1px solid black;}</style></head><body>Здравствуйте!<br/><br/>В Компании зарегистрирована новая жалоба " + number + " от организации " +
                    partnerSender.Name + ": " + "<a href=\"" + linkHtml + "\">" + linkHtml + "</a><br/>Содержание: <br/>";

                if (spbContentRows.Count != 0)
                {
                    text += "<table width=\"80%\" cellspacing=\"-1\">";

                    foreach (RowData contentTableRow in spbContentRows)
                    {
                        BaseUniversalItem docType = null;
                        string docTypeStr;
                        try
                        {
                            docType = sessionContext.ObjectContext.GetObject<BaseUniversalItem>(new Guid(contentTableRow["DocumentType"].ToString()));
                            docTypeStr = docType.Name;
                        }
                        catch(Exception ex)
                        {
                            WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                            docTypeStr = "Не удалось определить тип документа";
                        }
                        
                        text += "<tr><td>" + docTypeStr + "</td><td>" + contentTableRow["IdentityData"].ToString() + "</td></tr>";
                    }
                    text += "</table>";
                }


                text += "<br/>Документ будет передан: " + recieversFullName + "</body></html>";

                string topicText = "Входящая корресподенция. ";
                topicText += partnerSender.Name + "; " + number;

                notifyManagersAddresses.Add(reclamationsGroup);

                SendMessage(notifyManagersAddresses, sessionContext, card, topicText);
                return "OK";
            }
            catch(Exception ex)
            {
                WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                return ex.StackTrace;   
            }
            
        }
        /// <summary>
        /// Метод для уведомления перонала в полях уведомить и уведомить группу
        /// </summary>
        /// <param name="adressesStr"></param>
        /// <param name="sessionContext"></param>
        /// <param name="card"></param>
        private String SendMessage(List<string> adressesList, SessionContext sessionContext, CardData card,/* string text,*/ string topicText)
        {
            try
            {
                String text = "";
                RowDataCollection numbersRows = card.Sections[CardDocument.Numbers.ID].Rows;
                RowData mainInfoRow = card.Sections[CardDocument.MainInfo.ID].FirstRow;
                String number = "";

                foreach (RowData numberR in numbersRows)
                {
                    if (numberR["RowID"].ToString() == mainInfoRow["IncommingNumber"].ToString())
                    {
                        number = numberR["Number"].ToString();
                    }
                }

                if (mainInfoRow["HaveComplaint"].ToString() == "True")
                {
                    text = GetComplaintNotifyText(sessionContext.ObjectContext, card, number);
                }
                else
                {
                    text = GetNotifyText(sessionContext.ObjectContext, card, number);
                }

                List<String> filesNames = new List<String>();
                String filesFolder = "";

                //Получаем файлы для вложения 
                IVersionedFileCardService versionedFileCardService = sessionContext.ObjectContext.GetService<IVersionedFileCardService>();
                string directoryPath = @"C:\Файлы для вложения БП уведомления";
                filesFolder = Directory.CreateDirectory(directoryPath + @"\" + card.Id).FullName;
                directoryPath = filesFolder + @"\";

                long filesSize = 0;

                foreach (RowData fаilesRow in card.Sections[CardDocument.Files.ID].Rows)
                {
                    VersionedFileCard vfCard = GetFileVersionCard(fаilesRow, sessionContext);
                    string tmpPath = "";
                    try
                    {
                        tmpPath = versionedFileCardService.Download(vfCard);
                        FileInfo fileInf = new FileInfo(tmpPath);
                        filesSize += fileInf.Length;

                        File.Delete(tmpPath);
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                    }
                }


                if(filesSize < 41900000)
                {
                    foreach (RowData fаilesRow in card.Sections[CardDocument.Files.ID].Rows)
                    {
                        VersionedFileCard vfCard = GetFileVersionCard(fаilesRow, sessionContext);
                        string tmpPath = "";
                        try
                        {
                            tmpPath = versionedFileCardService.CheckOut(vfCard);
                            versionedFileCardService.UndoCheckOut(vfCard);
                            Directory.Move(tmpPath, directoryPath + vfCard.Name);
                        }
                        catch (Exception ex)
                        {
                            WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                            tmpPath = vfCard.CheckoutPath;
                        }

                        filesNames.Add(vfCard.Name);
                    }
                }
                else
                {
                    string oldText = "</body></html>";
                    string newText = "<br/><br/><em>PS: Общий размер файлов превышает допустимые размеры вложений. Вложения можно посмотреть в карточке документа на вкладке «Документы и ссылки», перейдя по ссылке выше.</em></body></html>";
                    text = text.Replace(oldText, newText);
                }
                
                //Создание БП

                Library library = new Library(sessionContext.Session);
                Process template = library.GetProcess(new Guid(BP_TEMPLATE_ID));
                Process process = library.CreateProcess(template);
                process.InitialDocument = card.Id.ToString();
                string addressesValueStr = "";

                foreach (string item in adressesList)
                {
                    addressesValueStr += item + ",";
                }

                addressesValueStr = addressesValueStr.Trim(',');

                if (addressesValueStr.Length > 0)
                {
                    process.Variables[addressesStrVarId].Value = addressesValueStr;
                }

                process.Variables[filesFolderVarId].Value = filesFolder;
                process.Variables[filesNamesStrVarId].Value = String.Join("/", filesNames);
                process.Variables[textVarId].Value = text;
                process.Variables[topicVarId].Value = topicText;

                // Запуск экземпляра бизнес-процесса
                process.Start(sessionContext.Session.Properties["AccountName"].Value.ToString(), library.Dictionary, ExecutionModeEnum.Automatic, true);

                return "Сообщение отправлено";
            }
            catch (Exception ex)
            {
                WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                throw ex;
            }
        }

        private String GetNotifyText(ObjectContext objCtxt,CardData incomming_SPbCardData, String number)
        {
            RowDataCollection recieversRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_RECIEVERS_SECTION_ID)].Rows;
            RowDataCollection spbContentRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;
            RowData senderPartnerRow = incomming_SPbCardData.Sections[new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID)].FirstRow;

            string recieversFullName = "";
            try
            {
                foreach (var recieversRow in recieversRows)
                {
                    StaffEmployee reciever = objCtxt.GetObject<StaffEmployee>(new Guid(recieversRow["ReceiverStaff"].ToString()));
                    recieversFullName += reciever.FullName + "; ";
                }

            }
            catch (Exception ex)
            {
                WriteLogFile(DateTime.Now.ToString() + "\n\tНе критическая ошибка - отсутствует получатель\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
            }

            
            PartnersCompany partnerSender = objCtxt.GetObject<PartnersCompany>(new Guid(senderPartnerRow["SenderOrg"].ToString()));

            String linkHtml = "http://ddsm-dvnew.digdes.com:81/DocsvisionWebClient/#/CardView/" + incomming_SPbCardData.Id.ToString();
            String text = "<html><head><style type=\"text/css\">td{border:1px solid black;}</style></head><body>Здравствуйте!<br/><br/>В Компанию поступил документ " + number + " от организации " +
                partnerSender.Name + ": " + "<a href=\"" + linkHtml + "\">" + linkHtml + "</a><br/>Содержание: <br/>";

            if (spbContentRows.Count != 0)
            {
                text += "<table width=\"80%\" cellspacing=\"-1\">";

                foreach (RowData contentTableRow in spbContentRows)
                {
                    string docTypeStr = "";
                    string identityDataStr = "";
                    try
                    {
                        BaseUniversalItem docType = objCtxt.GetObject<BaseUniversalItem>(new Guid(contentTableRow["DocumentType"].ToString()));
                        docTypeStr = docType.Name;
                        identityDataStr = contentTableRow["IdentityData"].ToString();
                    }
                    catch (NullReferenceException ex)
                    {
                        WriteLogFile(DateTime.Now.ToString() + "\n\tНе критическая ошибка" + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                    }

                    text += "<tr><td>" + docTypeStr + "</td><td>" + identityDataStr + "</td></tr>";
                }
                text += "</table>";
            }

            text += "<br/>Документ будет передан:" + recieversFullName + "</body></html>";

            return text;
        }
        private String GetComplaintNotifyText(ObjectContext objCtxt, CardData incomming_SPbCardData, String number)
        {
            RowDataCollection recieversRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_RECIEVERS_SECTION_ID)].Rows;
            RowDataCollection spbContentRows = incomming_SPbCardData.Sections[new Guid(DOCUMENT_INCOMMING_SPB_CORR_CONTENT_SECTION_ID)].Rows;
            RowData senderPartnerRow = incomming_SPbCardData.Sections[new Guid(DOCUMENT_SENDER_PARTNER_SECTION_ID)].FirstRow;

            string recieversFullName = "";
            try
            {
                foreach (var recieversRow in recieversRows)
                {
                    StaffEmployee reciever = objCtxt.GetObject<StaffEmployee>(new Guid(recieversRow["ReceiverStaff"].ToString()));
                    recieversFullName += reciever.FullName + "; ";
                }

            }
            catch (Exception ex)
            {
                WriteLogFile(DateTime.Now.ToString() + "\n\tНе критическая ошибка - отсутствует получатель\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
            }


            PartnersCompany partnerSender = objCtxt.GetObject<PartnersCompany>(new Guid(senderPartnerRow["SenderOrg"].ToString()));

            String linkHtml = "http://ddsm-dvnew.digdes.com:81/DocsvisionWebClient/#/CardView/" + incomming_SPbCardData.Id.ToString();
            String text = "<html><head><style type=\"text/css\">td{border:1px solid black;}</style></head><body>Здравствуйте!<br/><br/>В Компании зарегистрирована новая жалоба " + number + " от организации " +
                partnerSender.Name + ": " + "<a href=\"" + linkHtml + "\">" + linkHtml + "</a><br/>Содержание: <br/>";

            if (spbContentRows.Count != 0)
            {
                text += "<table width=\"80%\" cellspacing=\"-1\">";

                foreach (RowData contentTableRow in spbContentRows)
                {
                    BaseUniversalItem docType = null;
                    string docTypeStr;
                    try
                    {
                        docType = objCtxt.GetObject<BaseUniversalItem>(new Guid(contentTableRow["DocumentType"].ToString()));
                        docTypeStr = docType.Name;
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile(DateTime.Now.ToString() + "\n\t" + ex.StackTrace + "\n\t  " + ex.Message);
                        docTypeStr = "Не удалось определить тип документа";
                    }

                    text += "<tr><td>" + docTypeStr + "</td><td>" + contentTableRow["IdentityData"].ToString() + "</td></tr>";
                }
                text += "</table>";
            }


            text += "<br/>Документ будет передан: " + recieversFullName + "</body></html>";
            return text;
        }
        /// <summary>
        /// Метод сравнения файлов
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            //если один и тот же файл
            if (file1 == file2)
            {
                return true;
            }

            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            //сравнение размеров
            if (fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();

                return false;
            }

            //побитовое сравнение файлов
            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            fs1.Close();
            fs2.Close();

            return ((file1byte - file2byte) == 0);
        }
    }
}