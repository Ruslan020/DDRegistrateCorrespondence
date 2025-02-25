using System;
using System.Linq;
using System.Windows.Forms;
using DocsVision.Platform.ObjectManager;
using DocsVision.Platform.ObjectManager.SearchModel;
using DocsVision.Platform.ObjectManager.SystemCards;

using DocsVision.Platform.ObjectModel;
using DocsVision.Platform.ObjectModel.Search;
using DocsVision.Platform.WinForms;
using DocsVision.Platform.CardHost;

using DocsVision.Platform.ObjectModel.Mapping;
using DocsVision.ApprovalDesigner.ObjectModel.Mapping;
using DocsVision.DocumentsManagement.ObjectModel.Services;

using DocsVision.BackOffice.WinForms;
using DocsVision.BackOffice.WinForms.Controls;
using DocsVision.BackOffice.WinForms.Design.LayoutItems;
using DocsVision.BackOffice.ObjectModel;
using DocsVision.BackOffice.ObjectModel.Services;
using DocsVision.BackOffice.CardLib.CardDefs;
using System.Collections.Generic;

using DocsVision.Workflow.Objects;
using DocsVision.BackOffice.WinForms.Design.PropertyControls;

using BrovserProcess = System.Diagnostics;

namespace BackOffice
{

    public class CardDocumentПротоколScript : DocumentSRScript
    {
        private bool isCancel = false;
        private string oldNumber = "";

        #region Properties

        #endregion

        #region Methods
        // Нелюбин И.В. 20171206		
        public Guid UnitFromFolder()
        {

            FolderCard folderCard = (FolderCard)this.Session.CardManager.GetDictionary(new Guid("DA86FABF-4DD7-4A86-B6FF-C58C24D12DE2"));
            ShortcutCollection shortcutCollection = folderCard.GetShortcuts(this.CardData.Id);
            CardData UniCard = this.Session.CardManager.GetDictionaryData(Guid.Parse("4538149D-1FC7-4D41-A104-890342C6B4F8"));
            SectionData UniSection = UniCard.Sections[Guid.Parse("A1DCE6C1-DB96-4666-B418-5A075CDB02C9")];
            RowData TirRow = UniSection.FindRow("@Name=\"Тиражирование\"");
            RowData SettingRow = TirRow.ChildRows.Find("Name", "Настройка организаций");
            SubSectionData SettingSection = SettingRow.ChildSections[Guid.Parse("1B1A44FB-1FB1-4876-83AA-95AD38907E24")];
            foreach (RowData CurrentRow in SettingSection.Rows)
            {
                Guid CardID = (Guid)CurrentRow.GetGuid("ItemCard");
                CardData ExtSettingCard = this.Session.CardManager.GetCardData(CardID);
                RowData ExtSettingRow = ExtSettingCard.Sections[Guid.Parse("{F5641A7E-83AF-4C20-9C60-EA2973C4F135}")].Rows[0];
                if (ExtSettingRow.GetGuid("Папка").HasValue)
                {
                    Folder folder;
                    try
                    {
                        folder = folderCard.GetFolder((Guid)ExtSettingRow.GetGuid("Папка"));
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    //Folder folder = folderCard.GetFolder((Guid)ExtSettingRow.GetGuid("Папка"));
                    foreach (DocsVision.Platform.ObjectManager.SystemCards.Shortcut shortcut in shortcutCollection)
                    {
                        try
                        {
                            Folder ShortcutFolder = folderCard.GetFolder(shortcut.FolderId);
                            Folder UnitFolder = ShortcutFolder.ParentFolder;
                            if (UnitFolder.Id == folder.Id)
                            {
                                //   DV-1260
                                if (folder.Id == new Guid("1E1A307F-B910-4676-BB26-57D2A6606785") || folder.Id == new Guid("64CF7845-246D-412A-A9EF-97B0AC64D979") | folder.Id == new Guid("16552AE1-597B-4969-9F30-A5B47E9A07E0"))
                                {
                                    ICustomizableControl customizable = CardControl;
                                    ILayoutPropertyItem departmentSUEK = customizable.FindPropertyItem<ILayoutPropertyItem>("SUEKDepartment");
                                    return new Guid(departmentSUEK.ControlValue.ToString());
                                }
                                //   DV-1260
                                Guid DepartmentID = (Guid)ExtSettingRow.GetGuid("Организация");
                                //для теста возможно не пригодится при вставке будем возвращать ID
                                /*
	                            CardData UserCard = (CardData)this.Session.CardManager.GetDictionaryData(Guid.Parse("6710B92A-E148-4363-8A6F-1AA0EB18936C"));
	                            RowData DepartmentRow = (RowData)UserCard.Sections[Guid.Parse("7473F07F-11ED-4762-9F1E-7FF10808DDD1")].GetRow(DepartmentID);
	                            */
                                return DepartmentID;
                            }
                        }

                        catch
                        {
                            continue;
                        }
                    }
                }

            }

            return Guid.Empty;
        }


        private void SetREGSUEK()
        {
            //Нелюбин И.В. 20171212 

            ICustomizableControl customizable = CardControl;
            ILayoutPropertyItem departmentSUEK = customizable.FindPropertyItem<ILayoutPropertyItem>("SUEKDepartment");
            Guid FolderID = UnitFromFolder();
            if (FolderID != Guid.Empty)
            {
                departmentSUEK.ControlValue = FolderID;
                departmentSUEK.Commit();
            }
            else
            {
                MessageBox.Show("Невозможно определить Ваше предприятие \n\r Связь между папкой и предприятием отсутствует в справочнике");
            }

        }

        // Нелюбин И.В. 20171109
        // DV-880 Пендик И.О. - Оптимизация кода и добавление лого Еврохим
        // DV-922 Пендик И.О. - Оптимизация кода, перенос кода в "Документ УД", добавление лого НТК
        public void TopLogoWork()
        {
            ICustomizableControl customizable = this.CardControl;
            DocsVision.BackOffice.WinForms.Controls.HtmlBrowser Bro = customizable.FindPropertyItem<DocsVision.BackOffice.WinForms.Controls.HtmlBrowser>("LogoBro");
            Bro.CustomData = "";
            Bro.Refresh();
            string logoURL = GetLogoURL();
            Bro.CustomData = logoURL;
            Bro.Refresh();
        }
        // DV-880 END

        //Нелюбин И.В. 20180316	
        public void AddNewPeopleInTable(Guid GoodMemberID, string TableName)
        {
            ICustomizableControl customizable = CardControl as ICustomizableControl;
            ITableControl MailingListTable = customizable.FindPropertyItem<ITableControl>(TableName);
            bool ExistMember = false;
            for (int i = 0; i < MailingListTable.RowCount; i++)
            {
                Guid PeopleID = Guid.Empty;
                try
                {
                    PeopleID = Guid.Parse(MailingListTable[i][MailingListTable[i].Fields[0].Alias].ToString());
                }
                catch (Exception ex)
                {
                    MailingListTable.RemoveRow(this.BaseObject, i);
                    i = i - 1;
                    continue;
                }
                if (GoodMemberID == PeopleID)
                {
                    ExistMember = true;
                    break;
                }
            }
            if (!ExistMember)
            {
                BaseCardProperty MailingListProperty = MailingListTable.AddRow(this.BaseObject);
                MailingListProperty[MailingListProperty.Fields[0].Alias] = GoodMemberID;
                MailingListTable.RefreshRow(MailingListTable.RowCount - 1);
            }
        }


        #endregion

        #region Event Handlers

        private void MiniList_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                BrovserProcess.Process.Start("http://msk-dvapp101.corp.suek.ru/DVExtend/WebReportList.aspx?CardID=" + this.CardData.Id.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Что то пошло не так,\r\n попробуйте открыть лист еще раз");
            }
            /**
            try
            {
                dynamic IE = Microsoft.VisualBasic.Interaction.CreateObject("InternetExplorer.Application");
                IE.AddressBar = false;
                IE.MenuBar = false;
                //IE.Resizable = false;
                IE.StatusBar = false;
                IE.ToolBar = 0;
                IE.Height = 800;
                IE.Width = 1100;
                IE.Top = 100;
                IE.Left = 100;
                IE.Navigate("http://msk-dvapp101.corp.suek.ru/DVExtend/WebReportList.aspx?CardID=" + this.CardData.Id.ToString());
                IE.Visible = true;
            }				
            catch (Exception ex)
            {
                MessageBox.Show("Что то пошло не так,\r\n попробуйте открыть лист еще раз");
            } 	
            **/
        }

