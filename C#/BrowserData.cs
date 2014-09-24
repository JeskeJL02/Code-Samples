
using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using cf = System.Configuration.ConfigurationManager;

namespace JacobJeske.Models
{
	public class BrowserData
	{
        private bool isMobile = false;
        private bool mobileView = false;
        private string userAgent = "";
        private ControllerContext cContext;
        private string mobileMasterViewName = FixMasterName(cf.AppSettings["MobileMasterViewName"], "mobile");
        public bool IsMobile { get { return isMobile; } }
        public bool ViewMobile { get { return mobileView; } }
        public string UserAgent { get { return userAgent; } }

        public BrowserData(ControllerContext cc)
		{
            this.cContext = cc;
            this.isMobile = GetBrowserData(cc.RequestContext.HttpContext.Request);
            if (cf.AppSettings["MobileActive"].ToLower() != "false")
                this.mobileView = this.isMobile;
            else
                this.mobileView = false;
		}

		public void ForceFullSite()
		{
            this.mobileView = false;
		}

        public void ForceMobileSite()
        {
            this.mobileView = true;
        }
        //This method can be used in the controller to get the appropriate view mobile vs standard.
        public ViewResult GetView(object model, ViewDataDictionary ViewData, TempDataDictionary TempData)
        {
            ViewResult vr = new ViewResult();
            vr.ViewData = ViewData;
            vr.TempData = TempData;
            if (vr.ViewData["ViewMobile"] == null)
                vr.ViewData["ViewMobile"] = this.mobileView;

            if (model != null)
            {
                Type type = model.GetType();
                ViewEngineResult result = null;
                
                switch (type.Name)
                {
                    case "StreamEntry":
                        {
                            #region <<<<<<      Get StreamEntry View     >>>>>>>
                            StreamEntry entryData = (StreamEntry)model;
                            result = GetViewResult(entryData.Stream.StreamUrl + "Entry");
                            if (result.View == null)
                            {
                                result = GetViewResult(entryData.Stream.StreamType + "Entry");
                                if (result.View == null)
                                {
                                    if (entryData.Stream.StreamType.Equals("ImageGallery"))
                                        result = GetViewResult("PhotoEntry");
                                    else if (entryData.Stream.StreamType.Equals("VideoGallery"))
                                        result = GetViewResult("VideoEntry");
                                    if (result.View == null)
                                        result = GetViewResult("Entry");
                                }
                            }

                            if (result.View != null)
                                vr.View = result.View;
                            else
                            {
                                //stream entry Defaults
                                vr.ViewName = "Entry";
                            }
                            vr.ViewData.Model = entryData;

                            if (this.mobileView)
                                vr.MasterName = mobileMasterViewName;
                            break;
                            #endregion
                        }
                    case "StreamViewModel":
                        {
                            #region <<<<<<      Get EntryList View       >>>>>>>
                            StreamViewModel svm = (StreamViewModel)model;
                            result = GetViewResult(svm.Stream.StreamUrl + "Stream");
                            if (result.View == null)
                            {
                                result = GetViewResult(svm.Stream.StreamType + "Stream");
                                if (result.View == null)
                                {
                                    if (svm.Stream.StreamType.Equals("ImageGallery"))
                                        result = GetViewResult("PhotoStream");
                                    else if (svm.Stream.StreamType.Equals("VideoGallery"))
                                        result = GetViewResult("VideoStream");
                                    if (result.View == null)
                                        result = GetViewResult("Stream");
                                }
                            }
                            if (result.View != null)
                                vr.View = result.View;
                            else
                            {
                                //stream entry Defaults
                                vr.ViewName = "Stream";
                            }
                            vr.ViewData.Model = svm;
                            if (this.mobileView)
                                vr.MasterName = mobileMasterViewName;
                            break;
                            #endregion
                        }
                    case "StreamNavLinksViewModel":
                        {
                            #region <<<<<< Get MenuOfStreamEntries View  >>>>>>>
                            StreamNavLinksViewModel snlvm = (StreamNavLinksViewModel)model;
                            string streamUrl = "";
                            foreach (KeyValuePair<string, object> r in snlvm.StreamRouteValues)
                            {
                                if (r.Key == "StreamUrl")
                                {
                                    streamUrl = r.Value.ToString();
                                    break;
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(streamUrl))
                            {
                                result = GetViewResult(streamUrl + "StreamMenu");
                                if (result.View == null)
                                {
                                    if (this.ViewMobile)
                                        result = GetViewResult("MobileStreamMenu");
                                    else
                                        result = GetViewResult("StreamMenu");
                                }
                            }
                            else
                            {
                                if (this.ViewMobile)
                                    result = GetViewResult("MobileStreamMenu");
                                else
                                    result = GetViewResult("StreamMenu");
                            }
                            vr.ViewData.Model = snlvm;
                            if (result.View != null)
                                vr.View = result.View;
                            else
                            {
                                //stream entry Defaults
                                vr.ViewName = "StreamMenu";
                            }
                            break;
                            #endregion
                        }
                    case "CategoryMenuViewModel":
                        {
                            #region <<<<<< Get CategoryEntryMenu View    >>>>>>>
                            CategoryMenuViewModel cmvm = (CategoryMenuViewModel)model;
                            if (cmvm != null)
                                result = GetViewResult("CategoryMenu" + cmvm.rootCategory.catID);
                            if (result.View == null)
                            {
                                //CategoryMenu
                                result = GetViewResult("CategoryMenu");
                            }
                            vr.ViewData.Model = cmvm;
                            if (result.View != null)
                                vr.View = result.View;
                            else
                            {
                                //stream entry Defaults
                                vr.ViewName = "CategoryMenu";
                            }
                            break;
                            #endregion
                        }
                    case "CategoryViewModel":
                        {
                            #region <<<<<<      Get Category View        >>>>>>>
                            CategoryViewModel cvm = (CategoryViewModel)model;
                            string catType = cvm.categoryMenu.rootCategory.CategoryType.ToLower();

                            if (catType.Equals("video"))
                            {
                                result = GetViewResult("VideoCategory" + cvm.categoryMenu.rootCategory.catName);
                                if(result.View == null)
                                    result = GetViewResult("VideoCategory");
                            }
                            else if (catType.Equals("photo"))
                            {
                                result = GetViewResult("PhotoCategory" + cvm.categoryMenu.rootCategory.catName);
                                if (result.View == null)
                                    result = GetViewResult("PhotoCategory");
                            }
                            else
                            {
                                result = GetViewResult("Category" + cvm.categoryMenu.rootCategory.catName);
                                if (result.View == null)
                                    result = GetViewResult("Category");
                            }
                            vr.ViewData.Model = cvm;
                            if (result.View != null)
                                vr.View = result.View;
                            else
                            {
                                //stream entry Defaults
                                vr.ViewName = "Category";
                            }
                            if (this.mobileView)
                                vr.MasterName = mobileMasterViewName;

                            break;
                            #endregion
                        }
                }
            }
            
            return vr;
        }

        private static string FixMasterName(string masterViewName, string defaultView)
        {
            int perIndex = masterViewName.IndexOf('.');
            if (perIndex > -1) { masterViewName = masterViewName.Substring(0, perIndex); }
            if (String.IsNullOrWhiteSpace(masterViewName)) { masterViewName = defaultView; }
            return masterViewName;
        }

        private bool GetBrowserData(HttpRequestBase Request)
        {
            bool mobile = false;
            string u = Request.ServerVariables["HTTP_USER_AGENT"].ToString();
            this.userAgent = u;
            Regex b = new Regex(@"android.+mobile|avantgo|bada\\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\\-(n|u)|c55\\/|capi|ccwa|cdm\\-|cell|chtm|cldc|cmd\\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\\-s|devi|dica|dmob|do(c|p)o|ds(12|\\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\\-|_)|g1 u|g560|gene|gf\\-5|g\\-mo|go(\\.w|od)|gr(ad|un)|haie|hcit|hd\\-(m|p|t)|hei\\-|hi(pt|ta)|hp( i|ip)|hs\\-c|ht(c(\\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\\-(20|go|ma)|i230|iac( |\\-|\\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\\/)|klon|kpt |kwc\\-|kyo(c|k)|le(no|xi)|lg( g|\\/(k|l|u)|50|54|e\\-|e\\/|\\-[a-w])|libw|lynx|m1\\-w|m3ga|m50\\/|ma(te|ui|xo)|mc(01|21|ca)|m\\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\\-2|po(ck|rt|se)|prox|psio|pt\\-g|qa\\-a|qc(07|12|21|32|60|\\-[2-7]|i\\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\\-|oo|p\\-)|sdk\\/|se(c(\\-|0|1)|47|mc|nd|ri)|sgh\\-|shar|sie(\\-|m)|sk\\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\\-|v\\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\\-|tdg\\-|tel(i|m)|tim\\-|t\\-mo|to(pl|sh)|ts(70|m\\-|m3|m5)|tx\\-9|up(\\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|xda(\\-|2|g)|yas\\-|your|zeto|zte\\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (b.IsMatch(u)) { mobile = true; }
            else if (u.Length >= 4) { if (v.IsMatch(u.Substring(0, 4))) { mobile = true; } }
            return mobile;
        }

        private ViewEngineResult GetViewResult(string ViewName, string masterName = "")
        {
            if (masterName == "")
                masterName = null;
            return ViewEngines.Engines.FindView(this.cContext, ViewName, masterName);
        }

	}
}
//This is the model binder for BrowserData
namespace JacobJeske.Utilities
{
    public class BrowserModelBinder : IModelBinder
    {
        public const string browserSessionKey = "_browser";
        public BrowserModelBinder() { }
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.Model != null) { throw new InvalidOperationException("Cannot create/update browser session."); }
            BrowserData bd = (BrowserData)controllerContext.HttpContext.Session[browserSessionKey];
            if (bd == null)
            {
                bd = new BrowserData(controllerContext);
                controllerContext.HttpContext.Session[browserSessionKey] = bd;
            }
            return bd;
        }
    }
}