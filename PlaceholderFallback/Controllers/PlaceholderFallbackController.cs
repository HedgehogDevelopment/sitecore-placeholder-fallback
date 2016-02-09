using System.Web.Mvc;
using HedgehogDevelopment.PlaceholderFallback.Pipelines.GetXmlBasedLayoutDefinition;
using PlaceholderFallback.Models.PlaceholderFallback;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;

namespace HedgehogDevelopment.PlaceholderFallback.Controllers
{
    public class PlaceholderFallbackController : SitecoreController
    {
        /// <summary>
        /// If a fallback rendering is shown on the page, make it invisible.
        /// </summary>
        /// <returns></returns>
        public ActionResult FallbackRendering()
        {
            if (Sitecore.Context.PageMode.IsPageEditor || Sitecore.Context.PageMode.IsPreview)
            {
                return View(new FallbackRenderingInfo
                {
                    FallbackLocation = RenderingContext.Current.Rendering.Properties[InsertFallbackRenderings.FALLBACK_LOCATIONS_PROPERTY]
                });
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}