        private void ExtList_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                BrovserProcess.Process.Start("http://msk-dvapp101.corp.suek.ru/DVExtend/WebReportListExt.aspx?CardID=" + this.CardData.Id.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Что то пошло не так,\r\n попробуйте открыть лист еще раз");
            }
            /**		
            try
            {
                dynamic IE = Microsoft.VisualBasic.Interaction.CreateObject("InternetExplorer.Application");
                IE.AddressBar = false;
                IE.MenuBar = false;
                //IE.Resizable = false;
                IE.StatusBar = false;
                IE.ToolBar = 0;
                IE.Height = 800;
                IE.Width = 1100;
                IE.Top = 100;
                IE.Left = 100;
                IE.Navigate("http://msk-dvapp101.corp.suek.ru/DVExtend/WebReportListExt.aspx?CardID=" + this.CardData.Id.ToString());
                IE.Visible = true;
            }				
            catch (Exception ex)
            {
                MessageBox.Show("Что то пошло не так,\r\n попробуйте открыть лист еще раз");
            } 	
                    **/
        }

        private void Протокол_CardActivated(System.Object sender, DocsVision.Platform.WinForms.CardActivatedEventArgs e)
        {
            int i = 201;
            TopLogoWork();
            ICustomizableControl customizable = this.CardControl;
            DocsVision.BackOffice.WinForms.Controls.HtmlBrowser Bro = customizable.FindPropertyItem<DocsVision.BackOffice.WinForms.Controls.HtmlBrowser>("ListBro");
            Bro.CustomData = "http://msk-dv103.corp.suek.ru/DVExtend/WebReportListExt.aspx?CardID=" + this.CardData.Id.ToString();
            DocsVision.BackOffice.WinForms.Controls.HtmlBrowser BroLog = customizable.FindPropertyItem<DocsVision.BackOffice.WinForms.Controls.HtmlBrowser>("WebLogBro");
            BroLog.CustomData = "http://msk-dvapp101.corp.suek.ru/DVExtend/WebLogCardView.aspx?CardID=" + this.CardData.Id.ToString();

            SectionData MainSection = (SectionData)this.CardData.Sections[new Guid("30EB9B87-822B-4753-9A50-A1825DCA1B74")];

            ILayoutPropertyItem SentMarkText = customizable.FindPropertyItem<ILayoutPropertyItem>("Признак рассылки");
            SentMarkText.ControlValue = "";
            if (MainSection.Rows.Count > 0)
            {
                RowData MainRow = (RowData)MainSection.Rows[0];
                int? SentMark = MainRow.GetInt32("SentMark");
                if (SentMark.HasValue)
                {
                    if (SentMark == 1)
                    {
                        SentMarkText.ControlValue = "Разослан";
                    }
                }
            }

            if ((CardControl.ActivateFlags & ActivateFlags.New) == ActivateFlags.New)// ... карточка новая
            {



                const string ID_CARDTYPE = "{E898C387-0162-4F37-A93C-13BAA07FF183}";//тип строка справочника
                const string ID_SECTION = "{30EB9B87-822B-4753-9A50-A1825DCA1B74}"; "{B3BEBF53-90F1-4965-9717-91C58376E01D}"

                SearchQuery searchQuery = userSession.CreateSearchQuery();

                CardTypeQuery typeQuery = searchQuery.AttributiveSearch.CardTypeQueries.AddNew(new Guid(ID_CARDTYPE));


                SectionQuery sectionQuery = typeQuery.SectionQueries.AddNew(new Guid(ID_SECTION));

                sectionQuery.ConditionGroup.Conditions.AddNew("Name", FieldType.Unistring, ConditionOperation.Equals, "Sample");

                string query = searchQuery.GetXml();

                CardDataCollection coll = userSession.CardManager.FindCards(query);




                try
                {
                    string curUserID = this.Session.Properties["EmployeeID"].Value.ToString();
                    //DvDebug.TestScript dv = new DvDebug.TestScript();
                    //dv.TestMethod3( base.CardControl, base.BaseObject, Session,e);
                    BaseUniversal bu = base.CardControl.ObjectContext.GetObject<BaseUniversal>(RefBaseUniversal.ID);// гуид конструктора справочников
                    i = 646;
                    foreach (BaseUniversalItemType itemtype in bu.ItemTypes)
                    {

                        if (itemtype.Name == "Коллегиальный орган для протоколов")
                        {
                            foreach (BaseUniversalItemType item in itemtype.ItemTypes)
                            {
                                i = 667;
                                foreach (BaseUniversalItem item2 in item.Items)

                                {
                                    i = 672;
                                    if (item2.ItemCard != null)
                                    {
                                        Guid ItemCardID = base.CardControl.ObjectContext.GetObjectRef<BaseUniversalItemCard>(item2.ItemCard).Id;
                                        i = 677;
                                        CardData ItemCard = Session.CardManager.GetCardData(ItemCardID);
                                        i = 679;
                                        try
                                        {
                                            string CustomFieldValue = ItemCard.Sections[ItemCard.Type.Sections["Секретари"].Id].FirstRow.GetGuid("Secretary1").ToString();
                                            i = 681;

                                            if (curUserID.Trim().ToUpper() == CustomFieldValue.Trim().ToUpper())
                                            {
                                                i = 684;
                                                //ICustomizableControl customizable = this.CardControl;
                                                UniversalItemChooseBox collegial = customizable.FindPropertyItem<UniversalItemChooseBox>("BoardType");
                                                collegial.Value = item2.GetObjectId();
                                                collegial.Update();
                                                return;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }

                            }
                        }

                    }

                    i = 697;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Протокол_CardActivated: " + ex.Message + ex.StackTrace + i.ToString());
                }
            }
            // DV-788
            Document document = (Document)BaseObject;
            Guid numberId = document.MainInfo.GetGuid(CardDocument.MainInfo.RegNumber);
            if (numberId == Guid.Empty)
                return;

            BaseCardNumber number = document.Numbers.FirstOrDefault(item => CardControl.ObjectContext.GetObjectRef(item).Id == numberId);

            if (number == null)
                return;

            oldNumber = number.Number;
            //  DV-788		
        }

        private void Протокол_Saved(System.Object sender, System.EventArgs e)
        {
            SetREGSUEK();

            ICustomizableControl customizable = this.CardControl;
            ILayoutPropertyItem numberProp = customizable.FindPropertyItem<ILayoutPropertyItem>("RegNumber");
            //MessageBox.Show(numberProp.GetType().GetProperty("Rule").GetValue(numberProp, null).ToString());
            if ((Guid)numberProp.ControlValue == Guid.Empty)
            {
                Guid? ruleId = numberProp.GetType().GetProperty("Rule").GetValue(numberProp, null) as Guid?;
                if (ruleId != null)
                {

                    INumerationRulesService iNumerationRulesService = base.CardControl.ObjectContext.GetService<INumerationRulesService>();
                    NumerationRulesRule numerationRulesRule = base.CardControl.ObjectContext.GetObject<NumerationRulesRule>(ruleId);
                    BaseCardNumber number = iNumerationRulesService.CreateNumber(this.CardData, this.BaseObject, numerationRulesRule);

                    try
                    {
                        numberProp.ControlValue = base.CardControl.ObjectContext.GetObjectRef(number).Id;
                        numberProp.Commit();
                    }
                    catch
                    {
                        MessageBox.Show("не могу определить номер. В правиле отсутствуют соответствия имеющимся условиям");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("не могу определить номер. Не задано правило");
                    return;
                }
            }

            TopLogoWork();
        }

        private void Протокол_Saving(System.Object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string> Assistants = new List<string>();
            IStaffService staffService = CardControl.ObjectContext.GetService<IStaffService>();

            CardData DictPeople = this.Session.CardManager.GetDictionaryData(Guid.Parse("6710B92A-E148-4363-8A6F-1AA0EB18936C"));
            SectionData PeopleSection = (SectionData)DictPeople.Sections[Guid.Parse("DBC8AE9D-C1D2-4D5E-978B-339D22B32482")];

            ICustomizableControl customizable = CardControl as ICustomizableControl;

            ITableControl MailingListTable = customizable.FindPropertyItem<ITableControl>("MailingList");
            ITableControl AssistantsTable = customizable.FindPropertyItem<ITableControl>("Assistants");
            while (AssistantsTable.RowCount > 0)
            {
                AssistantsTable.RemoveRow(this.BaseObject, AssistantsTable.RowCount - 1);
            }

            for (int i = 0; i < MailingListTable.RowCount; i++)
            {

                Guid PeopleID = Guid.Empty;
                try
                {
                    PeopleID = Guid.Parse(MailingListTable[i][MailingListTable[i].Fields[0].Alias].ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                    MailingListTable.RemoveRow(this.BaseObject, i);
                    i = i - 1;
                    continue;
                }

                RowData PeopleRow = PeopleSection.GetRow(PeopleID);
                string PeopleEmail = PeopleRow["Email"].ToString();
                RowDataCollection AssistantsRows = PeopleRow.ChildSections[Guid.Parse("ED414CB4-B205-4BE4-A2FA-5C0D3347CEB3")].GetAllRows();
                foreach (RowData CurrentRow in AssistantsRows)
                {
                    Guid AssistantID = (Guid)CurrentRow.GetGuid("DeputyID");
                    StaffEmployee Assistant = staffService.Get(AssistantID);
                    if (Assistant.Email.ToUpper() == PeopleEmail.ToUpper()) continue;
                    if (Assistants.FindIndex(item => item == AssistantID.ToString()) == -1)
                        Assistants.Add(AssistantID.ToString());
                }
            }
            foreach (string Assistant in Assistants)
            {
                BaseCardProperty AssistantsProperty = AssistantsTable.AddRow(this.BaseObject);
                AssistantsProperty[AssistantsProperty.Fields[0].Alias] = new Guid(Assistant);
                AssistantsTable.RefreshRow(AssistantsTable.RowCount - 1);
            }
            //SetREGSUEK();
        }

        private void Протокол_StateChanged(System.Object sender, System.EventArgs e)
        {
            CardControl.Save();
            SetREGSUEK();
            TopLogoWork();
            //MessageBox.Show("Состояние изменилось :)");
        }

        private void РассылкаПротокола_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ICustomizableControl customizable = CardControl as ICustomizableControl;
            ITableControl MailingListTable = customizable.FindPropertyItem<ITableControl>("MailingList");
            if (MailingListTable.RowCount > 0)
            {
                if (CardControl.Save())
                {
                    Library library = new Library(base.Session);
                    Process template = library.GetProcess(new Guid("14234534-43C9-E711-80C0-984BE1614FF4"));
                    Process process = library.CreateProcess(template);
                    process.InitialDocument = base.CardData.Id.ToString("B");
                    process.Variables[new Guid("56DCCBBE-C307-4EE5-9F69-31B0B86FF482")].Value = base.CardData.Id.ToString("B");
                    process.Start(base.Session.Properties["AccountName"].Value.ToString(), library.Dictionary, ExecutionModeEnum.Automatic, true);
                    MessageBox.Show("Заявка на рассылку зарегистрирована.\n\r Документ будет разослан получателям и заместителям в течении 2-3 минут", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Внимание, не удалось сохранить карточку.. рассылка не будет произведена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Внимание, не заполнен список для рассылки.. рассылка не будет произведена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void newTasksGroup_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Document document = (Document)this.BaseObject;

            //MessageBox.Show(document.MainInfo.Name);
            //base.CreateTasksGroup_ItemClick(sender, e);
            ITaskGroupService taskGroupService = this.CardControl.ObjectContext.GetService<ITaskGroupService>();
            IStaffService staffService = this.CardControl.ObjectContext.GetService<IStaffService>();
            KindsCardKind kind = this.CardControl.ObjectContext.GetObject<KindsCardKind>(new Guid("5A0F4B42-92D7-43E4-AF98-EB4BA0D74F25"));
            TaskGroup taskGroup = taskGroupService.CreateChildTaskGroup(null, kind, document, document.MainInfo.Tasks);

            //TaskGroup taskGroup = taskGroupService.CreateTaskGroup(kind);						   
            taskGroup.MainInfo.ExecutionType = TaskGroupExecutionType.Parallel;
            taskGroup.MainInfo.Author = staffService.GetCurrentEmployee();
            taskGroup.MainInfo.Controller = staffService.GetCurrentEmployee();

            try
            {
                taskGroup.Description = document.MainInfo.Name;
                if (document.MainInfo.Name.Length > 116)
                {
                    taskGroup.MainInfo.Name = document.MainInfo.Name.Substring(0, 116) + " " + document.Numbers.Last().Number;
                }
                else
                {
                    taskGroup.MainInfo.Name = document.MainInfo.Name + " " + document.Numbers.Last().Number;
                }
            }
            catch (Exception ex)
            {
                taskGroup.MainInfo.Name = "поручение";
            }


            /*
            ILinkService linkService = this.CardControl.ObjectContext.GetService<ILinkService>();
            LinksLinkType linksLinkType = linkService.FindLink("Связано с");

            this.CardControl.ObjectContext.AcceptChanges();
            this.CardControl.ObjectContext.SaveObject(taskGroup);
            IReferenceListService referenceListService = this.CardControl.ObjectContext.GetService<IReferenceListService>();
            ReferenceList referenceList;

            bool eee = referenceListService.TryGetReferenceListFromCard(taskGroup.GetObjectId(), true, out referenceList);
            taskGroup.MainInfo.ReferenceList = referenceList;
            this.CardControl.ObjectContext.AcceptChanges();
            this.CardControl.ObjectContext.SaveObject(taskGroup);

            ReferenceListReference RLR = referenceListService.CreateReference(taskGroup.MainInfo.ReferenceList, linksLinkType, this.CardData.Id, this.CardData.Type.Id, true);	

            MessageBox.Show("5");
            //taskGroup.MainInfo.ReferenceList.References.Add(RLR);
            MessageBox.Show("6");
            */
            this.CardControl.ObjectContext.AcceptChanges();
            this.CardControl.ObjectContext.SaveObject(taskGroup);
            Guid taskGroupId = this.CardControl.ObjectContext.GetObjectRef(taskGroup).Id;
            CardFrame.CardHost.ShowCard(taskGroupId, ActivateMode.Edit, ActivateFlags.New);


        }

        private void NewControlTask_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            if (!CardControl.Save())
                return;

            ITaskService taskService = CardControl.ObjectContext.GetService<ITaskService>();
            ITaskListService taskListService = CardControl.ObjectContext.GetService<ITaskListService>();

            // Получение вида Контрольное поручение
            QueryObject query = new QueryObject(RefKinds.CardKinds.Name, "Контрольное поручение");
            KindsCardKind cardKind = CardControl.ObjectContext.FindObject<KindsCardKind>(query);
            Document document = (Document)BaseObject;
            if (document.MainInfo.Tasks == null)
            {
                //Создание списка заданий и присвоение его документу 
                TaskList taskList = taskListService.CreateTaskList();
                CardControl.ObjectContext.SaveObject(taskList);
                document.MainInfo.Tasks = taskList;
            }

            // Создание задания  
            Task newTask = taskService.CreateChildTask(null, cardKind, document, document.MainInfo.Tasks);

            // Инициализация основных параметров задания
            taskService.InitializeDefaults(newTask);
            newTask.MainInfo.Name = newTask.Description = "Контрольное поручение";

            //  if (CardControl.Save())
            // {

            Guid newTaskId = CardControl.ObjectContext.GetObjectRef(newTask).Id;
            CardControl.ObjectContext.SaveObject(newTask);

            // Отображение задания
            CardControl.CardHost.ShowCard(newTaskId, ActivateMode.Edit, ActivateFlags.New);

            //    CardControl.Save();

        }

        private void NewDoubleControlTask_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!CardControl.Save())
                return;

            Document document = (Document)BaseObject;

            ITaskService taskService = CardControl.ObjectContext.GetService<ITaskService>();

            QueryObject query = new QueryObject(RefKinds.CardKinds.Name, "Дублирующие поручения"); // тут нужно указать свое название для промежуточного вида задания
            KindsCardKind cardKind = CardControl.ObjectContext.FindObject<KindsCardKind>(query);

            Task newTask = taskService.CreateTask(cardKind);
            taskService.InitializeDefaults(newTask);
            newTask.MainInfo.Name = newTask.Description = "Контрольное поручение";

            newTask.MainInfo["MainDoc"] = CardData.Id;

            Guid numberId = document.MainInfo.GetGuid(CardDocument.MainInfo.RegNumber);
            if (numberId != Guid.Empty)
            {
                BaseCardNumber number = document.Numbers.FirstOrDefault(item => CardControl.ObjectContext.GetObjectRef(item).Id == numberId);
                if (number != null)
                    newTask.MainInfo["НомерДокумента"] = number.Number;
            }

            CardControl.ObjectContext.SaveObject(newTask);

            Guid newTaskId = CardControl.ObjectContext.GetObjectRef(newTask).Id;

            CardControl.CardHost.ShowCardModal(newTaskId, ActivateMode.Edit, ActivateFlags.New);

            Session.CardManager.DeleteCard(newTaskId);
        }

        private void командаЗарегистрировать_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int i = 0;

            try
            {

                if (!CardControl.Save())
                    return;
                i = 1;
                SetREGSUEK();
                i = 2;
                CardControl.Save();
                i = 3;

                ICustomizableControl customizable = this.CardControl;
                i = 4;
                ILayoutPropertyItem numberProp = customizable.FindPropertyItem<ILayoutPropertyItem>("RegNumber");
                i = 5;
                if ((Guid)numberProp.ControlValue == Guid.Empty)
                {
                    Guid? ruleId = numberProp.GetType().GetProperty("Rule").GetValue(numberProp, null) as Guid?;
                    if (ruleId != null)
                    {
                        i = 6;
                        INumerationRulesService iNumerationRulesService = base.CardControl.ObjectContext.GetService<INumerationRulesService>();
                        NumerationRulesRule numerationRulesRule = base.CardControl.ObjectContext.GetObject<NumerationRulesRule>(ruleId);
                        BaseCardNumber number = iNumerationRulesService.CreateNumber(this.CardData, this.BaseObject, numerationRulesRule);
                        SectionData MainSection = (SectionData)this.CardData.Sections[new Guid("30EB9B87-822B-4753-9A50-A1825DCA1B74")];
                        if (MainSection.Rows.Count > 0)
                        {
                            i = 7;
                            RowData MainRow = (RowData)MainSection.Rows[0];
                            MainRow.SetGuid("RegNumber", base.CardControl.ObjectContext.GetObjectRef(number).Id);
                            numberProp.ControlValue = base.CardControl.ObjectContext.GetObjectRef(number).Id;
                            numberProp.Commit();
                            NumeratorBox rnum = customizable.FindPropertyItem<NumeratorBox>("RegNumber");
                            CardControl.ShowMessage("Документ зарегистрирован c номером № " + rnum.Text, "Документ", null, MessageType.Information, MessageButtons.Ok);
                            //   DV-788
                            SavePrevRegNumber(rnum.Text);
                            //   DV-788
                            i = 8;
                        }
                        else
                        {
                            MessageBox.Show("Отсутствует базовая секция");
                            return;
                        }
                        i = 9;
                    }
                    else
                    {
                        MessageBox.Show("не могу определить номер. Не задано правило");
                        return;
                    }
                }
                this.Registrate_ItemClick();
                TopLogoWork();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в модуле командаЗарегистрировать_ItemClick: " + ex.Message + ex.StackTrace + ", на строке " + i.ToString(), "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }



        }

        private void Синхронизация_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ICustomizableControl customizable = CardControl as ICustomizableControl;
            ILayoutPropertyItem BoardType = customizable.FindPropertyItem<ILayoutPropertyItem>("BoardType");
            Guid BoardTypeID = (Guid)BoardType.ControlValue;
            CardData UniCard = this.Session.CardManager.GetDictionaryData(Guid.Parse("4538149D-1FC7-4D41-A104-890342C6B4F8"));
            SectionData UniSection = UniCard.Sections[Guid.Parse("A1DCE6C1-DB96-4666-B418-5A075CDB02C9")];
            RowData TirRow = UniSection.FindRow("@Name=\"Коллегиальный орган для протоколов\"");

            foreach (RowData CurrentBigRow in TirRow.ChildRows)
            {
                SubSectionData SettingSection = CurrentBigRow.ChildSections[Guid.Parse("1B1A44FB-1FB1-4876-83AA-95AD38907E24")];
                RowData BoardTypeRow = SettingSection.Rows.Find("RowID", BoardTypeID);
                if (BoardTypeRow != null)
                {
                    if (BoardTypeRow.GetGuid("ItemCard").HasValue)
                    {
                        Guid CardID = (Guid)BoardTypeRow.GetGuid("ItemCard");
                        CardData ExtSettingCard = this.Session.CardManager.GetCardData(CardID);
                        SectionData InviteParticipants = ExtSettingCard.Sections[Guid.Parse("{E5F3659B-9CD7-47D4-9231-EEB7CFB67ED1}")];
                        foreach (RowData CurrentRow in InviteParticipants.Rows)
                        {
                            Guid? MemberID = CurrentRow.GetGuid("Participant");
                            if (MemberID.HasValue)
                            {
                                Guid GoodMemberID = (Guid)MemberID;
                                AddNewPeopleInTable(GoodMemberID, "MailingList");
                                AddNewPeopleInTable(GoodMemberID, "Agreement");
                            }
                        }

                        SectionData BoardMembers = ExtSettingCard.Sections[Guid.Parse("{DE199F2F-117D-4B2C-9803-FEB7EA7EE6CA}")];
                        foreach (RowData CurrentRow in BoardMembers.Rows)
                        {
                            Guid? MemberID = CurrentRow.GetGuid("Member");
                            if (MemberID.HasValue)
                            {
                                Guid GoodMemberID = (Guid)MemberID;
                                AddNewPeopleInTable(GoodMemberID, "MailingList");
                                AddNewPeopleInTable(GoodMemberID, "Agreement");
                            }
                        }
                        // DV-754					
                        ILayoutPropertyItem SecretaryControl = customizable.FindPropertyItem<ILayoutPropertyItem>("Свойство6");
                        Guid SecretaryID = (Guid)SecretaryControl.ControlValue;
                        if (SecretaryID != Guid.Empty)
                            AddNewPeopleInTable(SecretaryID, "MailingList");

                        ILayoutPropertyItem ChairmanControl = customizable.FindPropertyItem<ILayoutPropertyItem>("Свойство5");
                        Guid ChairmanID = (Guid)ChairmanControl.ControlValue;
                        if (ChairmanID != Guid.Empty)
                            AddNewPeopleInTable(ChairmanID, "MailingList");
                        // DV-754 END
                    }
                    break;
                }
            }

            CardControl.Save();

        }

        private void AgreeDefault_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

            RowData MainRow = this.CardData.Sections[Guid.Parse("30EB9B87-822B-4753-9A50-A1825DCA1B74")].Rows[0];
            if (MainRow.GetBoolean("AgreeDefault").HasValue)
            {
                if ((bool)MainRow.GetBoolean("AgreeDefault"))
                {
                    MessageBox.Show("Процесс автозавершения уже запущен. Дождитесь его завершения");
                    return;
                }
            }

            Library library = new Library(base.Session);
            Process template = library.GetProcess(new Guid("86F355FF-5E2B-E811-80C5-984BE1614FF4"));
            Process process = library.CreateProcess(template);
            process.InitialDocument = base.CardData.Id.ToString("B");
            process.Variables[new Guid("932C93C4-9C91-4D09-A40C-8190D8821780")].Value = base.CardData.Id.ToString("B");
            process.Variables[Guid.Parse("C246F3C7-C98C-4537-AB72-D4935AC34960")].Value = this.Session.Properties["EmployeeID"].Value.ToString();
            string AccountName = @"SUEKCORP\SaperionService2";
            process.Start(AccountName, library.Dictionary, ExecutionModeEnum.Automatic, true);
            MainRow.SetBoolean("AgreeDefault", true);
            MessageBox.Show("Запрос на завершение заданий согласующих по умолчанию зарегистрирован в системе \n\r В течении нескольких минут он будет обработан. \n\r О результатах система проинформирует Вас по электронной почте", "Автоматическое завершение заданий");
            //this.CardControl.CardClosed();
            this.CardFrame.Close();
        }

        private void SUEKDepartment_ValueChanged(System.Object sender, System.EventArgs e)
        {
            //method code here
            //method code here
            //ICustomizableControl customizable = CardControl;
            //ILayoutPropertyItem departmentSUEK = customizable.FindPropertyItem<ILayoutPropertyItem>("SUEKDepartment");
            //MessageBox.Show("1");
            //MessageBox.Show(departmentSUEK.ControlValue.ToString());
            //MessageBox.Show("2");
        }

        private void BoardType_EditValueChanging(System.Object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            ICustomizableControl customizable = CardControl;
            ILayoutPropertyItem Control = customizable.FindPropertyItem<ILayoutPropertyItem>("BoardType");
            ILayoutPropertyItem Control2 = customizable.FindPropertyItem<ILayoutPropertyItem>("Свойство7");
            Control2.ControlValue = "";

            string KOrganIDString = Control.ControlValue.ToString();
            CardData UniCard = Session.CardManager.GetDictionaryData(Guid.Parse("4538149D-1FC7-4D41-A104-890342C6B4F8"));


            SectionData UniSection = UniCard.Sections[Guid.Parse("A1DCE6C1-DB96-4666-B418-5A075CDB02C9")];

            //
            RowData TirRow = UniSection.FindRow("@Name=\"Коллегиальный орган для протоколов\"");

            bool Found = false;
            foreach (RowData CurrentRow in TirRow.AllChildRows)
            {
                if (Found) break;
                SubSectionData SettingSection = CurrentRow.ChildSections[Guid.Parse("1B1A44FB-1FB1-4876-83AA-95AD38907E24")];
                foreach (RowData SettingRow in SettingSection.Rows)
                {
                    if (SettingRow.Id.ToString() == KOrganIDString)
                    {
                        try
                        {
                            Guid CardID = (Guid)SettingRow.GetGuid("ItemCard");
                            CardData ExtSettingCard = Session.CardManager.GetCardData(CardID);

                            RowData ExtSettingRow = ExtSettingCard.Sections[Guid.Parse("{F5641A7E-83AF-4C20-9C60-EA2973C4F135}")].Rows[0];
                            Control2.ControlValue = ExtSettingRow["МестоРегистрации"].ToString();
                            Found = true;
                        }
                        catch
                        {

                        }

                    }
                }
            }
        }

        private void UsersListWorkerWithCard_Click(System.Object sender, System.EventArgs e)
        {
            //method code here
            ICustomizableControl control = CardControl;
            ICustomPropertyItem icpiMH = control.FindPropertyItem<ICustomPropertyItem>("HTMLLog");

            if (icpiMH != null)

            {

                HtmlBrowser hbMH = icpiMH.Control as HtmlBrowser;

                if (hbMH != null)

                {

                    WebBrowser wbMH = hbMH.Controls[0] as WebBrowser;

                    if (wbMH != null)

                    {

                        wbMH.Document.OpenNew(false);
                        string InstanceID = this.CardData.Id.ToString();
                        string Url = "http://msk-sprweb101.corp.suek.ru/DVCardLog/DVCardLog.aspx?CardID=" + InstanceID + "&ModeWork=1";
                        Uri sd = new Uri(Url, UriKind.Absolute);
                        wbMH.Url = sd;

                        //      wbMH.Document.Write(sMHDef);
                        //MessageBox.Show("1");

                    }

                }

            }
        }

        private void HtmlLogBtn_Click(System.Object sender, System.EventArgs e)
        {
            //method code here
            ICustomizableControl control = CardControl;
            ICustomPropertyItem icpiMH = control.FindPropertyItem<ICustomPropertyItem>("HTMLLog");

            if (icpiMH != null)

            {

                HtmlBrowser hbMH = icpiMH.Control as HtmlBrowser;

                if (hbMH != null)

                {

                    WebBrowser wbMH = hbMH.Controls[0] as WebBrowser;

                    if (wbMH != null)

                    {

                        wbMH.Document.OpenNew(false);
                        string InstanceID = this.CardData.Id.ToString();
                        string Url = "http://msk-sprweb101.corp.suek.ru/DVCardLog/DVCardLog.aspx?CardID=" + InstanceID;
                        Uri sd = new Uri(Url, UriKind.Absolute);
                        wbMH.Url = sd;

                        //      wbMH.Document.Write(sMHDef);
                        //MessageBox.Show("1");

                    }

                }

            }
        }

        private void КраткаяИстория_Click(System.Object sender, System.EventArgs e)
        {
            //method code here
            ICustomizableControl control = CardControl;
            ICustomPropertyItem icpiMH = control.FindPropertyItem<ICustomPropertyItem>("HTMLLog");

            if (icpiMH != null)

            {

                HtmlBrowser hbMH = icpiMH.Control as HtmlBrowser;

                if (hbMH != null)

                {

                    WebBrowser wbMH = hbMH.Controls[0] as WebBrowser;

                    if (wbMH != null)

                    {

                        wbMH.Document.OpenNew(false);
                        string InstanceID = this.CardData.Id.ToString();
                        string Url = "http://msk-dvapp101.corp.suek.ru/DVExtend/WebLogCardView.aspx?CardID=" + this.CardData.Id.ToString();
                        Uri sd = new Uri(Url, UriKind.Absolute);
                        wbMH.Url = sd;

                        //      wbMH.Document.Write(sMHDef);
                        //MessageBox.Show("1");

                    }

                }

            }
        }

        private void OnClickBtnOK(System.Object sender, System.EventArgs e)
        {
            RowData NewEmployee;
            Guid EmployeeID;

            IStaffService staffService = CardControl.ObjectContext.GetService<IStaffService>();
            ICustomizableControl customizable = CardControl;
            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            Form form = btn.Parent as Form;
            System.Windows.Forms.ListBox ListEmployees = form.Controls["Employees"] as System.Windows.Forms.ListBox;
            CardData CardDoc = Session.CardManager.GetCardData(BaseObject.GetObjectId());
            SectionData UniSectionRow = CardDoc.Sections[new Guid("0A0243DA-1D75-463D-B55D-7EDA243FFC88")];

            UniSectionRow.Rows.Clear();
            foreach (var Employee in ListEmployees.Items)
            {
                EmployeeID = new Guid(Employee.ToString().Substring(Employee.ToString().Length - 38, 36));
                NewEmployee = UniSectionRow.Rows.AddNew();
                NewEmployee["UniRefID1"] = EmployeeID;
                AddNewPeopleInTable(EmployeeID, "MailingList");
            }

            Library library = new Library(Session);
            Process template = library.GetProcess(new Guid("14234534-43C9-E711-80C0-984BE1614FF4"));
            Process process = library.CreateProcess(template);
            process.InitialDocument = CardControl.CardData.Id.ToString("B");
            process.Variables.Values.Cast<Variable>().Where(var => var.Name == "КАРТОЧКА").First().Value = CardControl.CardData.Id.ToString("B");
            process.Variables.Values.Cast<Variable>().Where(var => var.Name == "Дополнительная рассылка").First().Value = 1;
            process.Start(Session.Properties["AccountName"].Value.ToString(), library.Dictionary, ExecutionModeEnum.Automatic, true);
            MessageBox.Show("Заявка на рассылку зарегистрирована.\n\r Документ будет разослан получателям и заместителям в течении 2-3 минут", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Document document = (Document)BaseObject;
            CardControl.ObjectContext.SaveObject(document);
            form.Close();
        }

        private void OnClickBtnAdd(System.Object sender, System.EventArgs e)
        {
            Button btn = sender as Button;
            Form form = btn.Parent as Form;
            CommunicativeChooseBox ccb = form.Controls["Employee"] as CommunicativeChooseBox;
            ListBox ListEmployees = form.Controls["Employees"] as ListBox;
            if (ccb.Value != Guid.Empty)
            {
                StaffEmployee employee = base.CardControl.ObjectContext.GetObject<StaffEmployee>(ccb.Value);
                ListEmployees.DisplayMember = "DisplayName";
                ListEmployees.ValueMember = "Value";
                ListEmployees.Items.Add(new { employee.DisplayName, ccb.Value });
                ccb.Value = Guid.Empty;
            }
            else
            {
                MessageBox.Show("Требуется указать сотрудника");
            }
        }

        private void OnClickBtnCancel(System.Object sender, System.EventArgs e)
        {
            Button btn = sender as Button;
            Form form = btn.Parent as Form;
            form.Close();
        }

        private void ДополнительнаяРассылка_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Form form = new Form();
            form.Size = new System.Drawing.Size(350, 280);
            form.Location = new System.Drawing.Point(100, 100);
            form.Text = "Дополнительная рассылка";

            CommunicativeChooseBox ccb = new CommunicativeChooseBox();
            ccb.Name = "Employee";
            ccb.Text = "Выберите сотрудника для рассылки";
            ccb.Location = new System.Drawing.Point(10, 10);
            ccb.Width = 300;

            Button btnAdd = new Button();
            btnAdd.Text = "Добавить сотрудника";
            btnAdd.Location = new System.Drawing.Point(10, 40);
            btnAdd.Width = 200;
            btnAdd.Click += OnClickBtnAdd;

            Button btnOK = new Button();
            btnOK.Text = "Рассылка выбранным сотрудникам";
            btnOK.Location = new System.Drawing.Point(10, 180);
            btnOK.Width = 200;
            btnOK.Click += OnClickBtnOK;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new System.Drawing.Point(10, 210);
            btnCancel.Width = 100;
            btnCancel.Click += OnClickBtnCancel;

            ListBox DopList = new ListBox();
            DopList.Name = "Employees";
            DopList.Location = new System.Drawing.Point(10, 70);
            DopList.Size = new System.Drawing.Size(300, 100);
            DopList.TabIndex = 1;

            form.Controls.Add(ccb);
            form.Controls.Add(btnOK);
            form.Controls.Add(btnAdd);
            form.Controls.Add(btnCancel);
            form.Controls.Add(DopList);

            form.Show();
        }

        private void НоваяРекомендация_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!CardControl.Save())
                return;

            ITaskService taskService = CardControl.ObjectContext.GetService<ITaskService>();
            ITaskListService taskListService = CardControl.ObjectContext.GetService<ITaskListService>();

            // Получение вида Контрольное поручение
            QueryObject query = new QueryObject(RefKinds.CardKinds.Name, "Рекомендация");
            KindsCardKind cardKind = CardControl.ObjectContext.FindObject<KindsCardKind>(query);
            Document document = (Document)BaseObject;
            if (document.MainInfo.Tasks == null)
            {
                //Создание списка заданий и присвоение его документу 
                TaskList taskList = taskListService.CreateTaskList();
                CardControl.ObjectContext.SaveObject(taskList);
                document.MainInfo.Tasks = taskList;
            }

            // Создание задания  
            Task newTask = taskService.CreateChildTask(null, cardKind, document, document.MainInfo.Tasks);

            // Инициализация основных параметров задания
            taskService.InitializeDefaults(newTask);
            newTask.MainInfo.Name = newTask.Description = "Рекомендация";

            //  if (CardControl.Save())
            // {

            Guid newTaskId = CardControl.ObjectContext.GetObjectRef(newTask).Id;
            CardControl.ObjectContext.SaveObject(newTask);

            // Отображение задания
            CardControl.CardHost.ShowCard(newTaskId, ActivateMode.Edit, ActivateFlags.New);

            //    CardControl.Save();
        }

        private void GroupBtn_Click(System.Object sender, System.EventArgs e)
        {
            IStaffService staffService = CardControl.ObjectContext.GetService<IStaffService>();
            ICustomizableControl customizable = CardControl;
            ILayoutPropertyItem distribution_Lists = customizable.FindPropertyItem<ILayoutPropertyItem>("ГруппаРассылки");
            IEnumerable<Guid> distributionArray = distribution_Lists.ControlValue as IEnumerable<Guid>;

            if (distribution_Lists.ControlValue == null)
                return;

            IEnumerable<Guid> employees = distribution_Lists.ControlValue as IEnumerable<Guid>;
            List<StaffEmployee> employeesList = new List<StaffEmployee>();
            foreach (Guid employeeId in employees)
            {
                AddNewPeopleInTable(employeeId, "MailingList");
                //    IEnumerable<StaffEmployee> zamemployees = staffService.Get(employeeId).Deputies.Select(t => t.Employee);
                //    foreach (var izam in zamemployees)
                //     {
                //        AddNewPeopleInTable( izam.GetObjectId(), "Assistants");
                //    }
            }

            distribution_Lists.ControlValue = null;
        }
        // DV-754
        private void BoardType_ValueChanged(System.Object sender, System.EventArgs e)
        {
            ICustomizableControl customizable = CardControl as ICustomizableControl;
            ILayoutPropertyItem BoardTypeControl = customizable.FindPropertyItem<ILayoutPropertyItem>("BoardType");
            ILayoutPropertyItem SecretaryControl = customizable.FindPropertyItem<ILayoutPropertyItem>("Свойство6");

            Guid BoardTypeID = (Guid)BoardTypeControl.ControlValue;

            if (BoardTypeID == Guid.Empty)
            {
                SecretaryControl.ControlValue = this.Session.Properties["EmployeeID"].Value;
                return;
            }

            List<Guid> SecretaryList = getSecretaryList(BoardTypeID);
            if (SecretaryList.Count > 0)
            {
                SecretaryControl.ControlValue = SecretaryList[0];
            }
            else
            {
                SecretaryControl.ControlValue = this.Session.Properties["EmployeeID"].Value;
            }
        }
        private List<Guid> getSecretaryList(Guid BoardTypeID)
        {
            List<Guid> SecretaryList = new List<Guid>();
            CardData UniCard = this.Session.CardManager.GetDictionaryData(Guid.Parse("4538149D-1FC7-4D41-A104-890342C6B4F8"));
            SectionData UniSection = UniCard.Sections[Guid.Parse("A1DCE6C1-DB96-4666-B418-5A075CDB02C9")];
            RowData TirRow = UniSection.FindRow("@Name=\"Коллегиальный орган для протоколов\"");

            foreach (RowData CurrentBigRow in TirRow.ChildRows)
            {
                SubSectionData SettingSection = CurrentBigRow.ChildSections[Guid.Parse("1B1A44FB-1FB1-4876-83AA-95AD38907E24")];
                RowData BoardTypeRow = SettingSection.Rows.Find("RowID", BoardTypeID);
                if (BoardTypeRow != null)
                {
                    if (BoardTypeRow.GetGuid("ItemCard").HasValue)
                    {
                        Guid CardID = (Guid)BoardTypeRow.GetGuid("ItemCard");
                        CardData ExtSettingCard = this.Session.CardManager.GetCardData(CardID);
                        SectionData Secretary = ExtSettingCard.Sections[Guid.Parse("{2B206B1D-84EA-454A-9D38-DC771770482C}")];
                        foreach (RowData CurrentRow in Secretary.Rows)
                        {
                            Guid? MemberID = CurrentRow.GetGuid("Secretary1");
                            if (MemberID.HasValue)
                            {
                                Guid SecretaryID = (Guid)MemberID;
                                SecretaryList.Add(SecretaryID);
                            }
                        }
                    }
                }
            }
            return SecretaryList;
        }
        // DV-754 END
        private void AddGroupBtn_Click(System.Object sender, System.EventArgs e)
        {
            string ItemID = new Guid("579FA1FD-83AB-4960-8450-4B8060097368").ToString("B").ToUpperInvariant();
            Guid cardID = DocsVision.BackOffice.CardLib.CardDefs.RefStaff.ID;
            string sectionID = DocsVision.BackOffice.CardLib.CardDefs.RefStaff.AlternateHierarchy.ID.ToString("B").ToUpperInvariant();
            object activateParams = new object[] { sectionID, ItemID };
            try
            {
                Guid SelectedGroup = new Guid(CardControl.CardHost.SelectFromCard(cardID, "Выберите группу", activateParams).ToString());
                if (SelectedGroup != null)
                {
                    IStaffService staffService = CardControl.ObjectContext.GetService<IStaffService>();
                    StaffGroup SG = staffService.GetGroup(SelectedGroup);
                    foreach (StaffEmployee employee in SG.Employees)
                    {
                        AddNewPeopleInTable(employee.GetObjectId(), "MailingList");
                    }
                }
            }
            catch { }
        }

        //   DV-788
        private int GetNumberPartOfRegNumber(string regNumber)
        {
            int numberValue = -1;
            bool result;

            int n1 = regNumber.IndexOf("-");
            if ((n1 == -1))
                result = int.TryParse(regNumber, out numberValue);

            else
                result = int.TryParse(regNumber.Substring(0, n1), out numberValue);

            if (!result)
                numberValue = -1;

            return numberValue;
        }

        private void SavePrevRegNumber(string regNumber)
        {
            string errT = "";

            try
            {
                Document doc = (Document)BaseObject;
                INumerationRulesService numerationRulesService = base.CardControl.ObjectContext.GetService<INumerationRulesService>();
                if (doc == null)
                    return;
                Guid id1 = new Guid(doc.MainInfo["RegNumber"].ToString());
                if (id1 == Guid.Empty)
                    return;
                BaseCardNumber number2 = doc.Numbers.FirstOrDefault(item => CardControl.ObjectContext.GetObjectRef(item).Id == id1);
                if (number2 == null)
                    return;
                number2.Number = regNumber;
                doc.MainInfo["НомерДляСортировки"] = GetNumberPartOfRegNumber(regNumber);

                CardControl.ObjectContext.SaveObject(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void SaveCurrentSortNumber()
        {
            try
            {
                Document doc = (Document)BaseObject;
                Guid numberId = doc.MainInfo.GetGuid(CardDocument.MainInfo.RegNumber);
                if (numberId == Guid.Empty)
                    return;
                BaseCardNumber number = doc.Numbers.FirstOrDefault(item => CardControl.ObjectContext.GetObjectRef(item).Id == numberId);

                if (number == null)
                    return;
                doc.MainInfo["НомерДляСортировки"] = GetNumberPartOfRegNumber(number.Number);

                CardControl.ObjectContext.SaveObject(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Протокол_CardClosed(System.Object sender, System.EventArgs e)
        {
            if (isCancel)
                SavePrevRegNumber(oldNumber);
            else
                SaveCurrentSortNumber();
        }

        //   DV-788


        //  DV-803
        private void NewDoubleControlTask2_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!CardControl.Save())
                return;

            Document document = (Document)BaseObject;

            ITaskService taskService = CardControl.ObjectContext.GetService<ITaskService>();

            QueryObject query = new QueryObject(RefKinds.CardKinds.Name, "Дублирующая рекомендация");
            KindsCardKind cardKind = CardControl.ObjectContext.FindObject<KindsCardKind>(query);

            Task newTask = taskService.CreateTask(cardKind);
            taskService.InitializeDefaults(newTask);
            newTask.MainInfo.Name = newTask.Description = "Рекомендация";

            newTask.MainInfo["MainDoc"] = CardData.Id;

            Guid numberId = document.MainInfo.GetGuid(CardDocument.MainInfo.RegNumber);
            if (numberId != Guid.Empty)
            {
                BaseCardNumber number = document.Numbers.FirstOrDefault(item => CardControl.ObjectContext.GetObjectRef(item).Id == numberId);
                if (number != null)
                    newTask.MainInfo["НомерДокумента"] = number.Number;
            }

            CardControl.ObjectContext.SaveObject(newTask);

            Guid newTaskId = CardControl.ObjectContext.GetObjectRef(newTask).Id;

            CardControl.CardHost.ShowCardModal(newTaskId, ActivateMode.Edit, ActivateFlags.New);

            Session.CardManager.DeleteCard(newTaskId);
        }

        //  DV-803


        private void НаПодписание_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ICustomizableControl customizable = CardControl;
            Guid OldReconciliation = Guid.Empty;
            RowData ReconciliationRow = null;

            MessageResult result = CardControl.ShowMessage("Отправить документ на подписание?", "Выбор", null, MessageType.Question, MessageButtons.YesNoCancel);
            switch (result)
            {
                case MessageResult.Yes:

                    Document document = (Document)BaseObject;
                    try
                    {
                        ReconciliationRow = CardControl.CardData.Sections[CardDocument.Reconciliation.ID].Rows[0];
                        OldReconciliation = (Guid)ReconciliationRow.GetGuid("Reconciliation");
                    }
                    catch
                    {
                    }
                    // ------------------- создание и запуск подписания
                    IServiceFactoryRegistry serviceFactoryRegistry = CardControl.ObjectContext.GetService<IServiceFactoryRegistry>();

                    serviceFactoryRegistry.RegisterFactory(typeof(ApprovalDesignerServiceFactory));

                    var mapperFactoryRegistry = CardControl.ObjectContext.GetService<IObjectMapperFactoryRegistry>();

                    mapperFactoryRegistry.RegisterFactory(typeof(ApprovalDesignerMapperFactory));

                    Guid initialCardId = CardControl.ObjectContext.GetObjectRef(BaseObject).Id;
                    KindsCardKind cardKind = CardControl.ObjectContext.GetObject<KindsCardKind>(new Guid("9C0E5586-41B8-411E-B5CD-C94B605CB7A1"));

                    KindsCardCreationSetting cardCreationSetting = cardKind.CreationSettings.FirstOrDefault(t => t.ModeName.Equals("Подписание протокола"));
                    Guid reconciliationId = CardControl.ObjectContext.GetService<IReconcileService>().CreateReconciliationCard(initialCardId, cardCreationSetting);
                    CardData reconciliationCardData = Session.CardManager.GetCardData(reconciliationId);
                    reconciliationCardData.Description = document.Description;

                    CardControl.ObjectContext.GetService<IReconcileService>().HandleDocumentAfterReconcileCreated((Document)BaseObject, reconciliationCardData);

                    // ------------------- создание и запуск согласования

                    MessageBox.Show("Задание на подписание документа создано.");
                    IStateService stateService = CardControl.ObjectContext.GetService<IStateService>();
                    StatesState endState = stateService.GetStates(document.SystemInfo.CardKind).First(t => t.DefaultName.Equals("Is signing"));
                    if (OldReconciliation != Guid.Empty)
                        ReconciliationRow.SetGuid("Reconciliation", (Guid)OldReconciliation);

                    CardControl.ObjectContext.AcceptChanges();
                    CardControl.ObjectContext.SaveObject(document);
                    CardFrame.Close();
                    break;

                case MessageResult.No:
                    break;
                default: break;
            }
        }

        private void SUEKDepartment_ControlValueChanged(System.Object sender, System.EventArgs e)
        {
            IsDepartmentControlValueCorret(sender as IPropertyControl);
        }

        #endregion

    }
}
